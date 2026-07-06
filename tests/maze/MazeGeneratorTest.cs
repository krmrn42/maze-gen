using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Areas.Evolving;
using PlayersWorlds.Maps.Maze.PostProcessing;
using static PlayersWorlds.Maps.Maze.PostProcessing.DijkstraDistance;

namespace PlayersWorlds.Maps.Maze {
    [TestFixture]
    public class MazeGeneratorTest : Test {

        private class CustomAreaGenerator : AreaGenerator {
            public CustomAreaGenerator() : base(
                new Mock<EvolvingSimulator>(MockBehavior.Loose, 1, 1).Object,
                new Mock<MapAreaSystemFactory>(
                    MockBehavior.Loose, new FakeRandomSource()).Object) { }
            protected override IEnumerable<Area> Generate(
                Area targetArea) {
                if (targetArea.Size.X <= 10 || targetArea.Size.Y <= 10) {
                    return Enumerable.Empty<Area>();
                }
                return new[] {
                    Area.Create(
                        new Vector(0, 0), new Vector(2, 3), AreaType.Hall)
                };
            }
        }

        [Test]
        public void CanUseCustomAreaGenerator() {
            var log = Log.ToConsole("MazeGeneratorTest.CanUseCustomAreaGenerator");
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(15, 15), null,
                new GeneratorOptions() {
                    AreaGenerator = new CustomAreaGenerator(),
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Auto
                });
            log.D(5, maze.ToString());
            Assert.That(maze.ChildAreas, Has.Count.EqualTo(1));
            Assert.That(maze.ChildAreas.First().Size,
                Is.EqualTo(new Vector(2, 3)));
            Assert.That(maze.ChildAreas.First().Position,
                Is.EqualTo(new Vector(0, 0)));
            Assert.That(maze.ChildAreas.First().Type,
                Is.EqualTo(AreaType.Hall));
        }

        [Test]
        public void CanGenerateMazes(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var randomSource = RandomSource.CreateFromEnv();
            var log = Log.ToConsole($"MazeGeneratorTest.CanGenerateMazes({generatorType.Name}); ");
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(20, 20), null, new GeneratorOptions() {
                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                    MazeAlgorithm = generatorType,
                    RandomSource = randomSource
                });
            Assert.That(MazeTestHelper.IsSolveable(maze), $"{generatorType.Name} generated an unsolveable maze with seed {randomSource.Seed}");
            Assert.That(maze.Grid.Count(cell => maze[cell].Links().Count == 0), Is.EqualTo(0));
        }

        [Test]
        public void CanGenerateMazes_Debug(
        ) {
            // CanGenerateMazes(typeof(SidewinderMazeGenerator));
            CanGenerateMazes(typeof(BinaryTreeMazeGenerator));
            // CanGenerateMazes(typeof(AldousBroderMazeGenerator));
        }

        [Test]
        public void OnlyFullGenerators() {
            Assert.Throws<ArgumentException>(() =>
                MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.BinaryTree,
                    FillFactor = GeneratorOptions.MazeFillFactor.Half
                }));
            Assert.Throws<ArgumentException>(() =>
                MazeTestHelper.GenerateMaze(new Vector(10, 10),
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.Sidewinder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Half
                }));
        }

        [Test]
        public void IsFillComplete_Half() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Half
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area / 2), maze.ToString());
        }

        [Test]
        public void IsFillComplete_Full() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Count(), Is.EqualTo(size.Area));
        }

        [Test]
        public void IsFillComplete_Quarter() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.Quarter
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.25), maze.ToString());
        }

        [Test]
        public void IsFillComplete_ThreeQuarters() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.ThreeQuarters
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.75), maze.ToString());
        }

        [Test]
        public void IsFillComplete_NinetyPercent() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.NinetyPercent
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Count(), Is.GreaterThanOrEqualTo(size.Area * 0.9), maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullWidth() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.FullWidth
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Min(cell => cell.X) == 0 && mazeCells.Max(cell => cell.X) == 9, Is.True, maze.ToString());
        }

        [Test]
        public void IsFillComplete_FullHeight() {
            var size = new Vector(10, 10);
            var maze = MazeTestHelper.GenerateMaze(size,
                new GeneratorOptions() {
                    MazeAlgorithm = GeneratorOptions.Algorithms.AldousBroder,
                    FillFactor = GeneratorOptions.MazeFillFactor.FullHeight
                });
            Assert.That(MazeTestHelper.IsSolveable(maze));
            var mazeCells = maze.Grid.Where(cell => maze[cell].HasLinks()).ToList();
            Assert.That(mazeCells.Min(cell => cell.Y) == 0 && mazeCells.Max(cell => cell.Y) == 9, Is.True, maze.ToString());
        }

        [Test, Category("Integration")]
        public void IsFillComplete(
            [ValueSource("GetAllGenerators")]
            Type generatorType,
            [ValueSource("GetGeneratorOptionsFillFactors")]
            GeneratorOptions.MazeFillFactor fillFactor,
            [ValueSource("GetGeneratorOptionsMapAreaOptions")]
            GeneratorOptions.AreaGenerationMode mapAreaOptions,
            [ValueSource("GetGeneratorOptionsMapAreas")]
            List<Area> mapAreas
        ) {
            var log = Log.ToConsole("IsFillComplete");
            if (!MazeTestHelper.IsSupported(generatorType, fillFactor)) {
                Assert.Ignore();
            }
            var size = new Vector(10, 10);
            var options = new GeneratorOptions() {
                MazeAlgorithm = generatorType,
                FillFactor = fillFactor,
                AreaGeneration = mapAreaOptions,
                RandomSource = RandomSource.CreateFromEnv()
            };

            var map = MazeTestHelper.GenerateMaze(size, mapAreas, options);
            Assert.That(map, Is.Not.Null);

            var solution = new LongestTrailExtension(new List<Vector>());
            Assert.DoesNotThrow(
                () => solution = DijkstraDistance.FindLongestTrail(map));
            Assert.That(solution, Is.Not.Null);
            Assert.That(solution.LongestTrail, Is.Not.Null.Or.Empty);

            map.X(DeadEnd.Find(map));
            map.X(solution);
        }

        [Test, Ignore("Debugging only")]
        public void IsFillComplete_Debug() {
            // // Fails: Could not generate rooms for maze of size 10x10. Last set of rooms had 2 errors (P5x4;S6x3, P-1x4;S6x3) Random(524).
            // IsFillComplete(typeof(AldousBroderMazeGenerator),
            //                GeneratorOptions.MazeFillFactor.Half,
            //                GeneratorOptions.AreaGenerationMode.Auto,
            //                null);
            // Fails: Could not generate rooms for maze of size 10x10. Last set of rooms had 4 errors (P1x7;S2x3, P8x5;S3x2, P2x-1;S3x6, P0x5;S3x2, P3x4;S4x3) Random(408).
            IsFillComplete(typeof(AldousBroderMazeGenerator),
                           GeneratorOptions.MazeFillFactor.Half,
                           GeneratorOptions.AreaGenerationMode.Auto,
                           new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Fill) });
        }

        [Test]
        public void WrongGeneratorOptions() {
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { MazeAlgorithm = typeof(string) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { MazeAlgorithm = typeof(TestGeneratorA) }), Throws.TypeOf<ArgumentException>());
            Assert.That(() => MazeTestHelper.GenerateMaze(new Vector(3, 4), new GeneratorOptions { MazeAlgorithm = typeof(TestGeneratorB) }), Throws.Nothing);
        }

        class TestGeneratorA : MazeGenerator {
            public TestGeneratorA(string _) : base() { }
            public override void GenerateMaze(Maze2DBuilder builder) { }
        }

        class TestGeneratorB : MazeGenerator {
            public override void GenerateMaze(Maze2DBuilder builder) {
                builder.AllCells.ForEach(cell => {
                    if (builder.MazeArea.Grid.Contains(cell + Vector.East2D)) {
                        builder.Connect(cell, cell + Vector.East2D);
                    }
                    if (builder.MazeArea.Grid.Contains(cell + Vector.North2D)) {
                        builder.Connect(cell, cell + Vector.North2D);
                    }
                });
            }
        }

        [Test, Category("Integration")]
        public void CanFindPaths(
            [ValueSource("GetAllGenerators")] Type generatorType
        ) {
            var maze = MazeTestHelper.GenerateMaze(
                new Vector(3, 4),
                new GeneratorOptions() { MazeAlgorithm = generatorType });
            var solution = new LongestTrailExtension(new List<Vector>());
            Assert.DoesNotThrow(
                () => solution = DijkstraDistance.FindLongestTrail(maze));
            Assert.That(solution, Is.Not.Null);
            Assert.That(solution.LongestTrail, Is.Not.Null.Or.Empty);
        }

        public static IEnumerable<Type> GetAllGenerators() =>
            MazeTestHelper.GetAllGenerators();

        public static IEnumerable<GeneratorOptions.MazeFillFactor>
            GetGeneratorOptionsFillFactors() =>
            MazeTestHelper.GetAllFillFactors();

        public static IEnumerable<GeneratorOptions.AreaGenerationMode>
            GetGeneratorOptionsMapAreaOptions() {
            yield return GeneratorOptions.AreaGenerationMode.Auto;
            yield return GeneratorOptions.AreaGenerationMode.Manual;
        }

        public static IEnumerable<List<Area>>
            GetGeneratorOptionsMapAreas() {
            yield return null;
            yield return new List<Area>();
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Fill) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(3, 2), AreaType.Hall) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Fill),
                Area.CreateUnpositioned(new Vector(6, 5), new Vector(3, 2), AreaType.Hall) };
            yield return new List<Area>() {
                Area.CreateUnpositioned(new Vector(3, 2), new Vector(2, 3), AreaType.Hall),
                Area.CreateUnpositioned(new Vector(6, 5), new Vector(3, 2), AreaType.Fill) };
        }
    }
}
