# ---- Builder stage ----
FROM rust:1.90-slim-bullseye AS builder

# Set up working directory
WORKDIR /usr/src/app

# Install dependencies for building static binaries
RUN apt-get update && apt-get install -y --no-install-recommends \
    musl-tools build-essential && \
    rm -rf /var/lib/apt/lists/*

# Add MUSL target for static linking
RUN rustup target add x86_64-unknown-linux-musl

# Explicitly set the C compiler for the target architecture to fix cross-compilation
ENV CC_x86_64_unknown_linux_musl=musl-gcc

# Copy manifests first (for caching)
COPY Cargo.toml Cargo.lock ./

# Pre-build dependencies
RUN mkdir src && echo "fn main() {}" > src/main.rs
RUN cargo build --target x86_64-unknown-linux-musl --release

# Copy the actual source code
COPY src ./src

# Build the final release binary
RUN cargo build --target x86_64-unknown-linux-musl --release

# ---- Runtime stage ----
FROM scratch

# Copy statically linked binary
COPY --from=builder /usr/src/app/target/x86_64-unknown-linux-musl/release/relay-server /relay-server

# Expose port and set command
EXPOSE 3000
CMD ["/relay-server"]


