using System;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// Selects the generation algorithm for a region. The blessed built-ins are
    /// discoverable as static members; a game can plug in its own algorithm via
    /// <see cref="Custom{T}"/> without the façade having to enumerate it — a
    /// closed set for discoverability, open for growth.
    /// </summary>
    /// <remarks>
    /// Algorithms are largely interchangeable — some bias toward winding mazes,
    /// others toward straighter corridors — so a <see cref="RegionRecipe"/>
    /// picks a sensible default per intent and you override it here only when
    /// you want a specific character.
    /// </remarks>
    public readonly struct RegionAlgorithm : IEquatable<RegionAlgorithm> {
        private readonly Type _generatorType;

        internal Type GeneratorType => _generatorType;

        private RegionAlgorithm(Type generatorType) {
            _generatorType = generatorType;
        }

        /// <summary>Recursive backtracker — long, winding corridors (default).
        /// </summary>
        public static RegionAlgorithm RecursiveBacktracker =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.RecursiveBacktracker);

        /// <summary>Aldous-Broder — uniform, unbiased spanning tree.</summary>
        public static RegionAlgorithm AldousBroder =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.AldousBroder);

        /// <summary>Hunt-and-kill — winding with fewer long dead ends.</summary>
        public static RegionAlgorithm HuntAndKill =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.HuntAndKill);

        /// <summary>Wilson's — uniform, unbiased spanning tree.</summary>
        public static RegionAlgorithm Wilsons =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.Wilsons);

        /// <summary>Sidewinder — horizontal bias, corridor-like.</summary>
        public static RegionAlgorithm Sidewinder =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.Sidewinder);

        /// <summary>Binary tree — strong diagonal bias, very open.</summary>
        public static RegionAlgorithm BinaryTree =>
            new RegionAlgorithm(GeneratorOptions.Algorithms.BinaryTree);

        /// <summary>
        /// Uses a custom generator. <typeparamref name="T"/> must derive from
        /// <see cref="MazeGenerator"/> and have a parameterless constructor.
        /// </summary>
        /// <typeparam name="T">A custom <see cref="MazeGenerator"/>.</typeparam>
        public static RegionAlgorithm Custom<T>() where T : MazeGenerator =>
            Custom(typeof(T));

        /// <summary>
        /// Uses a custom generator type. Must derive from
        /// <see cref="MazeGenerator"/> and have a parameterless constructor.
        /// </summary>
        /// <param name="generatorType">A <see cref="MazeGenerator"/> subtype.
        /// </param>
        public static RegionAlgorithm Custom(Type generatorType) {
            if (generatorType == null) {
                throw new ArgumentNullException(nameof(generatorType));
            }
            if (!typeof(MazeGenerator).IsAssignableFrom(generatorType) ||
                generatorType.IsAbstract) {
                throw new ArgumentException(
                    $"{generatorType.FullName} must be a concrete subtype of " +
                    nameof(MazeGenerator) + ".", nameof(generatorType));
            }
            if (generatorType.GetConstructor(Type.EmptyTypes) == null) {
                throw new ArgumentException(
                    $"{generatorType.FullName} must have a parameterless " +
                    "constructor.", nameof(generatorType));
            }
            return new RegionAlgorithm(generatorType);
        }

        /// <inheritdoc/>
        public bool Equals(RegionAlgorithm other) =>
            _generatorType == other._generatorType;

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is RegionAlgorithm other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            _generatorType?.GetHashCode() ?? 0;

        /// <inheritdoc/>
        public override string ToString() =>
            _generatorType?.Name ?? "<default>";
    }
}
