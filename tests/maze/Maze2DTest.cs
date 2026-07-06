using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class Maze2DTest : Test {

        private static Area ConvertMazeToMap(
            Area maze, Maze2DRendererOptions options) {
            var builder = new Maze2DBuilder(
                RandomSource.CreateFromEnv(),
                maze, null, null,
                MazeFillFactor.Full);
            builder.TestRebuildCellMaps();
            maze.X(builder);
            return new MazeAreaStyleConverter()
                    .ConvertMazeBorderToBlock(
                        maze, options: options);
        }

        [Test]
        public void Maze2D_ToMapWrongOptions() {
            Assert.DoesNotThrow(() => ConvertMazeToMap(
                Area.CreateMaze(new Vector(2, 3)),
                new Maze2DRendererOptions(new Vector(1, 1), new Vector(2, 2))));
            Assert.Throws<ArgumentException>(() =>
                ConvertMazeToMap(
                    Area.CreateMaze(new Vector(2, 3)),
                    new Maze2DRendererOptions(
                        new Vector(1, 1),
                        new Vector(new int[] { 1, 2, 3 }))));
            Assert.Throws<ArgumentException>(() =>
                ConvertMazeToMap(
                    Area.CreateMaze(new Vector(2, 3)),
                    new Maze2DRendererOptions(
                        new Vector(1, 1),
                        new Vector(0, 2))));
            Assert.Throws<ArgumentException>(() =>
                ConvertMazeToMap(
                    Area.CreateMaze(new Vector(2, 3)),
                    new Maze2DRendererOptions(
                        new Vector(1, 0),
                        new Vector(2, 2))));
        }

        [Test]
        public void Maze2D_CanRenderMap() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            var scaledMap = ConvertMazeToMap(map,
                new Maze2DRendererOptions(
                    new Vector(3, 2), new Vector(2, 1)));
            Assert.That(scaledMap.Size, Is.EqualTo(new Vector(17, 10)));
        }

        [Test]
        public void Maze2D_AddsNoRoomsWhenNoneRequested() {
            var maze = MazeTestHelper.GenerateMaze(new Vector(10, 10), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual,
                });
            Assert.That(maze.ChildAreas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsNoRoomsToASmallMaze() {
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    RandomSource = random,
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                    AreaGenerator = new RandomAreaGenerator(random)
                });
            Assert.That(maze.ChildAreas.Count, Is.EqualTo(0));
        }

        [Test]
        public void Maze2D_AddsRooms() {
            var random = RandomSource.CreateFromEnv();
            var maze = MazeTestHelper.GenerateMaze(new Vector(5, 5), null,
                new GeneratorOptions() {
                    RandomSource = random,
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto,
                    AreaGenerator = new RandomAreaGenerator(random)
                });
            Assert.That(maze.ChildAreas.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Maze2D_AddsOneHall() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual,
            };
            // 
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { MazeTestHelper.Parse("Area:{2x2;2x1;False;Hall;;;}") }, options);
            Assert.That(maze.ChildAreas.Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_AddsOneFill() {
            var options = new GeneratorOptions() {
                MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                FillFactor = GeneratorOptions.MazeFillFactor.Full,
                AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual
            };
            // 
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(5, 5), new List<Area> { MazeTestHelper.Parse("Area:{2x2;2x1;False;Fill;;;}") }, options);
            Assert.That(maze.ChildAreas.Count, Is.EqualTo(1));
        }

        [Test]
        public void Maze2D_HasExtensionsSet() {
            var map = MazeTestHelper.GenerateMaze(new Vector(3, 3), null,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            Assert.That(map.X<DeadEnd.DeadEndsExtension>(), Is.Not.Null);
            Assert.That(map.X<DijkstraDistance.LongestTrailExtension>(), Is.Not.Null);
        }
    }
}