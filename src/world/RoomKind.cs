namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// The structural kind of a room placed in a region — the small, stable set
    /// of area types maze-gen supports as rooms. What a room is <i>for</i>
    /// (armory, boss, shrine, …) is an open-ended concern expressed with tags,
    /// not this enum.
    /// </summary>
    public enum RoomKind {
        /// <summary>A walled room with controlled entrances (maze
        /// <c>Hall</c>) — walkable open space carved into the maze.</summary>
        Hall,
        /// <summary>An organic room with any number of entrances (maze
        /// <c>Cave</c>) — walkable, less regular than a hall.</summary>
        Cave,
        /// <summary>An impassable obstacle — rock, water, a pit (maze
        /// <c>Fill</c>). The player cannot enter it; the maze routes
        /// around it.</summary>
        Blocked,
    }
}
