using System;
using System.Linq;

namespace PlayersWorlds.Maps.World {
    /// <summary>
    /// A region's place on the region lattice: an N-dimensional integer
    /// coordinate naming which region this is, independent of the region's
    /// internal cell coordinates.
    /// </summary>
    /// <remarks>
    /// Stable identity of a game object is the pair
    /// <c>(RegionAddress, localCell)</c>. World (absolute) cell coordinates are
    /// <i>derived</i> via <see cref="ToWorld"/> / <see cref="FromWorld"/> so a
    /// region can unload and reload without the game re-keying anything — the
    /// endless-world requirement. The address space is unbounded; there is no
    /// fixed grid.
    /// </remarks>
    public readonly struct RegionAddress : IEquatable<RegionAddress> {
        private readonly Vector _value;

        /// <summary>
        /// The lattice coordinate of this region.
        /// </summary>
        public Vector Value => _value;

        /// <summary>
        /// Number of dimensions of the region lattice.
        /// </summary>
        public int Dimensions => _value.Dimensions;

        /// <summary>
        /// Creates a region address from a lattice coordinate.
        /// </summary>
        /// <param name="value">The lattice coordinate (must be non-empty).
        /// </param>
        public RegionAddress(Vector value) {
            if (value.IsEmpty) {
                throw new ArgumentException(
                    "RegionAddress requires a non-empty coordinate.",
                    nameof(value));
            }
            _value = value;
        }

        /// <summary>
        /// Maps a region-local cell coordinate to an absolute world cell
        /// coordinate, assuming a uniform lattice pitch of
        /// <paramref name="regionSize"/> (the region's block size).
        /// </summary>
        /// <param name="regionSize">The block size of a region (the lattice
        /// pitch).</param>
        /// <param name="local">A region-local cell coordinate.</param>
        /// <returns>The absolute world cell coordinate.</returns>
        public Vector ToWorld(Vector regionSize, Vector local) {
            var address = _value;
            if (regionSize.Dimensions != address.Dimensions ||
                local.Dimensions != address.Dimensions) {
                throw new ArgumentException(
                    "Region size, local coordinate, and address must share " +
                    "the same number of dimensions.");
            }
            return new Vector(Enumerable.Range(0, address.Dimensions)
                .Select(i => address.Value[i] * regionSize.Value[i] +
                             local.Value[i]));
        }

        /// <summary>
        /// Splits an absolute world cell coordinate into the region that owns
        /// it and the region-local coordinate within it, assuming a uniform
        /// lattice pitch of <paramref name="regionSize"/>.
        /// </summary>
        /// <param name="world">An absolute world cell coordinate.</param>
        /// <param name="regionSize">The block size of a region (the lattice
        /// pitch).</param>
        /// <returns>The owning region address and the local coordinate, such
        /// that <c>address.ToWorld(regionSize, local) == world</c>.</returns>
        public static (RegionAddress Address, Vector Local) FromWorld(
            Vector world, Vector regionSize) {
            if (world.Dimensions != regionSize.Dimensions) {
                throw new ArgumentException(
                    "World coordinate and region size must share the same " +
                    "number of dimensions.");
            }
            var address = new int[world.Dimensions];
            var local = new int[world.Dimensions];
            for (var i = 0; i < world.Dimensions; i++) {
                var size = regionSize.Value[i];
                // Floored division so negative world coordinates map to the
                // region below/left with a non-negative local remainder.
                var a = (int)Math.Floor((double)world.Value[i] / size);
                address[i] = a;
                local[i] = world.Value[i] - a * size;
            }
            return (new RegionAddress(new Vector(address)), new Vector(local));
        }

        /// <inheritdoc/>
        public bool Equals(RegionAddress other) => _value == other._value;

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is RegionAddress other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => _value.GetHashCode();

        /// <summary>Equality operator.</summary>
        public static bool operator ==(RegionAddress one, RegionAddress other) =>
            one.Equals(other);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(RegionAddress one, RegionAddress other) =>
            !one.Equals(other);

        /// <inheritdoc/>
        public override string ToString() => $"Region[{_value}]";
    }
}
