
namespace PlayersWorlds.Maps.Areas.Evolving {
    internal interface IEnvironmentForceProducer {
        VectorD GetEnvironmentForce(
            FloatingArea area, Vector environmentSize);
    }
}
