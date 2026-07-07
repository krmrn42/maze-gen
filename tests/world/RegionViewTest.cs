using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PlayersWorlds.Maps.Areas;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionViewTest {
        private static readonly RegionAddress Origin =
            new RegionAddress(new Vector(0, 0));

        private static RegionView Generate(int seed) =>
            new World(new NullRegionStore(), seed, new Vector(6, 6))
                .GetOrCreate(Origin);

        [Test]
        public void CellAt_ReportsPassabilityAndTags() {
            var region = Generate(12345);
            var entrance = region.Pois.Single(p => p.Kind == PoiKind.Entrance);
            var cell = region.CellAt(entrance.Local);
            Assert.That(cell.IsPassable, Is.True);
            Assert.That(cell.Type, Is.EqualTo(AreaType.Environment));
            Assert.That(cell.Tags, Does.Contain(RegionTags.Entrance));
        }

        [Test]
        public void Contains_AndCellAt_RespectBounds() {
            var region = Generate(1);
            Assert.That(region.Contains(new Vector(0, 0)), Is.True);
            Assert.That(region.Contains(region.Size), Is.False);
            Assert.That(() => region.CellAt(region.Size),
                Throws.InstanceOf<IndexOutOfRangeException>());
        }

        [Test]
        public void Pois_HaveDistinctEntranceAndExit_AndOptionalDeadEnds() {
            var region = Generate(777);
            Assert.That(region.Pois.Count(p => p.Kind == PoiKind.Entrance),
                Is.EqualTo(1));
            Assert.That(region.Pois.Count(p => p.Kind == PoiKind.Exit),
                Is.EqualTo(1));
            var entrance = region.Pois.Single(p => p.Kind == PoiKind.Entrance);
            var exit = region.Pois.Single(p => p.Kind == PoiKind.Exit);
            Assert.That(entrance.Local, Is.Not.EqualTo(exit.Local));
            // Dead ends never coincide with the entrance/exit cells.
            var endpoints = new[] { entrance.Local, exit.Local };
            foreach (var deadEnd in region.Pois.Where(p => p.Kind == PoiKind.DeadEnd)) {
                Assert.That(endpoints, Does.Not.Contain(deadEnd.Local));
            }
        }

        [Test]
        public void ToWorld_OffsetsByAddress() {
            var region = new World(new NullRegionStore(), 5, new Vector(6, 6))
                .GetOrCreate(new RegionAddress(new Vector(2, 3)));
            var local = new Vector(1, 1);
            Assert.That(region.ToWorld(local),
                Is.EqualTo(new Vector(2 * region.Size.X + 1,
                                      3 * region.Size.Y + 1)));
        }

        [Test]
        public void Gates_SurfacePassableBorderCells() {
            // A normal closed region has a fully walled border, so build a small
            // synthetic region with one passable cell on the x==0 border to
            // exercise gate detection directly.
            var area = Area.Create(new Vector(0, 0), new Vector(4, 4),
                AreaType.Environment);
            area[new Vector(0, 1)].Tags.Add(Cell.CellTag.MazeTrail);
            var region = new RegionView(Origin, area);

            Assert.That(region.Gates.Count, Is.EqualTo(1));
            var gate = region.Gates.Single();
            Assert.That(gate.Dimension, Is.EqualTo(0));
            Assert.That(gate.AtFarSide, Is.False);
            Assert.That(gate.OpenCells, Does.Contain(new Vector(0, 1)));
        }

        [Test]
        public void GeneratedRegion_IsAClosedBox_WithNoGates() {
            // Documents the Phase-0 reality: regions are closed; gate-aware
            // generation (openings that line up with neighbours) is Phase 2.
            var region = Generate(31);
            Assert.That(region.Gates, Is.Empty);
        }

        // Contract test (task 2.7): nothing on the façade's public surface
        // leaks Area / Cell / ExtensibleObject / Grid.
        [Test]
        public void PublicSurface_LeaksNoInternalTypes() {
            var forbidden = new HashSet<Type> {
                typeof(Area), typeof(Cell), typeof(ExtensibleObject),
                typeof(Grid), typeof(Cell.CellTag),
                // No renderer/rendering-option types in the game contract.
                typeof(Maze.Maze2DRenderer),
                typeof(Maze.Maze2DRenderer.Maze2DRendererOptions),
            };

            bool IsForbidden(Type t) {
                if (t == null) return false;
                if (t.IsArray) return IsForbidden(t.GetElementType());
                if (t.IsGenericType) {
                    return t.GetGenericArguments().Any(IsForbidden);
                }
                return forbidden.Contains(t);
            }

            var facadeTypes = typeof(RegionView).Assembly.GetTypes()
                .Where(t => t.IsPublic &&
                            t.Namespace == "PlayersWorlds.Maps.World")
                .ToList();
            Assert.That(facadeTypes, Is.Not.Empty);

            var offenders = new List<string>();
            foreach (var type in facadeTypes) {
                foreach (var p in type.GetProperties(BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.Static)) {
                    if (IsForbidden(p.PropertyType)) {
                        offenders.Add($"{type.Name}.{p.Name} : {p.PropertyType.Name}");
                    }
                }
                foreach (var m in type.GetMethods(BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.Static)
                        .Where(m => !m.IsSpecialName)) {
                    if (IsForbidden(m.ReturnType)) {
                        offenders.Add($"{type.Name}.{m.Name}() : {m.ReturnType.Name}");
                    }
                    foreach (var par in m.GetParameters()) {
                        if (IsForbidden(par.ParameterType)) {
                            offenders.Add($"{type.Name}.{m.Name}({par.ParameterType.Name})");
                        }
                    }
                }
                foreach (var f in type.GetFields(BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.Static)) {
                    if (IsForbidden(f.FieldType)) {
                        offenders.Add($"{type.Name}.{f.Name} : {f.FieldType.Name}");
                    }
                }
                foreach (var ctor in type.GetConstructors(BindingFlags.Public |
                        BindingFlags.Instance)) {
                    foreach (var par in ctor.GetParameters()) {
                        if (IsForbidden(par.ParameterType)) {
                            offenders.Add($"{type.Name}.ctor({par.ParameterType.Name})");
                        }
                    }
                }
            }

            Assert.That(offenders, Is.Empty,
                "façade public surface leaks internal types: " +
                string.Join(", ", offenders));
        }
    }
}
