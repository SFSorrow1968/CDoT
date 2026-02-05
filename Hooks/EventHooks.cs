using System;
using System.Collections.Generic;
using DOT.Configuration;
using DOT.Core;
using ThunderRoad;
using UnityEngine;

namespace DOT.Hooks
{
    public class EventHooks
    {
        private static EventHooks _instance;
        private bool _subscribed = false;
        private bool _spawnSubscribed = false;

        private readonly Dictionary<int, float> _recentSlicedParts = new Dictionary<int, float>(16);
        private readonly List<int> _expiredSliceParts = new List<int>(8); // Reusable list for cleanup
        private float _lastSliceCleanupTime = 0f;
        private const float SLICE_REARM_SECONDS = 30f;
        private const float SLICE_CLEANUP_INTERVAL = 10f;

        // Store delegate references to ensure proper unsubscription (prevents memory leaks)
        private EventManager.CreatureHitEvent _onCreatureHitHandler;
        private EventManager.CreatureKillEvent _onCreatureKillHandler;
        private EventManager.CreatureSpawnedEvent _onCreatureSpawnHandler;
        private EventManager.LevelLoadEvent _onLevelUnloadHandler;

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
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] Already subscribed to events");
                return;
            }

            Debug.Log("[DOT] Subscribing to EventManager events...");

            try
            {
                // Create and store delegate references for proper unsubscription
                _onCreatureHitHandler = new EventManager.CreatureHitEvent(this.OnCreatureHit);
                _onCreatureKillHandler = new EventManager.CreatureKillEvent(this.OnCreatureKill);

                EventManager.onCreatureHit += _onCreatureHitHandler;
                EventManager.onCreatureKill += _onCreatureKillHandler;
                SubscribeSpawnEvent();

                // Subscribe to level unload for cleanup on scene transitions
                _onLevelUnloadHandler = new EventManager.LevelLoadEvent(this.OnLevelUnload);
                EventManager.onLevelUnload += _onLevelUnloadHandler;

                _subscribed = true;
                Debug.Log("[DOT] Event hooks subscribed successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DOT] Failed to subscribe to events: {ex.Message}");
                _subscribed = false;
            }
        }

        private void SubscribeSpawnEvent()
        {
            if (_spawnSubscribed) return;

            try
            {
                _onCreatureSpawnHandler = new EventManager.CreatureSpawnedEvent(this.OnCreatureSpawn);
                EventManager.onCreatureSpawn += _onCreatureSpawnHandler;
                _spawnSubscribed = true;
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] Creature spawn hook subscribed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DOT] Failed to subscribe creature spawn hook: {ex.Message}");
            }
        }

        private void UnsubscribeEvents()
        {
            Debug.Log("[DOT] Unsubscribing from events...");

            try
            {
                // Use stored delegate references to ensure proper unsubscription
                if (_onCreatureHitHandler != null)
                    EventManager.onCreatureHit -= _onCreatureHitHandler;
                if (_onCreatureKillHandler != null)
                    EventManager.onCreatureKill -= _onCreatureKillHandler;
                if (_spawnSubscribed && _onCreatureSpawnHandler != null)
                    EventManager.onCreatureSpawn -= _onCreatureSpawnHandler;
                if (_onLevelUnloadHandler != null)
                    EventManager.onLevelUnload -= _onLevelUnloadHandler;
            }
            catch (Exception ex)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.LogWarning($"[DOT] Error during event unsubscription: {ex.Message}");
            }

            // Clear delegate references
            _onCreatureHitHandler = null;
            _onCreatureKillHandler = null;
            _onCreatureSpawnHandler = null;
            _onLevelUnloadHandler = null;

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

                if (!DOTModOptions.EnableMod) return;

                // Get hit info for logging
                var damageType = collisionInstance?.damageStruct.damageType ?? DamageType.Unknown;
                var hitPart = collisionInstance?.damageStruct.hitRagdollPart;
                var hitDamage = collisionInstance?.damageStruct.damage ?? 0f;
                string partTypeName = hitPart?.type.ToString() ?? "null";

                if (DOTModOptions.DebugLogging)
                {
                    Debug.Log("[DOT] ========== HIT EVENT ==========");
                    Debug.Log($"[DOT] Target: {creature.name} | Part: {partTypeName} | DamageType: {damageType} | Damage: {hitDamage:F1}");
                }

                // Detect effective damage type (may differ from reported type due to imbue/spell)
                DamageType effectiveDamageType = GetEffectiveDamageType(collisionInstance, damageType);
                if (effectiveDamageType != damageType && DOTModOptions.DebugLogging)
                    Debug.Log($"[DOT] Effective damage type: {effectiveDamageType} (from imbue/spell)");

                // Blunt damage does not cause bleeding
                if (effectiveDamageType == DamageType.Blunt)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] SKIP: Blunt damage does not cause bleeding");
                    return;
                }

                // Check if damage type is allowed by current profile
                if (!DOTModOptions.IsDamageTypeAllowed(effectiveDamageType))
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log($"[DOT] SKIP: DamageType {effectiveDamageType} not allowed by profile {DOTModOptions.ProfilePresetSetting}");
                    return;
                }
                
                // Use effective damage type for DOT
                damageType = effectiveDamageType;

                // Determine body zone from hit part
                BodyZone zone = ZoneDetector.GetZoneFromCollision(collisionInstance);
                if (zone == BodyZone.Unknown)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] SKIP: Unknown body zone");
                    return;
                }

                // Handle dismemberment separately to avoid double-counting
                if (zone == BodyZone.Dismemberment)
                {
                    if (hitPart != null && !IsNewSlice(hitPart))
                    {
                        if (DOTModOptions.DebugLogging)
                            Debug.Log("[DOT] SKIP: Dismemberment already processed for this part");
                        return;
                    }
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] Dismemberment: New slice detected");
                }

                // Get zone config for logging
                var config = DOTModOptions.GetZoneConfig(zone);
                if (DOTModOptions.DebugLogging)
                {
                    Debug.Log($"[DOT] Zone: {zone.GetDisplayName()} | Enabled: {config.Enabled}");
                    Debug.Log($"[DOT] Config: Chance={config.Chance:F0}%, Duration={config.Duration:F1}s, DmgPerTick={config.Damage:F2}, MaxStacks={config.StackLimit}");
                }

                // Apply bleed effect (chance is checked inside ApplyBleed)
                // Blood VFX is now spawned silently (without audio) inside BleedEffect
                bool applied = BleedManager.Instance.ApplyBleed(creature, zone, damageType, hitPart);
                if (DOTModOptions.DebugLogging)
                {
                    if (applied)
                        Debug.Log($"[DOT] RESULT: Bleed APPLIED to {creature.name} ({zone.GetDisplayName()})");
                    else
                        Debug.Log("[DOT] RESULT: Bleed NOT applied (zone disabled or other reason)");
                    Debug.Log("[DOT] ================================");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DOT] OnCreatureHit error: {ex.Message}\n{ex.StackTrace}");
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

                if (DOTModOptions.DebugLogging)
                    Debug.Log($"[DOT] Creature killed, effects cleared: {creature.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DOT] OnCreatureKill error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles level unload events to clean up state and prevent orphaned effects.
        /// </summary>
        private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
        {
            // Only handle OnEnd to avoid double-processing
            if (eventTime != EventTime.OnEnd) return;

            try
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] Level unloading - cleaning up all bleed effects");

                // Clear all active bleed effects
                BleedManager.Instance.ClearAll();

                // Reset tracking state
                ResetState();

                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] Level cleanup complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DOT] Error during level unload cleanup: {ex.Message}");
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
            _expiredSliceParts.Clear();
            
            var enumerator = _recentSlicedParts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (now - enumerator.Current.Value > SLICE_REARM_SECONDS)
                {
                    _expiredSliceParts.Add(enumerator.Current.Key);
                }
            }

            for (int i = 0; i < _expiredSliceParts.Count; i++)
            {
                _recentSlicedParts.Remove(_expiredSliceParts[i]);
            }
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
                    var imbues = sourceItem.imbues;
                    for (int i = 0; i < imbues.Count; i++)
                    {
                        var itemImbue = imbues[i];
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
                if (DOTModOptions.DebugLogging)
                    Debug.LogError($"[DOT] Error in GetEffectiveDamageType: {ex.Message}");
                return reportedType;
            }
        }

        private bool WasCausedByPlayer(CollisionInstance collision)
        {
            try
            {
                if (collision == null) return false;

                var sourceItem = collision.sourceColliderGroup?.collisionHandler?.item;

                if (DOTModOptions.DebugLogging && sourceItem != null)
                {
                    Debug.Log($"[DOT] Checking player source - Item: {sourceItem.name}");
                }

                // Check if the source is a player-held item
                if (sourceItem?.mainHandler?.creature?.isPlayer == true)
                {
                    if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: mainHandler");
                    return true;
                }

                // Check if the source is a previously-held player item (thrown, released)
                if (sourceItem?.lastHandler?.creature?.isPlayer == true)
                {
                    if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: lastHandler");
                    return true;
                }

                // Check if the source is the player's body
                if (collision.sourceColliderGroup?.collisionHandler?.ragdollPart?.ragdoll?.creature?.isPlayer == true)
                {
                    if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: ragdollPart");
                    return true;
                }

                // Check for spell projectiles/imbued items - check the imbue's caster
                var imbue = collision.sourceColliderGroup?.imbue;
                if (imbue?.imbueCreature?.isPlayer == true)
                {
                    if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: sourceColliderGroup.imbue");
                    return true;
                }

                // Check any imbues on the source item
                if (sourceItem != null)
                {
                    var imbues = sourceItem.imbues;
                    for (int i = 0; i < imbues.Count; i++)
                    {
                        if (imbues[i]?.imbueCreature?.isPlayer == true)
                        {
                            if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: item.imbues");
                            return true;
                        }
                    }
                }

                // Check if player is actively holding item via telekinesis
                if (sourceItem != null)
                {
                    var tkHandlers = sourceItem.tkHandlers;
                    for (int i = 0; i < tkHandlers.Count; i++)
                    {
                        if (tkHandlers[i]?.ragdollHand?.creature?.isPlayer == true)
                        {
                            if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: tkHandlers");
                            return true;
                        }
                    }
                }

                // Check for spell projectiles (fireballs, lightning bolts, etc.)
                // These are ItemMagicProjectile objects with a spellCaster reference
                if (sourceItem != null)
                {
                    var magicProjectile = sourceItem.GetComponent<ItemMagicProjectile>();
                    if (magicProjectile != null && DOTModOptions.DebugLogging)
                    {
                        Debug.Log($"[DOT] Found ItemMagicProjectile - imbueSpellCastCharge: {(magicProjectile.imbueSpellCastCharge != null ? "exists" : "null")}");
                        if (magicProjectile.imbueSpellCastCharge != null)
                        {
                            Debug.Log($"[DOT] spellCaster: {(magicProjectile.imbueSpellCastCharge.spellCaster != null ? "exists" : "null")}");
                            if (magicProjectile.imbueSpellCastCharge.spellCaster != null)
                            {
                                var hand = magicProjectile.imbueSpellCastCharge.spellCaster.ragdollHand;
                                Debug.Log($"[DOT] ragdollHand: {(hand != null ? "exists" : "null")}");
                                if (hand != null)
                                {
                                    Debug.Log($"[DOT] creature: {(hand.creature != null ? hand.creature.name : "null")} isPlayer: {hand.creature?.isPlayer}");
                                }
                            }
                        }
                    }
                    if (magicProjectile?.imbueSpellCastCharge?.spellCaster?.ragdollHand?.creature?.isPlayer == true)
                    {
                        if (DOTModOptions.DebugLogging) Debug.Log("[DOT] Player detected: ItemMagicProjectile");
                        return true;
                    }
                }

                if (DOTModOptions.DebugLogging)
                    Debug.Log("[DOT] Player NOT detected - all checks failed");

                return false;
            }
            catch (Exception ex)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.LogError($"[DOT] Error in WasCausedByPlayer: {ex.Message}");
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
