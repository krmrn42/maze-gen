using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class WorldTest {
        private static readonly Vector RegionMazeSize = new Vector(6, 6);
        private static readonly RegionAddress Origin =
            new RegionAddress(new Vector(0, 0));

        private static World NewWorld(IRegionStore store, int seed) =>
            new World(store, seed, RegionMazeSize);

        // A stable fingerprint of a region's rendered structure + POIs, so two
        // regions can be compared for equality without exposing internals.
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
            Assert.That(region.Size.Area, Is.GreaterThan(RegionMazeSize.Area));

            // Exactly one entrance and one exit, and they are distinct cells.
            var entrance = region.Pois.Single(p => p.Kind == PoiKind.Entrance);
            var exit = region.Pois.Single(p => p.Kind == PoiKind.Exit);
            Assert.That(entrance.Local, Is.Not.EqualTo(exit.Local));

            // Every POI sits on a passable cell.
            foreach (var poi in region.Pois) {
                Assert.That(region.CellAt(poi.Local).IsPassable, Is.True,
                    $"POI {poi.Kind} at {poi.Local} must be passable");
            }

            // There is at least some floor.
            var floors = 0;
            for (var y = 0; y < region.Size.Y; y++)
                for (var x = 0; x < region.Size.X; x++)
                    if (region.CellAt(new Vector(x, y)).IsPassable) floors++;
            Assert.That(floors, Is.GreaterThan(0));
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
        public void GetOrCreate_GeneratesOnMiss_ThenLoadsOnHit() {
            var store = new InMemoryRegionStore();
            var world = NewWorld(store, 555);

            var first = world.GetOrCreate(Origin);
            Assert.That(store.SaveCount, Is.EqualTo(1));
            Assert.That(store.Count, Is.EqualTo(1));

            var second = world.GetOrCreate(Origin);
            // No second save: the stored region was loaded, not regenerated.
            Assert.That(store.SaveCount, Is.EqualTo(1));
            Assert.That(Signature(second), Is.EqualTo(Signature(first)));
        }

        [Test]
        public void StoredRegion_IsLoaded_NotRegenerated() {
            var store = new InMemoryRegionStore();
            // Seed 1 generates and saves the region.
            var generated = NewWorld(store, 1).GetOrCreate(Origin);
            // A different seed sharing the store must return the STORED region,
            // proving it loaded rather than regenerating with the new seed.
            var loaded = NewWorld(store, 999999).GetOrCreate(Origin);
            Assert.That(Signature(loaded), Is.EqualTo(Signature(generated)));
        }

        [Test]
        public void StoreRoundTrip_PreservesCellsAndPois() {
            var store = new InMemoryRegionStore();
            var generated = NewWorld(store, 42).GetOrCreate(Origin);
            var reloaded = NewWorld(store, 42).GetOrCreate(Origin);
            // Full structural + POI equality across the serialize/deserialize.
            Assert.That(Signature(reloaded), Is.EqualTo(Signature(generated)));
            Assert.That(reloaded.Pois.Count, Is.EqualTo(generated.Pois.Count));
        }

        [Test]
        public void NullRegionStore_PersistsNothing_AlwaysRegenerates() {
            var store = new NullRegionStore();
            Assert.That(store.TryLoad(Origin, out var blob), Is.False);
            Assert.That(blob, Is.Null);
            // A world on a null store still works (regenerates each time).
            var region = NewWorld(store, 3).GetOrCreate(Origin);
            Assert.That(region.Pois.Any(p => p.Kind == PoiKind.Entrance), Is.True);
        }
    }
}
