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
- [ ] 3.5 Measure and document a region-size → generation-latency envelope (P7) so the game knows frame-safe vs. must-thread sizes — measured; documented in `docs/INTEGRATION.md` (see group 5)

## 4. mazzzze map-engine rewrite (mazzzze-integration, sibling repo — its own PR)

- [ ] 4.1 Add a `PlayersWorlds.Maps` (net8.0) reference to the `mazzzze` project
- [ ] 4.2 Add a map-engine node that calls the façade `GetOrCreate` once at startup with a `NullRegionStore` and holds the returned `RegionView`
- [ ] 4.3 Change `Chunk`/`ChunkManager` to sample cells from the resident `RegionView` (mapped to tiles by cell payload) instead of `MazeData.GetChunkData`/`IsFloor`; keep the load/unload residency ring
- [ ] 4.4 Derive player spawn from the region entrance POI and the level goal from the exit POI (remove hard-coded `(1,1)`/fixed entrance/exit)
- [ ] 4.5 Remove `MazeData.IsFloor`, the `0`/`1` encoding, and the fixed 10000×10000 bounds as the map source
- [ ] 4.6 Smoke-run `mazzzze`: player walks a solvable, room-bearing region with a correct entrance/exit; other game systems (creatures, mechanics) unaffected

## 5. Living integration guide (integration-guide)

- [ ] 5.1 Add the enforcement rule to `openspec/config.yaml` under `rules.tasks` (and `rules.specs`): a change that alters public API or a scenario's support status MUST update `docs/INTEGRATION.md` and re-mark the affected `docs/SCENARIOS.md` rows
- [ ] 5.2 Create `docs/INTEGRATION.md` **skeleton**: header (living doc; supersedes `API-FIT.md` for forward guidance), a "status at a glance" table with all 17 `SCENARIOS.md` rows (D1–D6, S1–S7, C1–C5) marked with current status + phase, and the three actor sections with per-scenario placeholders
- [ ] 5.3 Fill the Phase-0-supported rows enough to be usable (at minimum S1 region-factory and the C-actor consume/render/identity paths reference the façade usage sketch); leave 🔴 rows as "planned — Phase N" stubs
- [ ] 5.4 Add a one-line pointer to `docs/INTEGRATION.md` from `docs/ROADMAP.md` References (canonical living integration doc), and a memory pointer in `MEMORY.md`

## 6. Freeze, validate, document

- [ ] 6.1 Run `openspec validate maze-gen-mazzzze-phase0-contract --strict` and `make ci` (lint + build + test) green
- [ ] 6.2 Update `docs/ROADMAP.md` §3 to mark the Phase-0 contract as defined and frozen at this integration
- [ ] 6.3 Note the frozen façade surface as the freeze point; record that Phases 1–3 add capability behind it without changing the calls v1 makes
