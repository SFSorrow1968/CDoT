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

                // Check if this was player-caused damage
                if (!WasCausedByPlayer(collisionInstance))
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] SKIP: Not player caused");
                    return;
                }

                if (BDOTModOptions.DebugLogging)
                    Debug.Log("[BDOT] Player caused: YES");

                // Check if damage type is allowed by current profile
                if (!BDOTModOptions.IsDamageTypeAllowed(damageType))
                {
                    if (BDOTModOptions.DebugLogging)
                        Debug.Log("[BDOT] SKIP: DamageType " + damageType + " not allowed by profile " + BDOTModOptions.ProfilePresetSetting);
                    return;
                }

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

                // Apply bleed effect (chance is checked inside ApplyBleed)
                bool applied = BleedManager.Instance.ApplyBleed(creature, zone, damageType);
                if (BDOTModOptions.DebugLogging)
                {
                    if (applied)
                        Debug.Log("[BDOT] RESULT: Bleed APPLIED to " + creature.name + " (" + zone.GetDisplayName() + ")");
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

        private bool WasCausedByPlayer(CollisionInstance collision)
        {
            try
            {
                if (collision == null) return false;

                var sourceItem = collision.sourceColliderGroup?.collisionHandler?.item;

                // Check if the source is a player-held item
                if (sourceItem?.mainHandler?.creature?.isPlayer == true)
                    return true;

                // Check if the source is a previously-held player item (thrown, released)
                if (sourceItem?.lastHandler?.creature?.isPlayer == true)
                    return true;

                // Check if the source is the player's body
                if (collision.sourceColliderGroup?.collisionHandler?.ragdollPart?.ragdoll?.creature?.isPlayer == true)
                    return true;

                // Check for spell projectiles/imbued items - check the imbue's caster
                var imbue = collision.sourceColliderGroup?.imbue;
                if (imbue?.imbueCreature?.isPlayer == true)
                    return true;

                // Check any imbues on the source item
                if (sourceItem != null)
                {
                    foreach (var itemImbue in sourceItem.imbues)
                    {
                        if (itemImbue?.imbueCreature?.isPlayer == true)
                            return true;
                    }
                }

                // Check if player is actively holding item via telekinesis
                if (sourceItem != null)
                {
                    foreach (var tkHandler in sourceItem.tkHandlers)
                    {
                        if (tkHandler?.ragdollHand?.creature?.isPlayer == true)
                            return true;
                    }
                }

                // Check for spell projectiles (fireballs, lightning bolts, etc.)
                // These are ItemMagicProjectile objects with a spellCaster reference
                if (sourceItem != null)
                {
                    var magicProjectile = sourceItem.GetComponent<ItemMagicProjectile>();
                    if (magicProjectile?.imbueSpellCastCharge?.spellCaster?.ragdollHand?.creature?.isPlayer == true)
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
