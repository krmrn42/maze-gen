# API-Fit & Proposals — PlayersWorlds.Maps

> How the current **public** API answers each usage scenario, and — where it falls short — concrete API/pipeline proposals to close the gap. Every ✅/🟡 "current API path" was validated by compiling and running a snippet against the library **as a referenced DLL** (public surface only, as a Godot/Unity consumer sees it); results are in the [Validation log](#validation-log). 🔴 rows have no current path to run.
>
> Scenarios are defined in [SCENARIOS.md](SCENARIOS.md). Defect references point to [COMPONENT-REVIEW.md](COMPONENT-REVIEW.md) §9.

## Support taxonomy

- ✅ **Supported** — a documented public API path exists AND was exercised via a snippet.
- 🟡 **Partial** — primitives exist but the scenario needs glue, carries a caveat, or hits a known bug.
- 🔴 **Missing** — no public path; requires new API/pipeline work.

## Notable findings from validation

1. **The `Serialize()` trap.** `GeneratedWorld.Serialize()` and `Area.ToString()` return a *short debug label* (`"Maze:0x0;7x7"`), **not** a round-trippable string — feeding it to `AreaSerializer.Deserialize` throws. The only lossless path is `AreaSerializer.Serialize`/`Deserialize`. (Affects D5, S3, C2.)
2. **Cell tags require Block conversion.** A Border-style maze carries *links*, not `MazeWall`/`MazeTrail` tags. Tags appear only after `ToMap()` (Block conversion). A client mapping tags→tiles must stream Block maps. (Affects C2.)
3. **Pathing is public but unwired.** `DijkstraDistance.Find`/`Solve` and `Cell.Links()` are public and work (verified a 31-cell shortest path), but there is no per-region/streamed-graph convenience. (Affects C5.)
4. **Cold generation is slow.** A 32×32 border maze took ~174 ms cold (JIT-inclusive) — far over a frame budget; warm perf is unmeasured. (Affects C1.)
5. **`Maze2DRendererOptions` is nested** in `Maze2DRenderer` (needs `using static …Maze2DRenderer;`) — a discoverability quirk for D1/D4.

## Fit matrix

| Scenario | Status | Current public API path | Where it falls short | Gap | Routed to |
|----------|--------|-------------------------|----------------------|-----|-----------|
| <a id="d1"></a>[D1](SCENARIOS.md#d1) | ✅ | `GeneratedWorld.AddLayer/OfMaze/Map` + `Area.Render(AsciiRendererFactory)` | — | — | — |
| <a id="d2"></a>[D2](SCENARIOS.md#d2) | ✅ | `GeneratedWorld.WithAreas(params Area[])` + `Area.Create(pos,size,type[,tag])` | — | — | — |
| <a id="d3"></a>[D3](SCENARIOS.md#d3) | ✅ | `RandomSource.EnvRandomSeed` / seeded `RandomSource` | determinism tied to a global static | S | backlog |
| <a id="d4"></a>[D4](SCENARIOS.md#d4) | ✅ | `GeneratedWorld.AddLayer(Func<Area,Area>)` + `Area.ShallowCopy(cells:)` | `ShallowCopy` list-aliasing hazard (§9 #1) | S | bug-fix |
| <a id="d5"></a>[D5](SCENARIOS.md#d5) | ✅ | `AreaSerializer.Serialize/Deserialize` | `GeneratedWorld.Serialize()`/`Area.ToString()` look like the API but emit a non-round-trippable debug label | S | bug-fix |
| <a id="d6"></a>[D6](SCENARIOS.md#d6) | 🟡 | `WithAreas(types,tags,count,min,max)` → `BasicAreaGenerator` | coarse knobs; probability tables not exposed | M | C |
| <a id="s1"></a>[S1](SCENARIOS.md#s1) | 🔴 | pipeline produces an `Area` (own seed) | no coordinate-addressed region factory; no persistence hook (generate-once model) | M | D |
| <a id="s2"></a>[S2](SCENARIOS.md#s2) | 🔴 | `Area.Create` fixed areas only | no seam/border connection between *independently-generated* regions — **core** | L | D |
| <a id="s3"></a>[S3](SCENARIOS.md#s3) | 🟡 | `AreaSerializer` | no store, region keying, or load-by-region — **core** (source of truth) | L | D |
| <a id="s4"></a>[S4](SCENARIOS.md#s4) | 🟡 | seed determinism (in-process verified) | reframed: world consistency = persistence (S3), not regeneration | S | backlog |
| <a id="s5"></a>[S5](SCENARIOS.md#s5) | 🔴 | — | needs S2 + conflict resolution | L | D (post-v1) |
| <a id="s6"></a>[S6](SCENARIOS.md#s6) | 🟡 | `MarkDeadends`→`DeadEnd.DeadEndsExtension`; `MarkLongestPath`→`DijkstraDistance.LongestTrailExtension` | longest-path markers mis-tagged (§9 #2/#3) | S | bug-fix |
| <a id="s7"></a>[S7](SCENARIOS.md#s7) | 🔴 | — (game-side state) | engine must expose stable region+cell identity to key mutations to; store is game-side | M | D (identity) |
| <a id="c1"></a>[C1](SCENARIOS.md#c1) | 🟡 | full `GeneratedWorld` pipeline on the client | ~174 ms cold for 32×32; no budget; net47 target vs Godot .NET | L | D |
| <a id="c2"></a>[C2](SCENARIOS.md#c2) | ✅ | `AreaSerializer.Deserialize` → iterate cells → tags | tags exist only after Block `ToMap()`; Border carries links only | S | C (doc) |
| <a id="c3"></a>[C3](SCENARIOS.md#c3) | 🔴 | standalone `Area` per region | no chunk facade / cache / eviction | L | D |
| <a id="c4"></a>[C4](SCENARIOS.md#c4) | 🟡 | `Vector` arithmetic (convention) | no world-coordinate / stable-identity helper | M | D |
| <a id="c5"></a>[C5](SCENARIOS.md#c5) | 🟡 | `DijkstraDistance.Find/Solve` (public) + `Cell.Links()` | no per-region / streamed-graph convenience | M | D |

## Proposals

Signature-level sketches (illustrative). Each closes one or more matrix gaps; the seam theme is a *direction only* — its real design belongs to sub-project D.

### P1 — Coordinate-addressed region generation  *(closes S1; supports S4, S7)*
```csharp
// Generate a NEW region for a coordinate address, ONCE. Persistence (P2) stores it;
// it is never regenerated. The optional per-region seed only makes the FIRST
// generation reproducible for tests/bug-reports — it is NOT how consistency is achieved.
public Area GenerateRegion(Vector regionCoords, GeneratorOptions options, int? seed = null);
```
World consistency across players/instances comes from persisting and reloading this `Area` (P2), **not** from regenerating identical coordinates (that model was rejected — regions are independent generations).

### P2 — Region store & addressing (persistence source of truth)  *(closes S3; supports S1, S4, S7)*
```csharp
public interface IRegionStore {                 // implemented by the game/server
    bool TryLoad(Vector regionCoords, out Area region);
    void Save(Vector regionCoords, Area region);
}
public sealed class World {                      // engine-side façade over the store
    public World(IRegionStore store, GeneratorOptions options);
    public Area GetOrCreate(Vector regionCoords); // load persisted, else GenerateRegion + stitch + Save
}
```
Generate-once-then-load is the whole model. **Streaming/eviction (chunking, C3) is a *deferred* add-on** behind this same façade — not needed for first integration.

### P3 — Seam-aware stitching (core)  *(closes S2, S5 — direction only; designed in D)*
```csharp
// Generate a region that CONNECTS to already-persisted neighbors: their border
// entrances are injected as pre-carved HardLinks the new region's builder must honor.
public GeneratedWorld WithSeams(params Seam[] seams);
// Seam = a shared edge + the neighbor's entrance cells along it.
```
Key: the neighbor was generated **independently** (different seed) — this is border-connection between separate maps, not slicing one maze. Border coordination is the hard part and is D's central design problem.

### P4 — Stable world-cell identity  *(closes C4)*
```csharp
public static Vector ToWorld(Vector regionCoords, Vector regionSize, Vector localCoord);
public static (Vector region, Vector local) FromWorld(Vector worldCoord, Vector regionSize);
```
A tiny helper for fog-of-war "seen" state that survives region load/unload. No engine change.

### P5 — Modern .NET target  *(closes C1, C3; unblocks Actor 3 in Godot)*
Multi-target the core `src` project (`net472;net8.0`) — verified: the core library compiles unchanged on modern .NET. Add a CI matrix so the DLL is directly referenceable by Godot 4 (Mono/.NET 8+) and Unity (net47).

### P6 — Metadata & pathing surface  *(closes S6, C5)*
Fix `FindLongestTrail` marker mis-tagging (§9 #2/#3). Expose the already-public `DijkstraDistance.Find/Solve` through a small per-region convenience (e.g. `Area.Distances(from)`, `Area.Path(from,to)`) so consumers don't reach into the post-processing namespace.

### P7 — Real-time latency budget  *(closes C1)*
No new API — *measure* region-gen time (warm, per size) via the existing perf harness and document the budget envelope, so clients know what sizes are frame-safe vs. must be threaded/streamed.

## Prioritization & routing

Sorted by priority for the game build (D consumes the top cluster as its opening work-list).

| Priority | Item | Gap | Phase | Rationale |
|----------|------|-----|-------|-----------|
| 1 | P5 Modern .NET target | L | 1 | Blocks *all* Godot integration; low-risk (proven to compile) — the first domino |
| 2 | P3 Seam-aware stitching | L | 2 | **The** core MMO capability; connects independently-generated regions; hardest design |
| 3 | P2 Region store & addressing | L | 2 | Persistence = source of truth (generate-once-load-many) |
| 4 | P1 Coordinate-addressed region generation | M | 2 | The region factory the store calls |
| 5 | P4 Stable region/cell identity | M | 2 | Keys persisted mutations (S7) + fog-of-war; mazzzze already has FoW |
| 6 | P6 Metadata & pathing (+ bug fixes) | S–M | 1 | Cheap wins; longest-path bug affects entrance/exit markers |
| 7 | P7 Latency measurement | S | 1 | Informs whether client-side gen must be threaded |
| deferred | Chunk streaming / eviction (C3, within P2) | L | 2+ | Explicitly *not* needed for integration |
| bug-fix | D5 Serialize trap; D4 ShallowCopy aliasing; S6 longest-path | S | 1 | Correctness traps; bite integration early |
| cookbook | D6 distribution knobs; C2 tags-need-Block | S–M | 3 | Map-type recipes (tuning + tile mapping) |

## Validation log

Each ✅/🟡 claim was run through the library DLL on .NET (public API only). 🔴 rows (S1, S2, S5, C3) have no current path to run.

| ID | Result | Note |
|----|--------|------|
| D1 | RAN OK | rendered 338-char ASCII maze |
| D2 | RAN OK | 2 fixed child areas (Hall + Fill "lake") placed |
| D3 | RAN OK | identical serialization across two same-seed runs |
| D4 | RAN OK | second-layer maze built (378 cells) |
| D5 | RAN OK | `AreaSerializer` round-trip, 49 cells match (`Area.ToString()` does NOT round-trip) |
| D6 | RAN OK | 4 procedural child areas generated |
| S3 | RAN OK | serialized 1559 chars, reloaded 81 cells; no store/keying layer |
| S4 | RAN OK | deterministic in-process; cross-machine **asserted, not run** |
| S6 | RAN OK | `DeadEndsExtension.DeadEnds`=10; `LongestTrailExtension` present (tagging bug per §9) |
| C1 | RAN OK | 32×32 border maze in ~174 ms **cold** (JIT-inclusive); no documented budget |
| C2 | RAN OK | deserialized 36-cell region; 0 tags (Border maze — tags need Block `ToMap`) |
| C4 | RAN OK | world-coord convention computable via `Vector`; **no library API (asserted, not run)** |
| C5 | RAN OK | `DijkstraDistance.Find`+`Solve` public; 31-cell shortest path; 126 directed links |

*Harness: a net10 console app referencing the core `src` compiled as `PlayersWorlds.Maps.dll` — internal members correctly hidden, matching a real consumer.*
