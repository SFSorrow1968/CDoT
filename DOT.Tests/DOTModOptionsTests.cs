using DOT.Configuration;
using NUnit.Framework;

namespace DOT.Tests
{
    [TestFixture]
    public class DOTModOptionsTests
    {
        [Test]
        public void ModOptions_DefaultsAreSensible()
        {
            // Verify reasonable defaults
            Assert.That(DOTModOptions.EnableMod, Is.True, "Mod should be enabled by default");
            Assert.That(DOTModOptions.DebugLogging, Is.False, "Debug logging should be off by default");
            Assert.That(DOTModOptions.DebugOverlay, Is.False, "Debug overlay should be off by default");
        }

        [Test]
        public void ModOptions_VersionIsSet()
        {
            // Version should be a valid semver-like string
            Assert.That(DOTModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(DOTModOptions.VERSION, Does.Match(@"^\d+\.\d+\.\d+"));
        }

        [Test]
        public void ModOptions_AllZoneToggleOptionsExist()
        {
            // Verify all zone toggle option constants exist
            var zoneOptions = new[]
            {
                DOTModOptions.OptionThroatEnabled,
                DOTModOptions.OptionHeadEnabled,
                DOTModOptions.OptionNeckEnabled,
                DOTModOptions.OptionTorsoEnabled,
                DOTModOptions.OptionArmEnabled,
                DOTModOptions.OptionLegEnabled,
                DOTModOptions.OptionDismembermentEnabled
            };

            var uniqueSet = new System.Collections.Generic.HashSet<string>(zoneOptions);
            Assert.That(uniqueSet.Count, Is.EqualTo(zoneOptions.Length), "Zone toggle option constants should be unique");
        }

        [Test]
        public void ModOptions_AllZonesEnabledByDefault()
        {
            // All zones should be enabled by default
            Assert.That(DOTModOptions.ThroatEnabled, Is.True, "Throat should be enabled by default");
            Assert.That(DOTModOptions.HeadEnabled, Is.True, "Head should be enabled by default");
            Assert.That(DOTModOptions.NeckEnabled, Is.True, "Neck should be enabled by default");
            Assert.That(DOTModOptions.TorsoEnabled, Is.True, "Torso should be enabled by default");
            Assert.That(DOTModOptions.ArmEnabled, Is.True, "Arm should be enabled by default");
            Assert.That(DOTModOptions.LegEnabled, Is.True, "Leg should be enabled by default");
            Assert.That(DOTModOptions.DismembermentEnabled, Is.True, "Dismemberment should be enabled by default");
        }

        [Test]
        public void ModOptions_DamageTypeMultipliersArePositive()
        {
            // Damage type multipliers should be non-negative
            Assert.That(DOTModOptions.PierceMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DOTModOptions.SlashMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DOTModOptions.FireMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(DOTModOptions.LightningMultiplier, Is.GreaterThanOrEqualTo(0f));
        }
    }
}
