using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Generate areas randomly for a map of the provided size using simple
    /// backtracking.
    /// </summary>
    public class BasicAreaGenerator : AreaGenerator {
        private readonly RandomSource _randomSource;
        private readonly Area _targetArea;
        private readonly AreaType[] _areaTypes;
        private readonly string[] _tags;
        private readonly int _count;
        private readonly Vector _minSize;
        private readonly Vector _maxSize;
        private readonly IEnumerable<Area> _areasNoOverlap;

        /// <summary>
        /// Creates a new random area generator with the specified settings.
        /// </summary>
        /// <param name="randomSource">The source of randomness to be used by
        /// the generator.</param>
        /// <param name="targetArea">The area that will own the generated areas.
        /// </param>
        /// <param name="areaTypes">An array of area types that can be
        /// generated.</param>
        /// <param name="tags">An array of tags that can be assigned to the
        /// generated areas.</param>
        /// <param name="count">The target number of areas to generate.
        /// </param>
        /// <param name="minSize">The minimum size of any generated area.
        /// </param>
        /// <param name="maxSize">The maximum size of any generated area.
        /// </param>
        /// <param name="areasNoOverlap">Any other areas the new areas should
        /// not overlap.</param>
        public BasicAreaGenerator(RandomSource randomSource,
                                  Area targetArea,
                                  AreaType[] areaTypes,
                                  string[] tags,
                                  int count,
                                  Vector minSize,
                                  Vector maxSize,
                                  IEnumerable<Area> areasNoOverlap) :
            base(null, null) {
            _randomSource = randomSource;
            _targetArea = targetArea;
            _areaTypes = areaTypes;
            _tags = tags;
            _count = count;
            _minSize = minSize;
            _maxSize = maxSize;
            _areasNoOverlap = areasNoOverlap ?? new List<Area>();
        }

        protected override IEnumerable<Area> Generate(Area targetArea) {
            if (!_minSize.FitsInto(_targetArea.Size)) {
                // none of the areas fit the map
                return new List<Area>();
            }
            return PlaceArea(new List<Area>(targetArea.ChildAreas));
        }

        private ICollection<Area> PlaceArea(ICollection<Area> generatedAreas) {
            if (generatedAreas.Count == _count) return generatedAreas;
            var maxTries = 100;
            while (maxTries > 0) {
                var area = CreateRandomArea(generatedAreas);
                if (area != null) {
                    return PlaceArea(new List<Area>(generatedAreas) { area });
                }
                maxTries--;
            }
            return generatedAreas;
        }

        private Area CreateRandomArea(ICollection<Area> generatedAreas) {
            var type = _randomSource.RandomOf(_areaTypes);
            Area area;
            var size = new Vector(
                _randomSource.Next(_minSize.X, _maxSize.X),
                _randomSource.Next(_minSize.Y, _maxSize.Y));
            var pos = new Vector(
                _randomSource.Next(0, _targetArea.Size.X - size.X),
                _randomSource.Next(0, _targetArea.Size.Y - size.Y));
            // Tags are optional; only assign one when the caller supplied some.
            area = _tags.Length == 0
                ? Area.Create(pos, size, type)
                : Area.Create(pos, size, type, _randomSource.RandomOf(_tags));
            if (IsAValidLayout(generatedAreas, area)) {
                return area;
            }
            return null;
        }

        private bool IsAValidLayout(ICollection<Area> generatedAreas,
                                    Area newArea) =>
            !generatedAreas
                    .Where(area => newArea.Grid.Overlaps(area.Grid)).Any()
            && !_areasNoOverlap
                    .Where(area => newArea.Grid.Overlaps(area.Grid)).Any()
            && newArea.Grid.FitsInto(_targetArea.Grid);
    }
}
