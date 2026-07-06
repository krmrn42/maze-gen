using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class IRegionStoreTest {
        private static readonly RegionAddress Address =
            new RegionAddress(new Vector(1, 1));

        [Test]
        public void NullRegionStore_AlwaysMisses() {
            var store = new NullRegionStore();
            Assert.That(store.TryLoad(Address, out var blob), Is.False);
            Assert.That(blob, Is.Null);
        }

        [Test]
        public void NullRegionStore_SaveIsANoOp() {
            var store = new NullRegionStore();
            Assert.That(() => store.Save(Address, "anything"), Throws.Nothing);
            // Still a miss after "saving".
            Assert.That(store.TryLoad(Address, out _), Is.False);
        }
    }
}
