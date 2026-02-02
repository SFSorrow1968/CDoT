using BDOT.Configuration;
using ThunderRoad;

namespace BDOT.Core
{
    public class BleedEffect
    {
        public Creature Target { get; private set; }
        public BodyZone Zone { get; private set; }
        public DamageType DamageType { get; private set; }
        public float DamagePerTick { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TotalDuration { get; private set; }
        public float TickInterval { get; private set; }
        public int StackCount { get; private set; }
        public float TimeSinceLastTick { get; set; }

        public bool IsExpired => RemainingDuration <= 0f;
        public bool IsValid
        {
            get
            {
                try
                {
                    // Unity objects can be "fake null" after destruction
                    if (Target == null) return false;
                    if ((UnityEngine.Object)Target == null) return false;
                    return !Target.isKilled;
                }
                catch
                {
                    return false;
                }
            }
        }

        public BleedEffect(Creature target, BodyZone zone, DamageType damageType, float damagePerTick, float duration, float tickInterval)
        {
            Target = target;
            Zone = zone;
            DamageType = damageType;
            DamagePerTick = damagePerTick;
            RemainingDuration = duration;
            TotalDuration = duration;
            TickInterval = tickInterval;
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
            // Total damage = base damage * stack count * damage type multiplier
            float baseDamage = DamagePerTick * StackCount;
            float damageTypeMult = BDOTModOptions.GetDamageTypeMultiplier(DamageType);
            return baseDamage * damageTypeMult;
        }

        public override string ToString()
        {
            return $"BleedEffect[{Zone.GetDisplayName()} x{StackCount} | {RemainingDuration:F1}s | {GetTickDamage():F1} dmg/tick | {DamageType}]";
        }
    }
}
