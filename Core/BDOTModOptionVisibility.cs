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
        private bool _initialized;
        private BDOTModOptions.DamagePreset? _lastDamagePreset;
        private BDOTModOptions.DurationPreset? _lastDurationPreset;
        private BDOTModOptions.FrequencyPreset? _lastFrequencyPreset;
        private BDOTModOptions.ChancePreset? _lastChancePreset;
        private bool _lastResetStatsToggle;

        private readonly Dictionary<string, ModOption> _modOptionsByKey =
            new Dictionary<string, ModOption>(StringComparer.Ordinal);

        private const string OptionKeySeparator = "||";

        private static readonly BodyZone[] AllZones =
        {
            BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso,
            BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment
        };

        // Option key mappings for Chance
        private static readonly Dictionary<BodyZone, string> ChanceKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatChance) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadChance) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckChance) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoChance) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmChance) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegChance) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentChance) }
        };

        // Option key mappings for Damage
        private static readonly Dictionary<BodyZone, string> DamageKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatDamage) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadDamage) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckDamage) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoDamage) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmDamage) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegDamage) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentDamage) }
        };

        // Option key mappings for Duration
        private static readonly Dictionary<BodyZone, string> DurationKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatDuration) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadDuration) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckDuration) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoDuration) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmDuration) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegDuration) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentDuration) }
        };

        public void Initialize()
        {
            _initialized = false;
            _modData = null;
            _lastDamagePreset = null;
            _lastDurationPreset = null;
            _lastFrequencyPreset = null;
            _lastChancePreset = null;
            _modOptionsByKey.Clear();

            TryInitialize();
            if (_initialized)
                ApplyAllPresets(true);

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
                if (_initialized)
                    ApplyAllPresets(true);
                return;
            }

            ApplyAllPresets(false);
        }

        private void TryInitialize()
        {
            if (_initialized) return;

            if (!ModManager.TryGetModData(Assembly.GetExecutingAssembly(), out _modData))
                return;

            if (_modData?.modOptions == null || _modData.modOptions.Count == 0)
                return;

            // Cache all mod options by key
            foreach (var option in _modData.modOptions)
            {
                if (option == null || string.IsNullOrEmpty(option.name)) continue;
                _modOptionsByKey[MakeKey(option.category, option.name)] = option;
            }

            _initialized = true;
        }

        private void ApplyAllPresets(bool force)
        {
            ApplyDamagePreset(force);
            ApplyDurationPreset(force);
            ApplyFrequencyPreset(force);
            ApplyChancePreset(force);
            ApplyStatisticsReset();
        }

        private void ApplyDamagePreset(bool force)
        {
            var preset = BDOTModOptions.GetDamagePreset();
            if (!force && _lastDamagePreset == preset)
                return;

            Debug.Log("[BDOT] Applying Damage Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDamageValue(zone, preset);
                BDOTModOptions.SetZoneDamage(zone, value);
                SyncOption(DamageKeys[zone], value);
            }

            _lastDamagePreset = preset;
        }

        private void ApplyDurationPreset(bool force)
        {
            var preset = BDOTModOptions.GetDurationPreset();
            if (!force && _lastDurationPreset == preset)
                return;

            Debug.Log("[BDOT] Applying Duration Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDurationValue(zone, preset);
                BDOTModOptions.SetZoneDuration(zone, value);
                SyncOption(DurationKeys[zone], value);
            }

            _lastDurationPreset = preset;
        }

        private void ApplyFrequencyPreset(bool force)
        {
            var preset = BDOTModOptions.GetFrequencyPreset();
            if (!force && _lastFrequencyPreset == preset)
                return;

            Debug.Log("[BDOT] Applying Frequency Preset: " + preset);

            // Frequency applies globally to TickInterval
            float tickInterval = GetPresetFrequencyValue(preset);
            BDOTModOptions.TickInterval = tickInterval;

            _lastFrequencyPreset = preset;
        }

        private void ApplyChancePreset(bool force)
        {
            var preset = BDOTModOptions.GetChancePreset();
            if (!force && _lastChancePreset == preset)
                return;

            Debug.Log("[BDOT] Applying Chance Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetChanceValue(zone, preset);
                BDOTModOptions.SetZoneChance(zone, value);
                SyncOption(ChanceKeys[zone], value);
            }

            _lastChancePreset = preset;
        }

        private void ApplyStatisticsReset()
        {
            if (BDOTModOptions.ResetStatsToggle && !_lastResetStatsToggle)
            {
                BDOTModOptions.ResetStatistics();
                BDOTModOptions.ResetStatsToggle = false;
                Debug.Log("[BDOT] Statistics reset");
            }
            _lastResetStatsToggle = BDOTModOptions.ResetStatsToggle;
        }

        #region UI Sync

        private static string MakeKey(string category, string name)
        {
            return (category ?? "") + OptionKeySeparator + (name ?? "");
        }

        private void SyncOption(string key, float value)
        {
            if (!_modOptionsByKey.TryGetValue(key, out var option))
                return;

            // Load parameter values if not already loaded
            if (option.parameterValues == null || option.parameterValues.Length == 0)
                option.LoadModOptionParameters();

            // Find the index that matches this value
            int index = FindParameterIndex(option.parameterValues, value);
            if (index >= 0 && option.currentValueIndex != index)
            {
                option.Apply(index);
                option.RefreshUI();
            }
        }

        private static int FindParameterIndex(ModOptionParameter[] parameters, float value)
        {
            if (parameters == null) return -1;
            for (int i = 0; i < parameters.Length; i++)
            {
                var pv = parameters[i]?.value;
                if ((pv is float fv && Mathf.Abs(fv - value) < 0.0001f) ||
                    (pv is double dv && Mathf.Abs((float)dv - value) < 0.0001f) ||
                    (pv is int iv && Mathf.Abs(iv - value) < 0.0001f))
                    return i;
            }
            return -1;
        }

        #endregion

        #region Preset Value Tables

        // ========== DAMAGE PRESET VALUES ==========
        // 5 presets: Minimal (0), Low (1), Default (2), High (3), Extreme (4)
        // Each zone has specific values per preset

        public static float GetPresetDamageValue(BodyZone zone, BDOTModOptions.DamagePreset preset)
        {
            switch (zone)
            {
                case BodyZone.Throat:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.5f;
                        case BDOTModOptions.DamagePreset.Low: return 1.25f;
                        case BDOTModOptions.DamagePreset.Default: return 2.5f;
                        case BDOTModOptions.DamagePreset.High: return 5.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 10.0f;
                        default: return 2.5f;
                    }
                case BodyZone.Head:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case BDOTModOptions.DamagePreset.Low: return 0.75f;
                        case BDOTModOptions.DamagePreset.Default: return 1.5f;
                        case BDOTModOptions.DamagePreset.High: return 3.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 6.0f;
                        default: return 1.5f;
                    }
                case BodyZone.Neck:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.5f;
                        case BDOTModOptions.DamagePreset.Low: return 1.0f;
                        case BDOTModOptions.DamagePreset.Default: return 2.0f;
                        case BDOTModOptions.DamagePreset.High: return 4.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 8.0f;
                        default: return 2.0f;
                    }
                case BodyZone.Torso:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case BDOTModOptions.DamagePreset.Low: return 0.5f;
                        case BDOTModOptions.DamagePreset.Default: return 1.0f;
                        case BDOTModOptions.DamagePreset.High: return 2.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 4.0f;
                        default: return 1.0f;
                    }
                case BodyZone.Arm:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case BDOTModOptions.DamagePreset.Low: return 0.25f;
                        case BDOTModOptions.DamagePreset.Default: return 0.5f;
                        case BDOTModOptions.DamagePreset.High: return 1.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 2.0f;
                        default: return 0.5f;
                    }
                case BodyZone.Leg:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case BDOTModOptions.DamagePreset.Low: return 0.5f;
                        case BDOTModOptions.DamagePreset.Default: return 0.75f;
                        case BDOTModOptions.DamagePreset.High: return 1.5f;
                        case BDOTModOptions.DamagePreset.Extreme: return 3.0f;
                        default: return 0.75f;
                    }
                case BodyZone.Dismemberment:
                    switch (preset)
                    {
                        case BDOTModOptions.DamagePreset.Minimal: return 1.0f;
                        case BDOTModOptions.DamagePreset.Low: return 2.0f;
                        case BDOTModOptions.DamagePreset.Default: return 3.0f;
                        case BDOTModOptions.DamagePreset.High: return 6.0f;
                        case BDOTModOptions.DamagePreset.Extreme: return 12.0f;
                        default: return 3.0f;
                    }
                default:
                    return 1.0f;
            }
        }

        // ========== DURATION PRESET VALUES ==========
        // 5 presets: VeryShort (0), Short (1), Default (2), Long (3), Extended (4)

        public static float GetPresetDurationValue(BodyZone zone, BDOTModOptions.DurationPreset preset)
        {
            switch (zone)
            {
                case BodyZone.Throat:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 2.0f;
                        case BDOTModOptions.DurationPreset.Short: return 4.0f;
                        case BDOTModOptions.DurationPreset.Default: return 6.0f;
                        case BDOTModOptions.DurationPreset.Long: return 10.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 15.0f;
                        default: return 6.0f;
                    }
                case BodyZone.Head:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 1.5f;
                        case BDOTModOptions.DurationPreset.Short: return 3.0f;
                        case BDOTModOptions.DurationPreset.Default: return 5.0f;
                        case BDOTModOptions.DurationPreset.Long: return 8.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 12.0f;
                        default: return 5.0f;
                    }
                case BodyZone.Neck:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 2.0f;
                        case BDOTModOptions.DurationPreset.Short: return 3.5f;
                        case BDOTModOptions.DurationPreset.Default: return 5.5f;
                        case BDOTModOptions.DurationPreset.Long: return 9.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 14.0f;
                        default: return 5.5f;
                    }
                case BodyZone.Torso:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 1.5f;
                        case BDOTModOptions.DurationPreset.Short: return 2.5f;
                        case BDOTModOptions.DurationPreset.Default: return 4.0f;
                        case BDOTModOptions.DurationPreset.Long: return 7.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 10.0f;
                        default: return 4.0f;
                    }
                case BodyZone.Arm:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 1.0f;
                        case BDOTModOptions.DurationPreset.Short: return 2.0f;
                        case BDOTModOptions.DurationPreset.Default: return 3.0f;
                        case BDOTModOptions.DurationPreset.Long: return 5.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 8.0f;
                        default: return 3.0f;
                    }
                case BodyZone.Leg:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 1.0f;
                        case BDOTModOptions.DurationPreset.Short: return 2.5f;
                        case BDOTModOptions.DurationPreset.Default: return 3.5f;
                        case BDOTModOptions.DurationPreset.Long: return 6.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 9.0f;
                        default: return 3.5f;
                    }
                case BodyZone.Dismemberment:
                    switch (preset)
                    {
                        case BDOTModOptions.DurationPreset.VeryShort: return 3.0f;
                        case BDOTModOptions.DurationPreset.Short: return 5.0f;
                        case BDOTModOptions.DurationPreset.Default: return 8.0f;
                        case BDOTModOptions.DurationPreset.Long: return 12.0f;
                        case BDOTModOptions.DurationPreset.Extended: return 20.0f;
                        default: return 8.0f;
                    }
                default:
                    return 4.0f;
            }
        }

        // ========== FREQUENCY PRESET VALUES ==========
        // 5 presets: VerySlow (0), Slow (1), Default (2), Fast (3), Rapid (4)
        // Returns tick interval in seconds

        public static float GetPresetFrequencyValue(BDOTModOptions.FrequencyPreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.FrequencyPreset.VerySlow: return 2.0f;
                case BDOTModOptions.FrequencyPreset.Slow: return 1.0f;
                case BDOTModOptions.FrequencyPreset.Default: return 0.5f;
                case BDOTModOptions.FrequencyPreset.Fast: return 0.25f;
                case BDOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                default: return 0.5f;
            }
        }

        // ========== CHANCE PRESET VALUES ==========
        // 5 presets: Off (0), Rare (1), Default (2), Frequent (3), Always (4)

        public static float GetPresetChanceValue(BodyZone zone, BDOTModOptions.ChancePreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.ChancePreset.Off:
                    return 0f;

                case BDOTModOptions.ChancePreset.Rare:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 30f;
                        case BodyZone.Head: return 20f;
                        case BodyZone.Neck: return 25f;
                        case BodyZone.Torso: return 15f;
                        case BodyZone.Arm: return 10f;
                        case BodyZone.Leg: return 15f;
                        case BodyZone.Dismemberment: return 40f;
                        default: return 15f;
                    }

                case BDOTModOptions.ChancePreset.Default:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 60f;
                        case BodyZone.Head: return 40f;
                        case BodyZone.Neck: return 55f;
                        case BodyZone.Torso: return 35f;
                        case BodyZone.Arm: return 25f;
                        case BodyZone.Leg: return 30f;
                        case BodyZone.Dismemberment: return 80f;
                        default: return 35f;
                    }

                case BDOTModOptions.ChancePreset.Frequent:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 85f;
                        case BodyZone.Head: return 65f;
                        case BodyZone.Neck: return 80f;
                        case BodyZone.Torso: return 55f;
                        case BodyZone.Arm: return 45f;
                        case BodyZone.Leg: return 50f;
                        case BodyZone.Dismemberment: return 95f;
                        default: return 60f;
                    }

                case BDOTModOptions.ChancePreset.Always:
                    return 100f;

                default:
                    return 35f;
            }
        }

        #endregion
    }
}
