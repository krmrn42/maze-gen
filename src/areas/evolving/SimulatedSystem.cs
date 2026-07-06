namespace PlayersWorlds.Maps.Areas.Evolving {
    /// <summary>
    /// An implementing class is a system that can evolve under it's internal
    /// forces.
    /// </summary>
    public abstract class SimulatedSystem {
        /// <summary>
        /// When overridden, evolves the system using its internal forces to
        /// <paramref name="fragment" /> portion of the forces.
        /// </summary>
        /// <param name="fragment">The portion of the impacting factor to
        /// apply</param>
        /// <returns></returns>
        public abstract GenerationImpact Evolve(double fragment);

        /// <summary>
        /// Compares this epoch result to previous epochs results to determines
        /// if the evolution of the system has converged.
        /// </summary>
        /// <param name="previousEpochsResults">The impact of previous epochs.
        /// </param>
        /// <param name="thisEpochGenerationsImpacts">This generation impact.
        /// </param>
        /// <returns></returns>
        public abstract EpochResult CompleteEpoch(
            EpochResult[] previousEpochsResults,
            GenerationImpact[] thisEpochGenerationsImpacts);
    }

    /// <summary>
    /// An impact produced by a <see cref="SimulatedSystem" /> in one
    /// generation, which is 1/fragment of an epoch.
    /// </summary>
    public abstract class GenerationImpact { }
    /// <summary>
    /// The result of an evolution of a <see cref="SimulatedSystem" /> in one
    /// epoch.
    /// </summary>
    public class EpochResult {
        /// <summary>
        /// <c>true</c> when the <see cref="SimulatedSystem" /> concludes, i.e.,
        /// the further evolution would be insignificant; otherwise, <c>false
        /// </c>.
        /// </summary>
        public bool CompleteEvolution { get; set; }
    }
}
