using System.Collections.Generic;
using NUnit.Framework;
using PlayersWorlds.Maps.Maze;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class ExtensionsTest : Test {

        [Test]
        public void TryDequeue_DoesNotThrow() {
            var queue = new Queue<string>();
            Assert.That(queue.TryDequeue(out _), Is.False);
        }

        [Test]
        public void TryDequeue_Dequeues() {
            var queue = new Queue<string>();
            var a = "a";
            queue.Enqueue(a);
            Assert.That(queue.TryDequeue(out var element), Is.True);
            Assert.That(a, Is.EqualTo(element));
        }

        [Test]
        public void DebugString() {
            var o = new X { A = 1, B = "a" };
            var expectedLong =
                "PlayersWorlds.Maps.ExtensionsTest+X(\tA = 1\n, \tB = a\n)";
            Assert.That(o.DebugString(), Is.EqualTo(expectedLong));
        }

        [Test]
        public void Set() {
            var dict = new Dictionary<int, int>();
            dict.Set(1, 2);
            dict.Set(2, 3);
            dict.Set(1, 4);
            Assert.That(dict.Count, Is.EqualTo(2));
            Assert.That(dict[1], Is.EqualTo(4));
            Assert.That(dict[2], Is.EqualTo(3));
        }

        [Test]
        public void SetList() {
            var dict = new Dictionary<int, List<int>>();
            dict.Set(1, 2);
            dict.Set(2, 3);
            dict.Set(1, 4);
            Assert.That(dict.Count, Is.EqualTo(2));
            Assert.That(dict[1], Is.EqualTo(new List<int>() { 2, 4 }));
            Assert.That(dict[2], Is.EqualTo(new List<int>() { 3 }));
        }

        [Test]
        public void GetAllDictionaryItems() {
            var dict = new Dictionary<int, string>() {
                {1, "a"},
                {2, "b"},
                {3, "c"}
            };
            var items = dict.GetAll(new int[] { 1, 3 });
            Assert.That(items,
                Is.EqualTo(new (int, string)[] { (1, "a"), (3, "c") }));
        }

        [Test]
        public void GetAllHashSetKeys() {
            var dict = new HashSet<int>() { { 1 }, { 2 }, { 3 } };
            var items = dict.GetAll(new int[] { 1, 3 });
            Assert.That(items, Is.EqualTo(new int[] { 1, 3 }));
        }

        private class X {
            public int A { get; set; }
            public string B { get; set; }
        }
    }
}
