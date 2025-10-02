# where-ar-you

> **where-ar-you** — a small AR tracking system that shows the real-time location of mobile objects (phones) inside an Xreal/AR app.  
> Built as a 3-component monorepo: **broadcaster** apps (on tracked objects), an **HTTP server** (relay + store), and the **glasses**/Unity AR app (receiver + renderer).

---

## Overview

This project demonstrates **real-time AR object tracking** using phones as GPS broadcasters and a phone + Xreal glasses as the AR receiver.  
Key goals:

- Support **multiple simultaneous broadcasters** (each tracked object is a phone running the broadcaster app).
- Use a **central HTTP server** as relay (no P2P).
- The **glasses app** uses the paired phone’s GPS as the user location and requests object positions from the server, converting lat/lon → Unity world space and rendering markers in AR.
- Provide multiple visualizations (2D billboard, 2D+distance, 3D model, 3D+distance).

---

## Repo layout

where-ar-you/
│
├─ broadcaster/     # Mobile app: reads GPS, POSTs to server
│   └─ README.md    # Check this for more info
│
├─ glasses/         # Unity + mobile receiver: polls server, renders AR
│   └─ README.md    # Check this for more info
│
├─ server/          # HTTP service: accepts broadcasts, serves latest positions
│   └─ README.md    # Check this for more info
│
└─ README.md        # (this file)

---

## High-level architecture

         ┌───────────────────┐
         │   Broadcaster(s)  │   ← phones attached to tracked objects
         │  (GPS + network)  │
         └─────────┬─────────┘
                   │  sends GPS data
                   ▼
         ┌───────────────────┐
         │      Server       │   ← HTTP server that relays the data
         └─────────┬─────────┘
                   │  forwards object locations
                   ▼
         ┌───────────────────┐
         │     Glasses       │   ← phone tethered to Xreal AR glasses
         │ (GPS + Unity AR)  │   ← Xreal AR glasses implementation
         └───────────────────┘




