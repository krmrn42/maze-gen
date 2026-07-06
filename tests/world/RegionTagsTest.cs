using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionTagsTest {
        [Test]
        public void For_MapsEachKindToItsTag() {
            Assert.That(RegionTags.For(PoiKind.Entrance),
                Is.EqualTo(RegionTags.Entrance));
            Assert.That(RegionTags.For(PoiKind.Exit),
                Is.EqualTo(RegionTags.Exit));
            Assert.That(RegionTags.For(PoiKind.DeadEnd),
                Is.EqualTo(RegionTags.DeadEnd));
        }

        [Test]
        public void For_ThrowsForUnknownKind() {
            Assert.That(() => RegionTags.For((PoiKind)999),
                Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }
}
