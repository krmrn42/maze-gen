namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// A request to auto-place <see cref="Count"/> rooms of a kind and size
    /// range. Internal — a <see cref="RegionRecipe"/> accumulates these and
    /// <see cref="World"/> distributes them across the region footprint.
    /// Sizes are in world (Block) cells; the generator snaps them to the
    /// underlying maze grid.
    /// </summary>
    internal sealed class RoomRequest {
        public int Count { get; }
        public Vector MinSize { get; }
        public Vector MaxSize { get; }
        public RoomKind Kind { get; }
        public string[] Tags { get; }

        public RoomRequest(int count, Vector minSize, Vector maxSize,
                           RoomKind kind, string[] tags) {
            Count = count;
            MinSize = minSize;
            MaxSize = maxSize;
            Kind = kind;
            Tags = tags;
        }
    }
}
