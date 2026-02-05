using System;
using DOT.Configuration;
using DOT.Hooks;
using DOT.Integration;
using ThunderRoad;
using UnityEngine;

namespace DOT.Core
{
    public class DOTModule : ThunderScript
    {
        public static DOTModule Instance { get; private set; }

        public override void ScriptEnable()
        {
            base.ScriptEnable();

            try
            {
                Instance = this;

#if NOMAD
                Debug.Log("[DOT] ============================================");
                Debug.Log("[DOT] === DOT v" + DOTModOptions.VERSION + " (Nomad/Quest) ===");
                Debug.Log("[DOT] ============================================");
#else
                Debug.Log("[DOT] ============================================");
                Debug.Log("[DOT] === DOT v" + DOTModOptions.VERSION + " (PCVR) ===");
                Debug.Log("[DOT] ============================================");
#endif

                BleedManager.Instance.Initialize();
                DOTModOptionVisibility.Instance.Initialize();
                PerformanceMetrics.Instance.Initialize();
                DebugOverlay.Instance.Initialize();

#if NOMAD
                Debug.Log("[DOT] Subscribing event hooks (Nomad mode)...");
#else
                Debug.Log("[DOT] Subscribing event hooks (PCVR mode)...");
#endif
                EventHooks.Subscribe();

                // Initialize optional CSM integration (for bleed kill slow motion)
                CSMIntegration.Initialize();

                // Log configuration summary
                LogConfigurationSummary();

                Debug.Log("[DOT] ScriptEnable complete - DOT is active!");
                Debug.Log("[DOT] ============================================");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DOT] ScriptEnable FAILED: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void LogConfigurationSummary()
        {
            Debug.Log("[DOT] --- Configuration Summary ---");
            Debug.Log("[DOT] Enabled: " + DOTModOptions.EnableMod);
            Debug.Log("[DOT] Debug Logging: " + DOTModOptions.DebugLogging);
            Debug.Log("[DOT] Presets: Damage=" + DOTModOptions.DamagePresetSetting + ", Duration=" + DOTModOptions.DurationPresetSetting + ", Frequency=" + DOTModOptions.FrequencyPresetSetting + ", Chance=" + DOTModOptions.ChancePresetSetting);
            Debug.Log("[DOT] DamageType Multipliers: Pierce=" + DOTModOptions.PierceMultiplier.ToString("F1") + "x, Slash=" + DOTModOptions.SlashMultiplier.ToString("F1") + "x, Fire=" + DOTModOptions.FireMultiplier.ToString("F1") + "x, Lightning=" + DOTModOptions.LightningMultiplier.ToString("F1") + "x");
            Debug.Log("[DOT] --- Zone Settings ---");

            BodyZone[] zones = { BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso, BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment };
            foreach (var zone in zones)
            {
                var cfg = DOTModOptions.GetZoneConfig(zone);
                string status = cfg.Enabled ? "ON " : "OFF";
                Debug.Log("[DOT] " + zone.GetDisplayName().PadRight(13) + " [" + status + "] Chance=" + cfg.Chance.ToString("F0").PadLeft(3) + "% | Dur=" + cfg.Duration.ToString("F1").PadLeft(5) + "s | Freq=" + cfg.Frequency.ToString("F2").PadLeft(4) + "s | Dmg=" + cfg.Damage.ToString("F2").PadLeft(5) + " | Stacks=" + cfg.StackLimit);
            }
        }

        public override void ScriptUpdate()
        {
            try
            {
                base.ScriptUpdate();
                BleedManager.Instance?.Update();
                DOTModOptionVisibility.Instance?.Update();
                
                // Check for profile changes and sync multipliers
                DOTModOptions.CheckAndSyncProfileMultipliers();
            }
            catch (Exception ex)
            {
                Debug.LogError("[DOT] ScriptUpdate error: " + ex.Message);
            }
        }

        private void OnGUI()
        {
            try
            {
                DebugOverlay.Instance?.Draw();
            }
            catch (Exception ex)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.LogError("[DOT] OnGUI error: " + ex.Message);
            }
        }

        public override void ScriptDisable()
        {
            try
            {
                Debug.Log("[DOT] ScriptDisable...");

                BleedManager.Instance?.ClearAll();
                DOTModOptionVisibility.Instance?.Shutdown();
                PerformanceMetrics.Instance?.Shutdown();
                DebugOverlay.Instance?.Shutdown();
                EventHooks.Unsubscribe();
                EventHooks.ResetState();

                Debug.Log("[DOT] DOT deactivated");
            }
            catch (Exception ex)
            {
                Debug.LogError("[DOT] ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}
