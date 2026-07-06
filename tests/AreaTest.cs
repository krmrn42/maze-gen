using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class AreaTest : Test {
        private static Vector V10 => new Vector(10, 10);
        private static Vector V11 => new Vector(1, 1);

        [Test]
        public void IsInitialized() {
            var map = Area.CreateMaze(new Vector(2, 3));
            Assert.That(map.Grid, Has.Exactly(6).Items);
            Assert.That(map, Has.Exactly(6).Items);
            Assert.That(map.Count, Is.EqualTo(6));
            Assert.That(map.Size.Area, Is.EqualTo(6));
            Assert.That(map.Size, Is.EqualTo(new Vector(2, 3)));
        }

        [Test]
        public void WrongSize() {
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(-1, 1)));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(1, -1)));
            Assert.Throws<ArgumentException>(() => Area.CreateMaze(new Vector(-1, -1)));
        }

        [Test]
        public void ThrowsIfPositionIsFixedAndEmpty() {
            Assert.Throws<ArgumentException>(
                () => Area.Create(Vector.Empty, V11, AreaType.None));
        }

        [Test]
        public void ThrowsIfPositionIsEmpty() {
            var area = Area.CreateUnpositioned(V11, AreaType.None);
            Assert.Throws<InvalidOperationException>(
                () => { var _ = area.Position; });
        }

        [Test]
        public void FloatingArea() {
            var area = Area.CreateUnpositioned(V11, AreaType.None);
            Assert.That(area.IsPositionEmpty);
            Assert.That(!area.IsPositionFixed);
            area.Reposition(V11);
            Assert.That(!area.IsPositionEmpty);
            Assert.That(!area.IsPositionFixed);
            Assert.That(area.Position, Is.EqualTo(V11));
        }

        [Test]
        public void FixedPositionArea() {
            var area = Area.Create(V11, V11, AreaType.None);
            Assert.That(!area.IsPositionEmpty);
            Assert.That(area.IsPositionFixed);
            Assert.Throws<InvalidOperationException>(
                () => area.Reposition(V11));
        }

        [Test]
        public void CopyCtorWithNulls() {
            Assert.Throws<ArgumentNullException>(
                () => new Area(null, null, false, AreaType.None, null, null));
        }

        [Test]
        public void CopyCtorWithWrongNumberOfCells() {
            Assert.Throws<ArgumentException>(
                () => new Area(new Grid(Vector.Zero2D, V11),
                               Enumerable.Empty<Cell>(),
                               false,
                               AreaType.None,
                               null,
                               null));
        }

        [Test]
        public void CopyCtorChecksGridPosition() {
            Assert.Throws<ArgumentException>(
                () => new Area(new Grid(Vector.Empty, V11),
                               Enumerable.Repeat(new Cell(AreaType.None), 1),
                               true, AreaType.None,
                               null, null));
        }

        [Test]
        public void TagsNotNull() {
            var area = new Area(new Grid(Vector.Empty, V11),
                               Enumerable.Repeat(new Cell(AreaType.None), 1),
                                false, AreaType.None,
                                null, null);
            Assert.That(area.Tags, Is.Not.Null);
        }

        [Test]
        public void RepositionThrowsWhenPositionIsFixed() {
            var area = new Area(new Grid(V11, V11),
                               Enumerable.Repeat(new Cell(AreaType.None), 1),
                                true, AreaType.None,
                                null, null);
            Assert.Throws<InvalidOperationException>(
                () => area.Reposition(new Vector(1, 1)));
        }

        [Test]
        public void ClearChildAreas() {
            var area = new Area(new Grid(V11, V11),
                                Enumerable.Repeat(new Cell(AreaType.None), 1),
                                true, AreaType.None,
                                new List<Area>() {
                                    Area.Create(V11, V11, AreaType.None) },
                                null);
            Assert.That(area.ChildAreas, Has.Exactly(1).Items);
            area.ClearChildAreas();
            Assert.That(area.ChildAreas, Is.Empty);
        }

        [Test]
        public void ShallowCopyWithAreaType() {
            var original = Area.Create(V11, V11, AreaType.None, "foo");
            var copy = original.ShallowCopy(areaType: AreaType.Maze);
            Assert.That(copy.Type, Is.EqualTo(AreaType.Maze));
            Assert.That(copy.Tags, Is.EquivalentTo(original.Tags));
        }

        [Test]
        public void ShallowCopyWithCells() {
            var original = Area.Create(V11, V11, AreaType.None, "foo");
            var copy = original.ShallowCopy(
                cells: Enumerable.Repeat(new Cell(AreaType.None), 1));
            Assert.That(copy, Has.Exactly(1).Items);
            Assert.That(copy.Tags, Is.EquivalentTo(original.Tags));
        }

        [Test]
        public void ShallowCopyWithTags() {
            var original = Area.Create(V11, V11, AreaType.None, "foo");
            var copy = original.ShallowCopy(tags: new List<string>() { "bar" });
            Assert.That(copy.Type, Is.EqualTo(AreaType.None));
            Assert.That(copy.Tags, Has.All.AnyOf("bar"));
        }

        [Test]
        public void ShallowCopyWithChildAreas() {
            var original = Area.Create(V11, V11, AreaType.None, "foo");
            var copy = original.ShallowCopy(childAreas: new List<Area>() {
                Area.Create(V11, V11, AreaType.Maze)
            });
            Assert.That(copy.Tags, Is.EquivalentTo(original.Tags));
            Assert.That(copy.ChildAreas, Has.Exactly(1).Items);
            Assert.That(original.ChildAreas, Is.Empty);
        }

        [Test]
        public void CanAddChildArea() {
            var area = Area.CreateEnvironment(V11);
            Assert.That(area.ChildAreas, Is.Empty);
            area.AddChildArea(Area.Create(V11, V11, AreaType.Maze));
            Assert.That(area.ChildAreas, Has.Exactly(1).Items);
        }

        [Test]
        public void IsHollow() {
            Assert.That(Area.Create(V11, V11, AreaType.Hall).IsHollow, Is.True);
            Assert.That(Area.Create(V11, V11, AreaType.Cave).IsHollow, Is.True);
            Assert.That(Area.Create(V11, V11, AreaType.Maze).IsHollow, Is.False);
            Assert.That(Area.Create(V11, V11, AreaType.Fill).IsHollow, Is.False);
        }

        [Test]
        public void EnumeratorEnumeratesCells() {
            var cells = new List<Cell>() { new Cell(AreaType.None) };
            var env1 = Area.CreateEnvironment(V11);
            var env2 = env1.ShallowCopy(cells: cells);
            Assert.That(env2, Is.EquivalentTo(cells));
        }

        [Test]
        public void BakeFillAreas() {
            var env = Area.CreateEnvironment(V10);
            env.AddChildArea(Area.Create(V11, V11, AreaType.Fill));
            env.BakeChildAreas();
            Assert.That(env[V11].BakedNeighbors, Is.Empty);
            Assert.That(env[V11 + Vector.East2D].BakedNeighbors, Has.Exactly(3).Items);
            Assert.That(env[V11 + Vector.West2D].BakedNeighbors, Has.Exactly(2).Items);
            Assert.That(env[V11 + Vector.North2D].BakedNeighbors, Has.Exactly(3).Items);
            Assert.That(env[V11 + Vector.South2D].BakedNeighbors, Has.Exactly(2).Items);
        }

        [Test]
        public void RenderCallsFactory() {
            var env = Area.CreateEnvironment(V10);
            var renderFactoryMoq = new Moq.Mock<AsciiRendererFactory>();
            var renderMoq = new Moq.Mock<AreaToAsciiRenderer>();
            renderFactoryMoq.Setup(
                s => s.CreateRenderer(env))
                      .Returns(renderMoq.Object);
            renderMoq.Setup(s => s.Render()).Returns("rendered area");
            // renderFactory.VerifyAll();
            Assert.That(env.Render(renderFactoryMoq.Object),
                        Is.EqualTo("rendered area"));
            renderFactoryMoq.VerifyAll();
            renderMoq.VerifyAll();
        }

        [Test]
        public void ToString() {
            var env = Area.CreateEnvironment(V10);
            Assert.That(env.ToString(), Is.EqualTo("Environment:0x0;10x10"));
        }
    }
}
