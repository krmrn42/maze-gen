using System;
using System.Collections.Generic;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// The read-only payload of a single region cell, in Block style (walls
    /// occupy their own cells). This is a value snapshot: no <see cref="Area"/>
    /// or <see cref="Cell"/> object is exposed through it.
    /// </summary>
    /// <remarks>
    /// In the current Block output passability is carried by cell
    /// <see cref="Tags"/> (a floor cell is tagged as a trail; a wall cell is
    /// tagged as a wall), which is why <see cref="IsPassable"/> is the primary
    /// signal a renderer needs. <see cref="Type"/> is surfaced for forward
    /// compatibility; today the Block conversion reports
    /// <see cref="AreaType.Environment"/> uniformly, and per-cell room/cave
    /// typing is a later-phase enhancement behind this same surface.
    /// </remarks>
    public readonly struct RegionCell {
        private readonly string[] _tags;

        /// <summary>
        /// Whether the player can occupy this cell (floor) as opposed to it
        /// being a wall.
        /// </summary>
        public bool IsPassable { get; }

        /// <summary>
        /// The area type this cell belongs to.
        /// </summary>
        public AreaType Type { get; }

        /// <summary>
        /// The cell's tags, usable by the game engine to pick tiles, styles, or
        /// behaviours.
        /// </summary>
        public IReadOnlyList<string> Tags => _tags ?? Array.Empty<string>();

        internal RegionCell(bool isPassable, AreaType type, string[] tags) {
            IsPassable = isPassable;
            Type = type;
            _tags = tags;
        }
    }
}
