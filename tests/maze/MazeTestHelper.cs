
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze.PostProcessing;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;
using static PlayersWorlds.Maps.Maze.GeneratorOptions;

namespace PlayersWorlds.Maps.Maze {
    internal static class MazeTestHelper {
        private static readonly Log s_log = Log.ToConsole("MazeTestHelper");

        public static bool IsSolveable(Area maze) {
            var cells = new HashSet<Vector>(
                maze.Grid.Where(c => maze[c].HasLinks()));
            var dijkstra = DijkstraDistance.Find(maze, cells.First());
            cells.ExceptWith(dijkstra.Keys);

            if (cells.Count > 0) {
                s_log.I(
                    $"No solution for this maze between {cells.First()} and " +
                    $"{string.Join(",", cells)}:\n" + maze.ToString());
                return false;
            }
            return true;
        }

        public static Area GenerateMaze(
                                   Vector size,
                                   List<Area> childAreas,
                                   GeneratorOptions options,
                                   out Maze2DBuilder builder) {
            if (options.RandomSource == null) {
                options.RandomSource = RandomSource.CreateFromEnv();
            }
            if (options.AreaGeneration == AreaGenerationMode.Auto && options.AreaGenerator == null) {
                options.AreaGenerator = new RandomAreaGenerator(options.RandomSource);
            }
            var maze = Area.CreateMaze(size);
            childAreas?.ForEach(childArea => maze.AddChildArea(childArea));
            builder = Maze2DBuilder.BuildMaze(maze, options);
            s_log.D(2, maze.Render(new AsciiRendererFactory()));
            Assert.That(maze.ChildAreas.Count,
                Is.GreaterThanOrEqualTo(childAreas?.Count ?? 0),
                "Wrong number of areas");
            return maze;
        }

        public static Area GenerateMaze(
                                   Vector size,
                                   List<Area> childAreas,
                                   GeneratorOptions options) {
            return GenerateMaze(size, childAreas, options, out _);
        }

        public static Area GenerateMaze(
                                   Vector size,
                                   GeneratorOptions options) {
            return GenerateMaze(size, null, options, out _);
        }

        public static bool IsSupported(
            Type generatorType,
            MazeFillFactor fillFactor) {
            if ((generatorType == typeof(SidewinderMazeGenerator)
                 || generatorType == typeof(BinaryTreeMazeGenerator))
                && fillFactor != GeneratorOptions.MazeFillFactor.Full) {
                return false;
            }
            return true;
        }

        public static IEnumerable<Type> GetAllGenerators() {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => asm.GetName().Name == "PlayersWorlds.Maps")
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(MazeGenerator) != p &&
                            typeof(MazeGenerator).IsAssignableFrom(p))
                .Where(p =>
                    !TestContext.Parameters.Exists("MAZEGEN") ||
                    p.Name == TestContext.Parameters["MAZEGEN"]);
        }

        public static IEnumerable<AreaType> GetAllAreaTypes() {
            if (!TestContext.Parameters.Exists("MAZEGEN_AREATYPE")) {
                yield return AreaType.Cave;
                yield return AreaType.Hall;
                yield return AreaType.Fill;
            } else {
                yield return (AreaType)
                    Enum.Parse(typeof(AreaType),
                    TestContext.Parameters["MAZEGEN_AREATYPE"]);
            }
        }

        public static IEnumerable<MazeFillFactor> GetAllFillFactors() {
            if (!TestContext.Parameters.Exists("MAZEGEN_FILLFACTOR")) {
                yield return MazeFillFactor.Quarter;
                yield return MazeFillFactor.Half;
                yield return MazeFillFactor.ThreeQuarters;
                yield return MazeFillFactor.NinetyPercent;
                yield return MazeFillFactor.FullWidth;
                yield return MazeFillFactor.FullHeight;
                yield return MazeFillFactor.Full;
            } else {
                yield return (MazeFillFactor)
                    Enum.Parse(typeof(MazeFillFactor),
                    TestContext.Parameters["MAZEGEN_FILLFACTOR"]);
            }
        }

        internal static Area Parse(string area) {
            return new AreaSerializer().Deserialize(area);
        }
    }
}
