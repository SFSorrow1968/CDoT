using System;
using System.Collections.Generic;
using BDOT.Configuration;
using BDOT.Core;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Hooks
{
    public class EventHooks
    {
        private static EventHooks _instance;
        private bool _subscribed = false;
        private bool _spawnSubscribed = false;

        private readonly Dictionary<int, float> _recentSlicedParts = new Dictionary<int, float>();
        private float _lastSliceCleanupTime = 0f;
        private const float SLICE_REARM_SECONDS = 30f;
        private const float SLICE_CLEANUP_INTERVAL = 10f;

        public static void Subscribe()
        {
            if (_instance == null)
            {
                _instance = new EventHooks();
            }
            _instance.SubscribeEvents();
        }

        public static void Unsubscribe()
        {
            _instance?.UnsubscribeEvents();
        }

        public static void ResetState()
        {
            if (_instance != null)
            {
                _instance._recentSlicedParts.Clear();
                _instance._lastSliceCleanupTime = 0f;
            }
        }

        private void SubscribeEvents()
        {
            if (_subscribed)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Already subscribed to events");
                return;
            }

            Debug.Log("[BDOT] Subscribing to EventManager events...");

            try
            {
                EventManager.onCreatureHit += new EventManager.CreatureHitEvent(this.OnCreatureHit);
                EventManager.onCreatureKill += new EventManager.CreatureKillEvent(this.OnCreatureKill);
                SubscribeSpawnEvent();
                _subscribed = true;
                Debug.Log("[BDOT] Event hooks subscribed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] Failed to subscribe to events: " + ex.Message);
                _subscribed = false;
            }
        }

        private void SubscribeSpawnEvent()
        {
            if (_spawnSubscribed) return;

            try
            {
                EventManager.onCreatureSpawn += new EventManager.CreatureSpawnedEvent(this.OnCreatureSpawn);
                _spawnSubscribed = true;
                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Creature spawn hook subscribed");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] Failed to subscribe creature spawn hook: " + ex.Message);
            }
        }

        private void UnsubscribeEvents()
        {
            Debug.Log("[BDOT] Unsubscribing from events...");

            try
            {
                EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(this.OnCreatureHit);
                EventManager.onCreatureKill -= new EventManager.CreatureKillEvent(this.OnCreatureKill);
                if (_spawnSubscribed)
                {
                    EventManager.onCreatureSpawn -= new EventManager.CreatureSpawnedEvent(this.OnCreatureSpawn);
                }
            }
            catch { }

            _subscribed = false;
            _spawnSubscribed = false;
        }

        private void OnCreatureSpawn(Creature creature)
        {
            // Could be used for initialization if needed
        }

        private void OnCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            try
            {
                if (eventTime == EventTime.OnStart) return;
                if (creature == null) return;
                if (creature.isPlayer) return;
                if (creature.isKilled) return;

                if (!BDOTModOptions.EnableMod) return;

                // Get hit info for logging
                var damageType = collisionInstance?.damageStruct.damageType ?? DamageType.Unknown;
                var hitPart = collisionInstance?.damageStruct.hitRagdollPart;
                var hitDamage = collisionInstance?.damageStruct.damage ?? 0f;
                string partTypeName = hitPart?.type.ToString() ?? "null";

                if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] ========== HIT EVENT ==========");
                    Debug.Log("[BDOT] Target: " + creature.name + " | Part: " + partTypeName + " | DamageType: " + damageType + " | Damage: " + hitDamage.ToString("F1"));
                }

                // Detect effective damage type (may differ from reported type due to imbue/spell)
                DamageType effectiveDamageType = GetEffectiveDamageType(collisionInstance, damageType);
                if (effectiveDamageType != damageType && BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Effective damage type: " + effectiveDamageType + " (from imbue/spell)");

                // Check if damage type is allowed by current profile
                if (!BDOTModOptions.IsDamageTypeAllowed(effectiveDamageType))
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] SKIP: DamageType " + effectiveDamageType + " not allowed by profile " + BDOTModOptions.ProfilePresetSetting);
                    return;
                }
                
                // Use effective damage type for DOT
                damageType = effectiveDamageType;

                // Determine body zone from hit part
                BodyZone zone = ZoneDetector.GetZoneFromCollision(collisionInstance);
                if (zone == BodyZone.Unknown)
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] SKIP: Unknown body zone");
                    return;
                }

                // Handle dismemberment separately to avoid double-counting
                if (zone == BodyZone.Dismemberment)
                {
                    if (hitPart != null && !IsNewSlice(hitPart))
                    {
                        if (BDOTModOptions.DebugLogging)
                            Debug.Log("[BDOT] SKIP: Dismemberment already processed for this part");
                        return;
                    }
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] Dismemberment: New slice detected");
                }

                // Get zone config for logging
                var config = BDOTModOptions.GetZoneConfig(zone);
                if (BDOTModOptions.DebugLogging)
                {
                    Debug.Log("[BDOT] Zone: " + zone.GetDisplayName() + " | Enabled: " + config.Enabled);
                    Debug.Log("[BDOT] Config: Chance=" + config.Chance.ToString("F0") + "%, Duration=" + config.Duration.ToString("F1") + "s, DmgPerTick=" + config.Damage.ToString("F2") + ", MaxStacks=" + config.StackLimit);
                }

                // Get the blood effect instance from the collision (if any)
                // This is the effect the game already spawned - we'll extend it instead of spawning new ones
                var bloodEffectInstance = collisionInstance?.effectInstance;

                // Apply bleed effect (chance is checked inside ApplyBleed)
                // Pass the blood effect instance so we can capture and extend it
                bool applied = BleedManager.Instance.ApplyBleed(creature, zone, damageType, hitPart, bloodEffectInstance);
                if (BDOTModOptions.DebugLogging)
                {
                    if (applied)
                    {
                        string effectInfo = bloodEffectInstance != null ? " | Blood VFX captured" : " | No VFX to capture";
                        Debug.Log("[BDOT] RESULT: Bleed APPLIED to " + creature.name + " (" + zone.GetDisplayName() + ")" + effectInfo);
                    }
                    else
                        Debug.Log("[BDOT] RESULT: Bleed NOT applied (zone disabled or other reason)");
                    Debug.Log("[BDOT] ================================");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] OnCreatureHit error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
        {
            try
            {
                if (eventTime == EventTime.OnStart) return;
                if (creature == null) return;

                // Clear bleed effects when creature dies
                BleedManager.Instance.ClearCreature(creature);

                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Creature killed, effects cleared: " + creature.name);
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] OnCreatureKill error: " + ex.Message);
            }
        }

        private bool IsNewSlice(RagdollPart part)
        {
            if (part == null) return false;
            int id = part.GetInstanceID();
            float now = Time.unscaledTime;

            if (_recentSlicedParts.TryGetValue(id, out float lastTime) && now - lastTime < SLICE_REARM_SECONDS)
                return false;

            _recentSlicedParts[id] = now;
            CleanupSliceCache(now);
            return true;
        }

        private void CleanupSliceCache(float now)
        {
            if (now - _lastSliceCleanupTime < SLICE_CLEANUP_INTERVAL)
                return;

            _lastSliceCleanupTime = now;
            List<int> expired = null;
            foreach (var kvp in _recentSlicedParts)
            {
                if (now - kvp.Value > SLICE_REARM_SECONDS)
                {
                    if (expired == null) expired = new List<int>();
                    expired.Add(kvp.Key);
                }
            }

            if (expired == null) return;
            foreach (var key in expired)
                _recentSlicedParts.Remove(key);
        }

        /// <summary>
        /// Determines the effective damage type for DOT purposes by checking imbue and spell types.
        /// Imbued weapons may report Pierce/Slash/Blunt but have Fire/Lightning effects.
        /// Magic projectiles use Energy but may be fire/lightning spells.
        /// </summary>
        private DamageType GetEffectiveDamageType(CollisionInstance collision, DamageType reportedType)
        {
            try
            {
                // Already an elemental type - use as-is
                if (reportedType == DamageType.Fire || reportedType == DamageType.Lightning)
                    return reportedType;

                // Check for imbue on the source collider group
                var imbue = collision?.sourceColliderGroup?.imbue;
                if (imbue != null && imbue.spellCastBase != null)
                {
                    string spellId = imbue.spellCastBase.id;
                    if (ContainsIgnoreCase(spellId, "fire"))
                        return DamageType.Fire;
                    if (ContainsIgnoreCase(spellId, "lightning") || ContainsIgnoreCase(spellId, "electric"))
                        return DamageType.Lightning;
                }

                // Check for imbue on the source item
                var sourceItem = collision?.sourceColliderGroup?.collisionHandler?.item;
                if (sourceItem != null)
                {
                    foreach (var itemImbue in sourceItem.imbues)
                    {
                        if (itemImbue?.spellCastBase != null)
                        {
                            string spellId = itemImbue.spellCastBase.id;
                            if (ContainsIgnoreCase(spellId, "fire"))
                                return DamageType.Fire;
                            if (ContainsIgnoreCase(spellId, "lightning") || ContainsIgnoreCase(spellId, "electric"))
                                return DamageType.Lightning;
                        }
                    }

                    // Check for magic projectiles (fireballs, lightning bolts)
                    var magicProjectile = sourceItem.GetComponent<ItemMagicProjectile>();
                    if (magicProjectile?.imbueSpellCastCharge != null)
                    {
                        string spellId = magicProjectile.imbueSpellCastCharge.id;
                        if (ContainsIgnoreCase(spellId, "fire"))
                            return DamageType.Fire;
                        if (ContainsIgnoreCase(spellId, "lightning") || ContainsIgnoreCase(spellId, "electric"))
                            return DamageType.Lightning;
                        // Gravity/other spells stay as their reported type
                    }
                }

                // For Energy damage that doesn't match fire/lightning, treat as physical (won't apply DOT)
                return reportedType;
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.LogError("[BDOT] Error in GetEffectiveDamageType: " + ex.Message);
                return reportedType;
            }
        }

        private bool WasCausedByPlayer(CollisionInstance collision)
        {
            try
            {
                if (collision == null) return false;

                var sourceItem = collision.sourceColliderGroup?.collisionHandler?.item;

                if (BDOTModOptions.DebugLogging && sourceItem != null)
                {
                    Debug.Log("[BDOT] Checking player source - Item: " + sourceItem.name);
                }

                // Check if the source is a player-held item
                if (sourceItem?.mainHandler?.creature?.isPlayer == true)
                {
                    if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: mainHandler");
                    return true;
                }

                // Check if the source is a previously-held player item (thrown, released)
                if (sourceItem?.lastHandler?.creature?.isPlayer == true)
                {
                    if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: lastHandler");
                    return true;
                }

                // Check if the source is the player's body
                if (collision.sourceColliderGroup?.collisionHandler?.ragdollPart?.ragdoll?.creature?.isPlayer == true)
                {
                    if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: ragdollPart");
                    return true;
                }

                // Check for spell projectiles/imbued items - check the imbue's caster
                var imbue = collision.sourceColliderGroup?.imbue;
                if (imbue?.imbueCreature?.isPlayer == true)
                {
                    if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: sourceColliderGroup.imbue");
                    return true;
                }

                // Check any imbues on the source item
                if (sourceItem != null)
                {
                    foreach (var itemImbue in sourceItem.imbues)
                    {
                        if (itemImbue?.imbueCreature?.isPlayer == true)
                        {
                            if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: item.imbues");
                            return true;
                        }
                    }
                }

                // Check if player is actively holding item via telekinesis
                if (sourceItem != null)
                {
                    foreach (var tkHandler in sourceItem.tkHandlers)
                    {
                        if (tkHandler?.ragdollHand?.creature?.isPlayer == true)
                        {
                            if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: tkHandlers");
                            return true;
                        }
                    }
                }

                // Check for spell projectiles (fireballs, lightning bolts, etc.)
                // These are ItemMagicProjectile objects with a spellCaster reference
                if (sourceItem != null)
                {
                    var magicProjectile = sourceItem.GetComponent<ItemMagicProjectile>();
                    if (magicProjectile != null && BDOTModOptions.DebugLogging)
                    {
                        Debug.Log("[BDOT] Found ItemMagicProjectile - imbueSpellCastCharge: " + (magicProjectile.imbueSpellCastCharge != null ? "exists" : "null"));
                        if (magicProjectile.imbueSpellCastCharge != null)
                        {
                            Debug.Log("[BDOT] spellCaster: " + (magicProjectile.imbueSpellCastCharge.spellCaster != null ? "exists" : "null"));
                            if (magicProjectile.imbueSpellCastCharge.spellCaster != null)
                            {
                                var hand = magicProjectile.imbueSpellCastCharge.spellCaster.ragdollHand;
                                Debug.Log("[BDOT] ragdollHand: " + (hand != null ? "exists" : "null"));
                                if (hand != null)
                                {
                                    Debug.Log("[BDOT] creature: " + (hand.creature != null ? hand.creature.name : "null") + " isPlayer: " + hand.creature?.isPlayer);
                                }
                            }
                        }
                    }
                    if (magicProjectile?.imbueSpellCastCharge?.spellCaster?.ragdollHand?.creature?.isPlayer == true)
                    {
                        if (BDOTModOptions.DebugLogging) Debug.Log("[BDOT] Player detected: ItemMagicProjectile");
                        return true;
                    }
                }

                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Player NOT detected - all checks failed");

                return false;
            }
            catch (Exception ex)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.LogError("[BDOT] Error in WasCausedByPlayer: " + ex.Message);
                return false;
            }
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            return !string.IsNullOrEmpty(source) &&
                   source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
