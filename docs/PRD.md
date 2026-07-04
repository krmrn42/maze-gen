# Product Requirements Document — PlayersWorlds.Maps

> **Status:** Living document, reverse-engineered from the codebase (v0.2, branch `circles`) blended with the stated product vision.
> **Audience:** Library maintainers, game developers integrating the library, and future contributors.
> **One-line:** A procedural map-generation library that produces the *backbone* of maze/dungeon worlds for 2D/3D games — corridors, rooms, halls, caves, and impassable zones — while leaving the actual game content to the consuming game.

---

## 1. Vision

The long-term product is an **endless, dynamically generated maze/dungeon world** for an MMO RPG. The defining properties of that world are:

- **Lazy generation.** Areas that no player has visited do not exist yet. They are generated on demand as players approach.
- **Central persistence.** Once an area is generated (visited), it is persisted centrally on the server and streamed to every player, so the world is consistent and shared.
- **Organic growth.** New communities or "countries" of players start in fresh, disconnected starting zones. As players explore, those zones eventually connect to the main landmass, producing a single massive open world that grows with the player base.

`PlayersWorlds.Maps` is **not** that server. It is the **map backbone library**: the deterministic, reproducible engine that a game server calls to produce map geometry. The game itself owns persistence, networking, gameplay, loot, mobs, and the visual layer. The library's job is to answer, correctly and reproducibly: *"What does the map look like here?"*

```mermaid
graph LR
    subgraph Game["Game / MMO Server (out of scope)"]
        P[Persistence + streaming]
        N[Networking / player state]
        G[Gameplay: loot, mobs, quests]
    end
    subgraph Lib["PlayersWorlds.Maps (this library)"]
        E[Deterministic map engine]
    end
    subgraph Client["Unity / Godot client (out of scope)"]
        R[Tile rendering + physics]
    end
    P -->|"request: generate/stitch a region (seed + coords)"| E
    E -->|"Area (cells, tags, links, POIs)"| P
    P -->|streams cells| R
    G -.->|reads cell tags: dead-ends, start/end| R
```

### 1.1 What "backbone" means concretely

The library produces an **`Area`**: an N-dimensional grid of **`Cell`s**. Each cell carries:

- an **`AreaType`** (maze corridor, hall, cave, fill/impassable, environment),
- **links** to neighboring cells (the actual carved passages — this *is* the maze topology),
- **tags** describing block-level content once styled (wall, trail, wall-corner, void),
- **markers** attached during post-processing: dead-ends (good loot spots) and the guaranteed longest path's start/end cells (good spawn/exit points).

The consuming game iterates the cells and decides what to draw and spawn. The library guarantees the *structure* (solvable maze, rooms with entrances, impassable zones respected); the game supplies the *meaning*.

---

## 2. Personas & user journeys

### Persona A — "Backbone integrator" (game/server engineer)
Builds the MMO server. Wants a deterministic API to generate map regions from a seed, persist them, and stream them. Cares about reproducibility, serialization, and the ability to place rooms at fixed world coordinates so generated regions line up.

**Journey — generate and persist a starting zone:**
1. Server receives "new community needs a starting zone at world offset (2000, 2000)."
2. Server calls the library with a seed and a region size, requesting a maze layer with auto-placed rooms/halls/caves.
3. Library returns an `Area`; server serializes it (`Area:{…}` text format) and stores it keyed by region coordinates.
4. As players move, server streams cells; client renders walls/floors from cell tags and places loot on dead-end markers.

**Journey — stitch a newly explored region to existing map (vision / partially supported):**
1. A player walks off the edge of a generated region.
2. Server generates the adjacent region, passing the already-fixed border cells of the existing region as **fixed areas** so the new maze connects rather than overlaps.
3. New region persists and connects; the two "countries" are now one world.
   *(Today: fixed-position `Area`s and the force distributor's "never disturb fixed areas" rule are the building blocks for this. A first-class "stitch adjacent region" API does not yet exist — see §6.)*

### Persona B — "Solo game developer" (Unity/Godot)
Drops `PlayersWorlds.Maps.dll` into a Unity assets folder and generates dungeon levels at runtime or design time. Wants a simple fluent API, sensible defaults, and ASCII/PNG previews while iterating.

**Journey — generate a dungeon level:**
1. `new GeneratedWorld(RandomSource.CreateFromEnv())`
2. `.AddLayer(AreaType.Maze, new Vector(w, h))` then `.WithAreas(...)` to sprinkle rooms/halls/caves.
3. `.OfMaze(MazeStructureStyle.Block, options)` to carve the maze around the areas.
4. `.MarkDeadends().MarkLongestPath()` for loot/spawn hints.
5. `.ToMap(...)` to expand thin-wall topology into an explicit block/wall grid.
6. `.Map()` → iterate cells in Unity, instantiate wall/floor prefabs by tag.

### Persona C — "Level designer" (human-in-the-loop)
Uses the library to produce a *starting* layout that a human then hand-edits. Cares about controllable area types (symmetric human-made halls vs. organic caves), and natural-looking output (smoothed corners, cleaned-up speckle).

**Journey — seed a hand-designed level:** generate a maze with a few fixed halls at chosen coordinates and a "lake" fill area, export the ASCII/PNG, then iterate on seeds until the composition is pleasing before hand-finishing.

### Persona D — "Library contributor"
Adds a maze algorithm, an area type, or a renderer. Cares about the 98% coverage bar, the one-test-class-per-source convention, deterministic seeding for reproducible test failures, and the narrow contracts (`MazeGenerator` talks only to `Maze2DBuilder`).

---

## 3. Core capabilities (what the product does today)

| # | Capability | Status |
|---|-----------|--------|
| C1 | Generate a solvable 2D maze via 6 selectable algorithms (Aldous-Broder, Binary Tree, Hunt-and-Kill, Recursive Backtracker [default], Sidewinder, Wilson's) | ✅ Done |
| C2 | Partial-fill mazes (Quarter/Half/ThreeQuarters/NinetyPercent/Full/FullWidth/FullHeight) — sparse, cavern-like layouts | ✅ Done |
| C3 | Place typed areas — **Hall** (walled room, 1–2 entrances), **Cave** (organic, any entrances), **Fill** (impassable), **Environment** — that the maze respects | ✅ Done |
| C4 | Auto-distribute rooms without overlap via a force-directed physical simulation that also honors fixed, user-placed areas | ✅ Done |
| C5 | Fixed-coordinate area placement (`Area.Create(position, size, type)`) — the basis for aligning/stitching regions | ✅ Done |
| C6 | Layered / nested composition — a maze whose trail becomes the input grid for a finer sub-maze; nested `GeneratedWorld`s at offsets | ✅ Done |
| C7 | Post-processing markers — dead-ends, and a guaranteed-longest-path start/end pair | 🚧 Works but longest-path tagging has bugs (see COMPONENT-REVIEW §Maze/post_processing) |
| C8 | Two structural styles — **Border** (thin walls, implicit) and **Block** (walls occupy their own cells) with a converter between them | ✅ Done |
| C9 | Natural-look post-filters — outline walls around trails, smooth inside corners, erase small speckle blobs | ✅ Done |
| C10 | Rendering — ASCII line-art maze, ASCII block/shade map, area-layout diagnostic diagram, and PNG (Cairo) block render | ✅ ASCII done; 🚧 PNG rough (hard-coded path, block-style only) |
| C11 | Serialization — compact, human-readable, round-trippable `Area:{…}` text format | ✅ Done |
| C12 | Determinism — seeded `RandomSource` threaded through everything; seeds embedded in exception messages for reproducible failures | ✅ Done |
| C13 | CLI tool — `generate`, `parse`, `run`, `perfrun`, `usecase` verbs | ✅ Done |
| C14 | N-dimensional-ready primitives (`Vector`, `Grid`) — 3D volumes anticipated | 🚧 Iteration is N-D; concrete geometry (overlap/contains) is hard-wired 2D |

---

## 4. Non-goals (explicitly out of scope)

- **Not a game engine.** No gameplay, mobs, loot tables, physics, or save-games beyond map serialization.
- **Not a renderer for production.** ASCII/PNG output is for previews/tests/debugging; the real client (Unity/Godot) renders from cell data.
- **Not the MMO server.** No networking, no central persistence store, no player state, no authentication. The library is called *by* such a server.
- **Not a general graphics library.** The Cairo dependency is a developer preview aid, not a shipping rendering path.

---

## 5. Quality attributes (non-functional requirements)

| Attribute | Requirement | How it's met |
|-----------|-------------|--------------|
| **Reproducibility** | Same seed ⇒ identical map, across runs and machines | `RandomSource` DI everywhere; `EnvRandomSeed` global pin; seed printed in `MazeBuildingException`/test failures |
| **Determinism under test** | A failing random case must be re-runnable | `TestsSetup` reads `SEED` param → `RandomSource.EnvRandomSeed`; `flake.sh`/`deflake.sh` loop to catch seed-sensitivity |
| **Correctness** | Every generated maze is solvable; no orphaned connectable cells; Fill areas never breached; halls get ≥1 entrance | Asserted in `MazeGeneratorTest` (`IsSolveable`, no unlinked cells); `Maze2DBuilder` centralizes area/isolation handling |
| **Coverage** | ≥98% unit-test coverage; 1 test class per source file | AltCover → Cobertura → ReportGenerator; coverage task prints `MISSING TEST CLASS FOR …` |
| **Robustness at scale** | Large/dense mazes and 1000s of layouts must not hang or fail >1% | Load test: `Parallel.For` 1000 iterations, pass threshold 990; loop guards in `Maze2DBuilder`/generators |
| **Portability** | Must run inside Unity | Targets **.NET Framework 4.7** (later runtimes unsupported by Unity); built with Mono on Linux; core `src` has no third-party deps |
| **Performance** | Track integration-test runtime over time; warn on slow tests | Per-test `Stopwatch` warns >200 ms; `perfrun` verb + Mono profiler (`perf.sh`) |
| **Maintainability** | Narrow contracts; algorithms ignorant of areas/fill/isolation | `MazeGenerator` only calls `Maze2DBuilder`; `.editorconfig` enforces house style; Gendarme + .NET analyzers in CI-adjacent tasks |

---

## 6. Gaps between vision and implementation (honest assessment)

These are **NOT COMPLETE** relative to the MMO vision and are the natural roadmap:

- ❌ **Region stitching API.** No first-class "generate the missing region adjacent to this existing one, connecting at the shared border." The primitives exist (fixed areas, serialization, the distributor's fixed-area invariant) but no orchestration ties them together.
- ❌ **Central persistence / streaming.** Serialization exists (text `Area:{…}`), but there is no store, no chunk/region keying, no partial-load-by-coordinate.
- ❌ **Elevation / 3D.** `GeneratedWorld.WithElevation` throws `NotImplementedException`; primitives are N-D-ready but concrete geometry is 2D-only.
- ❌ **Environment tagging.** `GeneratedWorld.AddEnvironmentAreas` is a no-op stub (biomes/terrain tags planned, not built).
- 🚧 **Longest-path markers** are computed but mis-tagged (end marker lands on the start cell; trail cells mis-tagged) — see the component review.
- 🚧 **PNG rendering** writes to a hard-coded `antialias.png`, supports only block style, and has an invisible fallback color.
- 🚧 **>19 auto-distributed areas** crash the simulation (nickname array bound) — a scale ceiling for the distributor.

Representing these honestly is a product requirement in itself: consumers must not assume the stitching/persistence layer exists.

---

## 7. Success criteria

1. A game engineer can, from a seed and a size, generate a solvable, area-populated map and serialize/deserialize it losslessly. ✅
2. Two mazes generated with the same seed are byte-identical. ✅
3. Fixed, user-placed rooms are never moved or overlapped by the auto-distributor. ✅ (within the ≤19-area / <1% failure envelope)
4. Every generated maze passes the "is solvable" and "no orphaned cells" invariants. ✅
5. The path from vision → shipping is documented so nobody mistakes a preview feature (PNG, elevation) for a finished one. ✅ (this document)
