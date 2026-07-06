using System.Collections.Generic;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// An opening on a region's border: the passable cells lying on one border
    /// face of the region. Gates are the future seam anchors — the cells a
    /// neighbouring region will stitch onto.
    /// </summary>
    /// <remarks>
    /// A border face is identified N-dimensionally by an axis
    /// (<see cref="Dimension"/>) and which side of that axis it sits on
    /// (<see cref="AtFarSide"/>). Phase 0 <i>surfaces</i> the gates a region
    /// happens to have (the passable cells already on its border); gate-aware
    /// <i>generation</i> — carving a region so its gates line up with a
    /// neighbour's — is a later phase behind this same shape.
    /// </remarks>
    public readonly struct Gate {
        private readonly Vector[] _openCells;

        /// <summary>
        /// The axis of the border face this gate is on (0 = x, 1 = y, …).
        /// </summary>
        public int Dimension { get; }

        /// <summary>
        /// <c>false</c> for the low side of the axis (local coordinate 0),
        /// <c>true</c> for the high side (local coordinate <c>size - 1</c>).
        /// </summary>
        public bool AtFarSide { get; }

        /// <summary>
        /// The passable region-local cells lying on this border face.
        /// </summary>
        public IReadOnlyList<Vector> OpenCells => _openCells;

        internal Gate(int dimension, bool atFarSide, Vector[] openCells) {
            Dimension = dimension;
            AtFarSide = atFarSide;
            _openCells = openCells;
        }
    }
}
