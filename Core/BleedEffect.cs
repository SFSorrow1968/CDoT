using BDOT.Configuration;
using ThunderRoad;

namespace BDOT.Core
{
    public class BleedEffect
    {
        public Creature Target { get; private set; }
        public BodyZone Zone { get; private set; }
        public float DamagePerTick { get; private set; }
        public float Multiplier { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TotalDuration { get; private set; }
        public int StackCount { get; private set; }
        public float TimeSinceLastTick { get; set; }

        public bool IsExpired => RemainingDuration <= 0f;
        public bool IsValid => Target != null && !Target.isKilled;

        public BleedEffect(Creature target, BodyZone zone, float damagePerTick, float multiplier, float duration)
        {
            Target = target;
            Zone = zone;
            DamagePerTick = damagePerTick;
            Multiplier = multiplier;
            RemainingDuration = duration;
            TotalDuration = duration;
            StackCount = 1;
            TimeSinceLastTick = 0f;
        }

        public void AddStack(float damagePerTick, float duration, int maxStacks)
        {
            if (StackCount < maxStacks)
            {
                StackCount++;
            }

            // Refresh or extend duration
            if (duration > RemainingDuration)
            {
                RemainingDuration = duration;
            }

            // Update damage if the new hit is stronger
            if (damagePerTick > DamagePerTick)
            {
                DamagePerTick = damagePerTick;
            }
        }

        public void Update(float deltaTime)
        {
            RemainingDuration -= deltaTime;
            TimeSinceLastTick += deltaTime;
        }

        public float GetTickDamage()
        {
            // Total damage = base damage * multiplier * stack count * global multiplier
            // Note: Presets batch-write to zone values, so zone multiplier already reflects preset
            float baseDamage = DamagePerTick * Multiplier * StackCount;
            float globalMult = BDOTModOptions.GlobalDamageMultiplier;
            return baseDamage * globalMult;
        }

        public override string ToString()
        {
            return $"BleedEffect[{Zone.GetDisplayName()} x{StackCount} | {RemainingDuration:F1}s | {GetTickDamage():F1} dmg/tick]";
        }
    }
}
