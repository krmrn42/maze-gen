# Usage Scenarios — PlayersWorlds.Maps

> Catalog of how games actually use the library, organized by **runtime actor** — *who* invokes the library and *when*. Each entry is a short journey. The status hint (✅ supported / 🟡 partial / 🔴 missing) is a pointer only; the authoritative, *validated* API mapping lives in [API-FIT.md](API-FIT.md).
>
> Companion docs: [PRD.md](PRD.md) (why), [DESIGN.md](DESIGN.md) (how), [API-FIT.md](API-FIT.md) (support matrix + proposals).

The three actors:

| Actor | When it runs | Dominant constraints |
|-------|-------------|----------------------|
| **Game developer** | Design / dev time | Iteration speed, previewing, reproducibility |
| **Game server** | Authoritative, on demand | Determinism, persistence, coordinate addressing |
| **In-game client** | Real time, during play (server optional) | Latency budget, streaming, no server required |

---

## Actor 1 — Game developer (design / dev time)

### D1 — Generate & preview one map from a seed  ✅

**Trigger:** Tuning the generator; wants to see output fast.
**Wants:** A single map from a chosen seed, previewed as ASCII or PNG, to judge algorithm / fill / area mix.
**Interaction:** `GeneratedWorld(RandomSource) → AddLayer → OfMaze → (ToMap) → Map → Render(AsciiRendererFactory)`.
**Support:** ✅ — see [API-FIT: D1](API-FIT.md#d1).

### D2 — Author a fixed layout  ✅

**Trigger:** Hand-designing a level's skeleton.
**Wants:** Place specific halls / caves / fill areas at chosen coordinates, then let the maze carve around them.
**Interaction:** `WithAreas(params Area[])` with `Area.Create(position, size, type[, tag])`, then `OfMaze`.
**Support:** ✅ — see [API-FIT: D2](API-FIT.md#d2).

### D3 — Reproduce a specific map by seed  ✅

**Trigger:** Filing or fixing a bug against a particular map.
**Wants:** The exact same map every run from a fixed seed.
**Interaction:** `RandomSource.EnvRandomSeed = N` (or a seeded `RandomSource`), then generate as usual.
**Support:** ✅ — see [API-FIT: D3](API-FIT.md#d3).

### D4 — Compose layered / nested maps  ✅

**Trigger:** Wants structure richer than one flat maze (e.g. a coarse maze whose corridors become the frame for a finer maze).
**Wants:** Derive a new layer from the previous one and re-run generation on it.
**Interaction:** `AddLayer(Func<Area,Area>)` + `ShallowCopy(cells: …)` to transform cells, then `OfMaze` again.
**Support:** ✅ — see [API-FIT: D4](API-FIT.md#d4).

### D5 — Serialize & reload a map as a fixture  ✅

**Trigger:** Wants a generated map saved as a test asset / golden file.
**Wants:** Lossless round-trip to and from text.
**Interaction:** `AreaSerializer.Serialize(area)` / `AreaSerializer.Deserialize(text)`.
**Support:** ✅ — but note the `GeneratedWorld.Serialize()` / `Area.ToString()` trap in [API-FIT: D5](API-FIT.md#d5).

### D6 — Tune area distribution for a target "feel"  🟡

**Trigger:** Wants dungeons that feel a certain way (dense caves, sparse halls, …).
**Wants:** Control over how many areas, of which types and sizes, get scattered.
**Interaction:** `WithAreas(areaTypes, tags, count, minSize, maxSize)` (procedural placement).
**Support:** 🟡 — works, but the distribution knobs are coarse — see [API-FIT: D6](API-FIT.md#d6).

---

## Actor 2 — Game server (authoritative, persistent, on demand)

### S1 — Generate a new zone at world coordinates  🔴

**Trigger:** A new community needs a starting zone at world offset `(X, Y)`.
**Wants:** A map that is a deterministic function of `(world seed, region coordinates)` — so the same place is always the same map.
**Interaction (today):** none direct; seeds come from `EnvRandomSeed` / `DateTime.Now.Millisecond`, not from coordinates.
**Support:** 🔴 — see [API-FIT: S1](API-FIT.md#s1).

### S2 — Generate the missing region adjacent to an existing one  🔴

**Trigger:** A player walks off the edge of a generated region.
**Wants:** Generate the neighbor so its corridors *connect* at the shared border rather than dead-ending or overlapping.
**Interaction (today):** fixed-position `Area.Create` areas and the distributor's "don't disturb fixed areas" rule are primitives, but there is no seam/border-connection API.
**Support:** 🔴 — see [API-FIT: S2](API-FIT.md#s2).

### S3 — Persist a region and retrieve it later  🟡

**Trigger:** A region was generated (visited) and must be stored centrally and re-served.
**Wants:** Save a region under a key and load it back on demand.
**Interaction:** `AreaSerializer` gives lossless text; there is no store, region keying, or partial-load layer.
**Support:** 🟡 — serialization exists, persistence does not — see [API-FIT: S3](API-FIT.md#s3).

### S4 — Identical region on any machine  🟡

**Trigger:** Multiple server instances must agree on the same world.
**Wants:** `(world seed, coords)` → byte-identical region regardless of host.
**Interaction:** seed-based determinism holds in-process; cross-machine equality is untested and hinges on coordinate-seeding (S1).
**Support:** 🟡 — see [API-FIT: S4](API-FIT.md#s4).

### S5 — Merge two independently-grown "countries"  🔴  *(vision / post-v1)*

**Trigger:** Two separately-seeded player communities explore toward each other until their frontiers meet.
**Wants:** Stitch the two grown regions into one consistent world at the meeting frontier.
**Interaction (today):** none; depends entirely on seam stitching (S2) plus conflict resolution.
**Support:** 🔴 — explicitly a *vision* item, not a v1 goal — see [API-FIT: S5](API-FIT.md#s5).

### S6 — Extract gameplay metadata per region  🟡

**Trigger:** The server wants to seed content (loot at dead-ends, spawn/exit at the longest path's endpoints).
**Wants:** Per-region lists of dead-end cells and the guaranteed longest path's start/end.
**Interaction:** `MarkDeadends()` → `DeadEnd.DeadEndsExtension`; `MarkLongestPath()` → `DijkstraDistance.LongestTrailExtension`.
**Support:** 🟡 — data is produced, but the longest-path markers are mis-tagged (known bug) — see [API-FIT: S6](API-FIT.md#s6).

---

## Actor 3 — In-game client / runtime (real time, server optional)

### C1 — Generate a region locally in real time  🟡

**Trigger:** Single-player or early dev with no server; the game must produce a region itself, live.
**Wants:** Generate a region within a frame/latency budget so play isn't interrupted.
**Interaction:** the full `GeneratedWorld` pipeline, called on the client.
**Support:** 🟡 — works, but generation is not fast enough for a frame and no budget is documented — see [API-FIT: C1](API-FIT.md#c1).

### C2 — Consume a server-streamed region  ✅

**Trigger:** The server sends a region's data to the client.
**Wants:** Deserialize it and turn cells into Godot tiles / prefabs by tag.
**Interaction:** `AreaSerializer.Deserialize(text)` → iterate cells → map tags (`MazeWall`/`MazeTrail`/…) to tiles.
**Support:** ✅ (as data) — note cell *tags* exist only after Block conversion (`ToMap`); a Border maze carries links, not wall/floor tags — see [API-FIT: C2](API-FIT.md#c2).

### C3 — Stream only the chunk(s) around the player  🔴

**Trigger:** The world is far larger than memory; only nearby chunks should exist.
**Wants:** Generate/load the chunk(s) near the player and discard/reload as they move.
**Interaction (today):** each region is a standalone `Area`; there is no chunk facade, cache, or eviction.
**Support:** 🔴 — see [API-FIT: C3](API-FIT.md#c3).

### C4 — Render fog-of-war over the known map  🟡

**Trigger:** The client shows a mini-map with explored vs. unexplored areas.
**Wants:** Stable per-cell identity that survives region load/unload, so "seen" state persists.
**Interaction:** cell identity is positional within an `Area`; a global identity is a *convention* (`regionCoords * regionSize + localCoord`) with no library helper.
**Support:** 🟡 — computable from `Vector`, but no world-coordinate API — see [API-FIT: C4](API-FIT.md#c4).

### C5 — Query connectivity / pathing for mobs & minimap  🟡

**Trigger:** Mob AI needs to navigate; the minimap needs reachability.
**Wants:** Distances, shortest paths, reachability between cells.
**Interaction:** `DijkstraDistance.Find` (distance map) and `DijkstraDistance.Solve` (shortest path) are public; per-cell `Cell.Links()` is traversable.
**Support:** 🟡 — the primitives are public and work, but there is no per-region/streamed-graph convenience — see [API-FIT: C5](API-FIT.md#c5).
