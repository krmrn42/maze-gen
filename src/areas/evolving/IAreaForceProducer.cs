
namespace PlayersWorlds.Maps.Areas.Evolving {
    internal interface IAreaForceProducer {
        VectorD GetAreaForce(
            FloatingArea area, FloatingArea other);
    }
}
