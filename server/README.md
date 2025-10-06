# relay-server

> **Relay Server** — lightweight HTTP relay that stores and serves the latest broadcaster positions.

---

## Overview

The server is the **hub** between broadcasters and glasses.  
It provides a minimal API:

- **POST /update** → broadcaster sends current position.  
- **GET /positions** → glasses fetch all current positions.  

The server does not simulate movement or do heavy computation — it just relays and stores.

---

## Responsibilities

- Store the **latest position** per broadcaster ID.  
- Return all tracked positions on request.  
- Provide simple JSON responses.  
- Remain lightweight and stateless.

