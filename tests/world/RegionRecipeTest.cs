using NUnit.Framework;

namespace PlayersWorlds.Maps.World {
    [TestFixture]
    public class RegionRecipeTest {
        [Test]
        public void Presets_HaveSquareCellsByDefault() {
            Assert.That(RegionRecipe.Maze.CellSize, Is.EqualTo(new Vector(1, 1)));
            Assert.That(RegionRecipe.Maze.WallSize, Is.EqualTo(new Vector(1, 1)));
            Assert.That(RegionRecipe.Maze.Fill, Is.EqualTo(1.0));
        }

        [Test]
        public void Presets_DifferByAlgorithm() {
            Assert.That(RegionRecipe.Maze.Algorithm,
                Is.EqualTo(RegionAlgorithm.RecursiveBacktracker));
            Assert.That(RegionRecipe.Corridors.Algorithm,
                Is.EqualTo(RegionAlgorithm.Sidewinder));
        }

        [Test]
        public void With_ReturnsANewRecipe_LeavingTheOriginalUntouched() {
            var basic = RegionRecipe.Maze;
            var tuned = basic
                .WithAlgorithm(RegionAlgorithm.HuntAndKill)
                .WithFill(0.5)
                .WithCells(2);

            Assert.That(tuned.Algorithm, Is.EqualTo(RegionAlgorithm.HuntAndKill));
            Assert.That(tuned.Fill, Is.EqualTo(0.5));
            Assert.That(tuned.CellSize, Is.EqualTo(new Vector(2, 2)));
            // original unchanged (immutable)
            Assert.That(basic.Algorithm,
                Is.EqualTo(RegionAlgorithm.RecursiveBacktracker));
            Assert.That(basic.Fill, Is.EqualTo(1.0));
            Assert.That(basic.CellSize, Is.EqualTo(new Vector(1, 1)));
        }

        [Test]
        public void WithFill_IsClampedTo01() {
            Assert.That(RegionRecipe.Maze.WithFill(5.0).Fill, Is.EqualTo(1.0));
            Assert.That(RegionRecipe.Maze.WithFill(-1.0).Fill, Is.EqualTo(0.0));
        }

        [Test]
        public void WithCells_TakesExplicitCorridorAndWallSizes() {
            var recipe = RegionRecipe.Maze
                .WithCells(new Vector(3, 1), new Vector(1, 1));
            Assert.That(recipe.CellSize, Is.EqualTo(new Vector(3, 1)));
            Assert.That(recipe.WallSize, Is.EqualTo(new Vector(1, 1)));
        }

        [Test]
        public void WithRooms_AccumulatesRequests_Immutably() {
            var basic = RegionRecipe.Maze;
            var withRooms = basic.WithRooms(3, new Vector(4, 4),
                new Vector(6, 6), RoomKind.Hall, "armory");
            Assert.That(basic.Rooms, Is.Empty);              // original untouched
            Assert.That(withRooms.Rooms.Count, Is.EqualTo(1));
            Assert.That(withRooms.Rooms[0].Kind, Is.EqualTo(RoomKind.Hall));
            Assert.That(withRooms.Rooms[0].Count, Is.EqualTo(3));
            Assert.That(withRooms.Rooms[0].Tags, Does.Contain("armory"));

            var two = withRooms.WithRooms(2, new Vector(3, 3),
                new Vector(3, 3), RoomKind.Cave);
            Assert.That(two.Rooms.Count, Is.EqualTo(2));     // mixed kinds
        }

        [Test]
        public void RoomPresets_HaveRooms_PlainOnesDoNot() {
            Assert.That(RegionRecipe.Dungeon.Rooms, Is.Not.Empty);
            Assert.That(RegionRecipe.Caverns.Rooms, Is.Not.Empty);
            Assert.That(RegionRecipe.Maze.Rooms, Is.Empty);
            Assert.That(RegionRecipe.Corridors.Rooms, Is.Empty);
        }

        [Test]
        public void WithRooms_RejectsNegativeCount() {
            Assert.That(() => RegionRecipe.Maze.WithRooms(
                    -1, new Vector(2, 2), new Vector(3, 3)),
                Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }
    }
}
