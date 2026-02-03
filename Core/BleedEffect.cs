using System;
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

        // Blood VFX tracking
        public EffectInstance BloodEffectInstance { get; private set; }
        public float CurrentBloodIntensity { get; private set; }
        private float _timeSinceEffectRefresh = 0f;
        private float _timeSinceEffectRespawn = 0f;
        private const float EFFECT_REFRESH_INTERVAL = 0.1f; // Refresh intensity 10x per second
        private const float EFFECT_RESPAWN_INTERVAL = 0.8f; // Re-spawn particles every 0.8s
        private const float FADE_OUT_DURATION = 1.5f; // Start fading out VFX in last 1.5 seconds
        private bool _effectSpawnAttempted = false; // Track if we've tried to spawn
        private EffectData _cachedBloodEffectData = null; // Cache for re-spawning
        private bool _useRendererBinding = false; // Whether to bind to creature renderer
        private Transform _spawnTransform = null; // Cached spawn transform
        private Renderer _cachedRenderer = null; // Cached VFX renderer
        private float _zoneIntensityMultiplier = 1f; // Cached zone multiplier
        private float _damageTypeMultiplier = 1f; // Cached damage type multiplier
        private bool _isFadingOut = false; // Track if we're in fade-out phase

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
        /// Check if the HitPart is still valid
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
            _zoneIntensityMultiplier = GetZoneIntensityMultiplierStatic(zone);
            _damageTypeMultiplier = BDOTModOptions.GetDamageTypeMultiplier(damageType);
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
                // Reset fade-out state if duration extended past fade threshold
                if (RemainingDuration > FADE_OUT_DURATION)
                {
                    _isFadingOut = false;
                }
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
            _timeSinceEffectRefresh += deltaTime;
            _timeSinceEffectRespawn += deltaTime;

            // Check if entering fade-out phase
            if (!_isFadingOut && RemainingDuration <= FADE_OUT_DURATION)
            {
                _isFadingOut = true;
            }

            // Periodically refresh blood effect intensity (handles fade-out)
            if (_timeSinceEffectRefresh >= EFFECT_REFRESH_INTERVAL)
            {
                RefreshBloodEffect();
                _timeSinceEffectRefresh = 0f;
            }

            // Only respawn particles if NOT fading out - let existing particles die naturally
            if (!_isFadingOut && _timeSinceEffectRespawn >= EFFECT_RESPAWN_INTERVAL)
            {
                RespawnBloodEffect();
                _timeSinceEffectRespawn = 0f;
            }
        }

        public float GetTickDamage()
        {
            // Total damage = base damage * stack count * cached damage type multiplier
            return DamagePerTick * StackCount * _damageTypeMultiplier;
        }

        #region Blood VFX Methods

        // Static effect IDs to avoid allocating new array each spawn
        private static readonly string[] BloodEffectIds = new string[]
        {
            "PenetrationDeepBleeding",  // Best for continuous bleeding
            "SliceFleshChild",          // Slice blood spray
            "SliceFleshParent",         // Alternate slice effect  
            "PenetrationDeepFlesh"      // Single impact effect (last resort)
        };

        /// <summary>
        /// Spawns a silent blood effect (no audio) at the hit location.
        /// Uses meshBone-attached effects when available for better visual attachment.
        /// </summary>
        public void SpawnBloodEffect()
        {
            // Only attempt once per bleed effect
            if (_effectSpawnAttempted)
                return;
            _effectSpawnAttempted = true;

            try
            {
                // Need a valid hit part for positioning
                if (!HasValidHitPart)
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Cannot spawn blood effect: no valid hit part");
                    return;
                }

                EffectData bloodEffectData = null;
                Transform spawnTransform = null;
                bool useRendererBinding = false;

                // Determine spawn transform - prefer meshBone for better visual attachment
                if (HitPart.meshBone != null)
                {
                    spawnTransform = HitPart.meshBone.transform;
                    useRendererBinding = true;
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Using meshBone transform for " + HitPart.type);
                }
                else
                {
                    spawnTransform = HitPart.transform;
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Using hitPart transform for " + HitPart.type + " (no meshBone)");
                }

                // Try blood effect IDs in order (using static array)
                for (int i = 0; i < BloodEffectIds.Length; i++)
                {
                    if (Catalog.TryGetData<EffectData>(BloodEffectIds[i], out bloodEffectData, false))
                    {
                        if (BDOTModOptions.DebugLogging)
                            Debug.Log("[BDOT] Found blood effect: " + BloodEffectIds[i]);
                        break;
                    }
                }

                if (bloodEffectData == null)
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] No blood effect data available for " + HitPart.type);
                    return;
                }

                // Use hit part transform if spawnTransform still null
                if (spawnTransform == null)
                    spawnTransform = HitPart.transform;

                // Cache the effect data for re-spawning
                _cachedBloodEffectData = bloodEffectData;
                _useRendererBinding = useRendererBinding;
                _spawnTransform = spawnTransform;

                // Get spawn position and rotation
                Vector3 position = spawnTransform.position;
                Quaternion rotation = Quaternion.LookRotation(spawnTransform.forward, spawnTransform.up);

                // Calculate initial intensity
                CurrentBloodIntensity = CalculateBloodIntensity();

                // Spawn the effect WITHOUT audio by ignoring EffectModuleAudio
                BloodEffectInstance = bloodEffectData.Spawn(
                    position,
                    rotation,
                    spawnTransform,         // Parent to transform so it follows
                    null,                   // No collision instance
                    true,                   // Pooled
                    null,                   // No collider group
                    false,                  // Not from player
                    CurrentBloodIntensity,  // Initial intensity
                    1f,                     // Normal speed
                    typeof(EffectModuleAudio) // IGNORE AUDIO - this is the key!
                );

                if (BloodEffectInstance != null)
                {
                    // Cache and bind to creature's VFX renderer for better visual attachment
                    if (useRendererBinding && Target != null)
                    {
                        _cachedRenderer = Target.GetRendererForVFX();
                        if (_cachedRenderer != null)
                        {
                            BloodEffectInstance.SetRenderer(_cachedRenderer, false);
                        }
                    }

                    BloodEffectInstance.Play(0, false, false);

                    if (BDOTModOptions.DebugLogging)
                    {
                        Debug.Log("[BDOT] Spawned blood effect for " + Zone.GetDisplayName() + 
                                  " | Intensity: " + CurrentBloodIntensity.ToString("F2") + 
                                  " | DamageType: " + DamageType + 
                                  (useRendererBinding ? " (renderer-bound)" : ""));
                    }
                }
                else if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] EffectData.Spawn returned null for " + bloodEffectData.id);
                }
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Failed to spawn blood effect: " + ex.Message);
                BloodEffectInstance = null;
            }
        }

        /// <summary>
        /// Captures an existing blood effect from the game's collision system.
        /// This allows us to extend its lifetime without spawning new effects (which would cause sounds).
        /// </summary>
        public void CaptureBloodEffect(EffectInstance effectInstance)
        {
            try
            {
                if (effectInstance == null)
                    return;

                // Check if effect is valid and has active effects
                if (effectInstance.effects == null || effectInstance.effects.Count == 0)
                    return;

                BloodEffectInstance = effectInstance;
                CurrentBloodIntensity = CalculateBloodIntensity();

                // Immediately refresh to apply our intensity
                RefreshBloodEffect();

                if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] Captured blood effect for " + Zone.GetDisplayName() + " | Intensity: " + CurrentBloodIntensity.ToString("F2"));
                }
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Failed to capture blood effect: " + ex.Message);
                BloodEffectInstance = null;
            }
        }

        /// <summary>
        /// Calculates blood effect intensity based on stacks, damage, zone severity, and user preset.
        /// During fade-out phase, intensity gradually decreases to zero.
        /// </summary>
        public float CalculateBloodIntensity()
        {
            // Base intensity from stacks and damage (using cached zone multiplier)
            float baseIntensity = (StackCount * DamagePerTick * _zoneIntensityMultiplier) / 5f;

            // Apply Blood Amount preset multiplier
            float presetMultiplier = BDOTModOptions.GetBloodAmountMultiplier();

            // Calculate final intensity
            float finalIntensity = baseIntensity * presetMultiplier;

            // Apply fade-out if in final phase
            if (_isFadingOut && RemainingDuration > 0f)
            {
                float fadeProgress = RemainingDuration / FADE_OUT_DURATION;
                finalIntensity *= fadeProgress;
            }

            return Mathf.Clamp(finalIntensity, 0.05f, 5.0f);
        }

        /// <summary>
        /// Gets intensity multiplier based on zone severity (static version for caching).
        /// </summary>
        private static float GetZoneIntensityMultiplierStatic(BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return 1.5f;
                case BodyZone.Head: return 1.2f;
                case BodyZone.Neck: return 1.3f;
                case BodyZone.Torso: return 1.0f;
                case BodyZone.Arm: return 0.7f;
                case BodyZone.Leg: return 0.8f;
                case BodyZone.Dismemberment: return 2.0f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// Refreshes the blood effect to keep it active and update intensity.
        /// Called periodically during Update.
        /// </summary>
        public void RefreshBloodEffect()
        {
            try
            {
                if (BloodEffectInstance == null)
                    return;

                // Check if effect was despawned externally
                if (BloodEffectInstance.effects == null || BloodEffectInstance.effects.Count == 0)
                {
                    BloodEffectInstance = null;
                    return;
                }

                // Update intensity based on current state
                CurrentBloodIntensity = CalculateBloodIntensity();
                BloodEffectInstance.SetIntensity(CurrentBloodIntensity);

                // Keep effect playing if it stopped
                if (!BloodEffectInstance.isPlaying)
                {
                    BloodEffectInstance.Play(0, false, false);
                }
            }
            catch (Exception ex)
            {
                // Effect became invalid, clear reference
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Blood effect refresh failed: " + ex.Message);
                BloodEffectInstance = null;
            }
        }

        /// <summary>
        /// Re-spawns blood particles periodically to maintain continuous bleeding visuals.
        /// Particle effects have finite lifetimes, so we need to spawn new ones.
        /// </summary>
        private void RespawnBloodEffect()
        {
            try
            {
                // Need valid spawn transform and cached effect data
                if (_spawnTransform == null || _cachedBloodEffectData == null)
                    return;

                // Verify transform is still valid
                try
                {
                    if (_spawnTransform.gameObject == null || !_spawnTransform.gameObject.activeInHierarchy)
                    {
                        _spawnTransform = null;
                        return;
                    }
                }
                catch
                {
                    _spawnTransform = null;
                    return;
                }

                // End any existing effect gracefully
                if (BloodEffectInstance != null)
                {
                    try
                    {
                        if (BloodEffectInstance.effects != null && BloodEffectInstance.effects.Count > 0)
                        {
                            BloodEffectInstance.End(false, -1f);
                        }
                    }
                    catch { }
                    BloodEffectInstance = null;
                }

                // Get spawn position and rotation
                Vector3 position = _spawnTransform.position;
                Quaternion rotation = Quaternion.LookRotation(_spawnTransform.forward, _spawnTransform.up);

                // Update intensity
                CurrentBloodIntensity = CalculateBloodIntensity();

                // Spawn new effect WITHOUT audio
                BloodEffectInstance = _cachedBloodEffectData.Spawn(
                    position,
                    rotation,
                    _spawnTransform,
                    null,
                    true,
                    null,
                    false,
                    CurrentBloodIntensity,
                    1f,
                    typeof(EffectModuleAudio)
                );

                if (BloodEffectInstance != null)
                {
                    // Re-apply cached renderer binding if available
                    if (_useRendererBinding && _cachedRenderer != null)
                    {
                        BloodEffectInstance.SetRenderer(_cachedRenderer, false);
                    }

                    BloodEffectInstance.Play(0, false, false);
                }
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Blood effect respawn failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Called when the bleed effect expires or is cleared.
        /// Allows the blood effect to end naturally.
        /// </summary>
        public void ReleaseBloodEffect()
        {
            try
            {
                if (BloodEffectInstance != null)
                {
                    // Check if still valid before ending
                    if (BloodEffectInstance.effects != null && BloodEffectInstance.effects.Count > 0)
                    {
                        // End with normal speed for smooth fade-out
                        BloodEffectInstance.End(false, 1.0f);

                        if (BDOTModOptions.DebugLogging)
                            Debug.Log("[BDOT] Released blood effect for " + Zone.GetDisplayName());
                    }
                }
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Error releasing blood effect: " + ex.Message);
            }
            finally
            {
                BloodEffectInstance = null;
                CurrentBloodIntensity = 0f;
                _cachedRenderer = null;
            }
        }

        /// <summary>
        /// Called when stacking to boost the blood effect intensity.
        /// </summary>
        public void OnStackAdded()
        {
            // Immediately recalculate and refresh intensity
            if (BloodEffectInstance != null)
            {
                CurrentBloodIntensity = CalculateBloodIntensity();
                RefreshBloodEffect();

                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Blood intensity increased on stack: " + CurrentBloodIntensity.ToString("F2"));
            }
        }

        #endregion

        public override string ToString()
        {
            string effectInfo = BloodEffectInstance != null ? " | VFX:" + CurrentBloodIntensity.ToString("F1") : "";
            return $"BleedEffect[{Zone.GetDisplayName()} x{StackCount} | {RemainingDuration:F1}s | {GetTickDamage():F1} dmg/tick | {DamageType}{effectInfo}]";
        }
    }
}
