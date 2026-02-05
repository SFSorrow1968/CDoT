using System;
using System.Collections.Generic;
using System.Reflection;
using DOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace DOT.Core
{
    public class DOTModOptionVisibility
    {
        private static DOTModOptionVisibility _instance;
        public static DOTModOptionVisibility Instance => _instance ??= new DOTModOptionVisibility();

        private ModManager.ModData _modData;
        private bool _initialized;
        private DOTModOptions.DamagePreset? _lastDamagePreset;
        private DOTModOptions.DurationPreset? _lastDurationPreset;
        private DOTModOptions.FrequencyPreset? _lastFrequencyPreset;
        private DOTModOptions.ChancePreset? _lastChancePreset;

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
            { BodyZone.Throat, MakeKey(DOTModOptions.CategoryZoneThroat, DOTModOptions.OptionThroatChance) },
            { BodyZone.Head, MakeKey(DOTModOptions.CategoryZoneHead, DOTModOptions.OptionHeadChance) },
            { BodyZone.Neck, MakeKey(DOTModOptions.CategoryZoneNeck, DOTModOptions.OptionNeckChance) },
            { BodyZone.Torso, MakeKey(DOTModOptions.CategoryZoneTorso, DOTModOptions.OptionTorsoChance) },
            { BodyZone.Arm, MakeKey(DOTModOptions.CategoryZoneArm, DOTModOptions.OptionArmChance) },
            { BodyZone.Leg, MakeKey(DOTModOptions.CategoryZoneLeg, DOTModOptions.OptionLegChance) },
            { BodyZone.Dismemberment, MakeKey(DOTModOptions.CategoryZoneDismemberment, DOTModOptions.OptionDismembermentChance) }
        };

        // Option key mappings for Damage
        private static readonly Dictionary<BodyZone, string> DamageKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(DOTModOptions.CategoryZoneThroat, DOTModOptions.OptionThroatDamage) },
            { BodyZone.Head, MakeKey(DOTModOptions.CategoryZoneHead, DOTModOptions.OptionHeadDamage) },
            { BodyZone.Neck, MakeKey(DOTModOptions.CategoryZoneNeck, DOTModOptions.OptionNeckDamage) },
            { BodyZone.Torso, MakeKey(DOTModOptions.CategoryZoneTorso, DOTModOptions.OptionTorsoDamage) },
            { BodyZone.Arm, MakeKey(DOTModOptions.CategoryZoneArm, DOTModOptions.OptionArmDamage) },
            { BodyZone.Leg, MakeKey(DOTModOptions.CategoryZoneLeg, DOTModOptions.OptionLegDamage) },
            { BodyZone.Dismemberment, MakeKey(DOTModOptions.CategoryZoneDismemberment, DOTModOptions.OptionDismembermentDamage) }
        };

        // Option key mappings for Duration
        private static readonly Dictionary<BodyZone, string> DurationKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(DOTModOptions.CategoryZoneThroat, DOTModOptions.OptionThroatDuration) },
            { BodyZone.Head, MakeKey(DOTModOptions.CategoryZoneHead, DOTModOptions.OptionHeadDuration) },
            { BodyZone.Neck, MakeKey(DOTModOptions.CategoryZoneNeck, DOTModOptions.OptionNeckDuration) },
            { BodyZone.Torso, MakeKey(DOTModOptions.CategoryZoneTorso, DOTModOptions.OptionTorsoDuration) },
            { BodyZone.Arm, MakeKey(DOTModOptions.CategoryZoneArm, DOTModOptions.OptionArmDuration) },
            { BodyZone.Leg, MakeKey(DOTModOptions.CategoryZoneLeg, DOTModOptions.OptionLegDuration) },
            { BodyZone.Dismemberment, MakeKey(DOTModOptions.CategoryZoneDismemberment, DOTModOptions.OptionDismembermentDuration) }
        };

        // Option key mappings for Frequency
        private static readonly Dictionary<BodyZone, string> FrequencyKeys = new Dictionary<BodyZone, string>
        {
            { BodyZone.Throat, MakeKey(DOTModOptions.CategoryZoneThroat, DOTModOptions.OptionThroatFrequency) },
            { BodyZone.Head, MakeKey(DOTModOptions.CategoryZoneHead, DOTModOptions.OptionHeadFrequency) },
            { BodyZone.Neck, MakeKey(DOTModOptions.CategoryZoneNeck, DOTModOptions.OptionNeckFrequency) },
            { BodyZone.Torso, MakeKey(DOTModOptions.CategoryZoneTorso, DOTModOptions.OptionTorsoFrequency) },
            { BodyZone.Arm, MakeKey(DOTModOptions.CategoryZoneArm, DOTModOptions.OptionArmFrequency) },
            { BodyZone.Leg, MakeKey(DOTModOptions.CategoryZoneLeg, DOTModOptions.OptionLegFrequency) },
            { BodyZone.Dismemberment, MakeKey(DOTModOptions.CategoryZoneDismemberment, DOTModOptions.OptionDismembermentFrequency) }
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

            Debug.Log("[DOT] ModOptionVisibility initialized");
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
        }

        private void ApplyDamagePreset(bool force)
        {
            var preset = DOTModOptions.GetDamagePreset();
            if (!force && _lastDamagePreset == preset)
                return;

            Debug.Log("[DOT] Applying Damage Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDamageValue(zone, preset);
                DOTModOptions.SetZoneDamage(zone, value);
                SyncOption(DamageKeys[zone], value);
            }

            _lastDamagePreset = preset;
        }

        private void ApplyDurationPreset(bool force)
        {
            var preset = DOTModOptions.GetDurationPreset();
            if (!force && _lastDurationPreset == preset)
                return;

            Debug.Log("[DOT] Applying Duration Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetDurationValue(zone, preset);
                DOTModOptions.SetZoneDuration(zone, value);
                SyncOption(DurationKeys[zone], value);
            }

            _lastDurationPreset = preset;
        }

        private void ApplyFrequencyPreset(bool force)
        {
            var preset = DOTModOptions.GetFrequencyPreset();
            if (!force && _lastFrequencyPreset == preset)
                return;

            Debug.Log("[DOT] Applying Frequency Preset: " + preset);

            // Frequency is now per-zone
            foreach (var zone in AllZones)
            {
                float value = GetPresetFrequencyValue(zone, preset);
                DOTModOptions.SetZoneFrequency(zone, value);
                SyncOption(FrequencyKeys[zone], value);
            }

            _lastFrequencyPreset = preset;
        }

        private void ApplyChancePreset(bool force)
        {
            var preset = DOTModOptions.GetChancePreset();
            if (!force && _lastChancePreset == preset)
                return;

            Debug.Log("[DOT] Applying Chance Preset: " + preset);

            foreach (var zone in AllZones)
            {
                float value = GetPresetChanceValue(zone, preset);
                DOTModOptions.SetZoneChance(zone, value);
                SyncOption(ChanceKeys[zone], value);
            }

            _lastChancePreset = preset;
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

        public static float GetPresetDamageValue(BodyZone zone, DOTModOptions.DamagePreset preset)
        {
            switch (zone)
            {
                case BodyZone.Throat:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.5f;
                        case DOTModOptions.DamagePreset.Low: return 1.25f;
                        case DOTModOptions.DamagePreset.Default: return 2.5f;
                        case DOTModOptions.DamagePreset.High: return 5.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 10.0f;
                        default: return 2.5f;
                    }
                case BodyZone.Head:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case DOTModOptions.DamagePreset.Low: return 0.75f;
                        case DOTModOptions.DamagePreset.Default: return 1.5f;
                        case DOTModOptions.DamagePreset.High: return 3.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 6.0f;
                        default: return 1.5f;
                    }
                case BodyZone.Neck:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.5f;
                        case DOTModOptions.DamagePreset.Low: return 1.0f;
                        case DOTModOptions.DamagePreset.Default: return 2.0f;
                        case DOTModOptions.DamagePreset.High: return 4.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 8.0f;
                        default: return 2.0f;
                    }
                case BodyZone.Torso:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case DOTModOptions.DamagePreset.Low: return 0.5f;
                        case DOTModOptions.DamagePreset.Default: return 1.0f;
                        case DOTModOptions.DamagePreset.High: return 2.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 4.0f;
                        default: return 1.0f;
                    }
                case BodyZone.Arm:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case DOTModOptions.DamagePreset.Low: return 0.25f;
                        case DOTModOptions.DamagePreset.Default: return 0.5f;
                        case DOTModOptions.DamagePreset.High: return 1.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 2.0f;
                        default: return 0.5f;
                    }
                case BodyZone.Leg:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 0.25f;
                        case DOTModOptions.DamagePreset.Low: return 0.5f;
                        case DOTModOptions.DamagePreset.Default: return 0.75f;
                        case DOTModOptions.DamagePreset.High: return 1.5f;
                        case DOTModOptions.DamagePreset.Extreme: return 3.0f;
                        default: return 0.75f;
                    }
                case BodyZone.Dismemberment:
                    switch (preset)
                    {
                        case DOTModOptions.DamagePreset.Minimal: return 1.0f;
                        case DOTModOptions.DamagePreset.Low: return 2.0f;
                        case DOTModOptions.DamagePreset.Default: return 3.0f;
                        case DOTModOptions.DamagePreset.High: return 6.0f;
                        case DOTModOptions.DamagePreset.Extreme: return 12.0f;
                        default: return 3.0f;
                    }
                default:
                    return 1.0f;
            }
        }

        // ========== DURATION PRESET VALUES ==========
        // 5 presets: VeryShort (0), Short (1), Default (2), Long (3), Extended (4)

        public static float GetPresetDurationValue(BodyZone zone, DOTModOptions.DurationPreset preset)
        {
            switch (zone)
            {
                case BodyZone.Throat:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 2.0f;
                        case DOTModOptions.DurationPreset.Short: return 4.0f;
                        case DOTModOptions.DurationPreset.Default: return 6.0f;
                        case DOTModOptions.DurationPreset.Long: return 10.0f;
                        case DOTModOptions.DurationPreset.Extended: return 15.0f;
                        default: return 6.0f;
                    }
                case BodyZone.Head:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 1.5f;
                        case DOTModOptions.DurationPreset.Short: return 3.0f;
                        case DOTModOptions.DurationPreset.Default: return 5.0f;
                        case DOTModOptions.DurationPreset.Long: return 8.0f;
                        case DOTModOptions.DurationPreset.Extended: return 12.0f;
                        default: return 5.0f;
                    }
                case BodyZone.Neck:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 2.0f;
                        case DOTModOptions.DurationPreset.Short: return 3.5f;
                        case DOTModOptions.DurationPreset.Default: return 5.5f;
                        case DOTModOptions.DurationPreset.Long: return 9.0f;
                        case DOTModOptions.DurationPreset.Extended: return 14.0f;
                        default: return 5.5f;
                    }
                case BodyZone.Torso:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 1.5f;
                        case DOTModOptions.DurationPreset.Short: return 2.5f;
                        case DOTModOptions.DurationPreset.Default: return 4.0f;
                        case DOTModOptions.DurationPreset.Long: return 7.0f;
                        case DOTModOptions.DurationPreset.Extended: return 10.0f;
                        default: return 4.0f;
                    }
                case BodyZone.Arm:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 1.0f;
                        case DOTModOptions.DurationPreset.Short: return 2.0f;
                        case DOTModOptions.DurationPreset.Default: return 3.0f;
                        case DOTModOptions.DurationPreset.Long: return 5.0f;
                        case DOTModOptions.DurationPreset.Extended: return 8.0f;
                        default: return 3.0f;
                    }
                case BodyZone.Leg:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 1.0f;
                        case DOTModOptions.DurationPreset.Short: return 2.5f;
                        case DOTModOptions.DurationPreset.Default: return 3.5f;
                        case DOTModOptions.DurationPreset.Long: return 6.0f;
                        case DOTModOptions.DurationPreset.Extended: return 9.0f;
                        default: return 3.5f;
                    }
                case BodyZone.Dismemberment:
                    switch (preset)
                    {
                        case DOTModOptions.DurationPreset.VeryShort: return 3.0f;
                        case DOTModOptions.DurationPreset.Short: return 5.0f;
                        case DOTModOptions.DurationPreset.Default: return 8.0f;
                        case DOTModOptions.DurationPreset.Long: return 12.0f;
                        case DOTModOptions.DurationPreset.Extended: return 20.0f;
                        default: return 8.0f;
                    }
                default:
                    return 4.0f;
            }
        }

        // ========== FREQUENCY PRESET VALUES ==========
        // 5 presets: VerySlow (0), Slow (1), Default (2), Fast (3), Rapid (4)
        // Returns tick interval in seconds per zone

        public static float GetPresetFrequencyValue(BodyZone zone, DOTModOptions.FrequencyPreset preset)
        {
            switch (zone)
            {
                case BodyZone.Throat:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 2.0f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.0f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.5f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.3f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                        default: return 0.5f;
                    }
                case BodyZone.Head:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 2.5f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.2f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.6f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.3f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                        default: return 0.6f;
                    }
                case BodyZone.Neck:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 2.0f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.0f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.5f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.25f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                        default: return 0.5f;
                    }
                case BodyZone.Torso:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 3.0f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.5f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.8f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.4f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.2f;
                        default: return 0.8f;
                    }
                case BodyZone.Arm:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 3.5f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.8f;
                        case DOTModOptions.FrequencyPreset.Default: return 1.0f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.5f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.2f;
                        default: return 1.0f;
                    }
                case BodyZone.Leg:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 3.0f;
                        case DOTModOptions.FrequencyPreset.Slow: return 1.5f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.8f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.4f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.2f;
                        default: return 0.8f;
                    }
                case BodyZone.Dismemberment:
                    switch (preset)
                    {
                        case DOTModOptions.FrequencyPreset.VerySlow: return 1.5f;
                        case DOTModOptions.FrequencyPreset.Slow: return 0.8f;
                        case DOTModOptions.FrequencyPreset.Default: return 0.4f;
                        case DOTModOptions.FrequencyPreset.Fast: return 0.2f;
                        case DOTModOptions.FrequencyPreset.Rapid: return 0.1f;
                        default: return 0.4f;
                    }
                default:
                    return 0.5f;
            }
        }

        // ========== CHANCE PRESET VALUES ==========
        // 5 presets: Off (0), Rare (1), Default (2), Frequent (3), Always (4)

        public static float GetPresetChanceValue(BodyZone zone, DOTModOptions.ChancePreset preset)
        {
            switch (preset)
            {
                case DOTModOptions.ChancePreset.Off:
                    return 0f;

                case DOTModOptions.ChancePreset.Rare:
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

                case DOTModOptions.ChancePreset.Default:
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

                case DOTModOptions.ChancePreset.Frequent:
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

                case DOTModOptions.ChancePreset.Always:
                    return 100f;

                default:
                    return 35f;
            }
        }

        #endregion
    }
}
