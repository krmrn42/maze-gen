# Scenarios Catalog & API-Fit Review — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce two new docs — `docs/SCENARIOS.md` (runtime-actor scenario catalog) and `docs/API-FIT.md` (support matrix + signature-level API/pipeline proposals) — validated by running real API snippets, plus three targeted syncs to `DESIGN.md`/`PRD.md`.

**Architecture:** Docs-only deliverable. "Tests" = compile-and-run snippets through the core `src` library (validating ✅/🟡 API-path claims) and cross-reference/consistency checks. All content derives from the approved spec `docs/superpowers/specs/2026-07-05-scenarios-api-fit-design.md`; the plan points to spec sections for descriptive text rather than duplicating all 17 entries.

**Tech Stack:** Markdown; Mermaid (C4/flowchart) in DESIGN; .NET 10 SDK for the validation probe compiling `src/**/*.cs` (BCL-only core, proven this session).

## Global Constraints

- **Work location:** all edits happen in the git worktree checked out to branch `docs/project-documentation` at `<WORKTREE>` (already created). Commit commands assume cwd = worktree root.
- **Do not touch** the `circles` branch or its uncommitted refactor.
- **Spec is the content source of truth:** `docs/superpowers/specs/2026-07-05-scenarios-api-fit-design.md` (scenario list §4, proposal themes §5.3, syncs §6).
- **Scenario IDs are fixed:** `D1–D6`, `S1–S6`, `C1–C5` (17 total). `S5` is labeled "vision / post-v1".
- **Support taxonomy (verbatim):** ✅ Supported = public path exists AND exercised via snippet; 🟡 Partial = primitives exist but need glue / caveat / known bug; 🔴 Missing = no public path.
- **Validation rule:** every ✅/🟡 "current API path" claim must be run via the probe, or explicitly marked **"asserted, not run."**
- **Real symbol names only** in API-path columns (e.g. `GeneratedWorld.AddLayer/OfMaze/ToMap`, `Area.Create`, `AreaSerializer`, `RandomSource.CreateFromEnv`, `DijkstraDistance.FindLongestTrail`).
- **Pushing** to update PR #40 happens only in the final task.

---

### Task 1: Validation probe harness

**Files:**
- Create: `<SCRATCH>/apifit-probe/apifit-probe.csproj` (scratchpad, NOT committed to repo)
- Create: `<SCRATCH>/apifit-probe/Probe.cs`

**Interfaces:**
- Produces: a runnable console project that compiles `/home/data/repos/github.com/krmrn42/maze-gen/src/**/*.cs` on net10 and executes an arbitrary snippet. Later tasks drop code into `Probe.cs` `Main` and run `dotnet run` to validate claims.

- [ ] **Step 1: Write the probe project file**

`apifit-probe.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <Deterministic>false</Deterministic>
    <AssemblyName>apifitprobe</AssemblyName>
    <NoWarn>$(NoWarn);CS1591;SYSLIB0050;SYSLIB0051</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="/home/data/repos/github.com/krmrn42/maze-gen/src/**/*.cs" />
    <Compile Include="Probe.cs" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write a smoke-test Probe.cs**

```csharp
using System;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Renderers;

internal static class Probe {
    private static void Main() {
        RandomSource.EnvRandomSeed = 1;
        var map = new GeneratedWorld(RandomSource.CreateFromEnv())
            .AddLayer(AreaType.Maze, new Vector(5, 5))
            .OfMaze(MazeStructureStyle.Border, new GeneratorOptions {
                MazeAlgorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
            })
            .Map();
        Console.WriteLine("PROBE OK cells=" + map.Count);
    }
}
```

- [ ] **Step 3: Run the probe**

Run: `cd <SCRATCH>/apifit-probe && dotnet run -c Release`
Expected: prints `PROBE OK cells=25` (build succeeds; core library compiles on net10).

- [ ] **Step 4: No commit** (scratchpad tool, not part of the repo). Record the probe path in your notes for reuse in Tasks 3.

---

### Task 2: Author `docs/SCENARIOS.md`

**Files:**
- Create: `<WORKTREE>/docs/SCENARIOS.md`

**Interfaces:**
- Consumes: spec §4 (the 17 scenario descriptions).
- Produces: markdown anchors `#d1 … #c5` (GitHub auto-anchors from `### D1 …` headings) that `API-FIT.md` links to.

- [ ] **Step 1: Write the doc skeleton and one fully-worked entry**

Structure: title + intro (spine = runtime actor; status hint legend ✅/🟡/🔴; note that authoritative support mapping lives in `API-FIT.md`), then three `##` actor sections, each containing one `###` per scenario. Use this exact entry template, shown with `D1` fully worked:

```markdown
### D1 — Generate & preview one map from a seed  ✅

**Actor:** Game developer (dev time)
**Trigger:** Tuning the generator; wants to see output fast.
**Wants:** A single map from a chosen seed, previewed as ASCII or PNG, to judge algorithm / fill / area mix.
**Interaction:** `GeneratedWorld(RandomSource) → AddLayer → OfMaze → (ToMap) → Map → Render(AsciiRendererFactory)`.
**Support:** ✅ — see [API-FIT: D1](API-FIT.md#d1).
```

- [ ] **Step 2: Write all remaining entries**

Produce one entry per scenario using the template above, taking the description text from spec §4. Scenarios and status hints (verbatim from spec §4):
- Actor 1 (Game developer): `D1`✅ `D2`✅ `D3`✅ `D4`✅ `D5`✅ `D6`🟡
- Actor 2 (Game server): `S1`🔴 `S2`🔴 `S3`🟡 `S4`🟡 `S5`🔴(mark "vision / post-v1") `S6`🟡
- Actor 3 (In-game client): `C1`🟡 `C2`✅ `C3`🔴 `C4`🟡 `C5`🟡

Each entry's **Support** line links to `API-FIT.md#<lowercased-id>`.

- [ ] **Step 3: Verify structure**

Run: `grep -cE '^### (D[1-6]|S[1-6]|C[1-5]) ' docs/SCENARIOS.md`
Expected: `17`.

- [ ] **Step 4: Commit**

```bash
git add docs/SCENARIOS.md
git commit -m "docs: add runtime-actor scenario catalog (SCENARIOS.md)"
```

---

### Task 3: Validate ✅/🟡 API-path claims via the probe

**Files:**
- Modify (scratchpad, not committed): `<SCRATCH>/apifit-probe/Probe.cs`
- Create: `<SCRATCH>/apifit-probe/validation-log.md` (raw results — feeds Task 4; not committed)

**Interfaces:**
- Consumes: the probe from Task 1.
- Produces: a per-claim result record (`RAN OK` / `RAN OK with caveat: …` / `ASSERTED, NOT RUN: <reason>`) for every ✅ and 🟡 scenario. 🔴 scenarios are skipped (no current path to run).

- [ ] **Step 1: Enumerate the claims to validate**

The ✅/🟡 set (14 rows; 🔴 `S1`,`S2`,`S5`,`C3` excluded): `D1 D2 D3 D4 D5 D6 S3 S4 S6 C1 C2 C4 C5`.

- [ ] **Step 2: Write & run a snippet per claim**

For each, put a minimal snippet in `Probe.cs` `Main` exercising the *actual* current API path, then run `dotnet run -c Release`. Examples (illustrative — write real ones):

```csharp
// D5 round-trip: serialize an Area and read it back.
var s = new PlayersWorlds.Maps.Serializer.AreaSerializer();
string text = s.Serialize(map);
var back = s.Deserialize(text);
Console.WriteLine("D5 " + (back.Count == map.Count ? "RAN OK" : "MISMATCH"));
```
```csharp
// S6 metadata: dead-ends + longest path (note known FindLongestTrail tagging bug).
var world = new GeneratedWorld(RandomSource.CreateFromEnv())
    .AddLayer(AreaType.Maze, new Vector(8,8))
    .OfMaze(MazeStructureStyle.Border, new GeneratorOptions{ FillFactor = GeneratorOptions.MazeFillFactor.Full })
    .MarkDeadends().MarkLongestPath().Map();
Console.WriteLine("S6 RAN OK (verify tagging caveat in review)");
```

- [ ] **Step 3: Record each result** in `validation-log.md` as `ID | RAN OK | note` or `ID | ASSERTED, NOT RUN | reason`.

Verification: `grep -cE '^(D|S|C)[0-9]+ ' validation-log.md` → `14` (one line per ✅/🟡 claim).

- [ ] **Step 4: No commit** (scratchpad). Carry `validation-log.md` into Task 4.

---

### Task 4: Author `docs/API-FIT.md`

**Files:**
- Create: `<WORKTREE>/docs/API-FIT.md`

**Interfaces:**
- Consumes: spec §5 (taxonomy, matrix columns, proposal themes), Task 2 anchors, Task 3 `validation-log.md`.
- Produces: anchors `#d1 … #c5` that `SCENARIOS.md` links back to; the prioritization/routing table that sub-project D consumes.

- [ ] **Step 1: Write taxonomy + matrix header + one worked row**

Sections: `## Support taxonomy` (copy the three verbatim definitions from Global Constraints), then `## Fit matrix` as a table with header:
`| Scenario | Status | Current API path | Where it falls short | Gap size | Routed to |`
One fully-worked row to fix the format:
```markdown
| [D1](SCENARIOS.md#d1) | ✅ | `GeneratedWorld.AddLayer/OfMaze/Map` + `Area.Render(AsciiRendererFactory)` | — | — | — |
```

- [ ] **Step 2: Fill all 17 rows**

One row per scenario, statuses per Global Constraints. `Current API path` uses real symbols; `Status` reflects Task 3 results (downgrade any ✅ that failed to run to 🟡 "asserted, not run"). `Routed to` ∈ {C, D, bug-fix, backlog, —}. Anchor each `Scenario` cell to `SCENARIOS.md#<id>`.

- [ ] **Step 3: Write the proposals section**

`## Proposals` with one `###` subsection per theme from spec §5.3 (7 themes), each: what it closes, the signature sketch (fenced C#), and a one-line note on why. Themes: coordinate-deterministic seeding (S1,S4); region/chunk facade (S3,C3); seam-aware stitching (S2,S5 — mark "direction only, designed in D"); stable world-cell identity (C4); modern .NET target (C1,C3); metadata & pathing surface (S6,C5 — reference the `FindLongestTrail` bug from COMPONENT-REVIEW §9); real-time latency (C1 — measurement, no API change).

- [ ] **Step 4: Write prioritization + validation log sections**

`## Prioritization & routing` table sorted by (priority × gap size), each row stamped with owner. `## Validation log` reproducing the Task 3 results (per-claim RAN OK / caveat / asserted-not-run).

- [ ] **Step 5: Verify structure**

Run: `grep -cE '^\| \[(D[1-6]|S[1-6]|C[1-5])\]' docs/API-FIT.md`
Expected: `17` (matrix rows). Also confirm `## Proposals`, `## Prioritization & routing`, `## Validation log` headings all exist.

- [ ] **Step 6: Commit**

```bash
git add docs/API-FIT.md
git commit -m "docs: add API-fit matrix, proposals, and validation log (API-FIT.md)"
```

---

### Task 5: Apply the three targeted syncs to DESIGN.md and PRD.md

**Files:**
- Modify: `<WORKTREE>/docs/DESIGN.md` (Context C4 diagram §2.1; Component-view §2.3 callout)
- Modify: `<WORKTREE>/docs/PRD.md` (§6 gaps)

**Interfaces:**
- Consumes: `API-FIT.md` routing/gaps.
- Produces: consistent gap statements across DESIGN, PRD, API-FIT.

- [ ] **Step 1: Edit the DESIGN.md Context diagram**

In the `C4Context` block (§2.1), add a direct client→library generation relationship and keep it truthful. Add inside the diagram:
```
    Rel(client, lib, "Generates map parts in real time (server optional)")
```
And add one prose line under the diagram: *"The in-game client can call the library directly to generate map parts in real time — an optional path (no server required) useful for single-player and early development."*

- [ ] **Step 2: Add the Component-view callout**

Immediately under the §2.3 heading, add:
```markdown
> **Under review:** the generation pipeline is being assessed scenario-by-scenario in
> [SCENARIOS.md](SCENARIOS.md) and [API-FIT.md](API-FIT.md); it may be refactored in the
> chunked-engine design (sub-project D). No structural change is implied here yet.
```

- [ ] **Step 3: Sync PRD.md §6 gaps**

In PRD §6, ensure these gap items are present and each cross-links to its `API-FIT.md` proposal theme: coordinate-deterministic generation (S1/S4), region stitching (S2/S5), persistence/partial-load (S3), chunked streaming (C3), and the .NET-target compatibility item (C1/C3). Add any missing bullet using the existing ❌/🚧 style; append `— see [API-FIT: Proposals](API-FIT.md#proposals)` to the relevant lines.

- [ ] **Step 4: Verify Mermaid still parses**

Run: `grep -n "Rel(client, lib" docs/DESIGN.md`
Expected: one match. Eyeball the `C4Context` block for balanced `Rel(...)` lines (no stray `<`, `>`, or unquoted commas introduced).

- [ ] **Step 5: Commit**

```bash
git add docs/DESIGN.md docs/PRD.md
git commit -m "docs: sync Context diagram, Component-view note, and PRD gaps with scenario review"
```

---

### Task 6: Cross-reference integrity check + push to PR #40

**Files:**
- Modify (only if the check finds breaks): `docs/SCENARIOS.md`, `docs/API-FIT.md`

**Interfaces:**
- Consumes: all prior task outputs.
- Produces: a consistent, pushed documentation set on PR #40.

- [ ] **Step 1: Check every SCENARIOS→API-FIT link resolves**

Run:
```bash
for id in d1 d2 d3 d4 d5 d6 s1 s2 s3 s4 s5 s6 c1 c2 c3 c4 c5; do
  grep -qi "API-FIT.md#$id" docs/SCENARIOS.md || echo "MISSING forward link: $id";
  grep -qiE "^\| \[${id^^}\]|id=\"$id\"|^### ${id^^} " docs/API-FIT.md docs/SCENARIOS.md >/dev/null || echo "check anchor: $id";
done
echo "done"
```
Expected: prints only `done` (no MISSING lines). Fix any gaps and re-run.

- [ ] **Step 2: Check routing consistency**

Manually confirm: every 🔴/🟡 row in `API-FIT.md` has a non-empty `Routed to`; every gap named in PRD §6 appears in the API-FIT prioritization table; no scenario is ✅ in one doc and 🟡/🔴 in another. Fix contradictions inline; if fixes were needed, commit them:
```bash
git add docs/SCENARIOS.md docs/API-FIT.md docs/PRD.md
git commit -m "docs: fix cross-reference/consistency gaps in scenario review"
```

- [ ] **Step 3: Push to update PR #40**

Run: `git push origin docs/project-documentation`
Expected: branch updates; PR #40 now shows the spec, `SCENARIOS.md`, `API-FIT.md`, and the syncs.

- [ ] **Step 4: Report** the PR URL and a one-paragraph summary of gaps routed to D (the work-list D opens with).

---

## Self-Review

**Spec coverage:**
- §4 scenario catalog → Task 2. ✓
- §5.1 taxonomy → Task 4 Step 1. ✓
- §5.2 matrix → Task 4 Step 2. ✓
- §5.3 proposals → Task 4 Step 3. ✓
- §5.4 validation → Task 1 + Task 3 + Task 4 Step 4 (validation log). ✓
- §5.5 prioritization/routing → Task 4 Step 4. ✓
- §6 three syncs → Task 5. ✓
- §7 validation/consistency → Task 3 + Task 6. ✓
- §8 landing/push → Task 6 Step 3. ✓

**Placeholder scan:** No "TBD/TODO/handle edge cases". Illustrative snippets are marked illustrative with instruction to write real ones; content text is sourced from named spec sections (stable, in-repo). ✓

**Type/name consistency:** Anchors `#d1…#c5` used identically in Tasks 2, 4, 6. Scenario IDs and status hints match spec §4 across Tasks 2/3/4. Real symbol names match COMPONENT-REVIEW/DESIGN. ✓

**Placeholders to fill before running:** `<WORKTREE>` = the docs worktree root; `<SCRATCH>` = the session scratchpad dir. These are environment paths, resolved at execution start, not content gaps.
