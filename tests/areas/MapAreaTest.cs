using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps.Areas {

    [TestFixture]
    internal class MapAreaTest : Test {
        private static Area NewArea(int x, int y, int width, int height) {
            return Area.Create(
                new Vector(x, y), new Vector(width, height), AreaType.Maze);
        }

        [Test]
        public void Overlaps_ThrowsIfSame() {
            var area1 = NewArea(1, 1, 10, 10);
            var area2 = NewArea(1, 1, 10, 10);
            Assert.Throws<InvalidOperationException>(
                () => area1.Grid.Overlaps(area1.Grid));
            Assert.DoesNotThrow(() => area1.Grid.Overlaps(area2.Grid));
        }

        [Test]
        public void Overlaps_ReturnsTrueIfOverlaps() {
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(4, 4, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(1, 1, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(4, 1, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(7, 1, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(1, 7, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(4, 7, 4, 4).Grid), Is.True);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(7, 7, 4, 4).Grid), Is.True);

        }

        [Test]
        public void Overlaps_ReturnsFalseIfDoesNotOverlap() {
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(0, 0, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(1, 0, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(4, 0, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(7, 0, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(1, 8, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(4, 8, 4, 4).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(7, 8, 4, 4).Grid), Is.False);

            // Touches:
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(1, 4, 3, 3).Grid), Is.False, "Map areas touch (case 1)");
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(5, 3, 2, 1).Grid), Is.False, "Map areas touch (case 2)");
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(8, 5, 3, 2).Grid), Is.False, "Map areas touch (case 3)");
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(5, 8, 1, 4).Grid), Is.False, "Map areas touch (case 4)");

            // Does not touch:
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(0, 4, 3, 3).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(5, 2, 2, 1).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(9, 5, 3, 2).Grid), Is.False);
            Assert.That(NewArea(4, 4, 4, 4).Grid.Overlaps(NewArea(5, 9, 1, 4).Grid), Is.False);
        }

        [Test]
        public void Position_UnpositionedThrowsIfNotInitialized() {
            var area1 = Area.CreateUnpositioned(
                new Vector(10, 10), AreaType.Maze);
            Assert.That(() => { var pos = area1.Position; },
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Position_DoesNotThrowIfInitialized() {
            var area1 = Area.Create(
                new Vector(1, 1), new Vector(10, 10), AreaType.Maze);
            Assert.That(
                () => { var pos = area1.Position; }, Throws.Nothing);
        }

        [Test]
        public void ThrowsIfWrongParameters() {
            Assert.That(() =>
                Area.Create(
                    Vector.Empty,
                    new Vector(10, 10),
                    AreaType.Maze),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BakeCreatesNeighbors() {
            var area = Area.CreateMaze(new Vector(3, 3));
            area.BakeChildAreas();
            Assert.That(area[new Vector(0, 0)].BakedNeighbors, Has.Exactly(2).Items);
        }

        [Test]
        public void BakeCreatesLinks() {
            var area = Area.CreateMaze(new Vector(3, 3));
            area.AddChildArea(Area.Create(new Vector(0, 0), new Vector(3, 3), AreaType.Hall));
            area.BakeChildAreas();
            Assert.That(area[new Vector(0, 0)].BakedLinks, Has.Exactly(2).Items);
        }

        [Test]
        public void BakeCreatesProperLinks() {
            var area = Area.CreateMaze(new Vector(3, 3));
            area.AddChildArea(Area.Create(new Vector(1, 1), new Vector(2, 2), AreaType.Hall));
            area.BakeChildAreas();
            Assert.That(area[new Vector(0, 0)].BakedLinks, Is.Empty);
            Assert.That(area[new Vector(1, 1)].BakedLinks, Has.Exactly(2).Items);
        }
    }
}
