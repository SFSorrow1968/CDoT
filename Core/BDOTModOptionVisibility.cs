using System;
using System.Collections.Generic;
using System.Reflection;
using BDOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Core
{
    public class BDOTModOptionVisibility
    {
        private static BDOTModOptionVisibility _instance;
        public static BDOTModOptionVisibility Instance => _instance ??= new BDOTModOptionVisibility();

        private ModManager.ModData _modData;
        private bool _initialized = false;
        private bool _lastResetStatsToggle = false;
        private BDOTModOptions.IntensityPreset? _lastIntensityPreset;
        private BDOTModOptions.DurationPreset? _lastDurationPreset;
        private BDOTModOptions.DamagePreset? _lastDamagePreset;

        private readonly Dictionary<string, ModOption> _modOptionsByKey =
            new Dictionary<string, ModOption>(StringComparer.Ordinal);

        // Throttle: only run full check periodically
        private float _lastFullCheckTime;
        private const float IdleCheckInterval = 0.5f;

        private const string OptionKeySeparator = "||";

        // Zone ordering for preset values
        private static readonly BodyZone[] AllZones =
        {
            BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso,
            BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment
        };

        // Option key mappings for each zone's settings
        private static readonly Dictionary<BodyZone, string> MultiplierOptionKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatMultiplier) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadMultiplier) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckMultiplier) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoMultiplier) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmMultiplier) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegMultiplier) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentMultiplier) }
        };

        private static readonly Dictionary<BodyZone, string> DurationOptionKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatDuration) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadDuration) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckDuration) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoDuration) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmDuration) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegDuration) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentDuration) }
        };

        private static readonly Dictionary<BodyZone, string> DamageOptionKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatDamagePerTick) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadDamagePerTick) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckDamagePerTick) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoDamagePerTick) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmDamagePerTick) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegDamagePerTick) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentDamagePerTick) }
        };

        public void Initialize()
        {
            _initialized = false;
            _modData = null;
            _lastIntensityPreset = null;
            _lastDurationPreset = null;
            _lastDamagePreset = null;
            _lastResetStatsToggle = false;
            _modOptionsByKey.Clear();

            TryInitialize();
            if (_initialized)
            {
                // Apply all presets on first load
                if (ApplyAllPresets(true))
                    ModManager.RefreshModOptionsUI();
            }
            Debug.Log("[BDOT] ModOptionVisibility initialized");
        }

        public void Shutdown()
        {
            _initialized = false;
            _modOptionsByKey.Clear();
            _instance = null;
        }

        public void Update()
        {
            if (!_initialized)
            {
                TryInitialize();
                if (!_initialized)
                    return;

                ApplyAllPresets(true);
                return;
            }

            // Throttle: only run full check at intervals when idle
            float now = Time.unscaledTime;
            if (now - _lastFullCheckTime < IdleCheckInterval)
                return;
            _lastFullCheckTime = now;

            ApplyAllPresets(false);
        }

        private void TryInitialize()
        {
            if (_initialized) return;

            if (!ModManager.TryGetModData(Assembly.GetExecutingAssembly(), out _modData))
                return;

            if (_modData?.modOptions == null || _modData.modOptions.Count == 0)
                return;

            CacheModOptions();
            _initialized = true;
            Debug.Log("[BDOT] ModOption cache built with " + _modOptionsByKey.Count + " options");
        }

        private void CacheModOptions()
        {
            _modOptionsByKey.Clear();
            if (_modData?.modOptions == null) return;

            foreach (var option in _modData.modOptions)
            {
                if (option == null || string.IsNullOrEmpty(option.name)) continue;
                string key = MakeKey(option.category, option.name);
                _modOptionsByKey[key] = option;
            }
        }

        private bool ApplyAllPresets(bool force)
        {
            bool changed = false;

            changed |= ApplyIntensityPreset(force);
            changed |= ApplyDurationPreset(force);
            changed |= ApplyDamagePreset(force);
            changed |= ApplyStatisticsReset();

            return changed;
        }

        private bool ApplyStatisticsReset()
        {
            if (!BDOTModOptions.ResetStatsToggle || _lastResetStatsToggle)
            {
                _lastResetStatsToggle = BDOTModOptions.ResetStatsToggle;
                return false;
            }

            float oldDamage = BDOTModOptions.GetTotalBleedDamage();
            int oldCount = BDOTModOptions.GetTotalBleedCount();
            BDOTModOptions.ResetStatistics();
            BDOTModOptions.ResetStatsToggle = false;
            _lastResetStatsToggle = false;

            Debug.Log("[BDOT] ========== STATISTICS RESET ==========");
            Debug.Log("[BDOT] Cleared: " + oldDamage.ToString("F1") + " total damage, " + oldCount + " bleed effects");
            Debug.Log("[BDOT] =======================================");
            return true;
        }

        private bool ApplyIntensityPreset(bool force)
        {
            var preset = BDOTModOptions.GetIntensityPreset();
            if (!force && _lastIntensityPreset.HasValue && _lastIntensityPreset.Value == preset)
                return false;

            Debug.Log("[BDOT] ========== PRESET CHANGE: Intensity ==========");
            Debug.Log("[BDOT] Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetMultiplierValue(zone, preset);
                float oldValue = BDOTModOptions.GetZoneConfig(zone).Multiplier;
                BDOTModOptions.SetZoneMultiplier(zone, value);
                SyncOptionValue(MultiplierOptionKeys, zone, value);

                Debug.Log("[BDOT]   " + zone.GetDisplayName().PadRight(13) + ": " + oldValue.ToString("F1") + "x -> " + value.ToString("F1") + "x");
            }

            _lastIntensityPreset = preset;
            Debug.Log("[BDOT] ================================================");
            return true;
        }

        private bool ApplyDurationPreset(bool force)
        {
            var preset = BDOTModOptions.GetDurationPreset();
            if (!force && _lastDurationPreset.HasValue && _lastDurationPreset.Value == preset)
                return false;

            Debug.Log("[BDOT] ========== PRESET CHANGE: Duration ==========");
            Debug.Log("[BDOT] Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDurationValue(zone, preset);
                float oldValue = BDOTModOptions.GetZoneConfig(zone).Duration;
                BDOTModOptions.SetZoneDuration(zone, value);
                SyncOptionValue(DurationOptionKeys, zone, value);

                Debug.Log("[BDOT]   " + zone.GetDisplayName().PadRight(13) + ": " + oldValue.ToString("F1") + "s -> " + value.ToString("F1") + "s");
            }

            _lastDurationPreset = preset;
            Debug.Log("[BDOT] ===============================================");
            return true;
        }

        private bool ApplyDamagePreset(bool force)
        {
            var preset = BDOTModOptions.GetDamagePreset();
            if (!force && _lastDamagePreset.HasValue && _lastDamagePreset.Value == preset)
                return false;

            Debug.Log("[BDOT] ========== PRESET CHANGE: Damage ==========");
            Debug.Log("[BDOT] Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDamageValue(zone, preset);
                float oldValue = BDOTModOptions.GetZoneConfig(zone).DamagePerTick;
                BDOTModOptions.SetZoneDamagePerTick(zone, value);
                SyncOptionValue(DamageOptionKeys, zone, value);

                Debug.Log("[BDOT]   " + zone.GetDisplayName().PadRight(13) + ": " + oldValue.ToString("F2") + " -> " + value.ToString("F2"));
            }

            _lastDamagePreset = preset;
            Debug.Log("[BDOT] =============================================");
            return true;
        }

        #region Preset Value Tables

        /// <summary>
        /// Get multiplier value for a zone based on the intensity preset.
        /// Values are final (no runtime calculation).
        /// </summary>
        public static float GetPresetMultiplierValue(BodyZone zone, BDOTModOptions.IntensityPreset preset)
        {
            // Light=0.5x base, Default=1x base, Heavy=1.5x base, Brutal=2.5x base
            switch (zone)
            {
                case BodyZone.Throat: // Base: 3.0
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 1.5f;
                        case BDOTModOptions.IntensityPreset.Default: return 3.0f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 4.5f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 7.5f;
                    }
                    break;
                case BodyZone.Head: // Base: 2.0
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 1.0f;
                        case BDOTModOptions.IntensityPreset.Default: return 2.0f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 3.0f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 5.0f;
                    }
                    break;
                case BodyZone.Neck: // Base: 2.5
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 1.3f;
                        case BDOTModOptions.IntensityPreset.Default: return 2.5f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 3.8f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 6.3f;
                    }
                    break;
                case BodyZone.Torso: // Base: 1.0
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 0.5f;
                        case BDOTModOptions.IntensityPreset.Default: return 1.0f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 1.5f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 2.5f;
                    }
                    break;
                case BodyZone.Arm: // Base: 0.5
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 0.3f;
                        case BDOTModOptions.IntensityPreset.Default: return 0.5f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 0.8f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 1.3f;
                    }
                    break;
                case BodyZone.Leg: // Base: 0.6
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 0.3f;
                        case BDOTModOptions.IntensityPreset.Default: return 0.6f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 0.9f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 1.5f;
                    }
                    break;
                case BodyZone.Dismemberment: // Base: 2.5
                    switch (preset)
                    {
                        case BDOTModOptions.IntensityPreset.Light: return 1.3f;
                        case BDOTModOptions.IntensityPreset.Default: return 2.5f;
                        case BDOTModOptions.IntensityPreset.Heavy: return 3.8f;
                        case BDOTModOptions.IntensityPreset.Brutal: return 6.3f;
                    }
                    break;
            }
            return 1.0f;
        }

        /// <summary>
        /// Get duration value for a zone based on the duration preset.
        /// Values are final (no runtime calculation).
        /// </summary>
        public static float GetPresetDurationValue(BodyZone zone, BDOTModOptions.DurationPreset preset)
        {
            // Short=0.6x base, Default=1x base, Long=1.5x base, Extended=2x base
            switch (zone)
            {
                case BodyZone.Throat: // Base: 8.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 5.0f;
                        case BDOTModOptions.DurationPreset.Default: return 8.0f;
                        case BDOTModOptions.DurationPreset.Long: return 12.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 16.0f;
                    }
                    break;
                case BodyZone.Head: // Base: 6.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 3.5f;
                        case BDOTModOptions.DurationPreset.Default: return 6.0f;
                        case BDOTModOptions.DurationPreset.Long: return 9.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 12.0f;
                    }
                    break;
                case BodyZone.Neck: // Base: 7.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 4.0f;
                        case BDOTModOptions.DurationPreset.Default: return 7.0f;
                        case BDOTModOptions.DurationPreset.Long: return 10.5f;
                        case BDOTModOptions.DurationPreset.Extended: return 14.0f;
                    }
                    break;
                case BodyZone.Torso: // Base: 5.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 3.0f;
                        case BDOTModOptions.DurationPreset.Default: return 5.0f;
                        case BDOTModOptions.DurationPreset.Long: return 7.5f;
                        case BDOTModOptions.DurationPreset.Extended: return 10.0f;
                    }
                    break;
                case BodyZone.Arm: // Base: 4.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 2.5f;
                        case BDOTModOptions.DurationPreset.Default: return 4.0f;
                        case BDOTModOptions.DurationPreset.Long: return 6.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 8.0f;
                    }
                    break;
                case BodyZone.Leg: // Base: 5.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 3.0f;
                        case BDOTModOptions.DurationPreset.Default: return 5.0f;
                        case BDOTModOptions.DurationPreset.Long: return 7.5f;
                        case BDOTModOptions.DurationPreset.Extended: return 10.0f;
                    }
                    break;
                case BodyZone.Dismemberment: // Base: 10.0
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.Short: return 6.0f;
                        case BDOTModOptions.DurationPreset.Default: return 10.0f;
                        case BDOTModOptions.DurationPreset.Long: return 15.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 20.0f;
                    }
                    break;
            }
            return 5.0f;
        }

        /// <summary>
        /// Get damage per tick value for a zone based on the damage preset.
        /// Values are final (no runtime calculation).
        /// </summary>
        public static float GetPresetDamageValue(BodyZone zone, BDOTModOptions.DamagePreset preset)
        {
            // Low=0.5x base, Default=1x base, High=1.5x base, Extreme=2.5x base
            switch (zone)
            {
                case BodyZone.Throat: // Base: 5.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 2.5f;
                        case BDOTModOptions.DamagePreset.Default: return 5.0f;
                        case BDOTModOptions.DamagePreset.High: return 7.5f;
                        case BDOTModOptions.DamagePreset.Extreme: return 12.5f;
                    }
                    break;
                case BodyZone.Head: // Base: 3.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 1.5f;
                        case BDOTModOptions.DamagePreset.Default: return 3.0f;
                        case BDOTModOptions.DamagePreset.High: return 4.5f;
                        case BDOTModOptions.DamagePreset.Extreme: return 7.5f;
                    }
                    break;
                case BodyZone.Neck: // Base: 4.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 2.0f;
                        case BDOTModOptions.DamagePreset.Default: return 4.0f;
                        case BDOTModOptions.DamagePreset.High: return 6.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 10.0f;
                    }
                    break;
                case BodyZone.Torso: // Base: 2.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 1.0f;
                        case BDOTModOptions.DamagePreset.Default: return 2.0f;
                        case BDOTModOptions.DamagePreset.High: return 3.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 5.0f;
                    }
                    break;
                case BodyZone.Arm: // Base: 1.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 0.5f;
                        case BDOTModOptions.DamagePreset.Default: return 1.0f;
                        case BDOTModOptions.DamagePreset.High: return 1.5f;
                        case BDOTModOptions.DamagePreset.Extreme: return 2.5f;
                    }
                    break;
                case BodyZone.Leg: // Base: 1.5
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 0.75f;
                        case BDOTModOptions.DamagePreset.Default: return 1.5f;
                        case BDOTModOptions.DamagePreset.High: return 2.25f;
                        case BDOTModOptions.DamagePreset.Extreme: return 3.75f;
                    }
                    break;
                case BodyZone.Dismemberment: // Base: 6.0
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Low: return 3.0f;
                        case BDOTModOptions.DamagePreset.Default: return 6.0f;
                        case BDOTModOptions.DamagePreset.High: return 9.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 15.0f;
                    }
                    break;
            }
            return 2.0f;
        }

        #endregion

        #region UI Sync Helpers

        private static string MakeKey(string category, string name)
        {
            return (category ?? string.Empty) + OptionKeySeparator + (name ?? string.Empty);
        }

        private static string DescribeOption(ModOption option)
        {
            if (option == null) return string.Empty;
            if (string.IsNullOrEmpty(option.category)) return option.name;
            return option.category + " / " + option.name;
        }

        private bool SyncOptionValue(Dictionary<BodyZone, string> map, BodyZone zone, float value)
        {
            if (!map.TryGetValue(zone, out string optionKey))
                return false;
            return SyncOptionValue(optionKey, value);
        }

        private bool SyncOptionValue(string optionKey, float value)
        {
            if (!_modOptionsByKey.TryGetValue(optionKey, out ModOption option))
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.LogWarning("[BDOT] Menu sync missing option: " + optionKey);
                return false;
            }

            if (option.parameterValues == null || option.parameterValues.Length == 0)
                option.LoadModOptionParameters();

            if (option.parameterValues == null || option.parameterValues.Length == 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.LogWarning("[BDOT] Menu sync missing parameters: " + DescribeOption(option));
                return false;
            }

            int index = FindParameterIndex(option.parameterValues, value);
            if (index < 0)
            {
                if (BDOTModOptions.DebugLogging)
                    Debug.LogWarning("[BDOT] Menu sync no parameter match: " + DescribeOption(option) + " value=" + value);
                return false;
            }

            if (option.currentValueIndex == index)
                return false;

            option.Apply(index);
            if (BDOTModOptions.DebugLogging)
                Debug.Log("[BDOT] Menu sync updated: " + DescribeOption(option) + " -> " + value);
            return true;
        }

        private static int FindParameterIndex(ModOptionParameter[] parameters, float value)
        {
            if (parameters == null) return -1;

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramValue = parameters[i]?.value;
                if (paramValue is float fValue)
                {
                    if (Mathf.Abs(fValue - value) < 0.001f)
                        return i;
                }
                else if (paramValue is double dValue)
                {
                    if (Mathf.Abs((float)dValue - value) < 0.001f)
                        return i;
                }
                else if (paramValue is int iValue)
                {
                    if (Mathf.Abs(iValue - value) < 0.001f)
                        return i;
                }
            }

            return -1;
        }

        #endregion
    }
}
