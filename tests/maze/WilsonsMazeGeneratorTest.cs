

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class WilsonsMazeGeneratorTest : Test {
        [Test, Timeout(1000)]
        public void DuplicateRandom() {
            var maze = Area.CreateMaze(new Vector(5, 5));
            var opts = new GeneratorOptions() {
                MazeAlgorithm = typeof(WilsonsMazeGenerator),
                RandomSource = RandomSource.CreateFromEnv()
            };
            var builderMock = new Mock<Maze2DBuilder>(
                RandomSource.CreateFromEnv(),
                maze,
                new WilsonsMazeGenerator() as MazeGenerator,
                null,
                MazeFillFactor.Full);
            var firstCell = new Vector(4, 3);
            var randomNeighbor = new Vector(3, 3);

            builderMock.SetupGet(b => b.CellGroups)
                .Returns(new List<HashSet<Vector>>() {
                    new HashSet<Vector>() { firstCell }
                });

            builderMock.SetupSequence(b => b.PickNextCellToLink())
                .Returns(firstCell);
            builderMock.Setup(b => b.TryPickRandomNeighbor(
                    firstCell, out randomNeighbor, false, false))
                .Returns(true);
            builderMock.Setup(b => b.TryPickRandomNeighbor(
                    randomNeighbor, out firstCell, false, false))
                .Returns(true);
            builderMock.SetupSequence(b => b.IsFillComplete())
                .Returns(false)
                .Returns(true);

            Assert.That(() =>
                new WilsonsMazeGenerator()
                    .GenerateMaze(builderMock.Object),
                Throws.Nothing);
        }
    }
}
