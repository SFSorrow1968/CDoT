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

        // Blood effect data - loaded once from catalog
        private EffectData _bleedEffectData;
        private bool _bleedEffectLoaded = false;
        private const string BLEED_EFFECT_ID = "PenetrationDeepBleeding";

        public void Initialize()
        {
            try
            {
                _activeEffects.Clear();
                _bleedEffectLoaded = false;
                _bleedEffectData = null;
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

                        // Apply damage tick using per-effect tick interval
                        if (effect.TimeSinceLastTick >= effect.TickInterval)
                        {
                            ApplyBleedDamage(effect);
                            effect.TimeSinceLastTick = 0f;
                        }
                    }

                    // Remove expired/invalid effects
                    foreach (var effect in _effectsToRemove)
                    {
                        effects.Remove(effect);
                        
                        // End the blood VFX effect
                        effect.EndBloodEffect();
                        
                        // Remove visual status effect if no more fire/lightning DOT
                        if (effect.DamageType == DamageType.Fire || effect.DamageType == DamageType.Lightning)
                        {
                            RemoveStatusEffectVisualIfNeeded(effect.Target, effect.DamageType);
                        }
                        
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

        public bool ApplyBleed(Creature target, BodyZone zone, DamageType damageType, RagdollPart hitPart = null)
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
                existingEffect.AddStack(config.Damage, config.Duration, config.StackLimit, hitPart);
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
                // Create new effect with zone-specific tick interval
                var newEffect = new BleedEffect(
                    target,
                    zone,
                    damageType,
                    hitPart,
                    config.Damage,
                    config.Duration,
                    config.Frequency
                );
                effects.Add(newEffect);
                if (BDOTModOptions.DebugLogging)
                {
                    float damageTypeMult = BDOTModOptions.GetDamageTypeMultiplier(damageType);
                    Debug.Log("[BDOT] NEW BLEED: " + zone.GetDisplayName() + " on " + target.name);
                    Debug.Log("[BDOT]   BaseDmg=" + config.Damage.ToString("F2") + " | DamageType=" + damageType + " (" + damageTypeMult.ToString("F1") + "x) | Duration=" + config.Duration.ToString("F1") + "s | TickInterval=" + config.Frequency.ToString("F2") + "s");
                    Debug.Log("[BDOT]   Tick damage: " + newEffect.GetTickDamage().ToString("F2"));
                    Debug.Log("[BDOT]   HitPart: " + (hitPart != null ? hitPart.type.ToString() : "null"));
                }
            }

            // Apply visual status effect for fire/lightning DOT
            if (damageType == DamageType.Fire || damageType == DamageType.Lightning)
            {
                ApplyStatusEffectVisual(target, damageType);
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

            // Apply heat/electrocute status effect scaled by damage for fire/lightning DOT
            // This causes heat to accumulate → ignition → charring → combustion
            if (effect.DamageType == DamageType.Fire || effect.DamageType == DamageType.Lightning)
            {
                ApplyStatusEffectForDamage(target, effect.DamageType, damage);
            }

            // Spawn blood VFX for physical damage types
            if (effect.DamageType == DamageType.Pierce || effect.DamageType == DamageType.Slash)
            {
                SpawnBleedEffect(effect, damage);
            }

            // Apply damage by directly modifying currentHealth
            // This avoids triggering EventManager.InvokeCreatureHit which was causing null reference errors
            // when the game's internal damage processing tried to access null collider groups
            float healthBefore = -1f;
            float healthAfter = -1f;
            bool killedByBleed = false;
            
            try
            {
                // Final safety check before applying damage
                if (target == null || (UnityEngine.Object)target == null || target.isKilled)
                    return;

                healthBefore = target.currentHealth;
                float newHealth = healthBefore - damage;
                
                // Check if this will kill the creature
                if (newHealth <= 0f)
                {
                    // Use Kill() to properly trigger death events and animations
                    target.Kill();
                    killedByBleed = true;
                    BDOTModOptions.AddBleedDamage(damage);
                    Debug.Log("[BDOT] *** BLEED KILL! " + effect.Zone.GetDisplayName() + " bleed killed " + target.name + "! ***");
                    return;
                }
                
                // Apply damage directly to health (bypasses InvokeCreatureHit)
                target.currentHealth = newHealth;
                healthAfter = newHealth;
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

            if (BDOTModOptions.DebugLogging && !killedByBleed)
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

        /// <summary>
        /// Spawns blood spurt VFX at the wound location.
        /// Intensity scales with damage dealt.
        /// </summary>
        private void SpawnBleedEffect(BleedEffect effect, float damage)
        {
            if (!effect.HasValidHitPart)
                return;

            try
            {
                // Load effect data if not already loaded
                if (!_bleedEffectLoaded)
                {
                    _bleedEffectData = Catalog.GetData<EffectData>(BLEED_EFFECT_ID, true);
                    _bleedEffectLoaded = true;
                    if (_bleedEffectData == null)
                    {
                        Debug.LogWarning("[BDOT] Could not load bleed effect: " + BLEED_EFFECT_ID);
                    }
                }

                if (_bleedEffectData == null)
                    return;

                var hitPart = effect.HitPart;
                
                // Calculate intensity based on damage (typical damage 1-5, scale to 0.2-1.0)
                float intensity = Mathf.Clamp(damage * 0.2f, 0.2f, 1.0f);
                
                // Scale intensity by stack count (more stacks = more bleeding)
                intensity = Mathf.Clamp(intensity * (1f + (effect.StackCount - 1) * 0.3f), 0.2f, 1.5f);
                
                // Apply blood amount preset multiplier
                float bloodMultiplier = BDOTModOptions.GetBloodAmountMultiplier();
                intensity *= bloodMultiplier;
                
                // Clamp final intensity to reasonable bounds
                intensity = Mathf.Clamp(intensity, 0.1f, 3.0f);

                // Determine how many blood spurts to spawn based on blood amount preset
                // VeryLow=1, Low=1, Default=2, High=3, Extreme=4
                int spurtCount = 1;
                var bloodPreset = BDOTModOptions.GetBloodAmountPreset();
                switch (bloodPreset)
                {
                    case BDOTModOptions.BloodAmountPreset.VeryLow:
                    case BDOTModOptions.BloodAmountPreset.Low:
                        spurtCount = 1;
                        break;
                    case BDOTModOptions.BloodAmountPreset.Default:
                        spurtCount = 2;
                        break;
                    case BDOTModOptions.BloodAmountPreset.High:
                        spurtCount = 3;
                        break;
                    case BDOTModOptions.BloodAmountPreset.Extreme:
                        spurtCount = 4;
                        break;
                }

                // Spawn blood spurt effect(s) at the exact hit part location
                // Use hitPart transform directly to ensure all spurts spawn at the exact same position
                Vector3 position = hitPart.transform.position;
                Quaternion rotation = hitPart.transform.rotation;
                
                for (int i = 0; i < spurtCount; i++)
                {
                    var effectInstance = _bleedEffectData.Spawn(position, rotation, hitPart.transform, null, true, null, false, intensity, 1f);
                    if (effectInstance != null)
                    {
                        effectInstance.SetIntensity(intensity);
                        effectInstance.Play();
                        
                        // Store reference to the most recent effect so we can end it when bleed expires
                        // (older spurts will naturally despawn on their own)
                        effect.BloodEffectInstance = effectInstance;
                    }
                }
                
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Blood VFX: " + spurtCount + " spurt(s) | " + effect.Zone.GetDisplayName() + " | Intensity: " + intensity.ToString("F2") + " | Stacks: " + effect.StackCount);
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Blood VFX spawn failed: " + ex.Message);
            }
        }

        public void ClearCreature(Creature creature)
        {
            if (creature == null)
                return;

            int creatureId = creature.GetInstanceID();
            if (_activeEffects.TryGetValue(creatureId, out var effects))
            {
                // End all blood VFX effects for this creature
                foreach (var effect in effects)
                {
                    effect.EndBloodEffect();
                }
                _activeEffects.Remove(creatureId);
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Cleared effects for: " + creature.name);
            }
        }

        public void ClearAll()
        {
            // End all blood VFX effects
            foreach (var effects in _activeEffects.Values)
            {
                foreach (var effect in effects)
                {
                    effect.EndBloodEffect();
                }
            }
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

        /// <summary>
        /// Checks if a creature has any active fire DOT effects.
        /// </summary>
        public bool HasActiveFireDOT(Creature creature)
        {
            if (creature == null) return false;
            int creatureId = creature.GetInstanceID();
            if (!_activeEffects.TryGetValue(creatureId, out var effects)) return false;
            
            foreach (var effect in effects)
            {
                if (effect.DamageType == DamageType.Fire && !effect.IsExpired)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a creature has any active lightning DOT effects.
        /// </summary>
        public bool HasActiveLightningDOT(Creature creature)
        {
            if (creature == null) return false;
            int creatureId = creature.GetInstanceID();
            if (!_activeEffects.TryGetValue(creatureId, out var effects)) return false;
            
            foreach (var effect in effects)
            {
                if (effect.DamageType == DamageType.Lightning && !effect.IsExpired)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Applies heat or electrocute status based on fire/lightning DOT damage.
        /// Heat accumulates with each tick - more damage = faster ignition and charring.
        /// Electrocute power scales with damage for more intense shocking effect.
        /// </summary>
        /// <param name="creature">Target creature</param>
        /// <param name="damageType">Fire or Lightning</param>
        /// <param name="damage">DOT damage dealt this tick - used to scale heat/power</param>
        public void ApplyStatusEffectForDamage(Creature creature, DamageType damageType, float damage)
        {
            if (creature == null || creature.isKilled) return;

            try
            {
                if (damageType == DamageType.Fire)
                {
                    // Add heat proportional to damage dealt
                    // Higher damage = more heat = faster ignition/charring
                    // Typical DOT damage is 1-5 HP, so heat multiplier scales this to meaningful heat values
                    // Heat threshold for ignition is typically around 100, so we use damage * 10-20 range
                    float heatToAdd = damage * 15f; // Scales DOT damage to heat buildup
                    
                    creature.Inflict("Burning", this, float.PositiveInfinity, heatToAdd, true);
                    
                    if (BDOTModOptions.DebugLogging)
                    {
                        var burning = creature.GetStatusOfType<Burning>();
                        string heatInfo = burning != null ? " | Heat: " + burning.Heat.ToString("F1") + " | Ignited: " + burning.isIgnited : "";
                        Debug.Log("[BDOT] Fire tick on " + creature.name + " | Damage: " + damage.ToString("F2") + " | Heat added: " + heatToAdd.ToString("F1") + heatInfo);
                    }
                }
                else if (damageType == DamageType.Lightning)
                {
                    // Use TryElectrocute with power scaled by damage
                    // Power accumulates: existing power = (power + newPower) * 0.5
                    // Higher power = more intense convulsions and visual effects
                    // Typical damage 1-5 HP, scale to power 0.2-1.0 range
                    float power = Mathf.Clamp(damage * 0.3f, 0.1f, 1.0f);
                    float duration = 2f; // Duration refreshed each tick
                    
                    creature.TryElectrocute(power, duration, true, false, null);
                    
                    if (BDOTModOptions.DebugLogging)
                    {
                        bool isElectrocuted = creature.brain != null && creature.brain.isElectrocuted;
                        Debug.Log("[BDOT] Lightning tick on " + creature.name + " | Damage: " + damage.ToString("F2") + " | Power: " + power.ToString("F2") + " | Electrocuted: " + isElectrocuted);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] Error applying status effect: " + ex.Message);
            }
        }

        /// <summary>
        /// Initial status effect application when DOT first starts.
        /// Applies a small amount of heat/electrocute to begin the effect.
        /// </summary>
        public void ApplyStatusEffectVisual(Creature creature, DamageType damageType)
        {
            if (creature == null || creature.isKilled) return;

            try
            {
                if (damageType == DamageType.Fire)
                {
                    // Initial heat application - small amount to start smoking
                    creature.Inflict("Burning", this, float.PositiveInfinity, 20f, true);
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Started fire DOT on " + creature.name + " | Initial heat: 20");
                }
                else if (damageType == DamageType.Lightning)
                {
                    // Initial electrocute with low power - will intensify with each tick
                    creature.TryElectrocute(0.2f, 2f, true, false, null);
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Started lightning DOT on " + creature.name + " | Initial power: 0.2");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] Error applying initial status visual: " + ex.Message);
            }
        }

        /// <summary>
        /// Removes visual status effect if no more DOT of that type is active.
        /// Call this when an elemental DOT expires.
        /// Note: For burning, the status naturally decays via heat loss.
        /// For electrocute, it has a short duration and will auto-expire.
        /// We reapply during DOT ticks to keep the visual active.
        /// </summary>
        public void RemoveStatusEffectVisualIfNeeded(Creature creature, DamageType damageType)
        {
            // Status effects will naturally expire when we stop reapplying them
            // Burning decays via heat loss, Electrocute has short duration
            // No need to actively remove - they'll fade when DOT ends
            if (BDOTModOptions.DebugLogging && creature != null && !creature.isKilled)
            {
                if (damageType == DamageType.Fire && !HasActiveFireDOT(creature))
                    Debug.Log("[BDOT] Fire DOT ended on " + creature.name + " - Burning will fade");
                else if (damageType == DamageType.Lightning && !HasActiveLightningDOT(creature))
                    Debug.Log("[BDOT] Lightning DOT ended on " + creature.name + " - Electrocute will fade");
            }
        }
    }
}
