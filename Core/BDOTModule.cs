using System;
using BDOT.Configuration;
using BDOT.Hooks;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Core
{
    public class BDOTModule : ThunderScript
    {
        public static BDOTModule Instance { get; private set; }

        public override void ScriptEnable()
        {
            base.ScriptEnable();

            try
            {
                Instance = this;

#if NOMAD
                Debug.Log("[BDOT] ============================================");
                Debug.Log("[BDOT] === BDOT v" + BDOTModOptions.VERSION + " (Nomad/Quest) ===");
                Debug.Log("[BDOT] ============================================");
#else
                Debug.Log("[BDOT] ============================================");
                Debug.Log("[BDOT] === BDOT v" + BDOTModOptions.VERSION + " (PCVR) ===");
                Debug.Log("[BDOT] ============================================");
#endif

                BleedManager.Instance.Initialize();
                BDOTModOptionVisibility.Instance.Initialize();

#if NOMAD
                Debug.Log("[BDOT] Subscribing event hooks (Nomad mode)...");
#else
                Debug.Log("[BDOT] Subscribing event hooks (PCVR mode)...");
#endif
                EventHooks.Subscribe();

                // Log configuration summary
                LogConfigurationSummary();

                Debug.Log("[BDOT] ScriptEnable complete - BDOT is active!");
                Debug.Log("[BDOT] ============================================");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] ScriptEnable FAILED: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void LogConfigurationSummary()
        {
            Debug.Log("[BDOT] --- Configuration Summary ---");
            Debug.Log("[BDOT] Enabled: " + BDOTModOptions.EnableMod);
            Debug.Log("[BDOT] Debug Logging: " + BDOTModOptions.DebugLogging);
            Debug.Log("[BDOT] Global Multiplier: " + BDOTModOptions.GlobalDamageMultiplier.ToString("F2") + "x");
            Debug.Log("[BDOT] Tick Interval: " + BDOTModOptions.TickInterval.ToString("F2") + "s");
            Debug.Log("[BDOT] Presets: Intensity=" + BDOTModOptions.IntensityPresetSetting + ", Duration=" + BDOTModOptions.DurationPresetSetting + ", Damage=" + BDOTModOptions.DamagePresetSetting);
            Debug.Log("[BDOT] --- Zone Settings ---");

            BodyZone[] zones = { BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso, BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment };
            foreach (var zone in zones)
            {
                var cfg = BDOTModOptions.GetZoneConfig(zone);
                string status = cfg.Enabled ? "ON " : "OFF";
                Debug.Log("[BDOT] " + zone.GetDisplayName().PadRight(13) + " [" + status + "] Mult=" + cfg.Multiplier.ToString("F1").PadLeft(4) + "x | Dur=" + cfg.Duration.ToString("F1").PadLeft(5) + "s | Dmg=" + cfg.DamagePerTick.ToString("F2").PadLeft(5) + " | Stacks=" + cfg.StackLimit);
            }
        }

        public override void ScriptUpdate()
        {
            try
            {
                base.ScriptUpdate();
                BleedManager.Instance?.Update();
                BDOTModOptionVisibility.Instance?.Update();
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] ScriptUpdate error: " + ex.Message);
            }
        }

        public override void ScriptDisable()
        {
            try
            {
                Debug.Log("[BDOT] ScriptDisable...");

                BleedManager.Instance?.ClearAll();
                BDOTModOptionVisibility.Instance?.Shutdown();
                EventHooks.Unsubscribe();
                EventHooks.ResetState();

                Debug.Log("[BDOT] BDOT deactivated");
            }
            catch (Exception ex)
            {
                Debug.LogError("[BDOT] ScriptDisable error: " + ex.Message);
            }

            base.ScriptDisable();
        }
    }
}
