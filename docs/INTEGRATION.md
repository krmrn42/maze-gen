# Integrating PlayersWorlds.Maps

**This is the single, living integration guide — it always reflects the *latest*
state of the library, not a fixed version.** For each usage scenario in
[`SCENARIOS.md`](./SCENARIOS.md) it says what you can integrate today and how,
and stubs what is planned. It supersedes [`API-FIT.md`](./API-FIT.md) for
forward-looking guidance (API-FIT is now a one-time gap snapshot kept as
history).

> **Upkeep is enforced, not remembered.** A change that alters public API or a
> scenario's support status must update this file and the affected
> `SCENARIOS.md` rows in the same change — an `openspec/config.yaml` rule
> injects that obligation into every future change's instructions.

Status legend: ✅ supported now · 🟡 partial · 🔴 planned (with the phase it
lands in). Phases are defined in [`ROADMAP.md`](./ROADMAP.md).

## Consuming the library

The library ships to **nuget.org** as `PlayersWorlds.Maps` (BCL-only — no
transitive dependencies to fight the Godot runtime):

```bash
dotnet add package PlayersWorlds.Maps
```

Other devs and CI just `dotnet build` — nothing to clone. If you also work on the
library itself, use a **conditional reference** so a local source checkout wins
automatically and everyone else falls back to the package:

```xml
<PropertyGroup>
  <!-- true when the maze-gen checkout sits beside this repo; override with
       -p:MazeGenLocal=false to force the published package. -->
  <MazeGenLocal Condition="'$(MazeGenLocal)' == '' And Exists('../../krmrn42/maze-gen/src/PlayersWorlds.Maps.csproj')">true</MazeGenLocal>
</PropertyGroup>
<ItemGroup Condition="'$(MazeGenLocal)' == 'true'">
  <ProjectReference Include="../../krmrn42/maze-gen/src/PlayersWorlds.Maps.csproj" />
</ItemGroup>
<ItemGroup Condition="'$(MazeGenLocal)' != 'true'">
  <PackageReference Include="PlayersWorlds.Maps" Version="0.1.*" />
</ItemGroup>
```

Releases are cut with `make release-minor` (or `-patch`/`-major`), which tags
`vX.Y.Z`; CI publishes that version to NuGet via OIDC trusted publishing.

**Prerelease channel.** Every merge to `main` also publishes a prerelease
(`X.(Y+1).0-preview.<n>`) so you can track the bleeding edge. Opt in with a
floating prerelease version or `--prerelease`:

```xml
<PackageReference Include="PlayersWorlds.Maps" Version="0.2.0-preview.*" />
```

Stable restores never pick up a prerelease unless you ask for one.

## The contract in one breath

A game codes against one narrow, frozen surface — the **region façade**
(`PlayersWorlds.Maps.World`) — never against `Area`/`Cell`. The engine
generates a bounded **region** on request; the game owns persistence, streaming,
and all mutations, keyed to the engine's stable identity.

```mermaid
flowchart LR
  G["game (client or server)"] -->|GetOrCreate(address)| W["World façade"]
  W -->|TryLoad| S["IRegionStore (game impl)"]
  S -->|hit: blob| W
  S -->|miss| GEN["generate region (per-address seed)"]
  GEN -->|Save blob| S
  W -->|RegionView| G
```

## Status at a glance

| # | Scenario | Status | Notes / phase |
|---|----------|--------|---------------|
| **D1** | Generate & preview one map from a seed | ✅ | `GeneratedWorld` pipeline; `make demo` |
| **D2** | Author a fixed layout | ✅ | `WithAreas(...)` typed child areas |
| **D3** | Reproduce a specific map by seed | ✅ | `RandomSource.FromSeed`; deterministic |
| **D4** | Compose layered / nested maps | ✅ | `AddLayer` + child-area overlays |
| **D5** | Serialize & reload a map as a fixture | ✅ | `AreaSerializer` (lossless) — **not** `Serialize()`/`ToString()` |
| **D6** | Tune area distribution for a target "feel" | 🟡 | Distribution tuning; Phase 3 |
| **S1** | Generate a new region on first visit | ✅ | **Phase 0** — `World.GetOrCreate(address)` |
| **S2** | Connect a region to neighbors (seam-stitching) | 🔴 | **Phase 2** — gate *shape* reserved (`RegionView.Gates`) |
| **S3** | Persist a region and retrieve it later | ✅ | **Phase 0** — `IRegionStore` + lossless blob |
| **S4** | Consistent world across instances via persistence | 🟡 | Follows from S1+S3 given a shared store; Phase 1 |
| **S5** | Merge two independently-grown "countries" | 🔴 | Phase 2+ (post-v1 vision) |
| **S6** | Extract gameplay metadata per region | 🟡 | POIs (entrance/exit/dead-ends) ✅; richer metadata later |
| **S7** | Persist & share in-place mutations | 🔴 | Phase 1+ — engine owns identity, game owns state |
| **C1** | Generate a region locally in real time | 🟡 | `GetOrCreate` is synchronous; game threads it (see envelope) |
| **C2** | Consume a server-streamed region | ✅ | A `RegionView` (or its serialized blob) is the transport unit |
| **C3** | Stream only the chunk(s) around the player | 🔴 | Phase 1 — region residency ring (mazzzze already rings chunks) |
| **C4** | Render fog-of-war over the known map | 🟡 | Key to `(RegionAddress, localCell)` identity; game-side |
| **C5** | Query connectivity / pathing for mobs & minimap | 🟡 | Passability per cell ✅; richer graph queries later |

---

## Phase-0 quickstart — the region façade

Everything below is the **supported** integration path today.

```csharp
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.World;

// 1. GAME START — make the world once. It holds what every region inherits:
//    seed, store, region footprint, and a default recipe. NullRegionStore =
//    regenerate each run, persist nothing (supply your own to persist).
var world = new World(
    store: new NullRegionStore(),
    worldSeed: 12345,
    regionSize: new Vector(65, 65),     // the region's WORLD footprint (Block
                                        // cells) == RegionView.Size, exactly.
    defaultRecipe: RegionRecipe.Maze);  // inherited unless a call overrides it

// 2. PLAYER POSITIONING / LOADING — ask for the region at an address, and let
//    THIS region pick its kind. Synchronous: generate-once, then load. You
//    decide when to call it and off which thread. A region's kind binds at
//    first creation (a later call with a different recipe returns the stored
//    region unchanged).
RegionView region = world.GetOrCreate(new RegionAddress(new Vector(0, 0)));
RegionView caves  = world.GetOrCreate(new RegionAddress(new Vector(1, 0)),
    RegionRecipe.Corridors                       // intent preset...
        .WithAlgorithm(RegionAlgorithm.HuntAndKill)  // ...or a specific algorithm
        .WithCells(1));                              // square cells (the default)
// Custom algorithm: RegionAlgorithm.Custom<MyGenerator>() (T : MazeGenerator).

// Rooms: presets Dungeon (halls) and Caverns (caves), or add your own.
// RoomKind.Hall / Cave (walkable) and Blocked (impassable rock/water). Sizes
// are in world cells; "types" of room are open-ended tags, not an enum.
RegionView dungeon = world.GetOrCreate(new RegionAddress(new Vector(2, 0)),
    RegionRecipe.Maze
        .WithRooms(6, minSize: new Vector(6, 6), maxSize: new Vector(10, 10),
                   kind: RoomKind.Hall, tags: "armory")
        .WithRooms(3, new Vector(4, 4), new Vector(6, 6), RoomKind.Blocked));

// 3. Read cells by region-local (Block) coordinate. Passability drives tiles.
for (var y = 0; y < region.Size.Y; y++) {
    for (var x = 0; x < region.Size.X; x++) {
        RegionCell cell = region.CellAt(new Vector(x, y));
        bool floor = cell.IsPassable;           // wall vs floor
        // cell.Type, cell.Tags -> pick a tile / style
    }
}

// 4. Points of interest come in region-local coordinates — place directly.
Poi entrance = region.Pois.First(p => p.Kind == PoiKind.Entrance);
Poi exit     = region.Pois.First(p => p.Kind == PoiKind.Exit);
// PoiKind.DeadEnd -> loot / secrets / encounters

// 5. Stable identity: (address, localCell). World coords are derived, so a
//    region can unload/reload without re-keying anything.
Vector worldCell = region.ToWorld(entrance.Local);
```

### S1 — Generate a new region on first visit ✅
`World.GetOrCreate(address)` *is* this scenario: generate a correct, solvable,
room-bearing region once, keyed to its address, deterministic for a given
`worldSeed`. The engine never regenerates a region the store already has.

### S3 — Persist a region and retrieve it later ✅
Implement `IRegionStore` as a plain blob store — the engine owns the lossless
`AreaSerializer` round-trip; you only persist/retrieve opaque strings:

```csharp
class MyStore : IRegionStore {
    readonly IDictionary<string, string> _db; // your DB / files / KV
    public bool TryLoad(RegionAddress a, out string blob) =>
        _db.TryGetValue(Key(a), out blob);
    public void Save(RegionAddress a, string blob) => _db[Key(a)] = blob;
    static string Key(RegionAddress a) => a.ToString();
}
```

On a store hit `GetOrCreate` reloads the exact region (cells + POIs survive as
serialized tags); on a miss it generates, saves, and returns.

### C-actor: consume / render / identity ✅🟡
A `RegionView` (or its serialized blob) is the unit a server hands a client
(**C2** ✅). Render straight from `CellAt(...).IsPassable` and cell tags. Fog
of war (**C4**) and mob pathing/minimap (**C5**) key to the
`(RegionAddress, localCell)` identity and per-cell passability; richer graph
queries are a later phase.

### Generation latency envelope (plan your threading — C1)
Cold `GetOrCreate` (generate + Block render + serialize), Release build, with the
default **square** 1×1 cells (Block side = 2·maze + 1):

| Region (maze) | Block size | Latency |
|---|---|---|
| 8×8 | 17×17 | ~27 ms |
| 16×16 | 33×33 | ~87 ms |
| 24×24 | 49×49 | ~108 ms |
| 32×32 | 65×65 | ~215 ms |
| 48×48 | 97×97 | ~690 ms |
| 64×64 | 129×129 | ~1.7 s |

Latency grows super-linearly with area. Rule of thumb: **≤16² is
loading-screen-safe on the main thread; 32² and up should generate on a worker
thread** ahead of the player's frontier.

## Reference integration: mazzzze

`mazzzze` (Godot 4 / net8.0) is the first reference consumer. Its map engine
(`MazeData`) generates one region at startup via `GetOrCreate` on a
`NullRegionStore`, and answers every map query — passability, chunk data,
player spawn (entrance POI), and level goal (exit POI) — from the resident
`RegionView`, keeping its chunk residency ring unchanged. See the
`integrate-maze-gen-facade` branch of the mazzzze repo.

---

## Planned scenarios (stubs)

These have no integration path yet; they land behind the **same** façade so v1
calling code does not change shape.

- **S2 — Seam-stitching** 🔴 *(Phase 2, core)* — generate a region whose border
  gates line up with a neighbour's. The gate *shape* (`RegionView.Gates`) is
  reserved now; gate-aware *generation* is Phase 2.
- **S5 — Merge two grown regions** 🔴 *(Phase 2+)* — post-v1 vision.
- **S7 — Persist & share mutations** 🔴 *(Phase 1+)* — the game stores
  mutations keyed to the engine's `(address, cell)` identity; the engine never
  owns mutable state.
- **C3 — Stream chunks around the player** 🔴 *(Phase 1)* — a region residency
  ring; mazzzze already rings chunks and will generalise to regions.
- **D6 / S4 / S6 / C4 / C5 partials** 🟡 — advance incrementally; see the table.
