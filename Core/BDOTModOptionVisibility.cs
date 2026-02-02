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

        // Damage preset values - explicit values per zone, not multipliers
        // Low: Minimal bleed damage, very survivable
        // Default: Balanced bleed damage
        // High: Serious bleed damage, dangerous wounds
        // Extreme: Deadly bleed damage, quickly fatal

        public static float GetPresetDamageValue(BodyZone zone, BDOTModOptions.DamagePreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.DamagePreset.Low:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 1.5f;
                        case BodyZone.Head: return 1.0f;
                        case BodyZone.Neck: return 1.25f;
                        case BodyZone.Torso: return 0.75f;
                        case BodyZone.Arm: return 0.5f;
                        case BodyZone.Leg: return 0.5f;
                        case BodyZone.Dismemberment: return 2.0f;
                        default: return 0.75f;
                    }
                case BDOTModOptions.DamagePreset.High:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 8.0f;
                        case BodyZone.Head: return 5.0f;
                        case BodyZone.Neck: return 6.5f;
                        case BodyZone.Torso: return 3.5f;
                        case BodyZone.Arm: return 2.0f;
                        case BodyZone.Leg: return 2.5f;
                        case BodyZone.Dismemberment: return 10.0f;
                        default: return 3.5f;
                    }
                case BDOTModOptions.DamagePreset.Extreme:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 15.0f;
                        case BodyZone.Head: return 10.0f;
                        case BodyZone.Neck: return 12.0f;
                        case BodyZone.Torso: return 6.0f;
                        case BodyZone.Arm: return 4.0f;
                        case BodyZone.Leg: return 5.0f;
                        case BodyZone.Dismemberment: return 18.0f;
                        default: return 6.0f;
                    }
                default: // Default
                    switch (zone)
                    {
                        case BodyZone.Throat: return 5.0f;
                        case BodyZone.Head: return 3.0f;
                        case BodyZone.Neck: return 4.0f;
                        case BodyZone.Torso: return 2.0f;
                        case BodyZone.Arm: return 1.0f;
                        case BodyZone.Leg: return 1.5f;
                        case BodyZone.Dismemberment: return 6.0f;
                        default: return 2.0f;
                    }
            }
        }

        // Duration preset values - explicit values per zone, not multipliers
        // Short: Quick bleeds, recover fast
        // Default: Balanced bleed duration
        // Long: Extended bleeds, serious wounds
        // Extended: Very long bleeds, critical wounds

        public static float GetPresetDurationValue(BodyZone zone, BDOTModOptions.DurationPreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.DurationPreset.Short:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 4.0f;
                        case BodyZone.Head: return 3.0f;
                        case BodyZone.Neck: return 3.5f;
                        case BodyZone.Torso: return 2.5f;
                        case BodyZone.Arm: return 2.0f;
                        case BodyZone.Leg: return 2.5f;
                        case BodyZone.Dismemberment: return 5.0f;
                        default: return 2.5f;
                    }
                case BDOTModOptions.DurationPreset.Long:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 14.0f;
                        case BodyZone.Head: return 10.0f;
                        case BodyZone.Neck: return 12.0f;
                        case BodyZone.Torso: return 9.0f;
                        case BodyZone.Arm: return 7.0f;
                        case BodyZone.Leg: return 8.0f;
                        case BodyZone.Dismemberment: return 18.0f;
                        default: return 9.0f;
                    }
                case BDOTModOptions.DurationPreset.Extended:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 20.0f;
                        case BodyZone.Head: return 15.0f;
                        case BodyZone.Neck: return 18.0f;
                        case BodyZone.Torso: return 12.0f;
                        case BodyZone.Arm: return 10.0f;
                        case BodyZone.Leg: return 11.0f;
                        case BodyZone.Dismemberment: return 25.0f;
                        default: return 12.0f;
                    }
                default: // Default
                    switch (zone)
                    {
                        case BodyZone.Throat: return 8.0f;
                        case BodyZone.Head: return 6.0f;
                        case BodyZone.Neck: return 7.0f;
                        case BodyZone.Torso: return 5.0f;
                        case BodyZone.Arm: return 4.0f;
                        case BodyZone.Leg: return 5.0f;
                        case BodyZone.Dismemberment: return 10.0f;
                        default: return 5.0f;
                    }
            }
        }

        // Frequency preset values - explicit tick intervals
        // Slow: Damage ticks every 1 second
        // Normal: Damage ticks every 0.5 seconds
        // Fast: Damage ticks every 0.25 seconds
        // Rapid: Damage ticks every 0.1 seconds

        public static float GetPresetFrequencyValue(BDOTModOptions.FrequencyPreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.FrequencyPreset.Slow: return 1.0f;
                case BDOTModOptions.FrequencyPreset.Fast: return 0.25f;
                case BDOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                default: return 0.5f; // Normal
            }
        }

        // Chance preset values - explicit values per zone, not multipliers
        // Off: No bleeds trigger
        // Rare: Low chance, only severe wounds bleed
        // Default: Balanced chance
        // Frequent: High chance, most wounds bleed
        // Always: 100% chance, every hit bleeds

        public static float GetPresetChanceValue(BodyZone zone, BDOTModOptions.ChancePreset preset)
        {
            switch (preset)
            {
                case BDOTModOptions.ChancePreset.Off:
                    return 0f;
                case BDOTModOptions.ChancePreset.Rare:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 40f;
                        case BodyZone.Head: return 25f;
                        case BodyZone.Neck: return 35f;
                        case BodyZone.Torso: return 20f;
                        case BodyZone.Arm: return 15f;
                        case BodyZone.Leg: return 15f;
                        case BodyZone.Dismemberment: return 50f;
                        default: return 20f;
                    }
                case BDOTModOptions.ChancePreset.Frequent:
                    switch (zone)
                    {
                        case BodyZone.Throat: return 90f;
                        case BodyZone.Head: return 75f;
                        case BodyZone.Neck: return 85f;
                        case BodyZone.Torso: return 65f;
                        case BodyZone.Arm: return 50f;
                        case BodyZone.Leg: return 55f;
                        case BodyZone.Dismemberment: return 100f;
                        default: return 65f;
                    }
                case BDOTModOptions.ChancePreset.Always:
                    return 100f;
                default: // Default
                    switch (zone)
                    {
                        case BodyZone.Throat: return 75f;
                        case BodyZone.Head: return 50f;
                        case BodyZone.Neck: return 65f;
                        case BodyZone.Torso: return 40f;
                        case BodyZone.Arm: return 30f;
                        case BodyZone.Leg: return 35f;
                        case BodyZone.Dismemberment: return 100f;
                        default: return 50f;
                    }
            }
        }

        #endregion
    }
}
