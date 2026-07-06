# CLAUDE.md — PlayersWorlds.Maps

Guidance for AI assistants working in this repo. Keep it high-signal; update it when architecture or workflow changes.

## What this is

A **procedural maze/dungeon map-generation library** (`PlayersWorlds.Maps`) that produces the *backbone* of game maps — corridors, rooms, halls, caves, impassable zones — for 2D/3D games (the consuming game here is the Godot MMO `mazzzze`). It generates map geometry; the consuming game supplies gameplay, loot, mobs, and the visual layer. Long-term vision is an endless, server-persisted, dynamically-stitched MMO world (see `docs/PRD.md`); the library is only the deterministic map engine, not the server.

Full docs: **`docs/PRD.md`** (why/journeys), **`docs/DESIGN.md`** (architecture, API, algorithms, C4), **`docs/COMPONENT-REVIEW.md`** (per-component detail + defect register).

## Runtime constraint (important)

Targets **.NET 8** — the runtime of the consuming Godot game (`mazzzze`). Built with the **.NET SDK** on Linux (`dotnet build`/`dotnet test`, SDK-style projects, `PackageReference`). The core `src` library stays **BCL-only** (no third-party dependencies) so it drops cleanly into a Godot project. (History: the library previously targeted .NET Framework 4.7 on Mono for Unity; Unity/Mono support was intentionally dropped — see `docs/ROADMAP.md`.)

## Build / test / run

Everyday commands are in the **`Makefile`** (`make` or `make help` lists them); they wrap `dotnet` and the `tasks/*.sh` scripts (also wired into `.vscode/tasks.json`).

```bash
make build          # dotnet build
make test           # fast unit loop (what CI runs)  |  make test-all / test-integration / test-load
make lint           # dotnet format --verify-no-changes (enforces .editorconfig)
make format         # auto-format to .editorconfig
make demo           # render a maze with two rooms as ASCII (CASE=1 SEED=12345) — quick e2e smoke
make coverage       # -> build/coverage   |  make perf -> build/perf
make ci             # lint + build + test (CI parity)
```

The lower-level scripts still exist for finer control (`./tasks/test.sh --filter …`, `./tasks/flake.sh` / `deflake.sh` to loop tests for seed-sensitivity, etc.).

**Smoke render / e2e check.** `make demo` (== `usecase -c 1 -r <seed>`) builds **one maze with two rooms** (a `Maze` layer + two `Hall` child areas) and renders it to ASCII with square-looking cells (`RectCells(2, 1)` — 2 wide × 1 tall reads square in a terminal) — a fast, visible end-to-end check before a PR. CI runs the same render (fixed seed, deterministic), writes it to the run **job summary**, and posts it as a **sticky PR comment** (updated in place each push). A crash there fails the check. (The plain `generate` verb still crashes — see sharp edges — so `demo`/`usecase` is the go-to render.)

Reproduce a failing random test with its printed seed (NUnit `TestContext` params):
```bash
./tasks/test.sh --filter "FullyQualifiedName~SomeTest" -- 'TestRunParameters.Parameter(name="SEED", value="12345")'
```

### Coverage, performance, debugging

```bash
./tasks/coverage.sh              # coverlet (XPlat Code Coverage) + ReportGenerator
                                 #   -> build/coverage/report/{Summary.txt,index.html,Cobertura.xml}
                                 #   also audits: prints "MISSING TEST CLASS FOR <src>"
./tasks/perf.sh                  # dotnet-trace over the `perfrun` workload
                                 #   -> build/perf/perfrun.speedscope.json (open at speedscope.app) + wall-clock
./tasks/debugger.sh run -p <FullyQualified.Test>   # debug via netcoredbg (CLI), or press F5 in VS Code
```

- Coverage exclusions live in `tasks/coverlet.runsettings` (test/mock assemblies, renderers, a couple of noisy types).
- `reportgenerator` and `dotnet-trace` are pinned in `.config/dotnet-tools.json` and restored on demand (`dotnet tool restore`); `coverlet.collector` is a `PackageReference` in the test project. No global installs.
- Terminal debugging needs [`netcoredbg`](https://github.com/Samsung/netcoredbg) on `PATH`; in VS Code the "Debug maze-gen" launch configs use the C# extension's CoreCLR debugger directly.

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
- **Keep the `.csproj` files minimal SDK-style** — don't commit IDE-added cruft (globs, per-file `<Compile>`, absolute paths); the SDK globs sources automatically.
- **Coverage goal ≥98%**, **one test class per source file** (the coverage task prints `MISSING TEST CLASS FOR …`). New public code needs `///` XML docs; don't mark internal code `public`.
- **Testing tiers**: unit (fast, CI), `Category=Integration` (serialized tricky layouts), `Category=Load` (statistical, tolerates <1% failure), performance (`perfrun`).

## Assemblies

| Project | What | Deps |
|---------|------|------|
| `src/` → `PlayersWorlds.Maps` | Core library | none (BCL only) |
| `maze-gen/` → `mazegen` | CLI (`generate`/`parse`/`run`/`perfrun`/`usecase`) | CommandLineParser; refs src+tests |
| `tests/` | NUnit 4 suite | NUnit/NUnit3TestAdapter/Moq/coverlet |

All SDK-style, `net8.0`. (A Mono/Cairo `render/` PNG-preview project existed on the old `circles` branch; it was dropped — the consuming game owns rendering.)

## Known sharp edges (before you touch these)

- 🐞 **`Area.ShallowCopy` shares `_cells`/`_childAreas` references** — mutating a "copy" mutates the original. Use `Cell.Clone()` for real isolation. Biggest structural hazard.
- 🐞 **`DijkstraDistance.FindLongestTrail` mis-tags** the end marker onto the start cell and the trail onto `startingPoint` (`:144,:146-148`). Affects the "guaranteed start/end spawn points" feature.
- 🐞 **Auto-distributor crashes for >19 areas** (`MapAreasSystem.s_nicknames`).
- ❌ **Stubs that throw/no-op**: `GeneratedWorld.WithElevation` (throws), `AddEnvironmentAreas` (no-op).
- 🐞 **CLI `generate` verb crashes** — `GenerateCommand` calls `ConvertMazeBorderToBlock` on a `maze` that `BuildMaze` already block-converted and never attached a builder to, so the converter's `X<Maze2DBuilder>` guard throws. Pre-existing; the unit suite (which attaches the builder) is the source of truth.
- ⚠️ **Serializer has no escaping** — delimiters (`; , [ ] { }`) inside values corrupt the text format.

Full defect register with severities: `docs/COMPONENT-REVIEW.md §9`.

## Workflow notes for this repo

- We follow GitHub Flow: commit to a topic branch, never the default.
- Never amend commits — if an amend seems needed, stop and report.
- Original upstream is `aynurin/maze-gen`; this remote is `krmrn42/maze-gen`.
