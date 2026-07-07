using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RoomRequestTest {
        [Test]
        public void ExposesItsFields() {
            var request = new RoomRequest(3, new Vector(2, 2), new Vector(4, 4),
                RoomKind.Cave, new[] { "shrine" });
            Assert.That(request.Count, Is.EqualTo(3));
            Assert.That(request.MinSize, Is.EqualTo(new Vector(2, 2)));
            Assert.That(request.MaxSize, Is.EqualTo(new Vector(4, 4)));
            Assert.That(request.Kind, Is.EqualTo(RoomKind.Cave));
            Assert.That(request.Tags, Is.EqualTo(new[] { "shrine" }));
        }
    }
}
