
namespace PlayersWorlds.Maps.Areas.Evolving {
    internal interface IForceFormula {
        double NormalForce(double distance);

        /// <summary>
        /// Gets force between colliding objects. 
        /// </summary>
        /// <param name="sign">Indicates the collision sign.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        /// <returns></returns>
        double CollideForce(double sign, double fragment);

        /// <summary>
        /// Gets force between overlapping objects. 
        /// </summary>
        /// <param name="distance">Indicates how much the objects overlap in system units.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        double OverlapForce(double distance, double fragment);

    }
}
