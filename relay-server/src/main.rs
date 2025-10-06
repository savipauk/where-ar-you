use std::{fs, net::SocketAddr, sync::Arc};

use axum::{
    Json, Router,
    extract::{Query, State},
    http::StatusCode,
    response::IntoResponse,
    routing::{get, post},
};
use serde::{Deserialize, Serialize};
use sqlx::{SqlitePool, sqlite::SqlitePoolOptions};

struct AppState {
    db_pool: SqlitePool,
}

#[derive(Serialize, Deserialize, Debug, Clone, sqlx::FromRow)]
struct Location {
    username: String,
    latitude: f64,
    longitude: f64,
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
        create table if not exists locations (
            id integer primary key autoincrement,
            username text not null,
            latitude real not null,
            longitude real not null,
            timestamp integer not null
        );
        "#,
    )
    .execute(&pool)
    .await?;

    let app_state = Arc::new(AppState { db_pool: pool });

    let app = Router::new()
        .route("/location", post(create_location))
        .route("/locations", get(get_locations))
        .with_state(app_state);

    let addr = SocketAddr::from(([0, 0, 0, 0], 3000));
    println!("Server listening on {}", addr);
    let listener = tokio::net::TcpListener::bind(addr).await?;
    axum::serve(listener, app).await?;

    Ok(())
}

async fn create_location(
    State(state): State<Arc<AppState>>,
    Json(payload): Json<Location>,
) -> impl IntoResponse {
    println!("Received new location: {:#?}", payload);

    let result = sqlx::query(
        r#"
        insert into locations (username, latitude, longitude, timestamp)
        values (?, ?, ?, ?)
        "#,
    )
    .bind(payload.username)
    .bind(payload.longitude)
    .bind(payload.latitude)
    .bind(payload.timestamp)
    .execute(&state.db_pool)
    .await;

    match result {
        Ok(_) => {
            println!("Successfully stored location");
            (
                StatusCode::CREATED,
                Json(serde_json::json!({"status": "Location created successfully"})),
            )
        }
        Err(e) => {
            println!("Failed to store location: {}", e);
            (
                StatusCode::INTERNAL_SERVER_ERROR,
                Json(serde_json::json!({"error": "Failed to store location"})),
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
        select username, latitude, longitude, timestamp
        from locations
        where (username, timestamp) in (
            select username, max(timestamp)
            from locations
            where timestamp >= ?
            group by username
        )
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
