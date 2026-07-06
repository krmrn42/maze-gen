using System.Collections.Generic;
using System.Linq;
using PlayersWorlds.Maps.Areas.Evolving;

namespace PlayersWorlds.Maps.Areas {
    /// <summary>
    /// Implemented the AreaGenerator as a helper to generate areas for a map.
    /// </summary>
    public abstract class AreaGenerator {
        private readonly EvolvingSimulator _simulator;
        private readonly MapAreaSystemFactory _areaSystemFactory;

        protected virtual int MaxAttempts => 3;

        public AreaGenerator(EvolvingSimulator simulator,
                             MapAreaSystemFactory areaSystemFactory) {
            _simulator = simulator;
            _areaSystemFactory = areaSystemFactory;
        }

        /// <summary>
        /// When overridden in the deriving class, generates areas for a map of
        /// the specified size.
        /// </summary>
        /// <remarks>
        /// If pre-existing areas are specified, they should be counted to make
        /// sure there are enough and not too many areas in the maze.
        /// </remarks>
        /// <param name="targetArea">The map to generate areas for.</param>
        /// <returns>Areas to be added to the map.</returns>
        protected abstract IEnumerable<Area> Generate(Area targetArea);

        public void GenerateMazeAreas(Area targetArea) {
            // count existing (desired) placement errors we can ignore when
            // checking auto-generated areas.
            var existingErrors =
                targetArea.ChildAreas.Count(
                    area => area.IsPositionFixed &&
                            targetArea.ChildAreas.Any(other =>
                                area != other &&
                                other.IsPositionFixed &&
                                area.Grid.Overlaps(other.Grid))) +
                targetArea.ChildAreas.Count(
                    area => area.IsPositionFixed &&
                            !area.Grid.FitsInto(targetArea.Grid));
            var attempts = MaxAttempts;
            while (attempts > 0) {
                var allAreas = new List<Area>(targetArea.ChildAreas);
                // add more rooms
                //     AreaGenerator creates new rooms as a separate list
                // layout
                //     Tries to layout Area rooms with new rooms
                // if all worked out, stop.
                allAreas.AddRange(Generate(targetArea));
                if (allAreas.Any(a => !a.IsPositionFixed)) {
                    // When we auto-generate the areas, there is a <1% chance
                    // that we can't auto-distribute (see
                    // DirectedDistanceForceProducer.cs) so we make several
                    // attempts.
                    _simulator.ThrowIfNull("EvolvingSimulator");
                    _areaSystemFactory.ThrowIfNull("MapAreaSystemFactory");
                    _simulator.Evolve(
                        _areaSystemFactory.Create(targetArea,
                            targetArea.ChildAreas.Concat(allAreas)));
                }
                // problem is: how do we distribute the rooms w/o changing
                // the original room locations?
                // on the other hand, if we deep clone, how do we let the area
                // know which rooms are new to add?
                // in a perfect world, we would deep clone, try to layout, and
                // if it worked,
                //          a: return a new targetArea with the new layout
                //          b: add only new rooms to the original targetArea
                // FIXME: while distributing, we can't disturb the original
                //        layout.
                var errors = -existingErrors +
                    allAreas.Count(
                        area => allAreas.Any(other =>
                                    area != other &&
                                    area.Grid.Overlaps(other.Grid))) +
                    allAreas.Count(
                        area => !area.Grid.FitsInto(targetArea.Grid));
                if (errors <= 0) {
                    allAreas.Where(area => !targetArea.ChildAreas.Contains(area))
                            .ForEach(area => targetArea.AddChildArea(area));
                    return;
                } else if (--attempts == 0) {
                    var roomsDebugStr = string.Join(", ",
                        allAreas.Select(a => $"P{a.Position};S{a.Size}"));
                    var message =
                        $"Could not generate rooms for maze of size " +
                        $"{targetArea.Size}. Last set of rooms had {errors} " +
                        $"errors ({string.Join(" ", roomsDebugStr)}).";
                    throw new AreaGeneratorException(this, message);
                }
            }
        }
    }
}
