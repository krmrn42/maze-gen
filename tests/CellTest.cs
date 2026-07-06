using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class CellTest : Test {
        private static Vector V10 => new Vector(10, 10);
        private static Vector V11 => new Vector(1, 1);
        [Test]
        public void CtorTest() {
            var cell = new Cell(AreaType.Cave);
            Assert.That(cell.AreaType, Is.EqualTo(AreaType.Cave));
        }

        [Test]
        public void HardLinks() {
            var cell = new Cell(AreaType.Cave);
            cell.HardLinks.Add(V11);
            Assert.That(cell.HasLinks());
            Assert.That(cell.HasLinks(V11));
            Assert.That(cell.Links(), Contains.Item(V11));
            Assert.That(cell.HardLinks, Has.Exactly(1).Items);
            Assert.That(cell.BakedLinks, Has.Exactly(0).Items);
            Assert.That(cell.BakedNeighbors, Has.Exactly(0).Items);
        }

        [Test]
        public void BakedLinks() {
            var cell = new Cell(AreaType.Cave);
            cell.Bake(Enumerable.Empty<Vector>(), Enumerable.Repeat(V11, 1));
            Assert.That(cell.HasLinks());
            Assert.That(cell.HasLinks(V11));
            Assert.That(cell.Links(), Contains.Item(V11));
            Assert.That(cell.HardLinks, Has.Exactly(0).Items);
            Assert.That(cell.BakedLinks, Has.Exactly(1).Items);
            Assert.That(cell.BakedNeighbors, Has.Exactly(0).Items);
        }

        [Test]
        public void NoDoubleBake() {
            var cell = new Cell(AreaType.Cave);
            cell.Bake(Enumerable.Repeat(V10, 1), Enumerable.Repeat(V11, 1));
            cell.Bake(Enumerable.Repeat(V11, 1), Enumerable.Repeat(V10, 1));
            Assert.That(cell.BakedLinks, Has.Exactly(1).Items);
            Assert.That(cell.BakedLinks, Contains.Item(V10));
            Assert.That(cell.BakedNeighbors, Has.Exactly(1).Items);
            Assert.That(cell.BakedNeighbors, Contains.Item(V11));
        }

        [Test]
        public void Neighbors() {
            var cell = new Cell(AreaType.Cave);
            cell.Bake(Enumerable.Repeat(V11, 1), Enumerable.Empty<Vector>());
            Assert.That(cell.HardLinks, Has.Exactly(0).Items);
            Assert.That(cell.BakedLinks, Has.Exactly(0).Items);
            Assert.That(cell.BakedNeighbors, Has.Exactly(1).Items);
        }

        [Test]
        public void ToString() {
            var cell = new Cell(AreaType.Cave);
            cell.HardLinks.Add(V11);
            cell.Bake(Enumerable.Repeat(V11, 1), Enumerable.Repeat(V10, 1));
            Assert.That(cell.ToString(), Is.EqualTo("Cell(Cave);1x1;10x10;1x1"));
        }

        [Test]
        public void Clone() {
            var cell1 = new Cell(AreaType.Cave);
            cell1.HardLinks.Add(V10);
            cell1.Bake(Enumerable.Repeat(V10, 1), Enumerable.Repeat(V11, 1));
            cell1.Tags.Add(new Cell.CellTag("test1"));
            var cell2 = cell1.Clone();
            Assert.That(cell2.HardLinks, Has.Exactly(1).Items);
            Assert.That(cell2.BakedLinks, Has.Exactly(1).Items);
            Assert.That(cell2.BakedNeighbors, Has.Exactly(1).Items);
            Assert.That(cell2.Tags, Has.Exactly(1).Items);
        }

        [Test]
        public void CellTagEqualityTest() {
            var tag1 = new Cell.CellTag("test1");
            var tag2 = new Cell.CellTag("test1");
            Assert.That(tag1.GetHashCode(), Is.EqualTo("test1".GetHashCode()));
            Assert.That(tag1.Equals("test1"), Is.True);
            Assert.That(tag1.ToString(), Is.EqualTo("test1"));
            Assert.That(tag1, Is.EqualTo(tag2));
        }
    }
}
