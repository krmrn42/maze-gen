using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class PoiTest {
        [Test]
        public void ExposesKindAndLocal() {
            var poi = new Poi(PoiKind.Exit, new Vector(3, 4));
            Assert.That(poi.Kind, Is.EqualTo(PoiKind.Exit));
            Assert.That(poi.Local, Is.EqualTo(new Vector(3, 4)));
        }
    }
}
