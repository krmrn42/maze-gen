using System;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// Block cell tags the façade bakes onto region cells so that a region is
    /// fully self-describing — its points of interest survive a lossless
    /// <see cref="Serializer.AreaSerializer"/> round-trip as cell tags, with no
    /// separate side-channel and no need to keep the underlying maze around.
    /// </summary>
    internal static class RegionTags {
        internal const string Entrance = "REGION_ENTRANCE";
        internal const string Exit = "REGION_EXIT";
        internal const string DeadEnd = "REGION_DEADEND";

        internal static string For(PoiKind kind) => kind switch {
            PoiKind.Entrance => Entrance,
            PoiKind.Exit => Exit,
            PoiKind.DeadEnd => DeadEnd,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
