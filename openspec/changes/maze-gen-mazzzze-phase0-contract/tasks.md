## 1. maze-gen behavior fixes (documented with examples)

- [x] 1.1 Fix `DijkstraDistance.FindLongestTrail` longest-path mis-tag (end marker lands on start cell; trail cells mis-tagged) so entrance/exit resolve to two distinct correct cells (§9 #2/#3)
- [x] 1.2 Add/adjust unit tests asserting the entrance and exit tags land on the correct, distinct cells
- [x] 1.3 Confirm the lossless round-trip path is `AreaSerializer.Serialize/Deserialize`; add a test asserting `GeneratedWorld.Serialize()`/`Area.ToString()` are debug labels that MUST NOT be fed to the store seam (documents the D5 trap)
- [x] 1.4 Add a runnable example (CLI `usecase`/snippet) demonstrating the corrected POIs and the lossless round-trip

## 2. Façade contract types (region-facade)

- [x] 2.1 Add the façade namespace in `src` (one DLL, core stays BCL-only); decide final namespace name — `PlayersWorlds.Maps.World` under `src/world/`
- [x] 2.2 Define `RegionAddress` (a `Vector`, N-D-ready) and `ToWorld`/`FromWorld` identity helpers with round-trip tests, accounting for Block expansion (N maze cells → ~2N+1 world cells). Note: `ToWorld`/`FromWorld` operate in the actual Block size (read from `RegionView.Size`), so expansion is handled by using the real rendered size rather than assuming 2N+1
- [x] 2.3 Define `RegionCell` value type: passability (wall/floor from the trail tag), `AreaType`, block tags — no `Area`/`Cell` object exposed. Note: dropped `links` and `markers` — grounding showed Block cells carry no links, and POIs are surfaced at region level via `RegionView.Pois`
- [x] 2.4 Define `RegionView`: read-only Block cell accessor + `RegionAddress` + `Size`, backed by a baked Block region; isolation via factory-owned/deserialized `Area` (never aliased `ShallowCopy`)
- [x] 2.5 Define POIs on `RegionView`: entrance/exit pair (from the fixed longest-path) and dead-ends (from `DeadEndsExtension`), bridged from Border→Block coords and baked as serializable cell tags so they survive persistence
- [x] 2.6 Define `Gate` (border edge + open cells along it) and `RegionView.Gates`; surface gates a region has (gate-aware *generation* deferred to Phase 2)
- [x] 2.7 Add a contract test asserting no `Area`/`Cell`/`ExtensibleObject`/child-area reference is reachable through the façade's public surface (reflection over the `PlayersWorlds.Maps.World` public surface)

## 3. Region factory + persistence seam (region-facade)

- [x] 3.1 Define `IRegionStore { TryLoad(address, out serialized); Save(address, serialized); }` and a `NullRegionStore` (always-miss, never-save). Note: the store persists opaque serialized blobs keyed by address (the engine owns lossless `AreaSerializer` (de)serialization), rather than dealing in `RegionView` — cleaner for a game's blob store
- [x] 3.2 Implement `World.GetOrCreate(address)`: `TryLoad` → hit returns stored region; miss generates (per-address seed via `RandomSource.FromSeed`) + `Save` + returns; synchronous, no streaming
- [x] 3.3 Wire generation to the existing `GeneratedWorld` pipeline (Block-styled, area-populated, POIs marked as tags)
- [x] 3.4 Tests: deterministic generation for a fixed seed; stored region is loaded not regenerated (different-seed-shared-store proof); store round-trip preserves cells + POIs; `NullRegionStore` regenerates and persists nothing
- [x] 3.5 Measure and document a region-size → generation-latency envelope (P7) so the game knows frame-safe vs. must-thread sizes — measured (8²–64²) and documented in `docs/INTEGRATION.md`; ≤16² main-thread-safe, 32²+ thread it

## 4. mazzzze map-engine rewrite (mazzzze-integration, sibling repo — its own PR)

Done on the sibling `mazzzze` repo, branch `integrate-maze-gen-facade`, commit `c911af7` (own PR). Minimal-disruption (D7): `MazeData`'s public surface preserved, internals swapped to the façade, so `ChunkManager`/`Chunk`/`Minimap`/`Player` are untouched.

- [x] 4.1 Add a `PlayersWorlds.Maps` (net8.0) reference to the `mazzzze` project — `ProjectReference` to the sibling `src/` checkout (NuGet/DLL packaging is the CI/release follow-up)
- [x] 4.2 Add a map-engine node that calls the façade `GetOrCreate` once at startup with a `NullRegionStore` and holds the returned `RegionView` — `MazeData` (now façade-backed) generates one region in `_Ready`
- [x] 4.3 Change `Chunk`/`ChunkManager` to sample cells from the resident `RegionView` instead of `MazeData.GetChunkData`/`IsFloor`; keep the load/unload residency ring — `IsFloor`/`GetChunkData` now sample the region; the residency ring is unchanged
- [x] 4.4 Derive player spawn from the region entrance POI and the level goal from the exit POI (remove hard-coded `(1,1)`/fixed entrance/exit) — `PlayerStartCell`/`EntranceCell`/`ExitCell` come from POIs
- [x] 4.5 Remove `MazeData.IsFloor`, the `0`/`1` encoding, and the fixed 10000×10000 bounds as the map source — the hash is gone; bounds are the region's Block size (the `0`/`1` values remain only as `Chunk.Setup`'s tile ids, i.e. the render contract, not the map source)
- [ ] 4.6 Smoke-run `mazzzze`: player walks a solvable, room-bearing region with a correct entrance/exit; other game systems unaffected — ⚠️ NOT COMPLETE: the project **compiles** (0 errors) and the region logic is covered by maze-gen tests, but launching the Godot game is a GUI action that needs the user's Godot editor; the in-editor walk-through is pending

## 5. Living integration guide (integration-guide)

- [x] 5.1 Add the enforcement rule to `openspec/config.yaml` under `rules.tasks` (and `rules.specs`): a change that alters public API or a scenario's support status MUST update `docs/INTEGRATION.md` and re-mark the affected `docs/SCENARIOS.md` rows
- [x] 5.2 Create `docs/INTEGRATION.md` **skeleton**: header (living doc; supersedes `API-FIT.md` for forward guidance), a "status at a glance" table with all `SCENARIOS.md` rows (D1–D6, S1–S7, C1–C5) marked with current status + phase, and the three actor sections with per-scenario content
- [x] 5.3 Fill the Phase-0-supported rows enough to be usable (façade quickstart, S1 region-factory, S3 persistence, C-actor consume/render/identity, latency envelope, mazzzze reference); left 🔴 rows as "planned — Phase N" stubs
- [x] 5.4 Add a one-line pointer to `docs/INTEGRATION.md` from `docs/ROADMAP.md` References (canonical living integration doc), and a memory pointer in `MEMORY.md`. Also dogfooded the rule: re-marked `SCENARIOS.md` S1 (🔴→✅) and S3 (🟡→✅)

## 6. Freeze, validate, document

- [x] 6.1 Run `openspec validate maze-gen-mazzzze-phase0-contract --strict` (valid) and `make ci` (lint + build + 334 tests) green
- [x] 6.2 Update `docs/ROADMAP.md` §3 to mark the Phase-0 contract as defined and frozen at this integration (Phase 0 ✅ frozen; Phase 1 🚧 largely landed)
- [x] 6.3 Note the frozen façade surface as the freeze point; record that Phases 1–3 add capability behind it without changing the calls v1 makes (design.md "As-built freeze" + ROADMAP §3)
