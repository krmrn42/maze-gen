using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlayersWorlds.Maps;
using PlayersWorlds.Maps.Areas;

[TestFixture]
public class RandomAreaGeneratorTest : Test {

    [Test, Category("Integration")]
    public void ZoneGenerator_CanGenerateZones() {
        var random = RandomSource.CreateFromEnv();
        var zonesGenerator = new RandomAreaGenerator(random);
        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<string, int>();
        var types = new Dictionary<AreaType, int>();
        var count = 1000;
        foreach (var area in zonesGenerator.Generate(count)) {
            if (--count < 0) break;
            //Assert.That(areaCount, Is.GreaterThan(0));
            Assert.That(area.Tags.Length, Is.GreaterThan(0));

            if (sizes.ContainsKey(area.Size)) {
                sizes[area.Size] += +1;
            } else {
                sizes.Add(area.Size, 1);
            }

            foreach (var tag in area.Tags) {
                Assert.That(tag, Is.Not.Null.Or.Empty);
                if (tags.ContainsKey(tag)) {
                    tags[tag] += +1;
                } else {
                    tags.Add(tag, 1);
                }
            }

            Assert.That(area.Type, Is.Not.EqualTo(AreaType.Maze));
            if (types.ContainsKey(area.Type)) {
                types[area.Type] += 1;
            } else {
                types.Add(area.Type, 1);
            }
        }

        Assert.That(sizes.Values.Sum(), Is.EqualTo(1000));
        // Given 5 non-square and 2 square sizes, it can yield 12 area sizes
        // (one given and one rotated for each non-square size).
        Assert.That(sizes, Has.Exactly(12).Items);
        Assert.That(tags.Values.Sum(), Is.EqualTo(1000));
        Assert.That(tags, Has.Exactly(9).Items);
        Assert.That(types.Values.Sum(), Is.EqualTo(1000));
        Assert.That(types, Has.Exactly(3).Items);
    }

    [Test, Category("Integration")]
    public void ZoneGenerator_CustomSettings() {
        var random = RandomSource.CreateFromEnv();
        var zonesGenerator = new RandomAreaGenerator(random,
            new RandomAreaGenerator.RandomAreaGeneratorSettings(
                0.3f,
                new Dictionary<Vector, float>() { { new Vector(1, 2), 1 } },
                new Dictionary<AreaType, float>() { { AreaType.Hall, 1 } },
                new Dictionary<AreaType, Dictionary<string, float>>() {
                    { AreaType.Hall,
                        new Dictionary<string, float>() { { "some_tag", 1 } }
                    }
                }
        ));
        var sizes = new Dictionary<Vector, int>();
        var tags = new Dictionary<string, int>();
        var types = new Dictionary<AreaType, int>();
        var count = 10;
        foreach (var area in zonesGenerator.Generate(count)) {
            if (--count < 0) break;
            Assert.That(area.Tags.Length, Is.GreaterThan(0));

            if (sizes.ContainsKey(area.Size)) {
                sizes[area.Size] += +1;
            } else {
                sizes.Add(area.Size, 1);
            }

            foreach (var tag in area.Tags) {
                Assert.That(tag, Is.Not.Null.Or.Empty);
                if (tags.ContainsKey(tag)) {
                    tags[tag] += +1;
                } else {
                    tags.Add(tag, 1);
                }
            }

            Assert.That(area.Type, Is.Not.EqualTo(AreaType.Maze));
            if (types.ContainsKey(area.Type)) {
                types[area.Type] += 1;
            } else {
                types.Add(area.Type, 1);
            }
        }

        Assert.That(sizes.Values.Sum(), Is.EqualTo(10));
        // Given 1 non-square size, it can yield 2 area sizes
        // (one given and one rotated).
        Assert.That(sizes, Has.Exactly(2).Items);
        Assert.That(types.Values.Sum(), Is.EqualTo(10));
        Assert.That(types, Has.Exactly(1).Items);
        Assert.That(tags.Values.Sum(), Is.EqualTo(10));
        Assert.That(tags, Has.Exactly(1).Items);
    }
}
