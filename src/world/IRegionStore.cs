namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// The persistence seam the game implements so the engine generates a
    /// region exactly once and thereafter reloads it. The engine owns the
    /// (lossless) serialization; the store only persists and retrieves opaque
    /// serialized blobs keyed by <see cref="RegionAddress"/> — a plain
    /// key/value or blob store on the game side.
    /// </summary>
    public interface IRegionStore {
        /// <summary>
        /// Tries to load a previously saved region.
        /// </summary>
        /// <param name="address">The region address.</param>
        /// <param name="serializedRegion">The serialized region blob if found;
        /// otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if a stored region was found; otherwise
        /// <c>false</c>.</returns>
        bool TryLoad(RegionAddress address, out string serializedRegion);

        /// <summary>
        /// Persists a region blob for later retrieval by
        /// <see cref="TryLoad"/>.
        /// </summary>
        /// <param name="address">The region address.</param>
        /// <param name="serializedRegion">The serialized region blob.</param>
        void Save(RegionAddress address, string serializedRegion);
    }

    /// <summary>
    /// A store that never has anything (always a miss) and never saves — so
    /// <see cref="World.GetOrCreate"/> regenerates every time and persists
    /// nothing. The default for a game that regenerates its single region at
    /// startup (mazzzze v1).
    /// </summary>
    public sealed class NullRegionStore : IRegionStore {
        /// <inheritdoc/>
        public bool TryLoad(RegionAddress address, out string serializedRegion) {
            serializedRegion = null;
            return false;
        }

        /// <inheritdoc/>
        public void Save(RegionAddress address, string serializedRegion) {
            // Intentionally does nothing: this store never persists.
        }
    }
}
