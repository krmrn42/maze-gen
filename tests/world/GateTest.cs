using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class GateTest {
        [Test]
        public void ExposesBorderFaceAndOpenCells() {
            var open = new[] { new Vector(3, 0), new Vector(3, 1) };
            var gate = new Gate(1, true, open);
            Assert.That(gate.Dimension, Is.EqualTo(1));
            Assert.That(gate.AtFarSide, Is.True);
            Assert.That(gate.OpenCells, Is.EqualTo(open));
        }
    }
}
