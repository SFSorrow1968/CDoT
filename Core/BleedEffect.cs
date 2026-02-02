using BDOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Core
{
    public class BleedEffect
    {
        public Creature Target { get; private set; }
        public BodyZone Zone { get; private set; }
        public DamageType DamageType { get; private set; }
        public RagdollPart HitPart { get; private set; }
        public float DamagePerTick { get; private set; }
        public float RemainingDuration { get; private set; }
        public float TotalDuration { get; private set; }
        public float TickInterval { get; private set; }
        public int StackCount { get; private set; }
        public float TimeSinceLastTick { get; set; }
        
        /// <summary>
        /// The active blood VFX effect instance. Tracked so we can end it when the bleed expires.
        /// </summary>
        public EffectInstance BloodEffectInstance { get; set; }

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

        /// <summary>
        /// Check if the HitPart is still valid for spawning effects
        /// </summary>
        public bool HasValidHitPart
        {
            get
            {
                try
                {
                    if (HitPart == null) return false;
                    if ((UnityEngine.Object)HitPart == null) return false;
                    if (HitPart.gameObject == null) return false;
                    return HitPart.gameObject.activeInHierarchy;
                }
                catch
                {
                    return false;
                }
            }
        }

        public BleedEffect(Creature target, BodyZone zone, DamageType damageType, RagdollPart hitPart, float damagePerTick, float duration, float tickInterval)
        {
            Target = target;
            Zone = zone;
            DamageType = damageType;
            HitPart = hitPart;
            DamagePerTick = damagePerTick;
            RemainingDuration = duration;
            TotalDuration = duration;
            TickInterval = tickInterval;
            StackCount = 1;
            TimeSinceLastTick = 0f;
        }

        public void AddStack(float damagePerTick, float duration, int maxStacks, RagdollPart newHitPart = null)
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

            // Update hit part if new one is valid and old one isn't
            if (newHitPart != null && (!HasValidHitPart || newHitPart != HitPart))
            {
                HitPart = newHitPart;
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

        /// <summary>
        /// Ends and cleans up the blood VFX effect instance.
        /// Should be called when the bleed expires or is cleared.
        /// </summary>
        public void EndBloodEffect()
        {
            try
            {
                if (BloodEffectInstance != null)
                {
                    BloodEffectInstance.End(false, -1f);
                    BloodEffectInstance = null;
                }
            }
            catch
            {
                // Effect may already be destroyed
                BloodEffectInstance = null;
            }
        }

        public override string ToString()
        {
            return $"BleedEffect[{Zone.GetDisplayName()} x{StackCount} | {RemainingDuration:F1}s | {GetTickDamage():F1} dmg/tick | {DamageType}]";
        }
    }
}
