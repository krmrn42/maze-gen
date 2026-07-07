## ADDED Requirements

### Requirement: Region factory generates one bounded region on demand

The façade SHALL expose a synchronous factory that generates exactly one bounded region for a given coordinate address and returns it as a `RegionView`. The call MUST NOT be endless or stream; it generates a single region and returns. The caller (game) decides when and on which thread to invoke it.

#### Scenario: Generate a region for an address
- **WHEN** the game calls `GetOrCreate(address)` for an address with no stored region
- **THEN** the façade generates one region from its own seed and returns a `RegionView` for that address

#### Scenario: Region is a correct, solvable map
- **WHEN** a region is generated
- **THEN** the returned `RegionView` represents a solvable maze with no orphaned connectable cells, honoring the same correctness invariants the underlying pipeline guarantees

#### Scenario: Generation is deterministic for a fixed seed
- **WHEN** two regions are generated for the same address with the same seed
- **THEN** their `RegionView` cell data is identical

### Requirement: RegionView exposes Block cells with a typed payload and no Area internals

The façade SHALL return region cell data as a **Block** grid (walls occupy their own cells) via a `RegionView` whose cells carry passability, `AreaType`, block tags, links, and markers. The `RegionView` MUST NOT expose any internal `Area` or `Cell` object, `ExtensibleObject` attachment, or Border/Block conversion machinery.

#### Scenario: Cells report wall vs floor
- **WHEN** the game reads a cell from a `RegionView`
- **THEN** the cell reports whether it is passable (floor) or a wall

#### Scenario: Cells carry area type and markers
- **WHEN** the game reads a cell that belongs to a hall, cave, fill, or maze corridor, or that was marked as a dead-end or path endpoint
- **THEN** the cell exposes its `AreaType`, tags, and markers so the game can choose tiles and spawns

#### Scenario: No Area internals leak
- **WHEN** the game holds a `RegionView`
- **THEN** it cannot reach an `Area`, `Cell`, HardLinks/BakedLinks, or child-area structure through the façade's public surface

### Requirement: Regions expose first-class POIs with correct endpoint tagging

The façade SHALL surface points of interest — the region's entrance/exit pair and its dead-ends — as first-class metadata on the `RegionView`. The entrance/exit endpoints MUST be tagged on the correct cells (fixing the longest-path mis-tag where the end marker landed on the start cell and trail cells were mis-tagged).

#### Scenario: Entrance and exit resolve to distinct correct cells
- **WHEN** the game reads the entrance and exit POIs of a region
- **THEN** they resolve to two distinct cells at the ends of the guaranteed longest path, each tagged on its own cell

#### Scenario: Dead-ends are enumerable
- **WHEN** the game reads the dead-end POIs of a region
- **THEN** it receives the set of dead-end cells for placing loot or content

### Requirement: Regions carry a place and border gates

A `RegionView` SHALL carry its coordinate **address** (its place on the region lattice) and its **gates** — the openings on its border that are the anchors a neighboring region connects to. The `Gate` shape is part of the contract from this release even though gate-aware stitching generation is deferred.

#### Scenario: Region reports its address
- **WHEN** the game reads a `RegionView`
- **THEN** it can read the `RegionAddress` the region was generated for

#### Scenario: Region exposes its border gates
- **WHEN** the game reads a region's gates
- **THEN** it receives, per gate, the border edge and the open cells along it, in a shape a future neighbor could consume as connection anchors

### Requirement: Stable region-relative identity with derived world coordinates

The façade SHALL define cell identity as `(RegionAddress, localCell)` and provide `ToWorld`/`FromWorld` helpers that derive absolute world coordinates from a region address, region size, and local cell. Identity MUST be stable across a region unload/reload so the game can key fog-of-war and mutations to it.

#### Scenario: Local identity round-trips through world coordinates
- **WHEN** the game maps a `(RegionAddress, localCell)` to world coordinates and back with `ToWorld`/`FromWorld`
- **THEN** it recovers the original `(RegionAddress, localCell)`

#### Scenario: Identity survives reload
- **WHEN** a region is unloaded and later reloaded for the same address
- **THEN** a given cell resolves to the same `(RegionAddress, localCell)` identity as before

### Requirement: Persistence seam is generate-once and lossless

The façade SHALL accept an `IRegionStore` that the game implements, and `GetOrCreate` MUST load a stored region rather than regenerate it when one exists, generating and saving only on a store miss. Serialization used by the seam MUST be lossless (round-trippable), not the `Serialize()`/`ToString()` debug-label form.

#### Scenario: Stored region is loaded, not regenerated
- **WHEN** `GetOrCreate(address)` is called and the store reports a region for that address
- **THEN** the façade returns the stored region and does not regenerate it

#### Scenario: Missing region is generated then saved
- **WHEN** `GetOrCreate(address)` is called and the store has no region for that address
- **THEN** the façade generates the region, saves it to the store, and returns it

#### Scenario: Round-trip through the store preserves cells
- **WHEN** a region is saved and reloaded through the store seam
- **THEN** the reloaded `RegionView` has cell data identical to the original

#### Scenario: No-op store regenerates each call
- **WHEN** a `NullRegionStore` (always-miss, never-save) is supplied and `GetOrCreate` is called at startup
- **THEN** a fresh region is generated for use and nothing is persisted

### Requirement: Region generation is configurable by an intent recipe

The façade SHALL let the caller choose how a region is generated via an immutable `RegionRecipe` — algorithm, fill density, and cell shape — rather than baking them. It MUST provide intent presets and fluent overrides, and MUST default so that the no-argument path still works. The maze algorithm MUST be selectable from a discoverable built-in set AND extensible with a caller-supplied `MazeGenerator` via an escape hatch, without the façade exposing renderer types.

#### Scenario: A preset generates without extra configuration
- **WHEN** a caller uses a `RegionRecipe` preset (e.g. `Maze`)
- **THEN** the region generates with that preset's algorithm/fill/cells and no further configuration is required

#### Scenario: Overrides are additive and non-mutating
- **WHEN** a caller applies `WithAlgorithm` / `WithFill` / `WithCells` to a recipe
- **THEN** a new recipe with that override is returned and the original recipe is unchanged

#### Scenario: A custom algorithm can be supplied
- **WHEN** a caller supplies a custom `MazeGenerator` subtype via the algorithm escape hatch
- **THEN** the region is generated with that algorithm, and no renderer type appears on the façade's public surface

### Requirement: World holds shared parameters; region kind is chosen per call and binds at creation

Creating a `World` SHALL fix the shared, inherited parameters (seed, store, region footprint, default recipe). `GetOrCreate` SHALL accept a per-call recipe that overrides the default for that region only. A region's kind MUST bind at first generation: a later `GetOrCreate` on an already-stored address returns the stored region and ignores any newly supplied recipe.

#### Scenario: Different regions can be different kinds
- **WHEN** two different addresses are generated with different recipes
- **THEN** each region reflects its own recipe

#### Scenario: Recipe is ignored once a region exists
- **WHEN** `GetOrCreate(address, recipeB)` is called for an address already generated with `recipeA`
- **THEN** the stored region (from `recipeA`) is returned and `recipeB` is ignored

### Requirement: regionSize is the region's world footprint

`regionSize` SHALL be the region's footprint in the world, in Block cells — the uniform lattice pitch — and MUST equal exactly what `RegionView.Size` reports. The number of corridors/rooms within the footprint is derived internally from the recipe's cell sizing; the caller does not specify maze-cell counts.

#### Scenario: The requested footprint is the reported size
- **WHEN** a world is created with a given `regionSize` and a region is generated
- **THEN** the returned `RegionView.Size` equals that `regionSize` exactly, in every dimension

#### Scenario: A footprint too small to hold a region is rejected
- **WHEN** `regionSize` cannot fit at least one maze cell for the chosen cell sizing
- **THEN** generation fails with a clear error rather than producing a degenerate region
