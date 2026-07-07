# PlayersWorlds.Maps

[![NuGet](https://img.shields.io/nuget/v/PlayersWorlds.Maps.svg?logo=nuget)](https://www.nuget.org/packages/PlayersWorlds.Maps/)
[![NuGet prerelease](https://img.shields.io/nuget/vpre/PlayersWorlds.Maps.svg?label=nuget%20pre&color=orange)](https://www.nuget.org/packages/PlayersWorlds.Maps/absoluteLatest)
[![Build](https://github.com/krmrn42/maze-gen/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/krmrn42/maze-gen/actions/workflows/main.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A procedural **maze / dungeon map-generation** library — corridors, rooms, halls,
caves, and impassable zones for 2D/3D games. It generates the *backbone* of a map;
the consuming game supplies gameplay, loot, mobs, and the visual layer.

The core library is **BCL-only** (no third-party dependencies) and targets
**net8.0**, so it drops cleanly into a Godot 4 project.

## Install

```bash
dotnet add package PlayersWorlds.Maps
```

## Quick start — the region façade

Games code against `PlayersWorlds.Maps.World`, a narrow, frozen contract over the
generation pipeline (no internal `Area`/`Cell` types leak through):

```csharp
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.World;

// Game start: one seeded world, created once.
var world = new World(
    store: new NullRegionStore(),      // supply your own IRegionStore to persist
    worldSeed: 12345,
    regionSize: new Vector(65, 65),    // the region's world footprint == RegionView.Size
    defaultRecipe: RegionRecipe.Maze); // Maze / Corridors / Dungeon / Caverns

// As the player moves: generate (or load) a region, choosing its kind.
RegionView region = world.GetOrCreate(new RegionAddress(new Vector(0, 0)));

foreach (var poi in region.Pois) { /* Entrance / Exit / DeadEnd, in local coords */ }
bool floor = region.CellAt(new Vector(x, y)).IsPassable;  // drives your tiles
```

Full integration guide, scenario coverage, and the latency envelope:
**[`docs/INTEGRATION.md`](docs/INTEGRATION.md)**.

The lower-level `GeneratedWorld` pipeline (layers, area overlays, explicit
algorithm/render options) is still available for advanced composition — see
[`docs/DESIGN.md`](docs/DESIGN.md).

## Build from source

Requires the .NET SDK (net8.0). Everyday commands are in the **`Makefile`**:

```bash
make build     # dotnet build
make test      # fast unit tests (what CI runs)
make ci        # lint + build + test + smoke render
make demo      # render a maze with two rooms as ASCII
```

To develop the library against a consuming game locally, reference it as a
`ProjectReference` instead of the package — see the conditional-reference pattern
in [`docs/INTEGRATION.md`](docs/INTEGRATION.md#consuming-the-library).

## Docs

- [`docs/PRD.md`](docs/PRD.md) — vision & journeys
- [`docs/DESIGN.md`](docs/DESIGN.md) — architecture, API, algorithms
- [`docs/INTEGRATION.md`](docs/INTEGRATION.md) — the living integration guide
- [`docs/ROADMAP.md`](docs/ROADMAP.md) — phased plan

## Contributing

See the [contributing guide](CONTRIBUTING.md).

## License

[MIT](LICENSE).
