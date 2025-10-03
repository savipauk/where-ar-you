use std::{
    io::{BufReader, prelude::*},
    net::{TcpListener, TcpStream},
};

use axum::{
    Router,
    routing::{get, post},
};

#[tokio::main]
async fn main() {
    let app = Router::new()
        .route("/location", post(post_location))
        .route("/locations", get(get_locations));

    let listener = tokio::net::TcpListener::bind("127.0.0.1:7878")
        .await
        .unwrap();

    axum::serve(listener, app).await.unwrap();
}

async fn post_location() {}

async fn get_locations() {}

fn handle_connection(mut stream: TcpStream) {
    let buf_reader = BufReader::new(&stream);
    let http_request: Vec<_> = buf_reader
        .lines()
        .map(|result| result.unwrap())
        .take_while(|line| !line.is_empty())
        .collect();

    let response = "HTTP/1.1 200 OK\r\n\r\n";

    stream.write_all(response.as_bytes()).unwrap();
    println!("Request: {http_request:#?}");
}
