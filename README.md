# PMerOptimizer

**Server-side performance optimization plugin for SCP: Secret Laboratory servers using [ProjectMER](https://github.com/Michal78900/MapEditorReborn) schematics.**

Built for [LabAPI](https://github.com/northwood-studios/LabAPI) · Author: **Souin**

---

## Overview

PMerOptimizer is a server-side optimization plugin designed to eliminate the massive FPS drops and server lag caused by large ProjectMER schematics (7MB+, thousands of parts). On heavy schematic servers, player FPS typically sits around **20–40**. With PMerOptimizer, it jumps to **60+ FPS** — without any client-side mods.

---

## How It Works

### Proximity Culling (Distance-Based Filtering)

Manages all schematic objects (cubes, walls, lights, etc.) and dropped items (weapons, keycards, etc.) based on each player's distance.

- When a player moves **away** from schematic objects (default: **60m**) or dropped items (default: **35m**), those objects are **hidden** from that player's view.
- When the player moves **back into range**, objects are **instantly restored**.
- The player's GPU only renders nearby elements instead of the entire map, resulting in a **massive FPS boost**.

### Collision Optimization (Physics Cleanup)

Disables physics colliders on very small decorative objects in schematics (tiny buttons, small detail props) on the server side.

- **Eliminates micro-stuttering** and movement freezes players experience near small decorative details.
- **Reduces server CPU load** by removing thousands of unnecessary physics calculations per tick.

### Static Object Forcing (Network Traffic Reduction)

Flags all non-animated, stationary decorative objects as **network-static**.

- **Eliminates redundant network sync** — the server no longer wastes bandwidth re-sending coordinates for objects that never move.
- **Significantly reduces server CPU usage** dedicated to network serialization.

---

## Benefits

| Area | Without PMerOptimizer | With PMerOptimizer |
|------|---------------------|-------------------|
| **Player FPS** | 20–40 FPS (heavy drops) | 60+ FPS (stable) |
| **Server TPS** | Degraded under load | Healthy & stable |
| **Network Traffic** | Redundant syncs for static objects | Optimized, minimal |
| **Physics Load** | Thousands of unnecessary collider checks | Cleaned up |
| **Hit Registration** | Delayed / desync | Responsive |

---

## Installation

1. Make sure you have **LabAPI** and **ProjectMER** installed on your SCP:SL server.
2. Download the latest `PMerOptimizer.dll` from the [Releases](https://github.com/your-repo/releases) page.
3. Place the DLL into your server's `LabAPI/Plugins` folder.
4. Restart the server. A default configuration file will be generated automatically.

---

## Configuration

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsEnabled` | bool | `true` | Enable or disable the plugin entirely |
| `CullDistance` | float | `60.0` | Distance (meters) at which schematic objects are hidden from a player |
| `PickupCullDistance` | float | `35.0` | Distance (meters) at which dropped items are hidden from a player |
| `CullUpdateInterval` | float | `1.5` | How often (seconds) the culling system recalculates visibility |
| `MinColliderSize` | float | `0.3` | Colliders smaller than this size (meters) are stripped for physics optimization |
| `ForceStaticObjects` | bool | `true` | Whether to flag stationary objects as network-static |
| `EnablePickupCulling` | bool | `true` | Whether to apply proximity culling to dropped items (pickups) |

---

## Code Protection

PMerOptimizer is fully **obfuscated** using industry-standard techniques:

- All internal class and method names are **scrambled**
- String literals are **encrypted**
- Decompilation tools (dnSpy, ILSpy, dotPeek) will **not** produce readable source code

> The plugin entry point and configuration class remain unobfuscated to ensure LabAPI can load the plugin correctly.

---

## Technical Details

- **Framework:** .NET 4.8 (SCP:SL runtime)
- **Dependencies:** LabAPI, ProjectMER, Mirror (included with SCP:SL)
- **Networking:** Uses `Mirror.NetworkServer` internal methods via reflection delegates for per-connection object visibility control
- **Patching:** Harmony patches intercept `AdminToyBase` and `SchematicObject` initialization to register objects with the culling manager
- **Performance:** Culling checks run on a configurable timer — not every frame — to minimize overhead

---

## License

All rights reserved. Unauthorized redistribution or modification is prohibited.

---

<p align="center">
  Made by <strong>Souin</strong>
</p>
