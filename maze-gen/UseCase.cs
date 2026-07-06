using System;
using System.Linq;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Maze;
using PlayersWorlds.Maps.Renderers;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps {
    [Verb("usecase", HelpText = "Render a predefined composed world (e.g. a maze with rooms) by case number.")]
    class UseCaseCommand : BaseCommand {

        [Option('c', "case", Default = 1, Required = true, HelpText = "Use case number.")]
        public int Case { get; set; }

        override public int Run() {
            base.Run();
            var randomSource = RandomSource.CreateFromEnv();

            var maze = _useCases[Case](randomSource);

            Console.WriteLine(maze.ToString());
            Console.WriteLine(maze.Render(new AsciiRendererFactory()));
            return 0;
        }

        private readonly Func<RandomSource, Area>[] _useCases =
            new Func<RandomSource, Area>[] {
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Maze, new Vector(3, 2))
                .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                                    MazeAlgorithm = GeneratorOptions.Algorithms.HuntAndKill,
                                    FillFactor = GeneratorOptions.MazeFillFactor.Full
                                })
                .ToMap(Maze2DRendererOptions.RectCells(3, 2))
                .AddLayer(area => area.ShallowCopy(cells:
                    area.Select(cell =>
                        cell.Tags.Contains(Cell.CellTag.MazeTrail) ?
                            new Cell(AreaType.Maze) :
                            new Cell(AreaType.None))))
                .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                                    MazeAlgorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                                    FillFactor = GeneratorOptions.MazeFillFactor.ThreeQuarters
                                })
                .ToMap(Maze2DRendererOptions.RectCells(2, 1))
                .Map(),
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Maze, new Vector(15, 11))
                .WithAreas(
                    Area.Create(new Vector(1, 1), new Vector(4, 3), AreaType.Hall),
                    Area.Create(new Vector(9, 6), new Vector(4, 4), AreaType.Hall))
                .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                                    MazeAlgorithm = GeneratorOptions.Algorithms.RecursiveBacktracker,
                                    FillFactor = GeneratorOptions.MazeFillFactor.Full,
                                    AreaGeneration = GeneratorOptions.AreaGenerationMode.Manual
                                })
                .ToMap(Maze2DRendererOptions.RectCells(2, 1))
                .Map(),
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Environment, new Vector(80, 25))
                .WithAreas(
                    new GeneratedWorld(r)
                        .AddLayer(AreaType.Maze, new Vector(2, 0), new Vector(3, 2))
                        .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                                            MazeAlgorithm = GeneratorOptions.Algorithms.HuntAndKill,
                                        })
                        .ToMap(Maze2DRendererOptions.SquareCells(3, 2))
                        .AddLayer(area => area.ShallowCopy(cells:
                            area.Select(cell =>
                                cell.Tags.Contains(Cell.CellTag.MazeTrail) ?
                                    new Cell(AreaType.Maze) :
                                    new Cell(AreaType.None))))
                        .OfMaze(MazeStructureStyle.Border, new GeneratorOptions() {
                                            MazeAlgorithm = GeneratorOptions.Algorithms.HuntAndKill
                                        })
                        .ToMap(Maze2DRendererOptions.SquareCells(1, 1))
                        .Map())
                .Map(),
            r => new GeneratedWorld(r)
                .AddLayer(AreaType.Maze, new Vector(3, 3))
                .OfMaze(MazeStructureStyle.Block, new GeneratorOptions() {
                            MazeAlgorithm = GeneratorOptions.Algorithms.HuntAndKill
                        })
                // .ToMap(Maze2DRendererOptions.SquareCells(5, 2))
                .Map()
            };
    }
}
