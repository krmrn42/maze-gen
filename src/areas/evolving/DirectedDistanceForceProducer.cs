using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayersWorlds.Maps.Areas.Evolving {
    internal class DirectedDistanceForceProducer :
        IAreaForceProducer, IEnvironmentForceProducer, IForceFormula {
        private readonly Log _log = Log.ToConsole<DirectedDistanceForceProducer>();
        private readonly IForceFormula _forceFormula;
        private readonly RandomSource _random;
        private readonly double _overlapFactor;
        private readonly Dictionary<(FloatingArea, FloatingArea), VectorD>
            _opposingForces = new Dictionary<(FloatingArea, FloatingArea), VectorD>();

        public DirectedDistanceForceProducer(RandomSource random, double overlapFactor) {
            _forceFormula = this;
            _random = random;
            _overlapFactor = overlapFactor;
        }

        public VectorD GetAreaForce(FloatingArea area, FloatingArea other) {
            // !! We can't rely on just a center-to-center vector to determine
            // !! the distance between two areas. E.g., consider the following
            // !! example:
            // !!            ┌─┐
            // !!            │B│
            // !!            │ │
            // !!            │*│
            // !!            │ │
            // !! ┌──────────┼─┼┐
            // !! │    A *   └─┘│
            // !! └─────────────┘
            // !! The A(center)->B(center) vector shows a large distance which
            // !! is not the case.
            // !! In case of an overlap, we can take a vector from the center of
            // !! area A to the center of the overlap.
            // !! At the same time, if the box is fully contained by another box,
            // !! cropping will result in competing between two similar boxes, which
            // !! is less effective than we can do, e.g., we can check the
            // !! centers to not  be inside the other box.
            // !! But if the areas don't overlap, measuring the distance using
            // !! vectors is still not optimal. Consider this example:
            // !! ║     ┌─────────────────────────┐───┐         ║                                         
            // !! ║     │            *         B` │B  │         ║                                         
            // !! ║     └─────────S2.00x13.00─────┘   │         ║                                         
            // !! ║                            A` │A  │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                            S12.00x2.00      ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               │   │         ║                                         
            // !! ║                               └───┘         ║ 
            // !! The vector distance is large, but the actual distance is
            // !! close to 0, so there has to be a force pushing away.
            // !! We can fix this by measuring the distance between sides on one
            // !! axis. This is not optimal as well, consider this example:
            // !! ╔════════════════════════════════════════════════════════════════════════════════════════╗                                                 
            // !! ║                       ┌─────────────────┐   ┌─────────────────┐                        ║                                                 
            // !! ║                       └─────────────────┘───────────┐─────────┘                        ║                                                 
            // !! ║                           └─────────────────────────┘                                  ║                                                 
            // !! ╚════════════════════════════════════════════════════════════════════════════════════════╝ 
            // !! if the areas are pushed away taking only the vertical distance,
            // !! this layout cannot be enhanced. On the other hand, vector
            // !! distance would allow spreading the areas wider in the env.

            // !! Now we still need to deal with perfectly opposing areas that
            // !! also don't fit the map, e.g.:
            // !!  ╔═══════════════════╗         
            // !!  ║                   ║         
            // !!  ║                   ║         
            // !!┌───────────┐───────────┐       
            // !!LION        │EAR      ║ │       
            // !!│ ║         │         ║ │       
            // !!└───────────┘───────────┘       
            // !!  ║                   ║         
            // !!  ║                   ║         
            // !!  ║                   ║         
            // !!  ╚═══════════════════╝         


            // direction of the force to be applied to this area.
            var direction = area.Center - other.Center;
            // distance between the areas to calculate the force magnitude.
            var (distance, overlap) = area.DistanceTo(other);
            VectorD force;
            // overlap means there is some common area between the two areas.
            if (overlap) {
                // if the areas overlap and the distance is 0,
                // it means that the area centers match.
                if (distance.MagnitudeSq < VectorD.MIN) {
                    // if there is a stored force in cache, it means we have 
                    // already calculated this force while processing the other
                    // area. So we just use it and cleanup the cache.
                    if (_opposingForces.TryGetValue((area, other),
                                                    out var oppositeForce)) {
                        force = oppositeForce;
                        _opposingForces.Remove((area, other));
                    } else {
                        // if the centers match, we will explode the areas in
                        // random opposite directions. So:
                        // 1. target distance is the size of the intersection
                        distance = area.Intersection(other).Size;
                        // 2. direction is random, but it has to be exactly
                        //    opposite between these to areas.
                        direction = VectorD.RandomUnit(_random,
                            area.Position.Dimensions);
                        force = direction.WithMagnitude(
                            _forceFormula.OverlapForce(
                                distance.Magnitude, _overlapFactor));
                        _opposingForces.Add((other, area), force.Reverse());
                    }
                } else {
                    force = direction.WithMagnitude(
                        _forceFormula.OverlapForce(
                            distance.Magnitude, _overlapFactor));
                }
            } else {
                force = direction.WithMagnitude(_forceFormula.NormalForce(distance.Magnitude));
            }
            _log.D(5, $"GetAreaForce ({area.Nickname} {area}) ({other.Nickname} {other}): direction={direction},distance={distance},overlap={overlap},force={force}");
            return force;
        }

        public VectorD GetEnvironmentForce(FloatingArea area, Vector environmentSize) {
            // direction is always to the center of the area
            // the area touches env edge from the inside, force = 1/0.1
            // the area is somewhere inside the env, force = 1/distance
            // the area is outside of the env (crosses it's edge), force = (distance + 1) / timeBoost

            double OneForce(double distance, double overlapSign) {
                if (Math.Sign(distance) == Math.Sign(overlapSign)) {
                    return _forceFormula.OverlapForce(-distance, _overlapFactor);
                } else {
                    return _forceFormula.NormalForce(distance);
                }
            }

            var xTop = OneForce(area.HighX - environmentSize.X, 1);
            var xBottom = OneForce(area.LowX, -1);
            var yTop = OneForce(area.HighY - environmentSize.Y, 1);
            var yBottom = OneForce(area.LowY, -1);

            var force = new VectorD(xTop + xBottom, yTop + yBottom);
            _log.D(5, $"GetEnvironmentForce ({area.Nickname}, {area}, {environmentSize}): xTop={xTop} xBottom={xBottom} yTop={yTop} yBottom={yBottom} area.LowX={area.LowX} sign={Math.Sign(area.LowX)} _forceFormula.NormalForce(0)={_forceFormula.NormalForce(0)} force={force}");
            return force;
        }

        public double NormalForce(double distance) {
            double F(double x) {
                return x >= 3 ? 0 : x <= VectorD.MIN ? 3 : 3 - x;
            }
            var absDistance = Math.Abs(distance);
            return F(Math.Abs(distance)) * (absDistance < VectorD.MIN ? 1D : Math.Sign(distance));
        }

        public double CollideForce(double sign, double fragment) {
            throw new NotImplementedException();
        }

        public double OverlapForce(double distance, double fragment) {
            // return (distance + NormalForce(Math.Sign(distance))) * 2;
            return 3 * (distance + NormalForce(Math.Sign(distance)));
        }
    }
}
