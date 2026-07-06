## Context

`PlayersWorlds.Maps` (maze-gen) is a BCL-only .NET 8 maze/dungeon generation library. Its public entry point is the fluent `GeneratedWorld` pipeline, which produces an `Area` — a rich internal model (flat `List<Cell>`, child-area tree, `ExtensibleObject` attachments, split HardLinks/BakedLinks, Border-vs-Block styles). `mazzzze` (sibling Godot 4 / net8.0 MMO) currently ignores the library and builds its map from a coordinate hash (`MazeData.IsFloor`) on a fixed 10000×10000 grid.

The [PRD vision](../../../docs/PRD.md#1-vision) is an **endless, generate-once-persist-share, independently-generated-then-stitched** world. The [ROADMAP](../../../docs/ROADMAP.md) settled the world model and the "contract-first, capability-progressive" principle. This change is **Phase 0**: design the one library contract a Godot game codes against, freeze it at first integration, and prove it by rewriting `mazzzze`'s map engine to consume it for a single real region.

The vision's §1.2 responsibility boundary *is* the contract: every arrow crossing the **engine ↔ game** boundary is a protocol method; everything inside the game box (persist, stream, evict, transfer the player, mutate) the protocol must *enable*, never *do*.

## Goals / Non-Goals

**Goals:**
- A curated **façade** that exposes the vision's protocol as a narrow, freezable waist over the existing pipeline, leaking no `Area` internals.
- Give a region the two things a maze-gen `Area` lacks today: a **place** (coordinate address) and **gates** (border openings — the future seam anchors), with the gate *shape* reserved now.
- Correct, first-class **POIs** (entrance/exit + dead-ends) — which requires fixing the longest-path mis-tag.
- A synchronous `GetOrCreate(address) → RegionView` factory and an `IRegionStore` persistence seam the game implements.
- Rewrite `mazzzze`'s map engine to generate and render **one** real region, replacing the `IsFloor` hash — the first reference integration that validates the contract.

**Non-Goals (deferred behind the frozen contract):**
- Seam-**stitching** generation (Phase 2) — only the gate *shape* is reserved.
- Persistence store implementation, multi-region worlds, streaming/eviction, player hand-off across seams — all **game-side** and later-phase.
- Async/threaded generation *in the contract* — the call is synchronous; the game threads it.
- Elevation/3D, environment tagging, distribution tuning (Phase 3).

## Decisions

### D1 — Façade as a new namespace in `src` (one DLL), not raw `Area`, not a separate assembly
The game codes against new façade types (`RegionView`, `RegionCell`, POIs, gates, `IRegionStore`), **never** `Area`. Rationale: `Area` exposes internals that would couple Godot to the pipeline and prevent the contract from freezing. Keep the façade in `PlayersWorlds.Maps` (a `…World`/`…Contract` namespace) so Godot references **one** DLL; core stays BCL-only. *Alternative — separate façade assembly:* rejected for now (extra DLL, packaging overhead, marginal isolation). Extractable later without breaking consumers, since they only touch façade types.

### D2 — `RegionView` wraps a baked **Block** region; cells carry a typed payload
The façade emits **Block** cells (walls occupy their own cells) because a `GridMap`-style renderer is inherently block-based and tags exist only after Block conversion (`ToMap`). `RegionCell` exposes: passability (wall vs floor), `AreaType` (maze/hall/cave/fill/environment), block tags, links, and markers. No `Cell`/`Area` object escapes; `RegionView` returns read-only value data. *Alternative — expose Border + let game convert:* rejected (pushes internal knowledge to every consumer).

### D3 — Address + identity is region-relative; world coords are derived
`RegionAddress` is a `Vector` (N-D-ready) naming a region's place on the region lattice. Stable identity is `(RegionAddress, localCell)`; `ToWorld(address, size, local)` / `FromWorld(world, size)` derive absolute coords for the game's convenience. Fog-of-war and mutations key to the **relative** identity so a region can unload/reload transparently — the endless-world requirement. The fixed 10000² grid dies; the address space is unbounded.

### D4 — Gates are metadata surfaced now; gate-aware *generation* is deferred
`Gate` = a region border edge + the open cells along it. `RegionView.Gates` is part of the contract from day 1 (its shape is the one thing that forces a contract break if wrong). Phase 0 **surfaces** gates a region has; Phase 2 **consumes** a neighbor's gates as pre-carved stitch constraints (P3). This is the "entrance = minimap marker AND seam anchor, design once" unification.

### D5 — `GetOrCreate` is synchronous; the store seam owns generate-once
```mermaid
flowchart LR
  G["game (client or server)"] -->|GetOrCreate(address)| W["World façade"]
  W -->|TryLoad| S["IRegionStore (game impl)"]
  S -->|hit| RV["RegionView"]
  S -->|miss| GEN["generate region (own seed)<br/>+ stitch neighbors (Phase 2)"]
  GEN -->|Save| S
  GEN --> RV
```
The call blocks (maze gen is bounded but can be slow — ~174 ms cold for 32²); the **game** decides when to call it and off which thread (e.g. ahead of a frontier, on a worker). There is no "endless" call — each call generates one bounded region; the endless world emerges from the game calling repeatedly. mazzzze v1 passes a `NullRegionStore` (always-miss, never-save) → regenerate at startup.

### D6 — Documented maze-gen behavior changes: fix S6 and route persistence through `AreaSerializer`
Correct POIs require fixing `DijkstraDistance.FindLongestTrail` marker mis-tagging (end marker on start cell; trail mis-tagged). The `IRegionStore` round-trip MUST use the lossless `AreaSerializer.Serialize/Deserialize`, **not** `GeneratedWorld.Serialize()`/`Area.ToString()` (debug labels that do not round-trip — the D5 trap). Both are shipped with runnable examples documenting the changed behavior.

### D7 — mazzzze integration keeps chunk residency; swaps only the map *source*
`mazzzze`'s `ChunkManager` keeps its load/unload ring; `Chunk` keeps writing to `GridMap`. Only the **source** changes: a new map-engine node calls `GetOrCreate` once at startup, holds the `RegionView`, and chunks *sample the resident region* instead of calling the `IsFloor` hash. Spawn/goal come from POIs. This is the minimal disruption that still validates every load-bearing part of the contract, and it leaves the residency machinery that the endless version will later generalize to regions.

### D8 — Living integration guide as a doc; maintenance enforced by a config rule, not prose
`docs/INTEGRATION.md` is a **living** document (cumulative, never archived), organized by the three `SCENARIOS.md` runtime actors, listing **every** scenario with a current support status (`✅/🟡/🔴 + phase`) and — for supported ones — a recipe against the current public API. It is *not* folded into a change spec (specs are per-change deltas that archive; a living how-to is not). Its **upkeep** is made durable two ways: (1) a normative requirement in the `integration-guide` capability (survives archive as living-spec truth); (2) an `openspec/config.yaml` `rules` entry on `tasks`/`specs` — *"a change that alters public API or a scenario's support status MUST update `docs/INTEGRATION.md` and re-mark the affected `SCENARIOS.md` rows"* — which the tooling **auto-injects into every future change's instructions**. This converts a recurring manual burden into a one-time enforced rule, so neither the roadmap nor each future spec has to "remember."

**Seed depth:** first version is the **skeleton only** — the actor spine + a complete per-scenario status table (all 17 rows present, planned ones as "Phase N" stubs) — and recipes fill in as the guide is maintained. Rationale: prove the maintenance mechanism before investing in prose that would otherwise drift. *Alternative — seed full recipes now:* deferred; observe upkeep first. `docs/API-FIT.md` (a one-time gap snapshot) freezes as history once the guide exists.

## Risks / Trade-offs

- **Gate shape guessed wrong forces a contract break** → keep `Gate` minimal (border edge + open cells), design it against the Phase-2 stitch intent (P3) even though stitching isn't built; validate the shape with a paper walkthrough of a two-region seam.
- **Cold generation blocks the frame** (~174 ms/32² cold) → acceptable at v1 startup (loading screen); document a size/latency envelope (P7); the game threads larger gens.
- **Block expansion (N maze cells → ~2N+1 world cells) breeds off-by-one coordinate bugs** → centralize in `ToWorld`/`FromWorld` with tests; the façade owns the mapping, not consumers.
- **`Area.ShallowCopy` aliases `_cells`/`_childAreas`** (§9 #1) → if the façade copies regions, use `Cell.Clone()` or a serialize round-trip for real isolation; never hand out an aliased region.
- **Façade accidentally leaks `Area`** → `RegionView` returns value/read-only data only; no `Area`/`Cell` reference escapes; enforce with a contract test.
- **Cross-repo change** (mazzzze lives in a sibling repo) → mazzzze edits are confined to the map engine; other systems untouched; the maze-gen additions are inert until consumed, so rollback = revert the mazzzze PR.

## Migration Plan

1. **maze-gen (additive):** add the façade namespace + `RegionView`/`RegionCell`/`Gate`/`RegionAddress`/`IRegionStore`/`World`; fix S6 longest-path; add the lossless-serialization path + examples. No existing public API removed → non-breaking for any current consumer.
2. **mazzzze (behind its own PR):** replace the `MazeData` hash source with a façade consumer; wire spawn/goal to POIs; keep `ChunkManager`/`Chunk`. Rollback = revert this PR; the library additions remain dormant.
3. **Freeze** the façade surface at merge; Phases 1–3 add capability behind it.

## Open Questions

- Same-assembly namespace (D1 recommendation) vs. sibling façade DLL — revisit only if hard isolation is later required.
- Exact `Gate` representation (border edge + open-cell list vs. richer descriptor) — reserve now, refine with the Phase-2 stitch (P3) design.
- Whether mazzzze v1 should opt into persistence (a real `IRegionStore`) or stay regenerate-at-startup — v1 default is regenerate; persistence is a free later opt-in behind the seam.
- Region size / dimensionality defaults for mazzzze's single region — gameplay-driven, not a contract concern; `RegionAddress`/`Vector` stays N-D-ready.

## As-built freeze (Phase 0)

The **frozen façade surface** is `PlayersWorlds.Maps.World` (`src/world/`): `World.GetOrCreate(RegionAddress) → RegionView`; value types `RegionAddress`, `RegionCell`, `Poi`/`PoiKind`, `Gate`; the `IRegionStore` seam + `NullRegionStore`. Phases 1–3 add capability *behind* this surface without changing v1's calls.

Refinements the implementation made over the proposal (all validated by grounding a real Block region):
- **Passability lives in tags, not links.** Block cells carry no links, so `RegionCell` is `{ IsPassable (from the trail tag), Type, Tags }` — the proposal's `links`/`markers` fields were dropped as vestigial.
- **POIs are baked as serializable cell tags.** `ToMap` discards the Border-maze POI markers, so the factory captures POIs pre-`ToMap` and bridges Border→Block coordinates via `CellsMapping.CenterPosition`, then tags the Block cells (`REGION_ENTRANCE/EXIT/DEADEND`). `RegionView` derives POIs from those tags, so a region is self-describing and its POIs survive persistence with no side-channel.
- **`IRegionStore` is a blob store.** It persists opaque serialized strings keyed by address; the engine owns the lossless `AreaSerializer` round-trip (not `RegionView` objects). Cleaner for a game's KV/blob store.
- **Per-address determinism** via a new public `RandomSource.FromSeed(int)`; each region's seed derives from `(worldSeed, address)`.
- **Per-cell room type flattens to `Environment`** in current Block output; room/cave/corridor typing per cell is deferred behind the same `RegionCell.Type` field.
