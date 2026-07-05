# Holistic Component Review — PlayersWorlds.Maps

> **Status:** Component-by-component walkthrough of the codebase (v0.2, branch `circles`), with responsibilities, key APIs, and defects/smells at `file:line`.
> Companion documents: [PRD.md](PRD.md), [DESIGN.md](DESIGN.md).
>
> **Legend:** ✅ solid · 🚧 works-with-caveats · ❌ stub/broken · 🐞 likely bug · ⚠️ smell/tech-debt

---

## Map of the source tree

```
src/                         PlayersWorlds.Maps (core library, no 3rd-party deps)
├─ Vector, VectorD           N-D integer / floating-point coordinates
├─ Grid                      pure geometry (former NArray, payload removed)
├─ Area, Cell                the data model: grid of cells + typed child overlays
├─ ExtensibleObject          type-keyed property bag (base of Area/Cell)
├─ GeneratedWorld            fluent builder / public entry point
├─ RandomSource, Log         determinism + logging
├─ Optional, Preconditions, Extensions   small utilities
├─ areas/                    area types + generators
│  └─ evolving/              force-directed area distributor (physics)
├─ maze/                     Maze2DBuilder + 6 algorithms + styling + post-processing
├─ map_filters/              cellular-automata post-filters
├─ renderers/                ASCII renderers
└─ serializer/               text round-trip format
render/                      PlayersWorlds.Maps.Render (Cairo/PNG, Mono-bound)
maze-gen/                    CLI executable (generate/parse/run/perfrun/usecase)
tests/                       NUnit 4 suite (unit/integration/load/perf)
```

---

## 1. Core primitives

### `Vector` — `src/Vector.cs` ✅
Immutable N-dimensional **integer** coordinate/size/offset (`readonly struct`, value equality via cached hash). Not a math vector.
- **Key API:** `X`/`Y`/`Dimensions`/`Area` (product of components = cell count); `+`/`-`; `FitsInto`, `IsIn` (half-open bounds); **`ToIndex(size)`/`FromIndex(i,size)`** — the row-major linearization (dimension 0 fastest) that ties a coordinate to a slot in `Area._cells`; `Parse("1x2")`; compass constants; `NorthEastComparer` (Y then X).
- ⚠️ `Dimensions`/`Area`/`MagnitudeSq` dereference `_value` directly → NRE on `Empty`, inconsistent with guarded `X`/`Y`. Multiple `// TODO: _value can't be null` (`:78,:223`). Unreachable `ToString` branch (`:232`).

### `VectorD` — `src/VectorD.cs` ✅ (with a footgun)
Floating-point sibling for sub-cell positions & force math. Components rounded to 9 dp to tame FP noise; `MIN = 1e-10` epsilon.
- **Key API:** `RoundToInt()`, `WithMagnitude`, `Hadamard`, epsilon-tolerant equality, `RandomUnit`, lazily-cached `Magnitude`.
- ⚠️ **Mutable `struct` with a lazy cache field (`_mag`)** — a classic C# footgun (mutation on a copy is silently lost); safe here only because `Magnitude` recomputes from immutable `_value`. `X`/`Y` doc claims it throws for non-2D but it just indexes.

### `Grid` — `src/Grid.cs` ✅
The former `NArray`, now **pure coordinate space** (position + size + `Vector` iteration), storing no cells.
- **Key API:** `Region`/`SafeRegion` (bounds-clamped), `AdjacentRegion` (full 3ⁿ Moore neighborhood minus center), `Overlap`/`Contains`/`FitsInto`, `Reposition`.
- ⚠️ Iteration is N-D but `Overlap`/`Contains`/`Low/High` are hard-wired 2D — partial-generality smell. Excellent self-aware class remark (`:11-17`) about speculative N-D-ness.

### `Area` — `src/Area.cs` ✅ (central aggregate; one real hazard)
The composition unit: owns the flat `List<Cell>`, a `Grid`, an `AreaType`, tags, and a **tree of child areas**. Both a leaf map and a container.
- **Key API:** indexer `this[Vector]` (world→local index), factories (`Create`, `CreateUnpositioned`, `CreateEnvironment`, `CreateMaze`), **`ShallowCopy(...)`** (copy-on-write primitive), **`BakeChildAreas()`** (flattens overlapping overlays into per-cell availability/neighbors/links by `AreaType` priority), `IsHollow` (Hall/Cave), `AddChildArea`.
- 🐞 **`ShallowCopy` shares `_cells`/`_childAreas` list references by default** → mutating a "copy" mutates the original (`:134`). The single biggest structural hazard; callers must `Cell.Clone()` for real isolation.
- ⚠️ `// TODO: Why is this called 3-4 times?` on baking (`:210`); `// TODO: Area constructors seem to be redundant` (`:156`); `CreateMaze` name is misleading (`:125`, "does not actually create a maze").

### `Cell` — `src/Cell.cs` ✅
A single cell. Separates **`HardLinks`** (carved by algorithms, permanent) from **`BakedLinks`/`BakedNeighbors`** (derived, recomputed on bake). `Links() = hard ∪ baked` is the definition of a passage.
- **Nested `CellTag`** — string wrapper comparable to raw strings; predefined `MazeWall`/`MazeTrail`/`MazeWallCorner`/`MazeVoid`.
- ⚠️ Explicit author doubt (`:88-91`): tags should be strongly typed but requirements are unclear. A cell doesn't know its own position (implied by list index).

### `ExtensibleObject` — `src/ExtensibleObject.cs` ✅
Type-keyed property bag (`Dictionary<Type,object>`); `X<T>()` get / `X<T>(v)` set. Attaches algorithm results and the `Maze2DBuilder` onto `Area`/`Cell`.
- ⚠️ One value per type (can't store two `List<T>` of same T); getter doc claims it throws but returns `default`.

### `Optional<T>`, `Preconditions`, `Extensions`, `RandomSource`, `Log` ✅
- **`Optional<T>`** — nullable-ref wrapper with value semantics; a TODO wonders if `TryGet` would be better.
- **`Preconditions`** — `Check(bool,msg)` guard; generic `Check<T>` builds the exception reflectively (needs a `(string)` ctor).
- **`Extensions`** — `ForEach`, `ThrowIfNull(OrEmpty)`, `Stats()`, `DebugString()`, dictionary upserts. ⚠️ `BaseStats.Median` is naive (`list[count/2]` unsorted, `:150`); `Variance` uses the numerically-unstable `E[x²]−E[x]²` (`:155`).
- **`RandomSource`** — seeded RNG wrapper; `CreateFromEnv()` uses `EnvRandomSeed` or `DateTime.Now.Millisecond`. ⚠️ millisecond default is a weak seed (rapid successive worlds can collide).
- **`Log`** — self-described "quick" console logger; level-gated; `WriteEvery` throttling. ⚠️ not thread-safe.

---

## 2. Areas subsystem — `src/areas/`

### `AreaType` — `AreaType.cs` ✅
Enum whose **value is a priority** (`None=0, Environment=1, Maze=2, Hall=3, Cave=4, Fill=5`); highest wins on overlap. Drives `BakeChildAreas`. ⚠️ truncated doc comment (`:4`).

### `AreaGenerator` (base) — `AreaGenerator.cs` ✅
Ties random area creation to the evolving simulator and validates the result. `GenerateMazeAreas` counts pre-existing "acceptable" errors (so user-placed overlaps aren't blamed on the generator), runs the sim if any area is floating, scores overlaps/fit, commits only new areas if clean, retries up to `MaxAttempts=3`, else throws `AreaGeneratorException`.
- ⚠️ **`FIXME: while distributing, we can't disturb the original layout`** (`:74-75`) + design-doubt block (`:66-73`); O(n²) overlap scan.

### `BasicAreaGenerator` — `BasicAreaGenerator.cs` ✅
Simple **backtracking random placement, no physics** (passes `base(null,null)` so the simulator path is dead). Up to 100 tries per area; rejects overlaps and out-of-bounds. Used by `GeneratedWorld.WithAreas(...)`.
- ⚠️ recursion (stack depth = count), per-recursion list allocation.

### `RandomAreaGenerator` — `RandomAreaGenerator.cs` ✅
Produces **unpositioned** areas by probability distributions, defers placement to the simulator. Area count bounded by a **fill factor** (default 0.33) and 10 attempts. Distributions: sizes favor small rooms (2×2/2×3 dominant, 7×7 rare); types `Cave 0.4 / Hall 0.4 / Fill 0.2`; per-type tag tables (room/loot, cave/den/ruins, lake/swamp/void…). 50% random rotation of sizes.
- ⚠️ `// TODO: Make tags dependent on the area type` (`:157`).

### The evolving distributor — `src/areas/evolving/`

| Class | Role | Notes |
|-------|------|-------|
| `EvolvingSimulator` ✅ | Generic epoch×generation loop; each generation applies `1/N` of an epoch's force (numerical integration); early-exits on convergence | Defaults 2 epochs × 20 generations |
| `MapAreasSystem` ✅🐞 | Concrete physics system: pairwise repulsion + containment; snaps to grid at epoch end; converges when shift `Mode==0 && Variance≤0.1` | 🐞 **`s_nicknames` has 19 entries → `IndexOutOfRangeException` for >19 areas** (`:24`). `// TODO: Not covered` on `BakeChildAreas` (`:105`) |
| `FloatingArea` ✅ | Float rectangle proxy for an `Area`; free areas start at env center then explode outward; `SnapToGrid`/`AdjustPosition` write rounded positions back to the linked `Area` | `DistanceTo` computes signed gap-or-penetration per axis |
| `DirectedDistanceForceProducer` 🚧 | All three force laws: bounded repulsion (3-unit cutoff), penetration force, coincident-area random explosion (cached to be exactly opposite), wall containment | ⚠️ `CollideForce` throws `NotImplementedException` (dead); `OverlapForce` ignores its `fragment` param; large `!!` comment block admits the distance metric is "not optimal" |
| `MapAreaSystemFactory`, `SimulatedSystem`, `I*ForceProducer`, `IForceFormula` ✅ | Contracts + DI seams (enable mocking the sim in tests) | |

**Assessment:** a genuinely clever, well-isolated subsystem — the standout piece of engineering in the repo. Its ceiling (19 areas) and the acknowledged sub-optimal distance metric are the main risks; the retry+load-test envelope keeps it reliable in practice (<1% failure).

---

## 3. Maze subsystem — `src/maze/`

### `MazeGenerator` (base) — `MazeGenerator.cs` ✅
Abstract; single method `GenerateMaze(Maze2DBuilder)`. Instantiated reflectively (needs a public parameterless ctor). ⚠️ stale doc-comment promises helpers that actually live on the builder.

### `Maze2DBuilder` — `Maze2DBuilder.cs` ✅🐞 (the orchestrator)
Mediates every algorithm↔grid interaction so algorithms stay ignorant of areas/fill/isolation.
- **`RebuildCellMaps()`** builds Maze-only orthogonal neighbors, priority cells (Cave cells + Hall walk-in cells), connectable pools (excluding Fill/Hall interiors), and `_cellGroups` (connected components via `DijkstraDistance.FindRaw`).
- **`Connect(a,b)`** is the *single place* a passage is carved (`HardLinks.Add` both ways) plus bookkeeping (priority removal, connected-set updates).
- **`IsFillComplete()`** implements the `MazeFillFactor` semantics and a loop guard.
- **`ApplyAreas()`** punches exactly one entrance per Hall after carving.
- **`BuildMaze()`** pipeline: generate areas → bake → rebuild maps → run algorithm → apply areas → attach `DeadEnd`/`DijkstraDistance`/`this` extensions.
- 🐞 **The `_isFillCompleteAttemptsMade` loop guard never fires** (`:579-588`): the counter starts at 0 and is only *decremented*, so `made >= attempts` is never true. Protection here is effectively dead (per-generator idle guards still protect).
- ⚠️ `// TODO: What's a better name` (`:31`); several `// TODO: Not covered` branches.

### `GeneratorOptions` — `GeneratorOptions.cs` ✅
Config bag: `FillFactor`, `AreaGeneration`, `AreaGenerator`, `MazeAlgorithm` (Type), `RandomSource`, `MazeStructureStyle`, `MazeRendererOptions`. `Algorithms` static handles (default = Recursive Backtracker). ⚠️ copy-paste doc error on `FullHeight` (`:56`).

### The six algorithms ✅
`RecursiveBacktracker` (default, DFS/stack), `AldousBroder` (uniform walk, re-seeds on idle), `HuntAndKill` (walk + hunt in priority order), `Wilsons` (loop-erased walk, seeds per cell-group), `BinaryTree` (N/E per cell, `Full` only), `Sidewinder` (eastward runs + North close, `Full` only). All carve solely via `builder.Connect`. Wilson's carries an acknowledged isolation-handling TODO block.

### Styling
- **`MazeStyle` (`MazeStructureStyle`)** ✅ — `Border` (implicit walls) vs `Block` (walls occupy cells). ⚠️ wrong "Hunt-and-kill" class doc (`:6`).
- **`MazeAreaStyleConverter`** ✅ — Border→Block via `Maze2DRenderer` + a 5-stage filter chain (outline → smooth corners → outline → erase void spots ≤5×5 → erase wall spots ≤3×3). Guards that the maze was actually built (has the builder extension). ⚠️ wrong doc-comment; on `circles` this is being refactored to accept a `targetArea` and is now the single home of the filter chain (previously duplicated in `Maze2DBuilder`).
- **`Maze2DRenderer`** ✅ — tags trail/passage blocks and computes the 9-region (NW…SE) pixel layout that turns border adjacency into block pixels. `Maze2DRendererOptions` holds trail/wall cell sizes with `SquareCells`/`RectCells` factories.

### Post-processing — `src/maze/post_processing/`
- **`DeadEnd`** ✅ — cells with exactly one link; tagged for loot.
- **`DijkstraDistance`** ✅🐞 — relaxation BFS (`Find`), potential-graph BFS (`FindRaw`, used for cell-groups), `Solve` (gradient descent), `FindLongestTrail` (double-BFS diameter for start/end markers).
  - 🐞 **`FindLongestTrail` tags the *end* marker on `startingPoint` instead of `targetPoint`** (`:144`) — the end marker lands on the start cell.
  - 🐞 **The trail-tagging loop tags `maze[startingPoint]` for every cell** instead of `maze[cell]` (`:146-148`) — the whole-path tag is applied repeatedly to one cell.
  - ⚠️ Uses a LIFO `Stack` despite the "Dijkstra/BFS" name — DFS-order relaxation; correct distances, more nodes visited.

### `MazeBuildingException` — `MazeBuildingException.cs` ✅
Domain exception embedding the builder's RNG seed + full maze dump in `Message` — reproducible bug reports by construction.

---

## 4. Map filters — `src/map_filters/`

Cellular-automata-style passes that **mutate cell tags in place** to make output look natural.

| Filter | Effect | Notes |
|--------|--------|-------|
| `Map2DOutline` ✅ | Wrap a band of `outlineType` (walls) of thickness `outlineCellSize` around regions of `cellType` (trails) | |
| `Map2DSmoothCorners` ✅ | Fill inside-corner cells (has both a horizontal and vertical run of `cellType` nearby) with `cornerType` | ⚠️ dead commented-out earlier impl (`:58-78`) |
| `Map2DEraseSpots` ✅ | Flood-fill connected components; erase blobs whose bounding box is `< maxW × maxH` (both under threshold, AND) | ⚠️ strict `<` keeps an exactly-max blob (off-by-one worth noting) |
| `Map2DFilter` (base) ⚠️ | Single method `Render(Area)` that actually *mutates* — misleading name | |

---

## 5. Renderers

### ASCII — `src/renderers/`
- **`AsciiRendererFactory`** ✅ — dispatches by extension: built maze (`X<Maze2DBuilder>()!=null`) → `Maze2DStringBoxRenderer`, else `Map2DStringRenderer`.
- **`Map2DStringRenderer`** ✅ — block/shade glyphs per cell (`MazeVoid=' '`, `MazeWallCorner='▒'`, `MazeWall='▓'`, `MazeTrail='░'`, fallback `'0'`); recurses into child areas, compositing overlays at their offsets; flips Y for terminal output.
- **`Maze2DStringBoxRenderer`** ✅ — table-driven Unicode line-art. Each grid intersection accumulates wall-segment flags from surrounding cells; the exact flag combo maps to `─│┌┐└┘┴┬├┤┼` etc. Absence of a link = wall. Renders the longest-path trail with hex indices when the extension is present. ⚠️ an unanticipated flag combo would throw `KeyNotFoundException`.
- **`MapAreaStringRenderer`** ✅ — diagnostic box-drawing of *area bounding boxes* (double-line env, single-line rooms) with labels; unpositioned areas listed textually. Debug aid, not the maze.
- **`AsciiBuffer`** ⚠️ — mutable char canvas; `ToString` silently drops blank lines; the out-of-bounds error path can itself throw (`_buffer[row]` when `row` is the offender).
- **`AreaToAsciiRenderer`** ⚠️ — one-method abstract base; would be more idiomatic as an interface (one of three near-identical `Render` bases across the codebase).

### PNG / Cairo — `render/` 🚧
Separate assembly `PlayersWorlds.Maps.Render`, referencing **`Mono.Cairo`** (Mono-runtime bound). Added at HEAD ("PNG maze rendering").
- **`CairoBlockMazeAreaRenderer`** 🚧 — rasterizes block-style cells to `ppc=32` px squares colored by tag (the graphical twin of `Map2DStringRenderer`); on `circles` it now also composites child areas.
  - 🐞 **hard-coded output path `"antialias.png"`** (non-configurable, misleadingly named).
  - 🐞 fallback color is transparent red `(1,0,0,0)` — alpha 0 so it's invisible (probably meant as a visible debug color).
  - ⚠️ ~20 lines of commented-out demo code; block-style only (no line-art).
- **`RendererFactory`** ⚠️ — ignores its `area` argument, always returns the Cairo renderer (placeholder dispatch).
- **`AreaRenderer`** ⚠️ — copy-paste doc stub unrelated to rendering.

---

## 6. Serialization — `src/serializer/` ✅

Hand-rolled text format: `TypeName:{value;value;[item,item];[nested]}` with brace-depth-aware parsing.
- **`AreaSerializer`** — `Area:{Size;Position;IsPositionFixed;Type;[Tags];[Cells];[ChildAreas]}`; **omits cells entirely when none are "interesting"** (non-default type / has links / has tags) → compact empty grids; recurses child areas.
- **`CellSerializer`** — `Cell:{TYPE;[LINK,…];[TAG,…]}`; omits TYPE when it equals the area default.
- **`BasicStringReader`/`BasicStringWriter`** — fluent StringBuilder writer + brace-aware reader.
- ⚠️ Brittle hand-parser; **no escaping** — a value containing `; , [ ] { }` corrupts the format; `ArgumentException` used for control flow.

**Assessment:** genuinely useful — it's the round-trip format the CLI `parse` verb and the integration tests exercise, and the seed of the persistence story in the PRD.

---

## 7. CLI — `maze-gen/` ✅

`CommandLineParser`-based; the exe references `src`, `render`, **and `tests`** (so `run`/`perfrun` can reflectively invoke test methods — unusual for a shipping binary).

| Verb | Purpose |
|------|---------|
| `generate` | Build a maze (algorithm by name via reflection), print serialized + ASCII |
| `parse` | Deserialize a serialized maze, round-trip it, print stats (visited/areas by type) + ASCII |
| `run` | Reflectively run one test method N times (reproduce flaky/random behavior) |
| `perfrun` | Reflectively run all maze `[TestFixture]` `[Test]` methods as a perf baseline |
| `usecase` | Run one of the predefined `GeneratedWorld` recipes and exercise the PNG pipeline |
| `BaseCommand` | Shared `-r/--random` (seed) and `-d/--debug` options |

- **`UseCase.cs`** is a living demo of the fluent DSL: maze-within-a-maze, fixed rooms, nested `GeneratedWorld`s at offsets, and (on `circles`) a fixed `Fill` "lake" area. It's the best on-ramp for a new integrator.
- ⚠️ reflection-by-string algorithm resolution silently yields `null` on a bad name; shipping a tool that references the test assembly is convenient but atypical.

---

## 8. Tests & build — `tests/`, `.vscode/`, `.github/`

- **Framework:** NUnit 4.1 + Moq/Castle + AltCover (coverage) + ReportGenerator + Gendarme (static analysis). Targets .NET FW 4.7 under Mono.
- **Four-tier strategy** (`TESTING.md`): unit (≈1:1 per source file, enforced by the coverage task's `MISSING TEST CLASS FOR …` check), integration (`Category=Integration`, tricky serialized layouts), load (`Category=Load`, `Parallel.For` 1000 iters, pass ≥990 — an explicit <1% statistical tolerance), performance (Mono profiler via `perfrun`/`perf.sh`).
- **Determinism:** `TestsSetup` maps a `SEED` param → `RandomSource.EnvRandomSeed`; failures print the seed; `flake.sh`/`deflake.sh` loop to hunt seed-sensitivity.
- **CI:** `.github/workflows/main.yml` runs **unit tests only** on push (excludes Load+Integration) under Mono; `docs.yaml` builds docfx → GitHub Pages.
- **Last recorded run** (`TestResult.xml`): 1113 cases discovered, 304 in the unit subset, 302 passed / 0 failed / 2 skipped, 9078 asserts.
- ⚠️ `FakeRandomSource` is just a fixed-seed real `Random` (TODO: "How to properly implement a fake random?"); coverage goal (98%) is high but the four-project layout + Cairo/Mono binding makes the render assembly effectively untested.

---

## 9. Consolidated defect register (prioritized)

| # | Severity | Item | Location |
|---|----------|------|----------|
| 1 | 🐞 High | `Area.ShallowCopy` shares cell/child lists → cross-layer mutation aliasing | `src/Area.cs:134` |
| 2 | 🐞 High | `FindLongestTrail` mis-tags end marker onto the start cell | `src/maze/post_processing/DijkstraDistance.cs:144` |
| 3 | 🐞 High | `FindLongestTrail` mis-tags trail cells onto `startingPoint` | `DijkstraDistance.cs:146-148` |
| 4 | 🐞 Med | Distributor crashes for >19 auto areas (nickname array) | `src/areas/evolving/MapAreasSystem.cs:24` |
| 5 | 🐞 Med | `IsFillComplete` loop guard never fires (counter only decremented from 0) | `src/maze/Maze2DBuilder.cs:579-588` |
| 6 | 🐞 Med | Cairo renderer hard-codes `antialias.png`; invisible fallback color | `render/CairoBlockMazeAreaRenderer.cs:46,60` |
| 7 | ❌ | `WithElevation` throws; `AddEnvironmentAreas` is a no-op | `src/GeneratedWorld.cs:209,121` |
| 8 | ⚠️ | `CollideForce` `NotImplementedException`; `OverlapForce` ignores `fragment` | `DirectedDistanceForceProducer.cs:159,163` |
| 9 | ⚠️ | Serializer has no escaping (delimiters in values corrupt output) | `src/serializer/*` |
| 10 | ⚠️ | `VectorD` is a mutable struct with a lazy cache field | `src/VectorD.cs` |
| 11 | ⚠️ | Stale/wrong doc-comments (MazeStyle, MazeAreaStyleConverter, GeneratorOptions, render/*) | multiple |
| 12 | ⚠️ | `BaseStats.Median` unsorted; `Variance` numerically unstable | `src/Extensions.cs:150,155` |

**Overall assessment:** a thoughtfully layered, well-tested library with one standout subsystem (the force-directed distributor) and clean algorithm/orchestrator separation. The defects are mostly localized and low-blast-radius; the two that matter most for correctness are the `ShallowCopy` aliasing hazard (#1) and the longest-path mis-tagging (#2, #3), which directly affects the PRD's "guaranteed start/end spawn points" feature.
