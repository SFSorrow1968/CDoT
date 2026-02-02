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

        public static float GetPresetDamageValue(BodyZone zone, BDOTModOptions.DamagePreset preset)
        {
            // Low=0.5x, Default=1x, High=1.5x, Extreme=2.5x of base values
            switch (zone)
            {
                case BodyZone.Throat: // Base: 5.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 2.5f :
                           preset == BDOTModOptions.DamagePreset.High ? 7.5f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 12.5f : 5.0f;
                case BodyZone.Head: // Base: 3.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 1.5f :
                           preset == BDOTModOptions.DamagePreset.High ? 4.5f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 7.5f : 3.0f;
                case BodyZone.Neck: // Base: 4.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 2.0f :
                           preset == BDOTModOptions.DamagePreset.High ? 6.0f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 10.0f : 4.0f;
                case BodyZone.Torso: // Base: 2.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 1.0f :
                           preset == BDOTModOptions.DamagePreset.High ? 3.0f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 5.0f : 2.0f;
                case BodyZone.Arm: // Base: 1.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 0.5f :
                           preset == BDOTModOptions.DamagePreset.High ? 1.5f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 2.5f : 1.0f;
                case BodyZone.Leg: // Base: 1.5
                    return preset == BDOTModOptions.DamagePreset.Low ? 0.75f :
                           preset == BDOTModOptions.DamagePreset.High ? 2.25f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 3.75f : 1.5f;
                case BodyZone.Dismemberment: // Base: 6.0
                    return preset == BDOTModOptions.DamagePreset.Low ? 3.0f :
                           preset == BDOTModOptions.DamagePreset.High ? 9.0f :
                           preset == BDOTModOptions.DamagePreset.Extreme ? 15.0f : 6.0f;
                default:
                    return 2.0f;
            }
        }

        public static float GetPresetDurationValue(BodyZone zone, BDOTModOptions.DurationPreset preset)
        {
            // Short=0.6x, Default=1x, Long=1.5x, Extended=2x of base values
            switch (zone)
            {
                case BodyZone.Throat: // Base: 8.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 5.0f :
                           preset == BDOTModOptions.DurationPreset.Long ? 12.0f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 16.0f : 8.0f;
                case BodyZone.Head: // Base: 6.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 3.5f :
                           preset == BDOTModOptions.DurationPreset.Long ? 9.0f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 12.0f : 6.0f;
                case BodyZone.Neck: // Base: 7.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 4.0f :
                           preset == BDOTModOptions.DurationPreset.Long ? 10.5f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 14.0f : 7.0f;
                case BodyZone.Torso: // Base: 5.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 3.0f :
                           preset == BDOTModOptions.DurationPreset.Long ? 7.5f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 10.0f : 5.0f;
                case BodyZone.Arm: // Base: 4.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 2.5f :
                           preset == BDOTModOptions.DurationPreset.Long ? 6.0f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 8.0f : 4.0f;
                case BodyZone.Leg: // Base: 5.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 3.0f :
                           preset == BDOTModOptions.DurationPreset.Long ? 7.5f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 10.0f : 5.0f;
                case BodyZone.Dismemberment: // Base: 10.0
                    return preset == BDOTModOptions.DurationPreset.Short ? 6.0f :
                           preset == BDOTModOptions.DurationPreset.Long ? 15.0f :
                           preset == BDOTModOptions.DurationPreset.Extended ? 20.0f : 10.0f;
                default:
                    return 5.0f;
            }
        }

        public static float GetPresetFrequencyValue(BDOTModOptions.FrequencyPreset preset)
        {
            // Slow=1.0s, Normal=0.5s, Fast=0.25s, Rapid=0.1s
            switch (preset)
            {
                case BDOTModOptions.FrequencyPreset.Slow: return 1.0f;
                case BDOTModOptions.FrequencyPreset.Fast: return 0.25f;
                case BDOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                default: return 0.5f; // Normal
            }
        }

        public static float GetPresetChanceValue(BodyZone zone, BDOTModOptions.ChancePreset preset)
        {
            // Off=0%, Rare=base*0.5, Default=base, Frequent=base*1.5 (capped at 100%), Always=100%
            if (preset == BDOTModOptions.ChancePreset.Off) return 0f;
            if (preset == BDOTModOptions.ChancePreset.Always) return 100f;

            // Base chances per zone
            float baseChance;
            switch (zone)
            {
                case BodyZone.Throat: baseChance = 75f; break;
                case BodyZone.Head: baseChance = 50f; break;
                case BodyZone.Neck: baseChance = 65f; break;
                case BodyZone.Torso: baseChance = 40f; break;
                case BodyZone.Arm: baseChance = 30f; break;
                case BodyZone.Leg: baseChance = 35f; break;
                case BodyZone.Dismemberment: baseChance = 100f; break;
                default: baseChance = 50f; break;
            }

            switch (preset)
            {
                case BDOTModOptions.ChancePreset.Rare:
                    return Mathf.Min(baseChance * 0.5f, 100f);
                case BDOTModOptions.ChancePreset.Frequent:
                    return Mathf.Min(baseChance * 1.5f, 100f);
                default: // Default
                    return baseChance;
            }
        }

        #endregion
    }
}
