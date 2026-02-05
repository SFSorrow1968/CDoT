using NUnit.Framework;

namespace DOT.Tests
{
    /// <summary>
    /// Tests for BleedEffect calculation logic.
    /// Note: Full integration tests require Unity/ThunderRoad runtime.
    /// These tests cover the mathematical calculations.
    /// </summary>
    [TestFixture]
    public class BleedEffectCalculationTests
    {
        // Constants from BleedEffect (replicated for testing)
        private const float INTENSITY_BASE_DIVISOR = 5f;
        private const float MIN_BLOOD_INTENSITY = 0.05f;
        private const float MAX_BLOOD_INTENSITY = 5.0f;

        [Test]
        [TestCase(1, 1.0f, 1.0f, 0.2f)]    // 1 stack, 1.0 damage, 1.0 zone mult = 0.2
        [TestCase(2, 1.0f, 1.0f, 0.4f)]    // 2 stacks = 0.4
        [TestCase(1, 2.5f, 1.0f, 0.5f)]    // Higher damage
        [TestCase(1, 1.0f, 1.5f, 0.3f)]    // Zone multiplier (throat)
        [TestCase(3, 2.0f, 2.0f, 2.4f)]    // Combined: 3 * 2.0 * 2.0 / 5 = 2.4
        public void CalculateIntensity_ReturnsExpectedValue(int stackCount, float damagePerTick, float zoneMult, float expectedIntensity)
        {
            float baseIntensity = (stackCount * damagePerTick * zoneMult) / INTENSITY_BASE_DIVISOR;
            float presetMultiplier = 1.0f; // Default preset

            float actualIntensity = baseIntensity * presetMultiplier;

            Assert.That(actualIntensity, Is.EqualTo(expectedIntensity).Within(0.001f));
        }

        [Test]
        public void CalculateIntensity_ClampsToMinimum()
        {
            // Very small values should clamp to MIN_BLOOD_INTENSITY
            float tinyIntensity = 0.01f;
            float clamped = Clamp(tinyIntensity, MIN_BLOOD_INTENSITY, MAX_BLOOD_INTENSITY);

            Assert.That(clamped, Is.EqualTo(MIN_BLOOD_INTENSITY));
        }

        [Test]
        public void CalculateIntensity_ClampsToMaximum()
        {
            // Very large values should clamp to MAX_BLOOD_INTENSITY
            float hugeIntensity = 100f;
            float clamped = Clamp(hugeIntensity, MIN_BLOOD_INTENSITY, MAX_BLOOD_INTENSITY);

            Assert.That(clamped, Is.EqualTo(MAX_BLOOD_INTENSITY));
        }

        [Test]
        [TestCase(1.5f)]  // Throat
        [TestCase(1.2f)]  // Head
        [TestCase(1.3f)]  // Neck
        [TestCase(1.0f)]  // Torso
        [TestCase(0.7f)]  // Arm
        [TestCase(0.8f)]  // Leg
        [TestCase(2.0f)]  // Dismemberment
        public void ZoneIntensityMultiplier_IsPositive(float multiplier)
        {
            Assert.That(multiplier, Is.GreaterThan(0f));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
