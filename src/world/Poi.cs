namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// The kind of a <see cref="Poi"/> (point of interest).
    /// </summary>
    public enum PoiKind {
        /// <summary>
        /// The region entrance — one end of the region's longest path. A good
        /// default player spawn.
        /// </summary>
        Entrance,
        /// <summary>
        /// The region exit — the other end of the region's longest path. A good
        /// default level goal.
        /// </summary>
        Exit,
        /// <summary>
        /// A dead-end cell — a corridor tip with a single passage. Useful for
        /// placing loot, secrets, or encounters.
        /// </summary>
        DeadEnd,
    }

    /// <summary>
    /// A point of interest inside a region, addressed by a region-local
    /// (Block) cell coordinate.
    /// </summary>
    /// <remarks>
    /// POIs are computed on the underlying maze and bridged into Block
    /// coordinates by the façade, so a consumer can place content directly on
    /// the same cell coordinates it renders (see
    /// <see cref="RegionView.CellAt"/>).
    /// </remarks>
    public readonly struct Poi {
        /// <summary>
        /// What this point of interest represents.
        /// </summary>
        public PoiKind Kind { get; }

        /// <summary>
        /// The region-local (Block) cell coordinate of this point of interest.
        /// </summary>
        public Vector Local { get; }

        internal Poi(PoiKind kind, Vector local) {
            Kind = kind;
            Local = local;
        }
    }
}
