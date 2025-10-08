use std::sync::Arc;

use axum::{
    extract::{FromRef, FromRequestParts},
    http::{StatusCode, request::Parts},
};
use jsonwebtoken::{DecodingKey, Validation, decode, decode_header};
use reqwest::Client;
use serde::{Deserialize, Serialize};

use crate::AppState;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub(crate) struct GoogleClaims {
    pub subject: String,
    pub email: String,
    pub name: String,
    pub audience: String,
    pub issuer: String,
    pub expiry: usize,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
struct Jwk {
    kid: String,
    n: String,
    e: String,
}

#[derive(Debug, Serialize, Deserialize)]
struct Jwks {
    keys: Vec<Jwk>,
}

pub async fn verify_google_token(
    token: &str,
    http_client: &Client,
    client_id: &str,
) -> Result<GoogleClaims, String> {
    let header = decode_header(token).map_err(|e| format!("Invalid token header: {}", e))?;
    let kid = header
        .kid
        .ok_or_else(|| "Token header missing 'kid'".to_string())?;

    let jwks = http_client
        .get("https://www.googleapis.com/oauth2/v3/certs")
        .send()
        .await
        .map_err(|e| format!("Failed to fetch JWKS: {}", e))?
        .json::<Jwks>()
        .await
        .map_err(|e| format!("Failed to parse JWKS: {}", e))?;

    let jwk = jwks
        .keys
        .into_iter()
        .find(|k| k.kid == kid)
        .ok_or_else(|| format!("Public key with kid '{}' not found in JWK set", kid))?;

    let decoding_key =
        DecodingKey::from_rsa_components(&jwk.n, &jwk.e).map_err(|e| e.to_string())?;

    let mut validation = Validation::new(jsonwebtoken::Algorithm::RS256);
    validation.set_audience(&[client_id]);
    validation.set_issuer(&["https://accounts.google.com", "accounts.google.com"]);

    let decoded =
        decode::<GoogleClaims>(token, &decoding_key, &validation).map_err(|e| e.to_string())?;

    Ok(decoded.claims)
}

#[derive(Debug)]
pub struct AuthenticatedUser(pub GoogleClaims);

impl<S> FromRequestParts<S> for AuthenticatedUser
where
    S: Send + Sync,
    Arc<AppState>: FromRef<S>,
{
    type Rejection = (StatusCode, String);

    async fn from_request_parts(parts: &mut Parts, state: &S) -> Result<Self, Self::Rejection> {
        let state = Arc::<AppState>::from_ref(state);

        let auth_header = parts
            .headers
            .get("Authorization")
            .and_then(|value| value.to_str().ok())
            .ok_or_else(|| {
                (
                    StatusCode::UNAUTHORIZED,
                    "Missing Authorization header".to_string(),
                )
            })?;

        let token = auth_header.strip_prefix("Bearer ").ok_or_else(|| {
            (
                StatusCode::UNAUTHORIZED,
                "Invalid authorization scheme, must be Bearer".to_string(),
            )
        })?;

        match verify_google_token(token, &state.http_client, &state.google_client_id).await {
            Ok(claims) => Ok(AuthenticatedUser(claims)),
            Err(e) => {
                println!("JWT verification failed: {}", e);
                Err((StatusCode::UNAUTHORIZED, format!("Invalid token: {}", e)))
            }
        }
    }
}
