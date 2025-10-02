# glasses

> **Glasses** — AR receiver app that renders tracked object positions in real-time using Unity + Xreal AR glasses.

---

## Overview

This app runs on the phone tethered to the Xreal glasses.  
It combines the phone’s **own GPS** (user position) with the server’s **object positions** and projects them into Unity world space for AR visualization.

---

## Responsibilities

- Poll the server for the latest object locations.  
- Convert **GPS coordinates → Unity world space** relative to the user.  
- Render markers in AR:  
  - 2D billboard  
  - 2D billboard + distance  
  - 3D model  
  - 3D model + distance  
- Update positions continuously in near real-time.  

