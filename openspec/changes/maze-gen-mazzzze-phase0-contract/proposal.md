## Why

`mazzzze` renders its map from a throwaway coordinate hash (`MazeData.IsFloor`): an unsolvable 70%-open lattice on a fixed 10000×10000 grid, with none of the library's real maze structure, rooms/caves, POIs, or the endless/persisted/stitched world the [PRD vision](../../../docs/PRD.md#1-vision) describes. Phase 0 defines the **single library contract** that `mazzzze` — and any Godot game — codes against, derived top-down from the vision so it survives the whole evolution to the MMO, and proves the contract by rewriting `mazzzze`'s map engine to consume it for one real generated region.

## What Changes

- **New façade** (a curated public surface over the existing `GeneratedWorld` pipeline) exposing the vision's protocol as one narrow, freezable waist:
  - a **synchronous** region factory `GetOrCreate(regionAddress) → RegionView` — "generate a correct region here, once" (the game decides *when* to call it and off which thread);
  - a **`RegionView`**: Block-style cells carrying `AreaType` / tags / links / markers, with **no leakage** of internal `Area` machinery (`ExtensibleObject`, HardLinks/BakedLinks, child areas);
  - **POIs** — entrance/exit and dead-ends — surfaced as first-class metadata;
  - **region gates** — the openings on a region's *border* (the future seam anchors) — whose shape is reserved now even though stitching is not built;
  - **stable identity** — `(regionAddress, localCell) ⇄ world` — so the game keys mutations and fog-of-war to it across load/unload;
  - an **`IRegionStore`** persistence seam the game implements (engine never regenerates a stored region).
- **Give a region a place and gates** — a coordinate **address** (its place in the larger world) and **border gates** — two things a maze-gen `Area` does not model today (an `Area` is a closed box with only internal entrances).
- **BREAKING (mazzzze):** rewrite `mazzzze`'s map engine — replace `MazeData.IsFloor`/`GetChunkData` and the `0`/`1` tile encoding with a façade-driven region renderer. **v1 generates ONE region at startup and plays it**; no persistence, no stitching, no streaming yet.
- **Documented behavior changes on the maze-gen side** (with runnable examples): fix the longest-path marker **mis-tag (S6)** so entrance/exit POIs are correct; route the store seam through the **lossless `AreaSerializer`**, not the `Serialize()` debug-label **trap (D5)**.
- **Living integration guide.** Introduce `docs/INTEGRATION.md` as the single, always-latest integrator how-to covering **every** [`SCENARIOS.md`](../../../docs/SCENARIOS.md) scenario (D1–D6, S1–S7, C1–C5) with its current support status. Keep it from rotting by encoding the maintenance obligation as an **OpenSpec `config.yaml` rule** — auto-injected into every future change's task instructions — plus a normative requirement, rather than as prose in the roadmap.
- **Freeze the contract at this integration.** Later phases (persistence, seam-stitching, streaming/eviction) add capability *behind* the same surface — `mazzzze`'s calling code does not change shape.

## Capabilities

### New Capabilities
- `region-facade`: the frozen library protocol — region factory + `RegionView` + POIs + gates + stable identity + `IRegionStore` seam; synchronous, deterministic, transport-agnostic, `Area`-internals-free.
- `mazzzze-integration`: `mazzzze`'s map engine rewritten to consume the façade for one real region (render to `GridMap` + spawn/goal from POIs), replacing the `IsFloor` hash and validating the contract as the first reference consumer.
- `integration-guide`: a living, always-latest integration guide (`docs/INTEGRATION.md`) covering every `SCENARIOS.md` scenario with current status, kept current by an enforced OpenSpec config rule rather than by roadmap prose.

### Modified Capabilities
<!-- None — openspec/specs is empty; this is the first spec set. The longest-path and
     serialization fixes touch existing code but have no prior spec to modify; they are
     captured as requirements under region-facade. -->

## Impact

- **maze-gen (`src` + new façade):** a new façade namespace/type set over the `GeneratedWorld` pipeline; bug fixes to longest-path tagging (`DijkstraDistance.FindLongestTrail`) and a documented lossless-serialization path for the store seam. Core `src` stays **BCL-only**; façade may live in `src` or a thin sibling assembly (decided in design).
- **mazzzze (sibling repo `vasiliy-kiryukhin/mazzzze`, Godot 4 / net8.0):** map-engine rewrite — `MazeData` / `Chunk` / `ChunkManager` map-source replaced with a façade consumer; references `PlayersWorlds.Maps` (net8.0 — already the library's target). Game mechanics, creatures, and other systems owned by other developers are **not** touched.
- **Docs & process:** new `docs/INTEGRATION.md` (living, scenario-complete); a new `openspec/config.yaml` rule that enforces its upkeep on every future change; `docs/API-FIT.md` becomes a historical snapshot the guide supersedes for forward-looking guidance.
- **Contract stability:** this is the freeze point; Phases 1–3 capability lands behind the same surface.
