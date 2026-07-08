using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.MapFilters {

    /// <summary>
    /// A <see cref="Area" /> filter that classifies every wall cell by the
    /// orientation of its flat face and distinguishes true corners from
    /// straight walls, wall-ends and T-junctions.
    /// </summary>
    /// <remarks>
    /// Run this as the LAST filter of the wall-build chain, after the
    /// geometry (which cells are wall vs trail) is final. It reads each wall
    /// cell's four orthogonal neighbours and tags it:
    /// <list type="bullet">
    /// <item>an opposite wall-neighbour pair (straight wall, or the flat run
    /// of a T-junction — the perpendicular branch is ignored) →
    /// <see cref="Cell.CellTag.MazeWallAxisX" /> / <c>MazeWallAxisY</c>;</item>
    /// <item>exactly one wall-neighbour (a wall-end / T-junction offspring) →
    /// the axis of that neighbour, so it textures as a wall continuing in that
    /// direction rather than as a special end;</item>
    /// <item>two perpendicular wall-neighbours with no through-line (a true
    /// L-corner) → <see cref="Cell.CellTag.MazeWallCorner" />;</item>
    /// <item>zero neighbours (island) or an enclosed cross/interior cell → no
    /// orientation tag (no visible flat face).</item>
    /// </list>
    /// The coarse <c>MazeWallCorner</c> seeded mid-chain by
    /// <see cref="Map2DSmoothCorners" /> (which cannot tell a corner from a
    /// wall-end) is stripped first and re-applied only to true corners.
    /// Out-of-bounds neighbours count as open.
    /// </remarks>
    public class Map2DClassifyWalls : Map2DFilter {

        /// <summary>
        /// Apply the filter to the specified <see cref="Area" />.
        /// </summary>
        /// <param name="map">The map to apply the filter to.</param>
        override public void Render(Area map) {
            var walls = new HashSet<Vector>(
                map.Grid.Where(xy =>
                    map[xy].Tags.Contains(Cell.CellTag.MazeWall) ||
                    map[xy].Tags.Contains(Cell.CellTag.MazeWallCorner)));

            bool IsWall(Vector xy) =>
                map.Grid.Contains(xy) && walls.Contains(xy);

            var assign = new Dictionary<Vector, Cell.CellTag>();
            foreach (var xy in walls) {
                var n = IsWall(xy + Vector.North2D);
                var s = IsWall(xy + Vector.South2D);
                var e = IsWall(xy + Vector.East2D);
                var w = IsWall(xy + Vector.West2D);
                var hasNS = n && s;
                var hasEW = e && w;
                var count = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);

                Cell.CellTag tag;
                if (hasNS && hasEW) tag = null;
                else if (hasNS) tag = Cell.CellTag.MazeWallAxisY;
                else if (hasEW) tag = Cell.CellTag.MazeWallAxisX;
                else if (count == 2) tag = Cell.CellTag.MazeWallCorner;
                else if (count == 1)
                    tag = (n || s) ? Cell.CellTag.MazeWallAxisY
                                   : Cell.CellTag.MazeWallAxisX;
                else tag = null;
                assign[xy] = tag;
            }

            foreach (var xy in walls)
                map[xy].Tags.Remove(Cell.CellTag.MazeWallCorner);
            foreach (var kv in assign) {
                if (kv.Value != null && !map[kv.Key].Tags.Contains(kv.Value))
                    map[kv.Key].Tags.Add(kv.Value);
            }
        }
    }
}
