using CDoT.Configuration;
using NUnit.Framework;

namespace CDoT.Tests
{
    [TestFixture]
    public class CDoTModOptionsTests
    {
        [Test]
        public void ModOptions_DefaultsAreSensible()
        {
            // Verify reasonable defaults
            Assert.That(CDoTModOptions.EnableMod, Is.True, "Mod should be enabled by default");
            Assert.That(CDoTModOptions.DebugLogging, Is.False, "Debug logging should be off by default");
            Assert.That(CDoTModOptions.DebugOverlay, Is.False, "Debug overlay should be off by default");
        }

        [Test]
        public void ModOptions_VersionIsSet()
        {
            // Version should be a valid semver-like string
            Assert.That(CDoTModOptions.VERSION, Is.Not.Null.And.Not.Empty);
            Assert.That(CDoTModOptions.VERSION, Does.Match(@"^\d+\.\d+\.\d+"));
        }

        [Test]
        public void ModOptions_AllZoneToggleOptionsExist()
        {
            // Verify all zone toggle option constants exist
            var zoneOptions = new[]
            {
                CDoTModOptions.OptionThroatEnabled,
                CDoTModOptions.OptionHeadEnabled,
                CDoTModOptions.OptionNeckEnabled,
                CDoTModOptions.OptionTorsoEnabled,
                CDoTModOptions.OptionArmEnabled,
                CDoTModOptions.OptionLegEnabled,
                CDoTModOptions.OptionDismembermentEnabled
            };

            var uniqueSet = new System.Collections.Generic.HashSet<string>(zoneOptions);
            Assert.That(uniqueSet.Count, Is.EqualTo(zoneOptions.Length), "Zone toggle option constants should be unique");
        }

        [Test]
        public void ModOptions_AllZonesEnabledByDefault()
        {
            // All zones should be enabled by default
            Assert.That(CDoTModOptions.ThroatEnabled, Is.True, "Throat should be enabled by default");
            Assert.That(CDoTModOptions.HeadEnabled, Is.True, "Head should be enabled by default");
            Assert.That(CDoTModOptions.NeckEnabled, Is.True, "Neck should be enabled by default");
            Assert.That(CDoTModOptions.TorsoEnabled, Is.True, "Torso should be enabled by default");
            Assert.That(CDoTModOptions.ArmEnabled, Is.True, "Arm should be enabled by default");
            Assert.That(CDoTModOptions.LegEnabled, Is.True, "Leg should be enabled by default");
            Assert.That(CDoTModOptions.DismembermentEnabled, Is.True, "Dismemberment should be enabled by default");
        }

        [Test]
        public void ModOptions_DamageTypeMultipliersArePositive()
        {
            // Damage type multipliers should be non-negative
            Assert.That(CDoTModOptions.PierceMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(CDoTModOptions.SlashMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(CDoTModOptions.FireMultiplier, Is.GreaterThanOrEqualTo(0f));
            Assert.That(CDoTModOptions.LightningMultiplier, Is.GreaterThanOrEqualTo(0f));
        }
    }
}
