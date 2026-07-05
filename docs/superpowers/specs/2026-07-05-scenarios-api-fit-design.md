# Spec — Scenarios Catalog & API-Fit Review (Sub-project B)

- **Date:** 2026-07-05
- **Status:** Approved design; ready for implementation planning.
- **Owner:** maze-gen maintainers.
- **Part of:** the maze-gen documentation & engine-shaping effort (B → C → D). This is **B**, the keystone that C (map-type cookbook) and D (chunked engine + server) build on.

## 1. Purpose

Produce a scenario-driven review of how games actually use `PlayersWorlds.Maps`, map each scenario onto the current public API and generation pipeline, and — where the API falls short — sketch concrete API/pipeline changes to close the gap. The output gives sub-project D a ready, prioritized work-list and keeps the PRD/DESIGN docs honest about what is shipped vs. planned.

## 2. Goals & non-goals

**Goals**
- Catalog concrete usage scenarios organized by **runtime actor** (game developer, game server, in-game client).
- Assess each scenario's support against the *real* current public API (validated by running snippets where feasible).
- For every gap, provide a **signature-level API/pipeline sketch** and route it to an owner (C / D / bug-fix / backlog).
- Keep the existing docs truthful with three small targeted edits.

**Non-goals (explicitly deferred)**
- The deep design of the chunked engine, seam/stitching, persistence, and the .NET-target decision → **sub-project D**. B presents directions/sketches only.
- Actual implementation of any proposed API → later, after D.
- The Component-view *refactor* of DESIGN.md → deferred; B only adds a pointer/callout.
- Map-type recipe docs → **sub-project C**.

## 3. Decisions locked (from brainstorming)

| Decision | Choice |
|----------|--------|
| Scenario spine | **Runtime actor** (game developer / game server / in-game client) |
| Gap depth | **Describe + prioritize + route + concrete API sketches** (signature-level) |
| Deliverable shape | **Two new docs** (`SCENARIOS.md`, `API-FIT.md`) + 3 targeted syncs |
| Seam/stitching in B | Present as a **lightweight direction**; real design in D |
| .NET target-compat | Treated as a **cross-cutting gap** blocking Actor 3, not a scenario |
| Git landing | **Stack on PR #40** (branch `docs/project-documentation`); `circles` refactor untouched |
| Validation | Compile/run snippets via the .NET 10 path; mark unrun claims "asserted, not run" |

## 4. Deliverable 1 — `docs/SCENARIOS.md`

Scenario catalog, spine = runtime actor. Each scenario is a short journey entry: *trigger → what the actor wants → the shape of the interaction*. Status hint shown (✅ supported / 🟡 partial / 🔴 missing); the authoritative mapping lives in `API-FIT.md`.

**Actor 1 — Game developer (design/dev time; iteration & preview)**
- `D1` Generate one map from a seed and preview (ASCII/PNG) to tune algorithm/fill/area mix — ✅
- `D2` Author a fixed layout: place halls/caves/fill at chosen coordinates as a hand-designed level — ✅
- `D3` Reproduce a specific map by seed for debugging / bug reports — ✅
- `D4` Compose layered/nested maps (maze-within-a-maze; trail→sub-maze) — ✅
- `D5` Serialize a map and reload it as a fixture/test asset (round-trip) — ✅
- `D6` Tune area distribution (sizes/types/density) for a target "feel" — 🟡

**Actor 2 — Game server (authoritative, persistent, on-demand)**
- `S1` Generate a new zone at world coordinates from a world seed — *deterministic by coordinate* — 🔴
- `S2` Generate the missing region adjacent to an existing one, connecting at the shared border (stitching) — 🔴
- `S3` Persist a region and retrieve it later by region key/coords — partial load — 🟡
- `S4` Same `(world seed, coords)` → identical region on any machine (cross-instance determinism) — 🟡
- `S5` Merge two independently-grown "countries" when their frontiers meet — 🔴
- `S6` Extract gameplay metadata per region (dead-ends→loot, longest-path→spawn/exit) — 🟡

**Actor 3 — In-game client / runtime (real-time, server optional)**
- `C1` Generate a region locally in real time with no server, within a latency budget — 🟡
- `C2` Consume a server-streamed region (deserialize) → Godot tiles/prefabs from cell tags — ✅ (as data)
- `C3` Generate/stream only the chunk(s) around the player; load/discard as they move — 🔴
- `C4` Render fog-of-war over the known map — needs stable per-cell identity across loads — 🟡
- `C5` Query connectivity/pathing (links, solvability, distances) for mobs & minimap — 🟡

Each entry cross-links to its `API-FIT.md` row. YAGNI note captured in review: all 17 kept; `S5` explicitly marked "vision / post-v1".

## 5. Deliverable 2 — `docs/API-FIT.md`

**5.1 Support taxonomy**
- ✅ **Supported** — a documented public API path exists and was *exercised* (compiled/ran a snippet).
- 🟡 **Partial** — primitives exist but need glue, carry a caveat, or hit a known bug.
- 🔴 **Missing** — no public path; needs new API/pipeline work.

**5.2 Fit matrix** — one row per scenario `D1…C5`:

`Scenario | Status | Current API path (real methods/types) | Where it falls short | Gap size (S/M/L) | Routed to`

The "Current API path" column names actual symbols (e.g. `GeneratedWorld.AddLayer/OfMaze/ToMap`, `Area.Create`, `AreaSerializer`, `RandomSource.CreateFromEnv`, `DijkstraDistance.FindLongestTrail`).

**5.3 Proposals — grouped by theme** (gaps cluster; one representative signature each):

| Theme | Closes | Sketch (illustrative, signature-level) |
|-------|--------|----------------------------------------|
| Coordinate-deterministic seeding | S1, S4 | `RandomSource.ForRegion(long worldSeed, Vector regionCoords)` — hash-derived per-region seed; removes the `DateTime.Now.Millisecond` default from the server path |
| Region/chunk facade | S3, C3 | `ChunkedWorld(long worldSeed, Vector regionSize)` → `Area GetOrGenerate(Vector regionCoords)`, with a cache/eviction hook |
| Seam-aware stitching | S2, S5 | `GeneratedWorld.WithSeams(params Seam[])` — neighbor edge-links injected as pre-carved `HardLinks` the builder must honor (direction only; D designs it) |
| Stable world-cell identity | C4 | `WorldCoord` helper: `global = regionCoords * regionSize + localCoord` — convention + small helper for fog-of-war persistence |
| Modern .NET target | C1, C3 | Multi-target core `src` `net472;net8.0` (proven to compile on net10 unchanged) + CI matrix |
| Metadata & pathing surface | S6, C5 | Fix `FindLongestTrail` mis-tagging; make `DijkstraDistance.Solve`/dead-end queries cleanly public |
| Real-time latency | C1 | No API change — measure region-gen time via the existing perf harness; document the budget envelope |

**5.4 Validation approach** — for every ✅/🟡 row, the "current API path" is checked by compiling/running a snippet through the `src` library on .NET 10 (the method proven in this session). Any claim that cannot be run is marked **"asserted, not run."**

**5.5 Prioritization & routing summary** — closing table sorting gaps by (priority × size), each stamped with owner (C / D / bug-fix / backlog), so D opens with a ready work-list. Expected high-priority cluster for D: coordinate-deterministic seeding, region/chunk facade, seam stitching, .NET target.

## 6. Deliverable 3 — Targeted syncs to existing docs

1. **`DESIGN.md` Context diagram** — add the in-game client as an actor that can invoke the library **directly to generate map parts in real time (server optional)**, with a one-line note that this path suits single-player / early dev. (Explicitly requested.)
2. **`DESIGN.md` Component view** — add a short callout + links to `SCENARIOS.md`/`API-FIT.md` noting the pipeline is under scenario-driven review and may be refactored in D. No structural change.
3. **`PRD.md` §6 gaps** — sync so the gap list and API-FIT routing agree (coordinate-deterministic gen, stitching, persistence, .NET-target all present and cross-linked).

## 7. Validation / "testing" for this docs deliverable

- Runnable snippets confirm every ✅/🟡 API-path claim; unrun claims flagged.
- Cross-reference integrity: every `SCENARIOS.md` entry links to an `API-FIT.md` row and back.
- No contradictions between `API-FIT.md` routing, `PRD.md` §6 gaps, and the DESIGN callout.

## 8. Landing plan (git)

- All B artifacts land on branch `docs/project-documentation`, stacked onto PR #40, via an isolated worktree; the `circles` refactor is never touched.
- Commits: (a) this spec, (b) `SCENARIOS.md`, (c) `API-FIT.md`, (d) the three syncs. Push updates PR #40 (with the user's given approval to stack).

## 9. Open questions (for D, not B)

- Exact seam-coordination protocol and determinism guarantees at region borders.
- Persistence store shape and region keying.
- Whether the chunk facade lives in core `src` or a new assembly.
- .NET multi-target vs. a separate `netstandard2.1` build — resolved in D with the CI matrix.
