using System;
using System.Collections.Generic;
using BDOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Core
{
    public class BleedManager
    {
        private static BleedManager _instance;
        public static BleedManager Instance => _instance ??= new BleedManager();

        private readonly Dictionary<int, List<BleedEffect>> _activeEffects = new Dictionary<int, List<BleedEffect>>();
        private readonly List<int> _creaturesToRemove = new List<int>();
        private readonly List<BleedEffect> _effectsToRemove = new List<BleedEffect>();
        private readonly List<int> _creatureIds = new List<int>(); // For safe iteration
        private float _lastStatusLogTime = 0f;
        private const float STATUS_LOG_INTERVAL = 5f; // Log status every 5 seconds when effects are active

        public void Initialize()
        {
            try
            {
                _activeEffects.Clear();
                Debug.Log("[BDOT] BleedManager initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] BleedManager init failed: " + ex.Message);
            }
        }

        public void Update()
        {
            if (!BDOTModOptions.EnableMod)
                return;

            try
            {
                float deltaTime = Time.unscaledDeltaTime;
                float tickInterval = BDOTModOptions.TickInterval;

                _creaturesToRemove.Clear();

                // Copy keys to avoid collection modification during iteration
                _creatureIds.Clear();
                foreach (var key in _activeEffects.Keys)
                {
                    _creatureIds.Add(key);
                }

                foreach (var creatureId in _creatureIds)
                {
                    if (!_activeEffects.TryGetValue(creatureId, out var effects))
                        continue;

                    _effectsToRemove.Clear();

                    for (int i = 0; i < effects.Count; i++)
                    {
                        var effect = effects[i];
                        if (!effect.IsValid)
                        {
                            _effectsToRemove.Add(effect);
                            continue;
                        }

                        effect.Update(deltaTime);

                        if (effect.IsExpired)
                        {
                            _effectsToRemove.Add(effect);
                            continue;
                        }

                        // Apply damage tick
                        if (effect.TimeSinceLastTick >= tickInterval)
                        {
                            ApplyBleedDamage(effect);
                            effect.TimeSinceLastTick = 0f;
                        }
                    }

                    // Remove expired/invalid effects
                    foreach (var effect in _effectsToRemove)
                    {
                        effects.Remove(effect);
                        if (BDOTModOptions.DebugLogging)
                        {
                            string reason = effect.IsExpired ? "duration ended" : "target invalid/killed";
                            Debug.Log("[BDOT] EXPIRED: " + effect.Zone.GetDisplayName() + " on " + (effect.Target?.name ?? "null") + " (" + reason + ")");
                        }
                    }

                    // Mark creature for removal if no effects remain
                    if (effects.Count == 0)
                    {
                        _creaturesToRemove.Add(creatureId);
                    }
                }

                // Clean up creatures with no effects
                foreach (var creatureId in _creaturesToRemove)
                {
                    _activeEffects.Remove(creatureId);
                }

                // Periodic status logging
                if (BDOTModOptions.DebugLogging && _activeEffects.Count > 0)
                {
                    float now = Time.unscaledTime;
                    if (now - _lastStatusLogTime >= STATUS_LOG_INTERVAL)
                    {
                        _lastStatusLogTime = now;
                        LogActiveEffectsStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] BleedManager Update error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void LogActiveEffectsStatus()
        {
            int totalEffects = GetActiveEffectCount();
            int creatures = _activeEffects.Count;

            Debug.Log("[BDOT] --- Active Bleeds Status ---");
            Debug.Log("[BDOT] Creatures: " + creatures + " | Effects: " + totalEffects);

            foreach (var kvp in _activeEffects)
            {
                var effects = kvp.Value;
                if (effects.Count > 0 && effects[0].Target != null)
                {
                    string creatureName = effects[0].Target.name;
                    string effectList = "";
                    foreach (var effect in effects)
                    {
                        if (effectList.Length > 0) effectList += ", ";
                        effectList += effect.Zone.GetDisplayName() + " x" + effect.StackCount + " (" + effect.RemainingDuration.ToString("F1") + "s)";
                    }
                    Debug.Log("[BDOT]   " + creatureName + ": " + effectList);
                }
            }
            Debug.Log("[BDOT] --------------------------------");
        }

        public bool ApplyBleed(Creature target, BodyZone zone, DamageType damageType)
        {
            if (target == null || target.isKilled || target.isPlayer)
                return false;

            if (!BDOTModOptions.EnableMod)
                return false;

            var config = BDOTModOptions.GetZoneConfig(zone);
            if (!config.Enabled)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Zone disabled: " + zone.GetDisplayName());
                return false;
            }

            // Check chance roll
            float roll = UnityEngine.Random.value * 100f;
            if (roll > config.Chance)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Chance roll failed: " + roll.ToString("F1") + " > " + config.Chance.ToString("F0") + "%");
                return false;
            }

            int creatureId = target.GetInstanceID();

            if (!_activeEffects.TryGetValue(creatureId, out var effects))
            {
                effects = new List<BleedEffect>();
                _activeEffects[creatureId] = effects;
            }

            // Check for existing effect on same zone
            BleedEffect existingEffect = null;
            foreach (var effect in effects)
            {
                if (effect.Zone == zone)
                {
                    existingEffect = effect;
                    break;
                }
            }

            if (existingEffect != null)
            {
                // Stack the effect
                int oldStacks = existingEffect.StackCount;
                float oldDuration = existingEffect.RemainingDuration;
                existingEffect.AddStack(config.Damage, config.Duration, config.StackLimit);
                if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] STACK: " + zone.GetDisplayName() + " on " + target.name);
                    Debug.Log("[BDOT]   Stacks: " + oldStacks + " -> " + existingEffect.StackCount + " (max=" + config.StackLimit + ")");
                    Debug.Log("[BDOT]   Duration: " + oldDuration.ToString("F1") + "s -> " + existingEffect.RemainingDuration.ToString("F1") + "s");
                    Debug.Log("[BDOT]   New tick damage: " + existingEffect.GetTickDamage().ToString("F2"));
                }
            }
            else
            {
                // Create new effect
                var newEffect = new BleedEffect(
                    target,
                    zone,
                    damageType,
                    config.Damage,
                    config.Duration
                );
                effects.Add(newEffect);
                if (BDOTModOptions.DebugLogging)
                {
                    float damageTypeMult = BDOTModOptions.GetDamageTypeMultiplier(damageType);
                    Debug.Log("[BDOT] NEW BLEED: " + zone.GetDisplayName() + " on " + target.name);
                    Debug.Log("[BDOT]   BaseDmg=" + config.Damage.ToString("F2") + " | DamageType=" + damageType + " (" + damageTypeMult.ToString("F1") + "x) | Duration=" + config.Duration.ToString("F1") + "s");
                    Debug.Log("[BDOT]   Tick damage: " + newEffect.GetTickDamage().ToString("F2"));
                }
            }

            BDOTModOptions.IncrementBleedCount();
            return true;
        }

        private void ApplyBleedDamage(BleedEffect effect)
        {
            if (effect == null || !effect.IsValid)
                return;

            var target = effect.Target;
            
            // Double-check target validity with Unity null check
            if (target == null || (UnityEngine.Object)target == null)
                return;

            // Additional safety: check if creature is dead or being destroyed
            if (target.isKilled)
                return;

            float damage = effect.GetTickDamage();
            if (damage <= 0f)
                return;

            // Get health before damage for logging (optional, don't skip damage if this fails)
            float healthBefore = -1f;
            try
            {
                healthBefore = target.currentHealth;
            }
            catch
            {
                // Health read failed, but creature may still be valid - continue with damage
            }

            // Apply damage - this is the critical part, wrapped in its own try-catch
            try
            {
                // Final safety check before applying damage
                if (target == null || (UnityEngine.Object)target == null || target.isKilled)
                    return;

                // Apply damage to creature using the simple Damage(float, DamageType) overload
                // Using DamageType.Pierce for bleed damage (Energy causes fire effects!)
                target.Damage(damage, DamageType.Pierce);
                BDOTModOptions.AddBleedDamage(damage);
            }
            catch (Exception ex)
            {
                // Damage application failed - creature is likely destroyed or in invalid state
                if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] Tick failed to apply damage: " + ex.Message);
                }
                return;
            }

            // Check if creature was killed by this bleed tick
            bool killedByBleed = false;
            try
            {
                killedByBleed = target == null || (UnityEngine.Object)target == null || target.isKilled;
            }
            catch
            {
                killedByBleed = true;
            }

            if (killedByBleed)
            {
                Debug.Log("[BDOT] *** BLEED KILL! " + effect.Zone.GetDisplayName() + " bleed killed creature! ***");
                return; // Don't try to access target anymore
            }

            // Get health after damage for logging (optional)
            float healthAfter = -1f;
            try
            {
                healthAfter = target.currentHealth;
            }
            catch
            {
                // Health read failed, use placeholder
            }

            if (BDOTModOptions.DebugLogging)
            {
                float damageTypeMult = BDOTModOptions.GetDamageTypeMultiplier(effect.DamageType);
                string healthInfo = (healthBefore >= 0f && healthAfter >= 0f) 
                    ? healthBefore.ToString("F1") + " -> " + healthAfter.ToString("F1")
                    : "N/A";
                Debug.Log("[BDOT] TICK: " + effect.Zone.GetDisplayName() + " x" + effect.StackCount + " on " + (target?.name ?? "destroyed"));
                Debug.Log("[BDOT]   Damage: " + damage.ToString("F2") + " (base=" + effect.DamagePerTick.ToString("F2") + " * stacks=" + effect.StackCount + " * " + effect.DamageType + "=" + damageTypeMult.ToString("F1") + "x)");
                Debug.Log("[BDOT]   Health: " + healthInfo + " | Remaining: " + effect.RemainingDuration.ToString("F1") + "s");
            }
        }

        public void ClearCreature(Creature creature)
        {
            if (creature == null)
                return;

            int creatureId = creature.GetInstanceID();
            if (_activeEffects.ContainsKey(creatureId))
            {
                _activeEffects.Remove(creatureId);
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Cleared effects for: " + creature.name);
            }
        }

        public void ClearAll()
        {
            _activeEffects.Clear();
            if (BDOTModOptions.DebugLogging)
                Debug.Log("[BDOT] All effects cleared");
        }

        public int GetActiveEffectCount()
        {
            int count = 0;
            foreach (var effects in _activeEffects.Values)
            {
                count += effects.Count;
            }
            return count;
        }

        public int GetAffectedCreatureCount()
        {
            return _activeEffects.Count;
        }

        public void Shutdown()
        {
            ClearAll();
            _instance = null;
        }
    }
}
