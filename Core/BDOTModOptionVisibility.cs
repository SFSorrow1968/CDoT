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
        private BDOTModOptions.IntensityPreset? _lastIntensityPreset;
        private BDOTModOptions.DurationPreset? _lastDurationPreset;
        private BDOTModOptions.DamagePreset? _lastDamagePreset;
        private bool _lastResetStatsToggle;

        private readonly Dictionary<string, ModOption> _modOptionsByKey =
            new Dictionary<string, ModOption>(StringComparer.Ordinal);

        private const string OptionKeySeparator = "||";

        private static readonly BodyZone[] AllZones =
        {
            BodyZone.Throat, BodyZone.Head, BodyZone.Neck, BodyZone.Torso,
            BodyZone.Arm, BodyZone.Leg, BodyZone.Dismemberment
        };

        // Option key mappings
        private static readonly Dictionary<BodyZone, string> MultiplierKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(BDOTModOptions.CategoryZoneThroat, BDOTModOptions.OptionThroatMultiplier) },
            { BodyZone.Head, MakeKey(BDOTModOptions.CategoryZoneHead, BDOTModOptions.OptionHeadMultiplier) },
            { BodyZone.Neck, MakeKey(BDOTModOptions.CategoryZoneNeck, BDOTModOptions.OptionNeckMultiplier) },
            { BodyZone.Torso, MakeKey(BDOTModOptions.CategoryZoneTorso, BDOTModOptions.OptionTorsoMultiplier) },
            { BodyZone.Arm, MakeKey(BDOTModOptions.CategoryZoneArm, BDOTModOptions.OptionArmMultiplier) },
            { BodyZone.Leg, MakeKey(BDOTModOptions.CategoryZoneLeg, BDOTModOptions.OptionLegMultiplier) },
            { BodyZone.Dismemberment, MakeKey(BDOTModOptions.CategoryZoneDismemberment, BDOTModOptions.OptionDismembermentMultiplier) }
        };

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

        private static readonly Dictionary<BodyZone, string> DamageKeys = new Dictionary<BodyZone, string>
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
            ApplyIntensityPreset(force);
            ApplyDurationPreset(force);
            ApplyDamagePreset(force);
            ApplyStatisticsReset();
        }

        private void ApplyIntensityPreset(bool force)
        {
            var preset = BDOTModOptions.GetIntensityPreset();
            if (!force && _lastIntensityPreset == preset)
                return;

            Debug.Log("[BDOT] Applying Intensity Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetMultiplierValue(zone, preset);
                BDOTModOptions.SetZoneMultiplier(zone, value);
                SyncOption(MultiplierKeys[zone], value);
            }

            _lastIntensityPreset = preset;
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

        private void ApplyDamagePreset(bool force)
        {
            var preset = BDOTModOptions.GetDamagePreset();
            if (!force && _lastDamagePreset == preset)
                return;

            Debug.Log("[BDOT] Applying Damage Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDamageValue(zone, preset);
                BDOTModOptions.SetZoneDamagePerTick(zone, value);
                SyncOption(DamageKeys[zone], value);
            }

            _lastDamagePreset = preset;
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
            if (!_modOptionsByKey.TryGetValue(key, out ModOption option))
                return;

            if (option.parameterValues == null || option.parameterValues.Length == 0)
                option.LoadModOptionParameters();

            if (option.parameterValues == null)
                return;

            // Find matching parameter index
            int index = -1;
            for (int i = 0; i < option.parameterValues.Length; i++)
            {
                var pv = option.parameterValues[i]?.value;
                float f = pv is float fv ? fv : (pv is double dv ? (float)dv : (pv is int iv ? iv : float.NaN));
                if (!float.IsNaN(f) && Mathf.Abs(f - value) < 0.001f)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0 && option.currentValueIndex != index)
            {
                option.Apply(index);
                option.RefreshUI();
            }
        }

        #endregion

        #region Preset Value Tables

        public static float GetPresetMultiplierValue(BodyZone zone, BDOTModOptions.IntensityPreset preset)
        {
            // Light=0.5x, Default=1x, Heavy=1.5x, Brutal=2.5x of base values
            switch (zone)
            {
                case BodyZone.Throat: // Base: 3.0
                    return preset == BDOTModOptions.IntensityPreset.Light ? 1.5f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 4.5f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 7.5f : 3.0f;
                case BodyZone.Head: // Base: 2.0
                    return preset == BDOTModOptions.IntensityPreset.Light ? 1.0f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 3.0f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 5.0f : 2.0f;
                case BodyZone.Neck: // Base: 2.5
                    return preset == BDOTModOptions.IntensityPreset.Light ? 1.3f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 3.8f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 6.3f : 2.5f;
                case BodyZone.Torso: // Base: 1.0
                    return preset == BDOTModOptions.IntensityPreset.Light ? 0.5f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 1.5f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 2.5f : 1.0f;
                case BodyZone.Arm: // Base: 0.5
                    return preset == BDOTModOptions.IntensityPreset.Light ? 0.3f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 0.8f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 1.3f : 0.5f;
                case BodyZone.Leg: // Base: 0.6
                    return preset == BDOTModOptions.IntensityPreset.Light ? 0.3f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 0.9f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 1.5f : 0.6f;
                case BodyZone.Dismemberment: // Base: 2.5
                    return preset == BDOTModOptions.IntensityPreset.Light ? 1.3f :
                           preset == BDOTModOptions.IntensityPreset.Heavy ? 3.8f :
                           preset == BDOTModOptions.IntensityPreset.Brutal ? 6.3f : 2.5f;
                default:
                    return 1.0f;
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

        #endregion
    }
}
