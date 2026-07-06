# CLAUDE.md — PlayersWorlds.Maps

Guidance for AI assistants working in this repo. Keep it high-signal; update it when architecture or workflow changes.

## What this is

A **procedural maze/dungeon map-generation library** (`PlayersWorlds.Maps`) that produces the *backbone* of game maps — corridors, rooms, halls, caves, impassable zones — for 2D/3D games (Unity/Godot). It generates map geometry; the consuming game supplies gameplay, loot, mobs, and the visual layer. Long-term vision is an endless, server-persisted, dynamically-stitched MMO world (see `docs/PRD.md`); the library is only the deterministic map engine, not the server.

Full docs: **`docs/PRD.md`** (why/journeys), **`docs/DESIGN.md`** (architecture, API, algorithms, C4), **`docs/COMPONENT-REVIEW.md`** (per-component detail + defect register).

## Runtime constraint (important)

Targets **.NET 8** — the runtime of the consuming Godot game (`mazzzze`). Built with the **.NET SDK** on Linux (`dotnet build`/`dotnet test`, SDK-style projects, `PackageReference`). The core `src` library stays **BCL-only** (no third-party dependencies) so it drops cleanly into a Godot project. (History: the library previously targeted .NET Framework 4.7 on Mono for Unity; Unity/Mono support was intentionally dropped — see `docs/ROADMAP.md`.)

## Build / test / run

Tasks live in `tasks/*.sh` and `.vscode/tasks.json`:

```bash
./tasks/build.sh                 # dotnet build
./tasks/test.sh --filter "TestCategory!=Load & TestCategory!=Integration"   # fast unit loop (what CI runs)
./tasks/test.sh                  # all tests
./tasks/test.sh --filter "TestCategory=Integration"
./tasks/test.sh --filter "TestCategory=Load"
./tasks/flake.sh / deflake.sh    # loop tests to catch/confirm seed-sensitivity
dotnet run --project maze-gen -- generate -a RecursiveBacktracker -s 20x20
```

Reproduce a failing random test with its printed seed (NUnit `TestContext` params):
```bash
./tasks/test.sh --filter "FullyQualifiedName~SomeTest" -- 'TestRunParameters.Parameter(name="SEED", value="12345")'
```

> `tasks/coverage.sh`, `perf.sh`, and `debugger.sh` are **not yet migrated off Mono**
> (AltCover/Gendarme, Mono profiler, Mono soft-debugger) and currently do not run —
> see the note at the top of each. `dotnet run ... generate` also hits a pre-existing
> CLI double-conversion crash (unrelated to the build; the unit suite is the source of truth).

## Architecture in one breath

Staged pipeline over a shared `Area`/`Cell` model: **compose** (`GeneratedWorld` layers + typed child-area overlays) → **place** (`areas/evolving` force-directed distributor) → **carve** (`Maze2DBuilder` + one of 6 `MazeGenerator`s) → **analyze** (`DeadEnd`, `DijkstraDistance`) → **style** (`MazeAreaStyleConverter` Border→Block + `map_filters`) → **emit** (ASCII/PNG renderers or serializer).

Entry point is the fluent `GeneratedWorld` builder (`src/GeneratedWorld.cs`).

### Load-bearing concepts (learn these first)

- **`Area`** owns the flat `List<Cell>` (indexed by `Vector.ToIndex`, dim 0 fastest); it's also a tree of **child areas**. `Grid` is *pure geometry* (position+size+iteration), stores no cells — it's the former `NArray`.
- **`AreaType` value = priority** (`None=0 < Environment < Maze < Hall < Cave < Fill=5`). `Area.BakeChildAreas()` flattens overlapping overlays by this ordering.
- **`Cell` links are split**: `HardLinks` (carved by algorithms, permanent) vs `BakedLinks`/`BakedNeighbors` (derived, recomputed on bake). A passage = `Links()` (the union). Baking never touches hard links.
- **Algorithms are ignorant of complexity.** A `MazeGenerator` only calls `builder.Connect(a,b)` guided by `builder.IsFillComplete()`. All area/fill/isolation logic lives in `Maze2DBuilder`. Adding an algorithm = implement one method.
- **`ExtensibleObject.X<T>()`** attaches one value per Type onto `Area`/`Cell` — this is how `Maze2DBuilder`, `DeadEndsExtension`, and longest-path results ride along. `AsciiRendererFactory` and `MazeAreaStyleConverter` detect a "built maze" via `area.X<Maze2DBuilder>() != null`.
- **Border vs Block style**: Border = thin walls (a link = passage, walls implicit); Block = walls occupy their own cells. `ToMap`/`MazeAreaStyleConverter` converts Border→Block via the filter chain.
- **Determinism is mandatory.** Never `new Random()`; thread `RandomSource` through. Seeds surface in exceptions and test failures.

## Conventions

- **House style is non-standard on purpose** and enforced by `.editorconfig` — same-line `{` (no extra line break before braces). Auto-format before committing. Don't reformat unrelated code.
- **Don't commit `.csproj` changes an IDE made** (MonoDevelop/VS bloat them). README calls this out; keep the projects VSCode-clean.
- **Coverage goal ≥98%**, **one test class per source file** (the coverage task prints `MISSING TEST CLASS FOR …`). New public code needs `///` XML docs; don't mark internal code `public`.
- **Testing tiers**: unit (fast, CI), `Category=Integration` (serialized tricky layouts), `Category=Load` (statistical, tolerates <1% failure), performance (`perfrun`).

## Assemblies

| Project | What | Deps |
|---------|------|------|
| `src/` → `PlayersWorlds.Maps` | Core library | none (BCL only) |
| `render/` → `PlayersWorlds.Maps.Render` | PNG/Cairo (preview only) | `Mono.Cairo` (Mono-bound) |
| `maze-gen/` → `mazegen` | CLI (`generate`/`parse`/`run`/`perfrun`/`usecase`) | CommandLineParser; refs src+render+tests |
| `tests/` | NUnit 4 suite | NUnit/Moq/AltCover/Gendarme |

## Known sharp edges (before you touch these)

- 🐞 **`Area.ShallowCopy` shares `_cells`/`_childAreas` references** — mutating a "copy" mutates the original. Use `Cell.Clone()` for real isolation. Biggest structural hazard.
- 🐞 **`DijkstraDistance.FindLongestTrail` mis-tags** the end marker onto the start cell and the trail onto `startingPoint` (`:144,:146-148`). Affects the "guaranteed start/end spawn points" feature.
- 🐞 **Auto-distributor crashes for >19 areas** (`MapAreasSystem.s_nicknames`).
- ❌ **Stubs that throw/no-op**: `GeneratedWorld.WithElevation` (throws), `AddEnvironmentAreas` (no-op).
- 🐞 **Cairo renderer** hard-codes `antialias.png` output and has an invisible fallback color.
- ⚠️ **Serializer has no escaping** — delimiters (`; , [ ] { }`) inside values corrupt the text format.

Full defect register with severities: `docs/COMPONENT-REVIEW.md §9`.

## Workflow notes for this repo

- We follow GitHub Flow: commit to a topic branch, never the default. The current working branch is often a feature branch (e.g. `circles`, which is mid-refactor consolidating Border→Block conversion into `MazeAreaStyleConverter` and adding child-area PNG rendering).
- Never amend commits — if an amend seems needed, stop and report.
- Original upstream is `aynurin/maze-gen`; this remote is `krmrn42/maze-gen`.
