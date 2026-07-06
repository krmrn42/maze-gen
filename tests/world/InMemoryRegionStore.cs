using System.Collections.Generic;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// A simple recording <see cref="IRegionStore"/> for tests: keeps regions
    /// in a dictionary and counts saves so a test can assert generate-once.
    /// </summary>
    internal class InMemoryRegionStore : IRegionStore {
        private readonly Dictionary<RegionAddress, string> _blobs =
            new Dictionary<RegionAddress, string>();

        public int SaveCount { get; private set; }
        public int Count => _blobs.Count;

        public bool TryLoad(RegionAddress address, out string serializedRegion) =>
            _blobs.TryGetValue(address, out serializedRegion);

        public void Save(RegionAddress address, string serializedRegion) {
            SaveCount++;
            _blobs[address] = serializedRegion;
        }
    }
}
