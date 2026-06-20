# PMerOptimizer

**Server-side performance optimization plugin for SCP: Secret Laboratory servers using MapEditorReborn schematics.**

Built for **LabAPI** · Author: **Souin**

---

## Overview

PMerOptimizer is a server-side optimization plugin designed to eliminate the massive FPS drops and server lag caused by large MapEditorReborn schematics (7MB+, thousands of parts). On heavy schematic servers, player FPS typically sits around 20–40. With PMerOptimizer, it jumps to 60+ FPS — without any client-side mods.

---

## How It Works

### Proximity Culling (Distance-Based Filtering)
Manages all schematic objects and dropped items based on each player's distance.
*   **Dynamic Visibility:** Objects outside the specified range are hidden from the player to reduce GPU load.
*   **Instant Sync:** Objects are restored instantly when the player enters the range.

### Collision Optimization (Physics Cleanup)
*   **Stutter Reduction:** Disables physics colliders on small decorative objects to eliminate micro-stutters.
*   **CPU Efficiency:** Reduces server load by cutting down unnecessary physics calculations.

### Static Object Forcing (Network Traffic Reduction)
*   **Network Optimization:** Flags stationary objects as `network-static`, stopping redundant network synchronization and saving bandwidth.

---

## Performance Comparison

| Feature | Standard (No Plugin) | With PMerOptimizer |
| :--- | :--- | :--- |
| **Player FPS** | 20–40 FPS | 60+ FPS (Stable) |
| **Server TPS** | Degraded | Healthy & Stable |
| **Network Sync** | High Overhead | Minimized |
| **Physics Load** | Heavy | Optimized |

---

## Installation

1. Ensure **LabAPI** and **MapEditorReborn** are installed on your server.
2. Download the latest `PMerOptimizer.dll`.
3. Copy the file into your server's `LabAPI/Plugins` directory.
4. Restart the server. The configuration file will generate automatically on first run.

---

## Configuration

| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `IsEnabled` | bool | `true` | Toggles the plugin. |
| `CullDistance` | float | `60.0` | Range (meters) to hide schematic objects. |
| `PickupCullDistance` | float | `35.0` | Range (meters) to hide dropped items. |
| `CullUpdateInterval` | float | `1.5` | Update frequency in seconds. |
| `MinColliderSize` | float | `0.3` | Collider size threshold for removal. |
| `ForceStaticObjects` | bool | `true` | Enable network-static optimization. |
| `EnablePickupCulling` | bool | `true` | Enable/Disable item culling. |

---

## Technical Details

*   **Framework:** .NET 4.8
*   **Dependencies:** LabAPI, MapEditorReborn, Mirror
*   **Patching:** Uses Harmony to intercept `AdminToyBase` and `SchematicObject` initialization.
*   **Networking:** Utilizes `Mirror.NetworkServer` internal methods for visibility control.

---

## License

All rights reserved. Unauthorized redistribution or modification is prohibited.

---

<p align="center">
  Made by <strong>Souin</strong>
</p>