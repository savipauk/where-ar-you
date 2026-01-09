use std::{fs, net::SocketAddr, sync::Arc};

use axum::{
    Json, Router,
    extract::{Query, State},
    http::StatusCode,
    response::IntoResponse,
    routing::{get, post},
};
use reqwest::Client;
use serde::{Deserialize, Serialize};
use sqlx::{SqlitePool, sqlite::SqlitePoolOptions};

mod auth;
use auth::AuthenticatedUser;

pub struct AppState {
    db_pool: SqlitePool,
    http_client: Client,
    google_client_id: String,
}

#[derive(Serialize, Deserialize, Debug, Clone, sqlx::FromRow)]
struct Location {
    username: String,
    latitude: f64,
    longitude: f64,
    altitude: f64,
    timestamp: i64,
}

#[derive(Deserialize, Debug)]
struct GetLocationsParams {
    #[serde(default = "default_max_age")]
    max_age: u64,
}

fn default_max_age() -> u64 {
    300
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    println!("Starting up the server...");

    let google_client_id = std::env::var("GOOGLE_CLIENT_ID").expect("GOOGLE_CLIENT_ID must be set");

    let db_filename = "locations.db";

    if !std::path::Path::new(db_filename).exists() {
        fs::File::create(db_filename)?;
    }

    let pool = SqlitePoolOptions::new()
        .max_connections(1)
        .connect(&format!("sqlite://{}", db_filename))
        .await?;

    sqlx::query(
        r#"
        create table if not exists users (
            id text primary key,
            username text not null unique,
            name text,
            last_seen integer
        );

        create table if not exists locations (
            id integer primary key autoincrement,
            user_id text not null,
            latitude real not null,
            longitude real not null,
            altitude real not null,
            timestamp integer not null,
            foreign key (user_id) references users(id)
        );
        "#,
    )
    .execute(&pool)
    .await?;

    let app_state = Arc::new(AppState {
        db_pool: pool,
        http_client: Client::new(),
        google_client_id,
    });

    let app = Router::new()
        .route("/hello", get(hello))
        .route("/location", post(create_location))
        .route("/locations", get(get_locations))
        .with_state(app_state);

    let addr = SocketAddr::from(([0, 0, 0, 0], 3000));
    println!("Server listening on {}", addr);
    let listener = tokio::net::TcpListener::bind(addr).await?;
    axum::serve(listener, app).await?;

    Ok(())
}

async fn hello() -> impl IntoResponse {
    StatusCode::OK
}

async fn create_location(
    State(state): State<Arc<AppState>>,
    AuthenticatedUser(claims): AuthenticatedUser,
    Json(payload): Json<Location>,
) -> impl IntoResponse {
    println!(
        "Received new location: {:#?}, from user {}",
        payload, claims.email
    );

    // Do a transaction because either both the user and location insert succeed, or both fail
    let mut transaction = match state.db_pool.begin().await {
        Ok(transaction) => transaction,
        Err(e) => {
            println!("Failed to begin database transaction: {}", e);
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                "Database error".to_string(),
            );
        }
    };

    let current_timestamp = std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .map(|d| d.as_secs() as i64)
        .unwrap_or(0);

    if let Err(e) = sqlx::query(
        r#"
        insert into users (id, email, name, last_seen)
        values (?, ?, ?, ?)
        on conflict(id) do update set 
            email = excluded.email,
            name = excluded.name,
            last_seen = excluded.last_seen;
        "#,
    )
    .bind(&claims.subject)
    .bind(&claims.email)
    .bind(&claims.name)
    .bind(current_timestamp)
    .execute(&mut *transaction)
    .await
    {
        println!("Failed to update user: {}", e);
        transaction.rollback().await.ok(); // Attempt to rollback
        return (
            StatusCode::INTERNAL_SERVER_ERROR,
            "Failed to update user info".to_string(),
        );
    }

    let result = sqlx::query(
        r#"
        insert into locations (username, latitude, longitude, altitude, timestamp)
        values (?, ?, ?, ?, ?)
        "#,
    )
    .bind(payload.username)
    .bind(payload.longitude)
    .bind(payload.altitude)
    .bind(payload.latitude)
    .bind(payload.timestamp)
    .execute(&mut *transaction)
    .await;

    match result {
        Ok(_) => {
            if let Err(e) = transaction.commit().await {
                println!("Failed to commit transaction: {}", e);
                return (
                    StatusCode::INTERNAL_SERVER_ERROR,
                    "Database error on commit".to_string(),
                );
            }
            println!(
                "Successfully stored new location for user {}",
                &claims.email
            );
            (
                StatusCode::CREATED,
                "Location created successfully".to_string(),
            )
        }
        Err(e) => {
            println!("Failed to store location: {}", e);
            (
                StatusCode::INTERNAL_SERVER_ERROR,
                "Failed to store location".to_string(),
            )
        }
    }
}

async fn get_locations(
    State(state): State<Arc<AppState>>,
    Query(params): Query<GetLocationsParams>,
) -> impl IntoResponse {
    let current_timestamp = match std::time::SystemTime::now().duration_since(std::time::UNIX_EPOCH)
    {
        Ok(n) => n.as_secs() as i64,
        Err(_) => {
            return (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(serde_json::json!({"error": "Invalid system time"})),
            );
        }
    };

    let min_timestamp = current_timestamp.saturating_sub(params.max_age as i64);

    let result = sqlx::query_as::<_, Location>(
        r#"
        select 
            coalesce(u.name, 'Anonymous') as name,
            l.latitude,
            l.longitude,
            l.altitude,
            l.timestamp
        from locations l
        join users u on l.user_id = u.id
        where l.timestamp >= ?
        order by timestamp desc;
        "#,
    )
    .bind(min_timestamp)
    .fetch_all(&state.db_pool)
    .await;

    match result {
        Ok(locations) => {
            println!("Successfully fetched {} locations.", locations.len());
            (StatusCode::OK, Json(serde_json::json!(locations)))
        }
        Err(e) => {
            println!("Failed to fetch location: {}", e);
            (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(serde_json::json!({"error": "Failed to fetch locations"})),
            )
        }
    }
}
