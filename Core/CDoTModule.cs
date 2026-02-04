using System;
using CDoT.Configuration;
using CDoT.Hooks;
using CDoT.Integration;
using ThunderRoad;
using UnityEngine;

namespace CDoT.Core
{
    public class CDoTModule : ThunderScript
    {
        public static CDoTModule Instance { get; private set; }

        public override void ScriptEnable()
        {
            base.ScriptEnable();

            try
            {
                Instance = this;

#if NOMAD
                Debug.Log("[CDoT] ============================================");
                Debug.Log("[CDoT] === CDoT v" + CDoTModOptions.VERSION + " (Nomad/Quest) ===");
                Debug.Log("[CDoT] ============================================");
#else
                Debug.Log("[CDoT] ============================================");
                Debug.Log("[CDoT] === CDoT v" + CDoTModOptions.VERSION + " (PCVR) ===");
                Debug.Log("[CDoT] ============================================");
#endif

                BleedManager.Instance.Initialize();
                CDoTModOptionVisibility.Instance.Initialize();
                PerformanceMetrics.Instance.Initialize();
                DebugOverlay.Instance.Initialize();

#if NOMAD
                Debug.Log("[CDoT] Subscribing event hooks (Nomad mode)...");
#else
                Debug.Log("[CDoT] Subscribing event hooks (PCVR mode)...");
#endif
                EventHooks.Subscribe();

                // Initialize optional CSM integration (for bleed kill slow motion)
                CSMIntegration.Initialize();

                // Log configuration summary
                LogConfigurationSummary();

                Debug.Log("[CDoT] ScriptEnable complete - CDoT is active!");
                Debug.Log("[CDoT] ============================================");
            }
            catch (Exception ex)
            {
                Debug.LogError("[CDoT] ScriptEnable FAILED: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void LogConfigurationSummary()
        {
            Debug.Log("[CDoT] --- Configuration Summary ---");
            Debug.Log("[CDoT] Enabled: " + CDoTModOptions.EnableMod);
            Debug.Log("[CDoT] Debug Logging: " + CDoTModOptions.DebugLogging);
            Debug.Log("[CDoT] Presets: Damage=" + CDoTModOptions.DamagePresetSetting + ", Duration=" + CDoTModOptions.DurationPresetSetting + ", Frequency=" + CDoTModOptions.FrequencyPresetSetting + ", Chance=" + CDoTModOptions.ChancePresetSetting);
            Debug.Log("[CDoT] DamageType Multipliers: Pierce=" + CDoTModOptions.PierceMultiplier.ToString("F1") + "x, Slash=" + CDoTModOptions.SlashMultiplier.ToString("F1") + "x, Fire=" + CDoTModOptions.FireMultiplier.ToString("F1") + "x, Lightning=" + CDoTModOptions.LightningMultiplier.ToString("F1") + "x");
            Debug.Log("[CDoT] --- Zone Settings ---");

            BodyZone[] zones = { BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso, BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment };
            foreach (var zone in zones)
            {
                var cfg = CDoTModOptions.GetZoneConfig(zone);
                string status = cfg.Enabled ? "ON " : "OFF";
                Debug.Log("[CDoT] " + zone.GetDisplayName().PadRight(13) + " [" + status + "] Chance=" + cfg.Chance.ToString("F0").PadLeft(3) + "% | Dur=" + cfg.Duration.ToString("F1").PadLeft(5) + "s | Freq=" + cfg.Frequency.ToString("F2").PadLeft(4) + "s | Dmg=" + cfg.Damage.ToString("F2").PadLeft(5) + " | Stacks=" + cfg.StackLimit);
            }
        }

        public override void ScriptUpdate()
        {
            try
            {
                base.ScriptUpdate();
                BleedManager.Instance?.Update();
                CDoTModOptionVisibility.Instance?.Update();
                
                // Check for profile changes and sync multipliers
                CDoTModOptions.CheckAndSyncProfileMultipliers();
            }
            catch (Exception ex)
            {
                Debug.LogError("[CDoT] ScriptUpdate error: " + ex.Message);
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
                if (CDoTModOptions.DebugLogging)
                    Debug.LogError("[CDoT] OnGUI error: " + ex.Message);
            }
        }

        public override void ScriptDisable()
        {
            try
            {
                Debug.Log("[CDoT] ScriptDisable...");

                BleedManager.Instance?.ClearAll();
                CDoTModOptionVisibility.Instance?.Shutdown();
                PerformanceMetrics.Instance?.Shutdown();
                DebugOverlay.Instance?.Shutdown();
                EventHooks.Unsubscribe();
                EventHooks.ResetState();

                Debug.Log("[CDoT] CDoT deactivated");
            }
            catch (Exception ex)
            {
                Debug.LogError("[CDoT] ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}
