## ADDED Requirements

### Requirement: mazzzze map engine sources its map from the façade, not the hash

`mazzzze`'s map engine SHALL obtain its map by generating one region through the maze-gen façade at game start, replacing the `MazeData.IsFloor` coordinate hash and the fixed 10000×10000 grid. The removed hash, the `0`/`1` tile encoding, and the fixed world bounds MUST NOT remain the source of map data.

#### Scenario: Startup generates one real region
- **WHEN** the game starts
- **THEN** the map engine calls the façade `GetOrCreate` once and holds the returned `RegionView` as the map for the session

#### Scenario: No player-facing dependency on IsFloor remains
- **WHEN** the map is queried for what occupies a cell
- **THEN** the answer comes from the `RegionView`, not from `MazeData.IsFloor`

### Requirement: Chunks render from region cells while keeping residency

`mazzzze` SHALL keep its chunk load/unload residency ring and `GridMap` rendering, changing only the data source: a chunk's cells are sampled from the resident `RegionView` and mapped to tiles by cell payload (wall vs floor, and area type where the game distinguishes rooms/caves/corridors).

#### Scenario: A chunk renders sampled region cells
- **WHEN** a chunk is loaded
- **THEN** its `GridMap` cells are set from the corresponding `RegionView` cells rather than from `GetChunkData`'s hash output

#### Scenario: Residency ring is preserved
- **WHEN** the player moves
- **THEN** the existing chunk load/unload ring continues to operate, now sourcing cells from the resident region

### Requirement: Spawn and goal come from region POIs

`mazzzze` SHALL place the player start/spawn and the level goal from the region's entrance/exit POIs rather than from hard-coded fixed cells, and MAY place content on dead-end POIs.

#### Scenario: Player spawns at the region entrance
- **WHEN** the region is generated at startup
- **THEN** the player start is derived from the region's entrance POI, not a hard-coded `(1,1)`

#### Scenario: Goal is the region exit
- **WHEN** the game marks the level goal
- **THEN** it uses the region's exit POI, and the entrance and exit are distinct correct cells

### Requirement: v1 is single-region with no persistence, stitching, or streaming

The Phase-0 `mazzzze` integration SHALL use exactly one region for the whole session with a no-op store (regenerate at startup). Multi-region worlds, seam-stitching, cross-seam player transfer, and region streaming/eviction MUST NOT be required for v1 and are added later behind the same unchanged façade.

#### Scenario: One region for the session
- **WHEN** the game runs a session
- **THEN** it uses a single generated region and does not request neighbors, stitch, or stream additional regions

#### Scenario: Contract shape unchanged when capability is added later
- **WHEN** a later phase adds persistence, stitching, or streaming
- **THEN** it is introduced behind the existing façade surface without changing the calls v1 makes
