using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps.Serializer;

namespace PlayersWorlds.Maps.Serializer {

    [TestFixture]
    internal class BasicStringReaderTest : Test {
        [Test]
        public void CanReadEmptyObject() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{}");
            Assert.That(() => reader.FinishReading(), Throws.Nothing);
        }

        [Test]
        public void CanReadSimpleObject() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{foo;[bar,baz]}");
            Assert.That(reader.ReadValue(), Is.EqualTo("foo"));
            Assert.That(reader.ReadEnumerable().ToList(),
                Is.EquivalentTo(new string[] { "bar", "baz" }));
            Assert.That(reader.FinishReading());
        }

        [Test]
        public void CanReadTwoEnumerables() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{[foo,bar];[baz,qux]}");
            Assert.That(reader.ReadEnumerable().ToList(),
                Is.EquivalentTo(new string[] { "foo", "bar" }));
            Assert.That(reader.ReadEnumerable().ToList(),
                Is.EquivalentTo(new string[] { "baz", "qux" }));
            Assert.That(reader.FinishReading());
        }

        [Test]
        public void CanReadEmptyEnumerables() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{;}");
            Assert.That(reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(reader.FinishReading());
        }

        [Test]
        public void CanReadEmptyEnumerables2() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{[];[]}");
            Assert.That(reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(reader.FinishReading());
        }

        [Test]
        public void CanReadObjectsInArray() {
            var obj1 = "BasicStringReaderTest:{[];[]}";
            var obj2 = "BasicStringReaderTest:{[1,2,3]}";
            var obj3 = "BasicStringReaderTest:{[]}";
            var combined = "BasicStringReaderTest:{[" + obj1 + "," + obj2 + "," + obj3 + "]}";
            var reader = new BasicStringReader(this.GetType(), combined);
            var objArray = reader.ReadEnumerable().ToList();
            Assert.That(objArray, Has.Exactly(3).Items);
            Assert.That(objArray[0], Is.EqualTo(obj1));
            Assert.That(objArray[1], Is.EqualTo(obj2));
            Assert.That(objArray[2], Is.EqualTo(obj3));
            var obj1Reader = new BasicStringReader(this.GetType(), objArray[0]);
            Assert.That(obj1Reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(obj1Reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(obj1Reader.FinishReading());
            var obj2Reader = new BasicStringReader(this.GetType(), objArray[1]);
            var obj2Value = obj2Reader.ReadEnumerable().ToList();
            Assert.That(obj2Value, Has.Exactly(3).Items);
            Assert.That(obj2Value[0], Is.EqualTo("1"));
            Assert.That(obj2Value[1], Is.EqualTo("2"));
            Assert.That(obj2Value[2], Is.EqualTo("3"));
            Assert.That(obj2Reader.FinishReading());
            var obj3Reader = new BasicStringReader(this.GetType(), objArray[2]);
            var obj3Value = obj3Reader.ReadEnumerable().ToList();
            Assert.That(obj3Value, Is.Empty);
            Assert.That(obj3Reader.FinishReading());
        }

        [Test]
        public void CanReadObjectsInObject() {
            var obj1 = "BasicStringReaderTest:{[];[]}";
            var obj2 = "BasicStringReaderTest:{[1,2,3]}";
            var obj3 = "BasicStringReaderTest:{[]}";
            var combined = "BasicStringReaderTest:{" + obj1 + ";" + obj2 + ";" + obj3 + "}";
            var reader = new BasicStringReader(this.GetType(), combined);
            var obj1Obj = reader.ReadValue();
            Assert.That(obj1Obj, Is.EqualTo(obj1));
            var obj2Obj = reader.ReadValue();
            Assert.That(obj2Obj, Is.EqualTo(obj2));
            var obj3Obj = reader.ReadValue();
            Assert.That(obj3Obj, Is.EqualTo(obj3));
            var obj1Reader = new BasicStringReader(this.GetType(), obj1Obj);
            Assert.That(obj1Reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(obj1Reader.ReadEnumerable().ToList(), Is.Empty);
            Assert.That(obj1Reader.FinishReading());
            var obj2Reader = new BasicStringReader(this.GetType(), obj2Obj);
            var obj2Value = obj2Reader.ReadEnumerable().ToList();
            Assert.That(obj2Value, Has.Exactly(3).Items);
            Assert.That(obj2Value[0], Is.EqualTo("1"));
            Assert.That(obj2Value[1], Is.EqualTo("2"));
            Assert.That(obj2Value[2], Is.EqualTo("3"));
            Assert.That(obj2Reader.FinishReading());
            var obj3Reader = new BasicStringReader(this.GetType(), obj3Obj);
            var obj3Value = obj3Reader.ReadEnumerable().ToList();
            Assert.That(obj3Value, Is.Empty);
            Assert.That(obj3Reader.FinishReading());
        }

        [Test]
        public void ThrowsIfWrongType() {
            Assert.That(() => new BasicStringReader(this.GetType(), "SomeWrongTypeName:{}"),
                Throws.ArgumentException.And.Message.Contains("Invalid type. Expected: BasicStringReaderTest"));
        }

        [Test]
        public void ThrowsIfUnclosedEnumerable1() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{[foo,bar}");
            Assert.That(() => reader.ReadEnumerable().ToList(),
                Throws.ArgumentException.And.Message.Contains("Couldn't find end of enumerable starting at 23"));
        }

        [Test]
        public void ThrowsIfFinishReadingWhenNotFinished() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{[foo,bar}");
            Assert.That(() => reader.FinishReading(),
                Throws.ArgumentException.And.Message.Contains("Not finished reading data"));
        }

        [Test]
        public void ThrowsIfUnclosedEnumerable2() {
            var reader = new BasicStringReader(this.GetType(), "BasicStringReaderTest:{[foo,bar;}");
            Assert.That(() => reader.ReadEnumerable().ToList(),
                Throws.ArgumentException.And.Message.Contains("Couldn't find end of enumerable starting at 23"));
        }

        [Test]
        public void ThrowsIfNoSemicolon() {
            Assert.That(() => new BasicStringReader(this.GetType(), "BasicStringReaderTest{}"),
                Throws.ArgumentException.And.Message.Contains("Couldn't find ':' after 0"));
        }

        [Test]
        public void ThrowsIfNoOpeningBrace() {
            Assert.That(() => new BasicStringReader(this.GetType(), "BasicStringReaderTest:foo"),
                Throws.ArgumentException.And.Message.Contains("Invalid character at 22. Expected any of '{', actual 'f'"));
        }
    }
}
