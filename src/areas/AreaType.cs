
namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Defines if a given area is a regular maze (the default), a joint .
    /// </summary>
    /// <remarks>
    /// Value is the priority. When combined across different layers, the type
    /// with the highest priority will be used.
    /// </remarks>
    public enum AreaType {
        /// <summary>
        /// Same as <see cref="Maze"/>.
        /// </summary>
        None = 0,
        /// <summary>
        /// Describes an environment within this area.
        /// </summary>
        Environment = 1,
        /// <summary>
        /// A regular area that might have different tags to denote any
        /// specific purpose like styling or activity, but does not limit
        /// procedural generation algorithms.
        /// </summary>
        Maze = 2,
        /// <summary>
        /// A hall with walls around it and one or two entrances.
        /// </summary>
        Hall = 3,
        /// <summary>
        /// Cave is similar to <see cref="Hall" /> but with no entrance
        /// placement rules, making any number of entrances.
        /// </summary>
        Cave = 4,
        /// <summary>
        /// An area the player cannot enter, e.g., a lake or a rock.
        /// </summary>
        Fill = 5,
    }
}
