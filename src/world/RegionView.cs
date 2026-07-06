using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// A read-only view of a single generated region: Block-style cells, its
    /// place on the region lattice, its points of interest, and its border
    /// gates. This is the frozen library contract a game codes against — no
    /// <see cref="Area"/>, <see cref="Cell"/>, or other internal machinery is
    /// reachable through it.
    /// </summary>
    public sealed class RegionView {
        private readonly Area _region;
        private readonly Poi[] _pois;
        private readonly Gate[] _gates;

        /// <summary>
        /// This region's place on the region lattice.
        /// </summary>
        public RegionAddress Address { get; }

        /// <summary>
        /// The region's size in Block cells.
        /// </summary>
        public Vector Size => _region.Size;

        /// <summary>
        /// The region's points of interest (entrance/exit and dead-ends), in
        /// region-local (Block) coordinates.
        /// </summary>
        public IReadOnlyList<Poi> Pois => _pois;

        /// <summary>
        /// The openings on this region's borders (the future seam anchors).
        /// </summary>
        public IReadOnlyList<Gate> Gates => _gates;

        /// <summary>
        /// Whether <paramref name="local"/> is a valid cell coordinate within
        /// this region.
        /// </summary>
        public bool Contains(Vector local) => _region.Grid.Contains(local);

        /// <summary>
        /// The cell payload at a region-local coordinate.
        /// </summary>
        /// <param name="local">A region-local (Block) cell coordinate.</param>
        /// <returns>The read-only <see cref="RegionCell"/> at that coordinate.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">The coordinate is
        /// outside the region bounds.</exception>
        public RegionCell CellAt(Vector local) {
            var cell = _region[local];
            var tags = cell.Tags.Select(t => t.ToString()).Distinct().ToArray();
            return new RegionCell(IsPassable(cell), cell.AreaType, tags);
        }

        /// <summary>
        /// Maps a region-local coordinate to its absolute world coordinate.
        /// </summary>
        public Vector ToWorld(Vector local) => Address.ToWorld(Size, local);

        internal RegionView(RegionAddress address, Area region) {
            Address = address;
            _region = region;
            _pois = ComputePois(region);
            _gates = ComputeGates(region);
        }

        private static bool IsPassable(Cell cell) =>
            cell.Tags.Any(t => t.Equals(Cell.CellTag.MazeTrail));

        private static Poi[] ComputePois(Area region) {
            var pois = new List<Poi>();
            foreach (var v in region.Grid) {
                foreach (var tag in region[v].Tags) {
                    if (tag.Equals(RegionTags.Entrance)) {
                        pois.Add(new Poi(PoiKind.Entrance, v));
                    } else if (tag.Equals(RegionTags.Exit)) {
                        pois.Add(new Poi(PoiKind.Exit, v));
                    } else if (tag.Equals(RegionTags.DeadEnd)) {
                        pois.Add(new Poi(PoiKind.DeadEnd, v));
                    }
                }
            }
            return pois.ToArray();
        }

        private static Gate[] ComputeGates(Area region) {
            var size = region.Size;
            var gates = new List<Gate>();
            for (var d = 0; d < size.Dimensions; d++) {
                foreach (var atFarSide in new[] { false, true }) {
                    var border = atFarSide ? size.Value[d] - 1 : 0;
                    var open = region.Grid
                        .Where(v => v.Value[d] == border)
                        .Where(v => IsPassable(region[v]))
                        .ToArray();
                    if (open.Length > 0) {
                        gates.Add(new Gate(d, atFarSide, open));
                    }
                }
            }
            return gates.ToArray();
        }
    }
}
