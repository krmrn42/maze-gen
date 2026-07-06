
using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class SidewinderMazeGeneratorTest : Test {
        [Test]
        public void ArchShapedAreas() {
            // TODO: This easily results in unvisitable maze areas (e.g., seed 935)
            var area1 = Area.Create(new Vector(2, 2), new Vector(3, 13), AreaType.Hall);
            var area2 = Area.Create(new Vector(10, 2), new Vector(3, 13), AreaType.Hall);
            var area3 = Area.Create(new Vector(4, 8), new Vector(7, 3), AreaType.Hall);
            MazeTestHelper.GenerateMaze(
                new Vector(15, 15), new List<Area>() { area1, area2, area3 },
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Sidewinder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                },
                out var builder);
            Assert.That(builder.TestCellsToConnect, Is.Empty);
        }

        [Test]
        [Repeat(10), Category("Integration")] // random factor
        public void ArchShapedAreasLeftExit() {
            var area1 = Area.Create(new Vector(2, 2), new Vector(3, 13), AreaType.Hall);
            var area2 = Area.Create(new Vector(10, 2), new Vector(3, 13), AreaType.Hall);
            var area3 = Area.Create(new Vector(6, 8), new Vector(7, 3), AreaType.Hall);
            MazeTestHelper.GenerateMaze(
                new Vector(15, 15), new List<Area>() { area1, area2, area3 },
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Sidewinder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                },
                out var builder);
            Assert.That(builder.TestCellsToConnect, Is.Empty);
        }
    }
}
