FROM rust:1.90-alpine3.20 AS builder

RUN apk add --no-cache musl-dev openssl-dev pkgconfig sqlite-dev

WORKDIR /app

COPY Cargo.toml Cargo.lock ./
COPY src ./src

RUN cargo build --release

FROM alpine:3.20
RUN apk add --no-cache sqlite-libs ca-certificates

RUN adduser -D appuser \
    && mkdir -p /data \
    && chown -R appuser:appuser /data

WORKDIR /data

COPY --from=builder /app/target/release/relay-server /usr/local/bin/relay-server
RUN chown appuser:appuser /usr/local/bin/relay-server && chmod +x /usr/local/bin/relay-server

USER appuser

EXPOSE 3000

CMD ["relay-server"]

