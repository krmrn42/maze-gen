using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionCellTest {
        [Test]
        public void ExposesPayload() {
            var cell = new RegionCell(true, AreaType.Hall,
                new[] { "A", "B" });
            Assert.That(cell.IsPassable, Is.True);
            Assert.That(cell.Type, Is.EqualTo(AreaType.Hall));
            Assert.That(cell.Tags, Is.EqualTo(new[] { "A", "B" }));
        }

        [Test]
        public void NullTags_ReadAsEmpty() {
            var cell = new RegionCell(false, AreaType.None, null);
            Assert.That(cell.IsPassable, Is.False);
            Assert.That(cell.Tags, Is.Empty);
        }
    }
}
