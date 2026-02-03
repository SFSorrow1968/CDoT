using CDoT.Configuration;
using NUnit.Framework;

namespace CDoT.Tests
{
    [TestFixture]
    public class BodyZoneTests
    {
        [Test]
        public void BodyZone_AllZonesHaveDisplayName()
        {
            var zones = System.Enum.GetValues(typeof(BodyZone));
            foreach (BodyZone zone in zones)
            {
                string displayName = zone.GetDisplayName();
                Assert.That(displayName, Is.Not.Null.And.Not.Empty,
                    $"Zone {zone} should have a display name");
            }
        }

        [Test]
        [TestCase(BodyZone.Head, "Head")]
        [TestCase(BodyZone.Neck, "Neck")]
        [TestCase(BodyZone.Throat, "Throat")]
        [TestCase(BodyZone.Torso, "Torso")]
        [TestCase(BodyZone.Arm, "Arm")]
        [TestCase(BodyZone.Leg, "Leg")]
        [TestCase(BodyZone.Dismemberment, "Dismemberment")]
        [TestCase(BodyZone.Unknown, "Unknown")]
        public void BodyZone_GetDisplayName_ReturnsExpectedValue(BodyZone zone, string expectedName)
        {
            Assert.That(zone.GetDisplayName(), Is.EqualTo(expectedName));
        }

        [Test]
        public void BodyZone_AllValuesAreUnique()
        {
            var values = System.Enum.GetValues(typeof(BodyZone));
            var uniqueValues = new System.Collections.Generic.HashSet<int>();

            foreach (BodyZone value in values)
            {
                Assert.That(uniqueValues.Add((int)value), Is.True,
                    $"Duplicate enum value found: {value}");
            }
        }
    }
}
