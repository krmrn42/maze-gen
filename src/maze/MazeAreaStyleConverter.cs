using System;
using PlayersWorlds.Maps.MapFilters;
using static PlayersWorlds.Maps.Maze.Maze2DRenderer;

namespace PlayersWorlds.Maps.Maze {
    /// <summary>
    /// Hunt-and-kill algorithm implementation.
    /// </summary>
    public class MazeAreaStyleConverter {
        public Area ConvertMazeBorderToBlock(
            Area mazeArea,
            Area targetArea = null,
            Maze2DRendererOptions options = null) {
            if (mazeArea.X<Maze2DBuilder>() == null) {
                throw new InvalidOperationException(
                    "Can't convert non Block style maze Areas.");
            }
            targetArea = targetArea ??
                Maze2DRenderer.CreateMapForMaze(mazeArea, options);
            new Maze2DRenderer(mazeArea, options)
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail },
                                        Cell.CellTag.MazeWall,
                                        options.WallCellSize))
                .With(new Map2DSmoothCorners(Cell.CellTag.MazeTrail,
                                                Cell.CellTag.MazeWallCorner,
                                                options.WallCellSize))
                .With(new Map2DOutline(new[] { Cell.CellTag.MazeTrail,
                                                Cell.CellTag.MazeWallCorner },
                                        Cell.CellTag.MazeWall,
                                        options.WallCellSize))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeVoid },
                                            includeVoids: true,
                                            Cell.CellTag.MazeWall,
                                            maxSpotWidth: 5,
                                            maxSpotHeight: 5))
                .With(new Map2DEraseSpots(new[] { Cell.CellTag.MazeWall,
                                                    Cell.CellTag.MazeWallCorner },
                                            includeVoids: false,
                                            Cell.CellTag.MazeTrail,
                                            maxSpotWidth: 3,
                                            maxSpotHeight: 3))
                .Render(targetArea);
            return targetArea;
        }
    }
}