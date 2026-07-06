using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    /// <summary>
    /// Evolves a <see cref="SimulatedSystem" /> over time using its own
    /// internal impact over time.
    /// </summary>
    public class EvolvingSimulator {
        private readonly int _epochs;
        private readonly int _generationsPerEpoch;

        /// <summary>
        /// Creates an instance of EvolvingSimulator with the given max number of epochs
        /// and given number of generations per epoch.
        /// </summary>
        /// <param name="maxEpochs">Max number of epochs in this simulation</param>
        /// <param name="generationsPerEpoch">Number of generations in each
        /// epoch</param>
        public EvolvingSimulator(int maxEpochs, int generationsPerEpoch) {
            if (maxEpochs < 1) {
                throw new ArgumentException($"Number of epochs must be greater than 0 (provided {maxEpochs})", "maxEpochs");
            }
            _epochs = maxEpochs;
            _generationsPerEpoch = generationsPerEpoch;
        }

        /// <summary>
        /// Evolves the given system over time.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The evolution happens in epochs, and each epoch can produce a total
        /// of epoch_impact.
        /// Epochs consist of generations, and each generation produces
        /// epoch_impact / _generationsPerEpoch impact.
        /// We evaluate epoch_impact to see if it's significant. If not, we
        /// conclude the evolution because we don't expect any more significant
        /// impact in the same system.
        /// </p><p>
        /// The point of this type of simulation is that we can't apply a full
        /// analogue impact in a discreet system immediately, because analogue
        /// impact changes along with the system changes. I.e., if we take all
        /// forces that apply to all objects and apply them at the same time,
        /// the overall impact will not account for forces changes that would
        /// have happened if the objects would naturally move under the impact.
        /// </p>
        /// </remarks>
        /// <param name="system">The system to evolve.</param>
        /// <returns></returns>
        public virtual int Evolve(SimulatedSystem system) {
            var epochResults = new List<EpochResult>();
            for (var e = 0; e < _epochs; e++) {
                var impact = Enumerable.Range(0, _generationsPerEpoch)
                    .Select(gen => system.Evolve(1D / _generationsPerEpoch));
                var epochResult = system.CompleteEpoch(
                    epochResults.ToArray(), impact.ToArray());
                epochResults.Add(epochResult);
                if (epochResult.CompleteEvolution) {
                    return e;
                }
            }
            return _epochs;
        }
    }
}
