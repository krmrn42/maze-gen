
using System.Collections.Generic;

namespace PlayersWorlds.Maps.Areas.Evolving {
    public class MapAreaSystemFactory {
        private readonly RandomSource _random;

        public MapAreaSystemFactory(RandomSource random) {
            _random = random;
        }

        public MapAreasSystem Create(
            Area env,
            IEnumerable<Area> areas) {
            return new MapAreasSystem(_random, env, areas);
        }
    }
}
