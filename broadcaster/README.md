# broadcaster

> **Broadcaster** — mobile app that turns a phone into a tracked object by broadcasting its GPS position to the server.

## Overview

Each broadcaster is just a phone running this app.  
Its job is simple:

1. **Read GPS location** at intervals (e.g. every few seconds).  
2. **Send updates** via HTTP POST to the central server.  
3. Identify itself with a **unique ID**.

Multiple broadcasters can be active simultaneously, each reporting independently to the server.

## Responsibilities

- Access device GPS.
- Format and send location data:
- Handle basic retry if connection fails.
- Run efficiently in background (low power use).
