using System;
using System.Linq;
using CommandLine;
using PlayersWorlds.Maps.Areas;
using PlayersWorlds.Maps.Renderers;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps {
    [Verb("parse", HelpText = "Parse and render a maze from a string.")]
    class ParseCommand : BaseCommand {
        [Value(0, MetaName = "serialized maze", HelpText = "Serialized maze.")]
        public string SerializedMaze { get; set; }

        override public int Run() {
            base.Run();
            var maze = new AreaSerializer().Deserialize(SerializedMaze);
            var mazeCells = maze.Grid.Where(c => maze[c].HasLinks()).ToList();
            var areaSerializer = new AreaSerializer();
            Console.WriteLine(areaSerializer.Serialize(maze));
            Console.WriteLine(maze.ToString());
            Console.WriteLine($"Visited: " +
                mazeCells.Count());
            Console.WriteLine($"Area Cells: ");
            Console.WriteLine("  Fill ({0}): ({1})",
                maze.ChildAreas.Count(
                                a => a.Type == AreaType.Fill),
                maze.ChildAreas.Where(
                                a => a.Type == AreaType.Fill)
                             .Select(a => a.Grid.Size.Area).Sum());
            Console.WriteLine("  Cave ({0}): ({1}): ",
                maze.ChildAreas.Count(
                                a => a.Type == AreaType.Cave),
                maze.ChildAreas.Where(
                                a => a.Type == AreaType.Cave)
                             .Select(a => a.Grid.Size.Area).Sum());
            Console.WriteLine("  Hall ({0}): ({1}): ",
                maze.ChildAreas.Count(
                                a => a.Type == AreaType.Hall),
                maze.ChildAreas.Where(
                                a => a.Type == AreaType.Hall)
                             .Select(a => a.Grid.Size.Area).Sum());
            Console.WriteLine(
                "Unvisited cells: " +
                string.Join(",", maze.Grid
                    .Where(c => !maze[c].HasLinks())));
            Console.WriteLine(maze.Render(new AsciiRendererFactory()));
            return 0;
        }
    }
}
