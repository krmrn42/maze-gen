using System;
using NUnit.Framework;

namespace PlayersWorlds.Maps {
    [TestFixture]
    public class OptionalTest : Test {
        private class A {
            public int Value { get; set; }
        }

        [Test]
        public void Optional_Equality() {
            var a = new A() { Value = 1 };
            var oa = new Optional<A>(a);
            var oa2 = new Optional<A>(a);
            Assert.That(oa.Equals(a));
            Assert.That(oa.Equals(oa2));
            Assert.That(!a.Equals(oa));
        }

        [Test]
        public void Optional_ThrowsIfNoValue() {
            var opt = Optional<object>.Empty;
            Assert.That(opt.HasValue, Is.False);
            Assert.Throws<InvalidOperationException>(() => { var shouldFail = opt.Value; });
        }

        [Test]
        public void Optional_Equals() {
            var a = new { x = 1 };
            var optA = new Optional<dynamic>(a);
            var optB = new Optional<dynamic>(a);
            Optional<dynamic> optNull = null;
            Assert.That(optA.Equals(optB), Is.False);
            Assert.That(optA.Equals(a), Is.True);
            Assert.That(a.Equals(optA), Is.False);
            Assert.That(optA == null, Is.False);
            Assert.That(optNull == null, Is.True);
        }

        [Test]
        public void Optional_NullEquality() {
            A a = null;
            var optA = new Optional<A>(a);
            var optB = new Optional<A>(a);
            Assert.That(optA.Equals(optB), Is.False);
        }

        [Test]
        public void Optional_Null() {
            var optNull1 = new Optional<object>(null);
            Optional<object> optNull2 = null;
            Assert.That(optNull1 == null, Is.True, "optNull1 == null");
            Assert.That(null == optNull1, Is.True, "optNull1 == null");
            Assert.That(optNull1 != null, Is.False, "optNull1 == null");
            Assert.That(null != optNull1, Is.False, "optNull1 == null");
            Assert.That(optNull1 is null, Is.False, "optNull1 == null");
            Assert.That(optNull2 == null, Is.True, "optNull2 == null");
            Assert.That(optNull2 is null, Is.True, "optNull2 == null");
            Assert.That(optNull1.HasValue, Is.False);
            Assert.Throws<NullReferenceException>(() => { var shouldFail = optNull2.HasValue; });
        }

        [Test]
        public void Optional_GetHashCode() {
            var a = new { x = 1 };
            var optA = new Optional<dynamic>(a);
            var optB = new Optional<dynamic>(a);
            Assert.That(optA.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(optB.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(Optional<dynamic>.Empty.GetHashCode(), Is.Not.EqualTo(a.GetHashCode()));
        }

        [Test]
        public void Optional_CanBeCastToT() {
            var a = "abc";
            Optional<string> optA = a;
            Assert.That("abc", Is.EqualTo((string)optA));
        }

        [Test]
        public void Optional_ToStringShowsValue() {
            Optional<string> optA = "abc";
            Assert.That("Optional<String>(abc)", Is.EqualTo(optA.ToString()));
            Assert.That("Optional<String>(<empty>)", Is.EqualTo(Optional<string>.Empty.ToString()));
        }
    }
}
