using System;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Configuration
{
    public static class BDOTModOptions
    {
        public const string VERSION = "1.0.0";

        #region Labels and Categories

        public const string CategoryPresetSelection = "Preset Selection";
        public const string CategoryZoneToggles = "Zone Toggles";
        public const string CategoryZoneThroat = "Throat";
        public const string CategoryZoneHead = "Head";
        public const string CategoryZoneNeck = "Neck";
        public const string CategoryZoneTorso = "Torso";
        public const string CategoryZoneArm = "Arm";
        public const string CategoryZoneLeg = "Leg";
        public const string CategoryZoneDismemberment = "Dismemberment";
        public const string CategoryAdvanced = "Advanced";
        public const string CategoryStatistics = "Statistics";

        // Global options
        public const string OptionEnableMod = "Enable Mod";
        public const string OptionIntensityPreset = "Intensity Preset";
        public const string OptionDurationPreset = "Duration Preset";
        public const string OptionDamagePreset = "Damage Preset";
        public const string OptionGlobalMultiplier = "Global Damage Multiplier";
        public const string OptionTickInterval = "Tick Interval";

        // Zone toggles
        public const string OptionThroatEnabled = "Throat Enabled";
        public const string OptionHeadEnabled = "Head Enabled";
        public const string OptionNeckEnabled = "Neck Enabled";
        public const string OptionTorsoEnabled = "Torso Enabled";
        public const string OptionArmEnabled = "Arm Enabled";
        public const string OptionLegEnabled = "Leg Enabled";
        public const string OptionDismembermentEnabled = "Dismemberment Enabled";

        // Throat zone - unique names
        public const string OptionThroatMultiplier = "Throat Multiplier";
        public const string OptionThroatDuration = "Throat Duration";
        public const string OptionThroatDamagePerTick = "Throat Damage Per Tick";
        public const string OptionThroatStackLimit = "Throat Stack Limit";

        // Head zone - unique names
        public const string OptionHeadMultiplier = "Head Multiplier";
        public const string OptionHeadDuration = "Head Duration";
        public const string OptionHeadDamagePerTick = "Head Damage Per Tick";
        public const string OptionHeadStackLimit = "Head Stack Limit";

        // Neck zone - unique names
        public const string OptionNeckMultiplier = "Neck Multiplier";
        public const string OptionNeckDuration = "Neck Duration";
        public const string OptionNeckDamagePerTick = "Neck Damage Per Tick";
        public const string OptionNeckStackLimit = "Neck Stack Limit";

        // Torso zone - unique names
        public const string OptionTorsoMultiplier = "Torso Multiplier";
        public const string OptionTorsoDuration = "Torso Duration";
        public const string OptionTorsoDamagePerTick = "Torso Damage Per Tick";
        public const string OptionTorsoStackLimit = "Torso Stack Limit";

        // Arm zone - unique names
        public const string OptionArmMultiplier = "Arm Multiplier";
        public const string OptionArmDuration = "Arm Duration";
        public const string OptionArmDamagePerTick = "Arm Damage Per Tick";
        public const string OptionArmStackLimit = "Arm Stack Limit";

        // Leg zone - unique names
        public const string OptionLegMultiplier = "Leg Multiplier";
        public const string OptionLegDuration = "Leg Duration";
        public const string OptionLegDamagePerTick = "Leg Damage Per Tick";
        public const string OptionLegStackLimit = "Leg Stack Limit";

        // Dismemberment zone - unique names
        public const string OptionDismembermentMultiplier = "Dismemberment Multiplier";
        public const string OptionDismembermentDuration = "Dismemberment Duration";
        public const string OptionDismembermentDamagePerTick = "Dismemberment Damage Per Tick";
        public const string OptionDismembermentStackLimit = "Dismemberment Stack Limit";

        // Advanced
        public const string OptionDebugLogging = "Debug Logging";
        public const string OptionResetStats = "Reset Statistics";

        #endregion

        #region Enums

        public enum IntensityPreset
        {
            Light = 0,
            Default = 1,
            Heavy = 2,
            Brutal = 3
        }

        public enum DurationPreset
        {
            Short = 0,
            Default = 1,
            Long = 2,
            Extended = 3
        }

        public enum DamagePreset
        {
            Low = 0,
            Default = 1,
            High = 2,
            Extreme = 3
        }

        #endregion

        #region Value Providers

        public static ModOptionString[] IntensityPresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Light", "Light"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("Heavy", "Heavy"),
                new ModOptionString("Brutal", "Brutal")
            };
        }

        public static ModOptionString[] DurationPresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Short", "Short"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("Long", "Long"),
                new ModOptionString("Extended", "Extended")
            };
        }

        public static ModOptionString[] DamagePresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Low", "Low"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("High", "High"),
                new ModOptionString("Extreme", "Extreme")
            };
        }

        public static ModOptionFloat[] GlobalMultiplierProvider()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("0.25x", 0.25f),
                new ModOptionFloat("0.5x", 0.5f),
                new ModOptionFloat("0.75x", 0.75f),
                new ModOptionFloat("1.0x", 1.0f),
                new ModOptionFloat("1.25x", 1.25f),
                new ModOptionFloat("1.5x", 1.5f),
                new ModOptionFloat("2.0x", 2.0f),
                new ModOptionFloat("3.0x", 3.0f)
            };
        }

        public static ModOptionFloat[] TickIntervalProvider()
        {
            return new ModOptionFloat[]
            {
                new ModOptionFloat("0.25s", 0.25f),
                new ModOptionFloat("0.5s", 0.5f),
                new ModOptionFloat("0.75s", 0.75f),
                new ModOptionFloat("1.0s", 1.0f),
                new ModOptionFloat("1.5s", 1.5f),
                new ModOptionFloat("2.0s", 2.0f)
            };
        }

        public static ModOptionFloat[] MultiplierProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // 0.1x to 10.0x in 0.1 increments (100 options)
            for (int i = 1; i <= 100; i++)
            {
                float val = i / 10f;
                list.Add(new ModOptionFloat(val.ToString("0.0") + "x", val));
            }
            return list.ToArray();
        }

        public static ModOptionFloat[] DurationProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // 0.5s to 30s in 0.5s increments (60 options)
            for (int i = 1; i <= 60; i++)
            {
                float val = i / 2f;
                list.Add(new ModOptionFloat(val.ToString("0.0") + "s", val));
            }
            return list.ToArray();
        }

        public static ModOptionFloat[] DamagePerTickProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // 0.25 to 20 in 0.25 increments (80 options)
            for (int i = 1; i <= 80; i++)
            {
                float val = i / 4f;
                list.Add(new ModOptionFloat(val.ToString("0.00"), val));
            }
            return list.ToArray();
        }

        public static ModOptionInt[] StackLimitProvider()
        {
            return new ModOptionInt[]
            {
                new ModOptionInt("1", 1),
                new ModOptionInt("2", 2),
                new ModOptionInt("3", 3),
                new ModOptionInt("4", 4),
                new ModOptionInt("5", 5),
                new ModOptionInt("10", 10)
            };
        }

        #endregion

        private const int CategoryOrderPreset = 10;
        private const int CategoryOrderToggles = 20;
        private const int CategoryOrderThroat = 30;
        private const int CategoryOrderHead = 31;
        private const int CategoryOrderNeck = 32;
        private const int CategoryOrderTorso = 33;
        private const int CategoryOrderArm = 34;
        private const int CategoryOrderLeg = 35;
        private const int CategoryOrderDismemberment = 40;
        private const int CategoryOrderAdvanced = 90;
        private const int CategoryOrderStatistics = 95;

        #region Global Settings (Preset Selection)

        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = "Master switch for the entire mod")]
        public static bool EnableMod = true;

        [ModOption(name = OptionIntensityPreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 10, defaultValueIndex = 1, valueSourceName = "IntensityPresetProvider", tooltip = "Overall bleed intensity. Applies multiplier values to all zones.")]
        public static string IntensityPresetSetting = "Default";

        [ModOption(name = OptionDurationPreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 20, defaultValueIndex = 1, valueSourceName = "DurationPresetProvider", tooltip = "Bleed duration preset. Applies duration values to all zones.")]
        public static string DurationPresetSetting = "Default";

        [ModOption(name = OptionDamagePreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 30, defaultValueIndex = 1, valueSourceName = "DamagePresetProvider", tooltip = "Damage per tick preset. Applies damage values to all zones.")]
        public static string DamagePresetSetting = "Default";

        [ModOption(name = OptionGlobalMultiplier, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 40, defaultValueIndex = 3, valueSourceName = "GlobalMultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Final multiplier applied to all bleed damage")]
        public static float GlobalDamageMultiplier = 1.0f;

        [ModOption(name = OptionTickInterval, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 50, defaultValueIndex = 1, valueSourceName = "TickIntervalProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Time between damage ticks (all zones)")]
        public static float TickInterval = 0.5f;

        #endregion

        #region Zone Toggles

        [ModOption(name = OptionThroatEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 10, defaultValueIndex = 1, tooltip = "Enable bleeding from throat wounds")]
        public static bool ThroatEnabled = true;

        [ModOption(name = OptionHeadEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 20, defaultValueIndex = 1, tooltip = "Enable bleeding from head wounds")]
        public static bool HeadEnabled = true;

        [ModOption(name = OptionNeckEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 30, defaultValueIndex = 1, tooltip = "Enable bleeding from neck wounds")]
        public static bool NeckEnabled = true;

        [ModOption(name = OptionTorsoEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 40, defaultValueIndex = 1, tooltip = "Enable bleeding from torso wounds")]
        public static bool TorsoEnabled = true;

        [ModOption(name = OptionArmEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 50, defaultValueIndex = 1, tooltip = "Enable bleeding from arm wounds")]
        public static bool ArmEnabled = true;

        [ModOption(name = OptionLegEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 60, defaultValueIndex = 1, tooltip = "Enable bleeding from leg wounds")]
        public static bool LegEnabled = true;

        [ModOption(name = OptionDismembermentEnabled, category = CategoryZoneToggles, categoryOrder = CategoryOrderToggles, order = 70, defaultValueIndex = 1, tooltip = "Enable bleeding from dismemberment")]
        public static bool DismembermentEnabled = true;

        #endregion

        #region Throat Zone
        // Multiplier: index = (value * 10) - 1, Duration: index = (value * 2) - 1, Damage: index = (value * 4) - 1

        [ModOption(name = OptionThroatMultiplier, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 10, defaultValueIndex = 29, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for throat wounds")]
        public static float ThroatMultiplier = 3.0f;

        [ModOption(name = OptionThroatDuration, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 20, defaultValueIndex = 15, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long throat bleeds last")]
        public static float ThroatDuration = 8.0f;

        [ModOption(name = OptionThroatDamagePerTick, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 30, defaultValueIndex = 19, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for throat wounds")]
        public static float ThroatDamagePerTick = 5.0f;

        [ModOption(name = OptionThroatStackLimit, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 40, defaultValueIndex = 2, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for throat wounds")]
        public static int ThroatStackLimit = 3;

        #endregion

        #region Head Zone

        [ModOption(name = OptionHeadMultiplier, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 10, defaultValueIndex = 19, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for head wounds")]
        public static float HeadMultiplier = 2.0f;

        [ModOption(name = OptionHeadDuration, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 20, defaultValueIndex = 11, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long head bleeds last")]
        public static float HeadDuration = 6.0f;

        [ModOption(name = OptionHeadDamagePerTick, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 30, defaultValueIndex = 11, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for head wounds")]
        public static float HeadDamagePerTick = 3.0f;

        [ModOption(name = OptionHeadStackLimit, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 40, defaultValueIndex = 2, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for head wounds")]
        public static int HeadStackLimit = 3;

        #endregion

        #region Neck Zone

        [ModOption(name = OptionNeckMultiplier, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 10, defaultValueIndex = 24, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for neck wounds")]
        public static float NeckMultiplier = 2.5f;

        [ModOption(name = OptionNeckDuration, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 20, defaultValueIndex = 13, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long neck bleeds last")]
        public static float NeckDuration = 7.0f;

        [ModOption(name = OptionNeckDamagePerTick, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 30, defaultValueIndex = 15, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for neck wounds")]
        public static float NeckDamagePerTick = 4.0f;

        [ModOption(name = OptionNeckStackLimit, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 40, defaultValueIndex = 2, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for neck wounds")]
        public static int NeckStackLimit = 3;

        #endregion

        #region Torso Zone

        [ModOption(name = OptionTorsoMultiplier, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 10, defaultValueIndex = 9, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for torso wounds")]
        public static float TorsoMultiplier = 1.0f;

        [ModOption(name = OptionTorsoDuration, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 20, defaultValueIndex = 9, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long torso bleeds last")]
        public static float TorsoDuration = 5.0f;

        [ModOption(name = OptionTorsoDamagePerTick, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 30, defaultValueIndex = 7, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for torso wounds")]
        public static float TorsoDamagePerTick = 2.0f;

        [ModOption(name = OptionTorsoStackLimit, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 40, defaultValueIndex = 4, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for torso wounds")]
        public static int TorsoStackLimit = 5;

        #endregion

        #region Arm Zone

        [ModOption(name = OptionArmMultiplier, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 10, defaultValueIndex = 4, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for arm wounds")]
        public static float ArmMultiplier = 0.5f;

        [ModOption(name = OptionArmDuration, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 20, defaultValueIndex = 7, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long arm bleeds last")]
        public static float ArmDuration = 4.0f;

        [ModOption(name = OptionArmDamagePerTick, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 30, defaultValueIndex = 3, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for arm wounds")]
        public static float ArmDamagePerTick = 1.0f;

        [ModOption(name = OptionArmStackLimit, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 40, defaultValueIndex = 3, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for arm wounds")]
        public static int ArmStackLimit = 4;

        #endregion

        #region Leg Zone

        [ModOption(name = OptionLegMultiplier, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 10, defaultValueIndex = 5, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for leg wounds")]
        public static float LegMultiplier = 0.6f;

        [ModOption(name = OptionLegDuration, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 20, defaultValueIndex = 9, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long leg bleeds last")]
        public static float LegDuration = 5.0f;

        [ModOption(name = OptionLegDamagePerTick, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 30, defaultValueIndex = 5, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for leg wounds")]
        public static float LegDamagePerTick = 1.5f;

        [ModOption(name = OptionLegStackLimit, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 40, defaultValueIndex = 3, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for leg wounds")]
        public static int LegStackLimit = 4;

        #endregion

        #region Dismemberment Zone

        [ModOption(name = OptionDismembermentMultiplier, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 10, defaultValueIndex = 24, valueSourceName = "MultiplierProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Damage multiplier for dismemberment")]
        public static float DismembermentMultiplier = 2.5f;

        [ModOption(name = OptionDismembermentDuration, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 20, defaultValueIndex = 19, valueSourceName = "DurationProvider", interactionType = (ModOption.InteractionType)2, tooltip = "How long dismemberment bleeds last")]
        public static float DismembermentDuration = 10.0f;

        [ModOption(name = OptionDismembermentDamagePerTick, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 30, defaultValueIndex = 23, valueSourceName = "DamagePerTickProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for dismemberment")]
        public static float DismembermentDamagePerTick = 6.0f;

        [ModOption(name = OptionDismembermentStackLimit, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 40, defaultValueIndex = 0, valueSourceName = "StackLimitProvider", interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for dismemberment (per limb)")]
        public static int DismembermentStackLimit = 1;

        #endregion

        #region Advanced

        [ModOption(name = OptionDebugLogging, category = CategoryAdvanced, categoryOrder = CategoryOrderAdvanced, order = 10, defaultValueIndex = 0, tooltip = "Enable verbose debug logging")]
        public static bool DebugLogging = false;

        #endregion

        #region Statistics

        [ModOption(name = OptionResetStats, category = CategoryStatistics, categoryOrder = CategoryOrderStatistics, order = 10, defaultValueIndex = 0, tooltip = "Toggle to reset all statistics")]
        public static bool ResetStatsToggle = false;

        #endregion

        #region Helper Methods

        public struct ZoneConfig
        {
            public bool Enabled;
            public float Multiplier;
            public float Duration;
            public float DamagePerTick;
            public int StackLimit;
        }

        public static bool IsZoneEnabled(BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return ThroatEnabled;
                case BodyZone.Head: return HeadEnabled;
                case BodyZone.Neck: return NeckEnabled;
                case BodyZone.Torso: return TorsoEnabled;
                case BodyZone.Arm: return ArmEnabled;
                case BodyZone.Leg: return LegEnabled;
                case BodyZone.Dismemberment: return DismembermentEnabled;
                default: return false;
            }
        }

        public static ZoneConfig GetZoneConfig(BodyZone zone)
        {
            var config = new ZoneConfig();
            switch (zone)
            {
                case BodyZone.Throat:
                    config.Enabled = ThroatEnabled;
                    config.Multiplier = ThroatMultiplier;
                    config.Duration = ThroatDuration;
                    config.DamagePerTick = ThroatDamagePerTick;
                    config.StackLimit = ThroatStackLimit;
                    break;
                case BodyZone.Head:
                    config.Enabled = HeadEnabled;
                    config.Multiplier = HeadMultiplier;
                    config.Duration = HeadDuration;
                    config.DamagePerTick = HeadDamagePerTick;
                    config.StackLimit = HeadStackLimit;
                    break;
                case BodyZone.Neck:
                    config.Enabled = NeckEnabled;
                    config.Multiplier = NeckMultiplier;
                    config.Duration = NeckDuration;
                    config.DamagePerTick = NeckDamagePerTick;
                    config.StackLimit = NeckStackLimit;
                    break;
                case BodyZone.Torso:
                    config.Enabled = TorsoEnabled;
                    config.Multiplier = TorsoMultiplier;
                    config.Duration = TorsoDuration;
                    config.DamagePerTick = TorsoDamagePerTick;
                    config.StackLimit = TorsoStackLimit;
                    break;
                case BodyZone.Arm:
                    config.Enabled = ArmEnabled;
                    config.Multiplier = ArmMultiplier;
                    config.Duration = ArmDuration;
                    config.DamagePerTick = ArmDamagePerTick;
                    config.StackLimit = ArmStackLimit;
                    break;
                case BodyZone.Leg:
                    config.Enabled = LegEnabled;
                    config.Multiplier = LegMultiplier;
                    config.Duration = LegDuration;
                    config.DamagePerTick = LegDamagePerTick;
                    config.StackLimit = LegStackLimit;
                    break;
                case BodyZone.Dismemberment:
                    config.Enabled = DismembermentEnabled;
                    config.Multiplier = DismembermentMultiplier;
                    config.Duration = DismembermentDuration;
                    config.DamagePerTick = DismembermentDamagePerTick;
                    config.StackLimit = DismembermentStackLimit;
                    break;
                default:
                    config.Enabled = false;
                    config.Multiplier = 1.0f;
                    config.Duration = 5.0f;
                    config.DamagePerTick = 2.0f;
                    config.StackLimit = 3;
                    break;
            }
            return config;
        }

        public static IntensityPreset GetIntensityPreset()
        {
            switch (IntensityPresetSetting)
            {
                case "Light": return IntensityPreset.Light;
                case "Heavy": return IntensityPreset.Heavy;
                case "Brutal": return IntensityPreset.Brutal;
                default: return IntensityPreset.Default;
            }
        }

        public static DurationPreset GetDurationPreset()
        {
            switch (DurationPresetSetting)
            {
                case "Short": return DurationPreset.Short;
                case "Long": return DurationPreset.Long;
                case "Extended": return DurationPreset.Extended;
                default: return DurationPreset.Default;
            }
        }

        public static DamagePreset GetDamagePreset()
        {
            switch (DamagePresetSetting)
            {
                case "Low": return DamagePreset.Low;
                case "High": return DamagePreset.High;
                case "Extreme": return DamagePreset.Extreme;
                default: return DamagePreset.Default;
            }
        }

        // Preset value application - these return the multipliers for each preset
        public static float GetIntensityMultiplier(IntensityPreset preset)
        {
            switch (preset)
            {
                case IntensityPreset.Light: return 0.5f;
                case IntensityPreset.Heavy: return 1.5f;
                case IntensityPreset.Brutal: return 2.5f;
                default: return 1.0f;
            }
        }

        public static float GetDurationMultiplier(DurationPreset preset)
        {
            switch (preset)
            {
                case DurationPreset.Short: return 0.6f;
                case DurationPreset.Long: return 1.5f;
                case DurationPreset.Extended: return 2.0f;
                default: return 1.0f;
            }
        }

        public static float GetDamageMultiplier(DamagePreset preset)
        {
            switch (preset)
            {
                case DamagePreset.Low: return 0.5f;
                case DamagePreset.High: return 1.5f;
                case DamagePreset.Extreme: return 2.5f;
                default: return 1.0f;
            }
        }

        // Zone value setters for preset application
        public static void SetZoneMultiplier(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatMultiplier = value; break;
                case BodyZone.Head: HeadMultiplier = value; break;
                case BodyZone.Neck: NeckMultiplier = value; break;
                case BodyZone.Torso: TorsoMultiplier = value; break;
                case BodyZone.Arm: ArmMultiplier = value; break;
                case BodyZone.Leg: LegMultiplier = value; break;
                case BodyZone.Dismemberment: DismembermentMultiplier = value; break;
            }
        }

        public static void SetZoneDuration(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatDuration = value; break;
                case BodyZone.Head: HeadDuration = value; break;
                case BodyZone.Neck: NeckDuration = value; break;
                case BodyZone.Torso: TorsoDuration = value; break;
                case BodyZone.Arm: ArmDuration = value; break;
                case BodyZone.Leg: LegDuration = value; break;
                case BodyZone.Dismemberment: DismembermentDuration = value; break;
            }
        }

        public static void SetZoneDamagePerTick(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatDamagePerTick = value; break;
                case BodyZone.Head: HeadDamagePerTick = value; break;
                case BodyZone.Neck: NeckDamagePerTick = value; break;
                case BodyZone.Torso: TorsoDamagePerTick = value; break;
                case BodyZone.Arm: ArmDamagePerTick = value; break;
                case BodyZone.Leg: LegDamagePerTick = value; break;
                case BodyZone.Dismemberment: DismembermentDamagePerTick = value; break;
            }
        }

        #endregion

        #region Statistics

        private const string StatPrefixTotalDamage = "BDOT_TotalDamage";
        private const string StatPrefixBleedCount = "BDOT_BleedCount";

        public static float GetTotalBleedDamage()
        {
            return PlayerPrefs.GetFloat(StatPrefixTotalDamage, 0f);
        }

        public static void AddBleedDamage(float damage)
        {
            float current = GetTotalBleedDamage();
            PlayerPrefs.SetFloat(StatPrefixTotalDamage, current + damage);
        }

        public static int GetTotalBleedCount()
        {
            return PlayerPrefs.GetInt(StatPrefixBleedCount, 0);
        }

        public static void IncrementBleedCount()
        {
            int current = GetTotalBleedCount();
            PlayerPrefs.SetInt(StatPrefixBleedCount, current + 1);
        }

        public static void ResetStatistics()
        {
            PlayerPrefs.SetFloat(StatPrefixTotalDamage, 0f);
            PlayerPrefs.SetInt(StatPrefixBleedCount, 0);
            PlayerPrefs.Save();
        }

        public static string GetStatisticsSummary()
        {
            float totalDamage = GetTotalBleedDamage();
            int bleedCount = GetTotalBleedCount();
            return "Total Damage: " + totalDamage.ToString("F1") + " | Bleeds: " + bleedCount;
        }

        #endregion
    }
}
