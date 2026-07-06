using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace PlayersWorlds.Maps {

    [TestFixture]
    internal class AssemblyInfoTest : Test {
        [Test]
        public void TestAssemblyTitle() {
            var assembly = typeof(Area).Assembly;
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            Assert.That(attributes.Count(), Is.EqualTo(1));
            var attribute = (AssemblyTitleAttribute)attributes.First();
            Assert.That(attribute.Title, Is.EqualTo("PlayersWorlds.Maps"));
        }
        [Test]
        public void TestAssemblyCompany() {
            var assembly = typeof(Area).Assembly;
            var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            Assert.That(attributes.Count(), Is.EqualTo(1));
            var attribute = (AssemblyCompanyAttribute)attributes.First();
            Assert.That(attribute.Company, Is.EqualTo("Player's Worlds, Inc."));
        }
    }
}
