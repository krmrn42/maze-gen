using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class WorldTest {
        // A region footprint (Block cells) — this is exactly what
        // RegionView.Size reports. 13 = 2*6+1 fits a 6-cell maze exactly.
        private static readonly Vector RegionSize = new Vector(13, 13);
        private static readonly RegionAddress Origin =
            new RegionAddress(new Vector(0, 0));

        private static World NewWorld(IRegionStore store, int seed) =>
            new World(store, seed, RegionSize);

        // A stable fingerprint of a region's rendered structure + POIs.
        private static string Signature(RegionView region) {
            var sb = new StringBuilder();
            sb.Append(region.Size).Append('|');
            for (var y = 0; y < region.Size.Y; y++) {
                for (var x = 0; x < region.Size.X; x++) {
                    sb.Append(region.CellAt(new Vector(x, y)).IsPassable ? '.' : '#');
                }
            }
            sb.Append('|');
            foreach (var poi in region.Pois
                         .OrderBy(p => p.Kind).ThenBy(p => p.Local.X)
                         .ThenBy(p => p.Local.Y)) {
                sb.Append(poi.Kind).Append(poi.Local).Append(',');
            }
            return sb.ToString();
        }

        [Test]
        public void GetOrCreate_GeneratesASolvableRegionWithPois() {
            var region = NewWorld(new NullRegionStore(), 12345)
                .GetOrCreate(Origin);

            Assert.That(region.Address, Is.EqualTo(Origin));

            var entrance = region.Pois.Single(p => p.Kind == PoiKind.Entrance);
            var exit = region.Pois.Single(p => p.Kind == PoiKind.Exit);
            Assert.That(entrance.Local, Is.Not.EqualTo(exit.Local));

            foreach (var poi in region.Pois) {
                Assert.That(region.CellAt(poi.Local).IsPassable, Is.True);
            }
        }

        [Test]
        public void RegionSize_IsTheFootprint_ReportedExactlyByRegionView() {
            var region = NewWorld(new NullRegionStore(), 1).GetOrCreate(Origin);
            Assert.That(region.Size, Is.EqualTo(RegionSize));
        }

        [Test]
        public void NonSquareFootprint_IsHonoredInBothDimensions() {
            var size = new Vector(21, 13);
            var region = new World(new NullRegionStore(), 1, size)
                .GetOrCreate(Origin);
            Assert.That(region.Size, Is.EqualTo(size));
        }

        [Test]
        public void TooSmallFootprint_Throws() {
            var world = new World(new NullRegionStore(), 1, new Vector(2, 2));
            Assert.That(() => world.GetOrCreate(Origin),
                Throws.ArgumentException);
        }

        [Test]
        public void Generation_IsDeterministic_ForAFixedSeed() {
            var a = NewWorld(new NullRegionStore(), 999).GetOrCreate(Origin);
            var b = NewWorld(new NullRegionStore(), 999).GetOrCreate(Origin);
            Assert.That(Signature(a), Is.EqualTo(Signature(b)));
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentRegions() {
            var a = NewWorld(new NullRegionStore(), 1).GetOrCreate(Origin);
            var b = NewWorld(new NullRegionStore(), 2).GetOrCreate(Origin);
            Assert.That(Signature(a), Is.Not.EqualTo(Signature(b)));
        }

        [Test]
        public void DifferentAddresses_ProduceDifferentRegions() {
            var world = NewWorld(new NullRegionStore(), 7);
            var a = world.GetOrCreate(new RegionAddress(new Vector(0, 0)));
            var b = world.GetOrCreate(new RegionAddress(new Vector(1, 0)));
            Assert.That(Signature(a), Is.Not.EqualTo(Signature(b)));
        }

        [Test]
        public void PerRegionRecipe_ChangesTheRegion() {
            var world = NewWorld(new NullRegionStore(), 7);
            var maze = world.GetOrCreate(Origin, RegionRecipe.Maze);
            var corridors = world.GetOrCreate(Origin, RegionRecipe.Corridors);
            // Same footprint, different content.
            Assert.That(corridors.Size, Is.EqualTo(maze.Size));
            Assert.That(Signature(corridors), Is.Not.EqualTo(Signature(maze)));
        }

        [Test]
        public void ExplicitCellSizes_ChangeStructure_NotFootprint() {
            var square = NewWorld(new NullRegionStore(), 7).GetOrCreate(Origin);
            var wide = NewWorld(new NullRegionStore(), 7).GetOrCreate(Origin,
                RegionRecipe.Maze.WithCells(
                    new Vector(2, 1), new Vector(1, 1)));
            Assert.That(wide.Size, Is.EqualTo(square.Size));         // footprint fixed
            Assert.That(Signature(wide), Is.Not.EqualTo(Signature(square)));
        }

        [Test]
        public void DefaultRecipe_IsInheritedUnlessOverridden() {
            // A world defaulting to Corridors yields the same as passing
            // Corridors explicitly to a Maze-defaulted world.
            var defaulted = new World(new NullRegionStore(), 7, RegionSize,
                    RegionRecipe.Corridors)
                .GetOrCreate(Origin);
            var explicitCorridors = NewWorld(new NullRegionStore(), 7)
                .GetOrCreate(Origin, RegionRecipe.Corridors);
            Assert.That(Signature(defaulted),
                Is.EqualTo(Signature(explicitCorridors)));
        }

        [Test]
        public void GetOrCreate_GeneratesOnMiss_ThenLoadsOnHit() {
            var store = new InMemoryRegionStore();
            var world = NewWorld(store, 555);

            var first = world.GetOrCreate(Origin);
            Assert.That(store.SaveCount, Is.EqualTo(1));

            var second = world.GetOrCreate(Origin);
            Assert.That(store.SaveCount, Is.EqualTo(1));
            Assert.That(Signature(second), Is.EqualTo(Signature(first)));
        }

        [Test]
        public void Recipe_IsIgnored_WhenTheRegionIsAlreadyStored() {
            var store = new InMemoryRegionStore();
            // First generation fixes the region as a Maze.
            var created = NewWorld(store, 7).GetOrCreate(Origin, RegionRecipe.Maze);
            // A later call with a different recipe returns the stored region.
            var reloaded = NewWorld(store, 7)
                .GetOrCreate(Origin, RegionRecipe.Corridors);
            Assert.That(Signature(reloaded), Is.EqualTo(Signature(created)));
        }

        [Test]
        public void StoredRegion_IsLoaded_NotRegenerated() {
            var store = new InMemoryRegionStore();
            var generated = NewWorld(store, 1).GetOrCreate(Origin);
            var loaded = NewWorld(store, 999999).GetOrCreate(Origin);
            Assert.That(Signature(loaded), Is.EqualTo(Signature(generated)));
        }

        [Test]
        public void StoreRoundTrip_PreservesCellsAndPois() {
            var store = new InMemoryRegionStore();
            var generated = NewWorld(store, 42).GetOrCreate(Origin);
            var reloaded = NewWorld(store, 42).GetOrCreate(Origin);
            Assert.That(Signature(reloaded), Is.EqualTo(Signature(generated)));
            Assert.That(reloaded.Pois.Count, Is.EqualTo(generated.Pois.Count));
        }

        [Test]
        public void NullRegionStore_PersistsNothing_AlwaysRegenerates() {
            var store = new NullRegionStore();
            Assert.That(store.TryLoad(Origin, out var blob), Is.False);
            Assert.That(blob, Is.Null);
            var region = NewWorld(store, 3).GetOrCreate(Origin);
            Assert.That(region.Pois.Any(p => p.Kind == PoiKind.Entrance), Is.True);
        }
    }
}
