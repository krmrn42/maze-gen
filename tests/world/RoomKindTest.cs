using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RoomKindTest {
        [Test]
        public void CoversTheRoomCapableAreaTypes() {
            Assert.That(Enum.GetValues(typeof(RoomKind)),
                Is.EquivalentTo(new[] {
                    RoomKind.Hall, RoomKind.Cave, RoomKind.Blocked }));
        }
    }
}
