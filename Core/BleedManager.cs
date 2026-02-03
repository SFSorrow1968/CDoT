using System;
using System.Collections.Generic;
using CDoT.Configuration;
using CDoT.Integration;
using ThunderRoad;
using UnityEngine;

namespace CDoT.Core
{
    public class BleedManager
    {
        private static BleedManager _instance;
        public static BleedManager Instance => _instance ??= new BleedManager();

        private readonly Dictionary<int, List<BleedEffect>> _activeEffects = new Dictionary<int, List<BleedEffect>>(32);
        private readonly Dictionary<(int creatureId, BodyZone zone), BleedEffect> _effectsByZone = new Dictionary<(int, BodyZone), BleedEffect>(64); // O(1) zone lookup
        private readonly List<int> _creaturesToRemove = new List<int>(16);
        private readonly List<BleedEffect> _effectsToRemove = new List<BleedEffect>(8);
        private readonly List<int> _creatureIds = new List<int>(32); // For safe iteration
        private float _lastStatusLogTime = 0f;
        private const float STATUS_LOG_INTERVAL = 5f; // Log status every 5s when effects are active

        // Fire DOT status effect constants
        private const float FIRE_HEAT_MULTIPLIER = 15f;     // Scales DOT damage to heat buildup (damage * multiplier = heat)
        private const float FIRE_INITIAL_HEAT = 20f;        // Initial heat application to start smoking

        // Lightning DOT status effect constants
        private const float LIGHTNING_POWER_MULTIPLIER = 0.3f;  // Scales DOT damage to electrocute power
        private const float LIGHTNING_MIN_POWER = 0.1f;         // Minimum electrocute power
        private const float LIGHTNING_MAX_POWER = 1.0f;         // Maximum electrocute power
        private const float LIGHTNING_DURATION = 2f;            // Electrocute duration per tick (refreshed)
        private const float LIGHTNING_INITIAL_POWER = 0.2f;     // Initial electrocute power

        public void Initialize()
        {
            try
            {
                _activeEffects.Clear();
                _effectsByZone.Clear();
                Debug.Log("[CDoT] BleedManager initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError("[CDoT] BleedManager init failed: " + ex.Message);
            }
        }

        public void Update()
        {
            if (!CDoTModOptions.EnableMod)
                return;

            try
            {
                if (_activeEffects.Count == 0)
                    return;

                float deltaTime = Time.unscaledDeltaTime;
                bool debugLogging = CDoTModOptions.DebugLogging; // Cache for this frame

                _creaturesToRemove.Clear();

                // Copy keys using struct enumerator to avoid allocation
                _creatureIds.Clear();
                var keyEnumerator = _activeEffects.GetEnumerator();
                while (keyEnumerator.MoveNext())
                {
                    _creatureIds.Add(keyEnumerator.Current.Key);
                }

                for (int ci = 0; ci < _creatureIds.Count; ci++)
                {
                    int creatureId = _creatureIds[ci];
                    if (!_activeEffects.TryGetValue(creatureId, out var effects))
                        continue;

                    _effectsToRemove.Clear();

                    for (int i = 0; i < effects.Count; i++)
                    {
                        var effect = effects[i];
                        try
                        {
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
                        catch (Exception effectEx)
                        {
                            // Error processing this effect - mark for removal and continue
                            if (debugLogging)
                                Debug.LogWarning($"[CDoT] Error processing effect, marking for removal: {effectEx.Message}");
                            _effectsToRemove.Add(effect);
                        }
                    }

                    // Remove expired/invalid effects
                    for (int ri = 0; ri < _effectsToRemove.Count; ri++)
                    {
                        var effect = _effectsToRemove[ri];
                        effects.Remove(effect);
                        _effectsByZone.Remove((creatureId, effect.Zone)); // Remove from O(1) lookup

                        // Release blood VFX effect so it can end naturally
                        effect.ReleaseBloodEffect();

                        // Remove visual status effect if no more fire/lightning DOT
                        if (effect.DamageType == DamageType.Fire || effect.DamageType == DamageType.Lightning)
                        {
                            RemoveStatusEffectVisualIfNeeded(effect.Target, effect.DamageType);
                        }

                        if (debugLogging)
                        {
                            string reason = effect.IsExpired ? "duration ended" : "target invalid/killed";
                            Debug.Log($"[CDoT] EXPIRED: {effect.Zone.GetDisplayName()} on {effect.Target?.name ?? "null"} ({reason})");
                        }
                    }

                    // Mark creature for removal if no effects remain
                    if (effects.Count == 0)
                    {
                        _creaturesToRemove.Add(creatureId);
                    }
                }

                // Clean up creatures with no effects
                for (int i = 0; i < _creaturesToRemove.Count; i++)
                {
                    _activeEffects.Remove(_creaturesToRemove[i]);
                }

                // Periodic status logging
                if (debugLogging && _activeEffects.Count > 0)
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
                Debug.LogError("[CDoT] BleedManager Update error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void LogActiveEffectsStatus()
        {
            int totalEffects = GetActiveEffectCount();
            int creatures = _activeEffects.Count;

            Debug.Log("[CDoT] --- Active Bleeds Status ---");
            Debug.Log($"[CDoT] Creatures: {creatures} | Effects: {totalEffects}");

            var enumerator = _activeEffects.GetEnumerator();
            var sb = new System.Text.StringBuilder(128);
            while (enumerator.MoveNext())
            {
                var effects = enumerator.Current.Value;
                if (effects.Count > 0 && effects[0].Target != null)
                {
                    string creatureName = effects[0].Target.name;
                    sb.Clear();
                    for (int i = 0; i < effects.Count; i++)
                    {
                        var effect = effects[i];
                        if (sb.Length > 0) sb.Append(", ");
                        sb.Append(effect.Zone.GetDisplayName())
                          .Append(" x").Append(effect.StackCount)
                          .Append(" (").Append(effect.RemainingDuration.ToString("F1")).Append("s)");
                    }
                    Debug.Log($"[CDoT]   {creatureName}: {sb}");
                }
            }
            Debug.Log("[CDoT] --------------------------------");
        }

        public bool ApplyBleed(Creature target, BodyZone zone, DamageType damageType, RagdollPart hitPart = null)
        {
            if (target == null || target.isKilled || target.isPlayer)
                return false;

            if (!CDoTModOptions.EnableMod)
                return false;

            var config = CDoTModOptions.GetZoneConfig(zone);
            if (!config.Enabled)
            {
                if (CDoTModOptions.DebugLogging)
                    Debug.Log($"[CDoT] Zone disabled: {zone.GetDisplayName()}");
                return false;
            }

            // Check chance roll
            float roll = UnityEngine.Random.value * 100f;
            if (roll > config.Chance)
            {
                if (CDoTModOptions.DebugLogging)
                    Debug.Log($"[CDoT] Chance roll failed: {roll:F1} > {config.Chance:F0}%");
                return false;
            }

            int creatureId = target.GetInstanceID();

            if (!_activeEffects.TryGetValue(creatureId, out var effects))
            {
                effects = new List<BleedEffect>(4); // Pre-allocate for typical max zones
                _activeEffects[creatureId] = effects;
            }

            // O(1) lookup for existing effect on same zone
            var zoneKey = (creatureId, zone);
            _effectsByZone.TryGetValue(zoneKey, out BleedEffect existingEffect);

            if (existingEffect != null)
            {
                // Stack the effect
                int oldStacks = existingEffect.StackCount;
                float oldDuration = existingEffect.RemainingDuration;
                existingEffect.AddStack(config.Damage, config.Duration, config.StackLimit, hitPart);
                
                // Boost blood effect intensity on stack
                existingEffect.OnStackAdded();
                
                if (CDoTModOptions.DebugLogging)
                {
                    Debug.Log($"[CDoT] STACK: {zone.GetDisplayName()} on {target.name}");
                    Debug.Log($"[CDoT]   Stacks: {oldStacks} -> {existingEffect.StackCount} (max={config.StackLimit})");
                    Debug.Log($"[CDoT]   Duration: {oldDuration:F1}s -> {existingEffect.RemainingDuration:F1}s");
                    Debug.Log($"[CDoT]   New tick damage: {existingEffect.GetTickDamage():F2}");
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
                _effectsByZone[zoneKey] = newEffect; // Add to O(1) lookup

                // Spawn silent blood effect for visual feedback
                newEffect.SpawnBloodEffect();
                
                if (CDoTModOptions.DebugLogging)
                {
                    float damageTypeMult = CDoTModOptions.GetDamageTypeMultiplier(damageType);
                    Debug.Log($"[CDoT] NEW BLEED: {zone.GetDisplayName()} on {target.name}");
                    Debug.Log($"[CDoT]   BaseDmg={config.Damage:F2} | DamageType={damageType} ({damageTypeMult:F1}x) | Duration={config.Duration:F1}s | TickInterval={config.Frequency:F2}s");
                    Debug.Log($"[CDoT]   Tick damage: {newEffect.GetTickDamage():F2}");
                    Debug.Log($"[CDoT]   HitPart: {(hitPart != null ? hitPart.type.ToString() : "null")}");
                    Debug.Log($"[CDoT]   BloodVFX: {(newEffect.BloodEffectInstance != null ? "Active" : "None")}");
                }
            }

            // Apply visual status effect for fire/lightning DOT
            if (damageType == DamageType.Fire || damageType == DamageType.Lightning)
            {
                ApplyStatusEffectVisual(target, damageType);
            }

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
                    if (CDoTModOptions.DebugLogging)
                        Debug.Log($"[CDoT] *** BLEED KILL! {effect.Zone.GetDisplayName()} bleed killed {target.name}! ***");

                    // Notify CSM of bleed kill for optional slow motion trigger
                    // Wrapped in try/catch for extra safety - CSM errors should never crash CDoT
                    try
                    {
                        CSMIntegration.NotifyBleedKill(target, effect.DamageType, damage);
                    }
                    catch (Exception csmEx)
                    {
                        if (CDoTModOptions.DebugLogging)
                            Debug.LogWarning($"[CDoT] CSM integration error (non-fatal): {csmEx.Message}");
                    }

                    return;
                }
                
                // Apply damage directly to health (bypasses InvokeCreatureHit)
                target.currentHealth = newHealth;
                healthAfter = newHealth;
            }
            catch (Exception ex)
            {
                // Damage application failed - creature is likely destroyed or in invalid state
                if (CDoTModOptions.DebugLogging)
                {
                    Debug.Log($"[CDoT] Tick failed to apply damage: {ex.Message}");
                }
                return;
            }

            if (CDoTModOptions.DebugLogging && !killedByBleed)
            {
                float damageTypeMult = CDoTModOptions.GetDamageTypeMultiplier(effect.DamageType);
                string healthInfo = (healthBefore >= 0f && healthAfter >= 0f)
                    ? $"{healthBefore:F1} -> {healthAfter:F1}"
                    : "N/A";
                Debug.Log($"[CDoT] TICK: {effect.Zone.GetDisplayName()} x{effect.StackCount} on {target?.name ?? "destroyed"}");
                Debug.Log($"[CDoT]   Damage: {damage:F2} (base={effect.DamagePerTick:F2} * stacks={effect.StackCount} * {effect.DamageType}={damageTypeMult:F1}x)");
                Debug.Log($"[CDoT]   Health: {healthInfo} | Remaining: {effect.RemainingDuration:F1}s");
            }
        }

        public void ClearCreature(Creature creature)
        {
            if (creature == null)
                return;

            int creatureId = creature.GetInstanceID();
            if (_activeEffects.TryGetValue(creatureId, out var effects))
            {
                // Release blood VFX and remove from zone lookup for each effect
                for (int i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    effect.ReleaseBloodEffect();
                    _effectsByZone.Remove((creatureId, effect.Zone));
                }
                effects.Clear();
                _activeEffects.Remove(creatureId);
                if (CDoTModOptions.DebugLogging)
                    Debug.Log($"[CDoT] Cleared effects for: {creature.name}");
            }
        }

        public void ClearAll()
        {
            // Release all blood VFX before clearing
            var enumerator = _activeEffects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var effects = enumerator.Current.Value;
                for (int i = 0; i < effects.Count; i++)
                {
                    effects[i].ReleaseBloodEffect();
                }
                effects.Clear();
            }
            _activeEffects.Clear();
            _effectsByZone.Clear();
            if (CDoTModOptions.DebugLogging)
                Debug.Log("[CDoT] All effects cleared");
        }

        public int GetActiveEffectCount()
        {
            int count = 0;
            // Use dictionary enumerator directly to avoid KeyValuePair allocation
            var enumerator = _activeEffects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                count += enumerator.Current.Value.Count;
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
            
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].DamageType == DamageType.Fire && !effects[i].IsExpired)
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
            
            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i].DamageType == DamageType.Lightning && !effects[i].IsExpired)
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
                    // Heat threshold for ignition is typically around 100
                    float heatToAdd = damage * FIRE_HEAT_MULTIPLIER;

                    creature.Inflict("Burning", this, float.PositiveInfinity, heatToAdd, true);
                    
                    if (CDoTModOptions.DebugLogging)
                    {
                        var burning = creature.GetStatusOfType<Burning>();
                        string heatInfo = burning != null ? $" | Heat: {burning.Heat:F1} | Ignited: {burning.isIgnited}" : "";
                        Debug.Log($"[CDoT] Fire tick on {creature.name} | Damage: {damage:F2} | Heat added: {heatToAdd:F1}{heatInfo}");
                    }
                }
                else if (damageType == DamageType.Lightning)
                {
                    // Use TryElectrocute with power scaled by damage
                    // Power accumulates: existing power = (power + newPower) * 0.5
                    // Higher power = more intense convulsions and visual effects
                    float power = Mathf.Clamp(damage * LIGHTNING_POWER_MULTIPLIER, LIGHTNING_MIN_POWER, LIGHTNING_MAX_POWER);

                    creature.TryElectrocute(power, LIGHTNING_DURATION, true, false, null);
                    
                    if (CDoTModOptions.DebugLogging)
                    {
                        bool isElectrocuted = creature.brain != null && creature.brain.isElectrocuted;
                        Debug.Log($"[CDoT] Lightning tick on {creature.name} | Damage: {damage:F2} | Power: {power:F2} | Electrocuted: {isElectrocuted}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CDoT] Error applying status effect: {ex.Message}");
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
                    creature.Inflict("Burning", this, float.PositiveInfinity, FIRE_INITIAL_HEAT, true);
                    if (CDoTModOptions.DebugLogging)
                        Debug.Log($"[CDoT] Started fire DOT on {creature.name} | Initial heat: {FIRE_INITIAL_HEAT}");
                }
                else if (damageType == DamageType.Lightning)
                {
                    // Initial electrocute with low power - will intensify with each tick
                    creature.TryElectrocute(LIGHTNING_INITIAL_POWER, LIGHTNING_DURATION, true, false, null);
                    if (CDoTModOptions.DebugLogging)
                        Debug.Log($"[CDoT] Started lightning DOT on {creature.name} | Initial power: {LIGHTNING_INITIAL_POWER}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CDoT] Error applying initial status visual: {ex.Message}");
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
            if (CDoTModOptions.DebugLogging && creature != null && !creature.isKilled)
            {
                if (damageType == DamageType.Fire && !HasActiveFireDOT(creature))
                    Debug.Log($"[CDoT] Fire DOT ended on {creature.name} - Burning will fade");
                else if (damageType == DamageType.Lightning && !HasActiveLightningDOT(creature))
                    Debug.Log($"[CDoT] Lightning DOT ended on {creature.name} - Electrocute will fade");
            }
        }
    }
}
