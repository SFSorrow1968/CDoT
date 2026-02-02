using System;
using ThunderRoad;
using UnityEngine;

namespace BDOT.Configuration
{
    public static class BDOTModOptions
    {
        public const string VERSION = "1.2.0";

        #region Labels and Categories

        public const string CategoryPresetSelection = "Preset Selection";
        public const string CategoryDamageTypeMultipliers = "Damage Type Multipliers";
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
        public const string OptionDamagePreset = "Damage Preset";
        public const string OptionDurationPreset = "Duration Preset";
        public const string OptionFrequencyPreset = "Frequency Preset";
        public const string OptionChancePreset = "Chance Preset";

        // Damage type multipliers
        public const string OptionPierceMultiplier = "Pierce Bleed Mult";
        public const string OptionSlashMultiplier = "Slash Bleed Mult";
        public const string OptionBluntMultiplier = "Blunt Bleed Mult";

        // Zone toggles
        public const string OptionThroatEnabled = "Throat Enabled";
        public const string OptionHeadEnabled = "Head Enabled";
        public const string OptionNeckEnabled = "Neck Enabled";
        public const string OptionTorsoEnabled = "Torso Enabled";
        public const string OptionArmEnabled = "Arm Enabled";
        public const string OptionLegEnabled = "Leg Enabled";
        public const string OptionDismembermentEnabled = "Dismemberment Enabled";

        // Throat zone - unique names
        public const string OptionThroatChance = "Throat Chance";
        public const string OptionThroatDamage = "Throat Damage";
        public const string OptionThroatDuration = "Throat Duration";
        public const string OptionThroatFrequency = "Throat Frequency";
        public const string OptionThroatStackLimit = "Throat Stack Limit";

        // Head zone - unique names
        public const string OptionHeadChance = "Head Chance";
        public const string OptionHeadDamage = "Head Damage";
        public const string OptionHeadDuration = "Head Duration";
        public const string OptionHeadFrequency = "Head Frequency";
        public const string OptionHeadStackLimit = "Head Stack Limit";

        // Neck zone - unique names
        public const string OptionNeckChance = "Neck Chance";
        public const string OptionNeckDamage = "Neck Damage";
        public const string OptionNeckDuration = "Neck Duration";
        public const string OptionNeckFrequency = "Neck Frequency";
        public const string OptionNeckStackLimit = "Neck Stack Limit";

        // Torso zone - unique names
        public const string OptionTorsoChance = "Torso Chance";
        public const string OptionTorsoDamage = "Torso Damage";
        public const string OptionTorsoDuration = "Torso Duration";
        public const string OptionTorsoFrequency = "Torso Frequency";
        public const string OptionTorsoStackLimit = "Torso Stack Limit";

        // Arm zone - unique names
        public const string OptionArmChance = "Arm Chance";
        public const string OptionArmDamage = "Arm Damage";
        public const string OptionArmDuration = "Arm Duration";
        public const string OptionArmFrequency = "Arm Frequency";
        public const string OptionArmStackLimit = "Arm Stack Limit";

        // Leg zone - unique names
        public const string OptionLegChance = "Leg Chance";
        public const string OptionLegDamage = "Leg Damage";
        public const string OptionLegDuration = "Leg Duration";
        public const string OptionLegFrequency = "Leg Frequency";
        public const string OptionLegStackLimit = "Leg Stack Limit";

        // Dismemberment zone - unique names
        public const string OptionDismembermentChance = "Dismemberment Chance";
        public const string OptionDismembermentDamage = "Dismemberment Damage";
        public const string OptionDismembermentDuration = "Dismemberment Duration";
        public const string OptionDismembermentFrequency = "Dismemberment Frequency";
        public const string OptionDismembermentStackLimit = "Dismemberment Stack Limit";

        // Advanced
        public const string OptionDebugLogging = "Debug Logging";
        public const string OptionResetStats = "Reset Statistics";

        #endregion

        #region Enums

        // 5 presets each: 0=left2, 1=left1, 2=Default(middle), 3=right1, 4=right2
        public enum DamagePreset
        {
            Minimal = 0,
            Low = 1,
            Default = 2,
            High = 3,
            Extreme = 4
        }

        public enum DurationPreset
        {
            VeryShort = 0,
            Short = 1,
            Default = 2,
            Long = 3,
            Extended = 4
        }

        public enum FrequencyPreset
        {
            VerySlow = 0,
            Slow = 1,
            Default = 2,
            Fast = 3,
            Rapid = 4
        }

        public enum ChancePreset
        {
            Off = 0,
            Rare = 1,
            Default = 2,
            Frequent = 3,
            Always = 4
        }

        #endregion

        #region Value Providers

        public static ModOptionString[] DamagePresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Minimal", "Minimal"),
                new ModOptionString("Low", "Low"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("High", "High"),
                new ModOptionString("Extreme", "Extreme")
            };
        }

        public static ModOptionString[] DurationPresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Very Short", "Very Short"),
                new ModOptionString("Short", "Short"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("Long", "Long"),
                new ModOptionString("Extended", "Extended")
            };
        }

        public static ModOptionString[] FrequencyPresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Very Slow", "Very Slow"),
                new ModOptionString("Slow", "Slow"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("Fast", "Fast"),
                new ModOptionString("Rapid", "Rapid")
            };
        }

        public static ModOptionString[] ChancePresetProvider()
        {
            return new ModOptionString[]
            {
                new ModOptionString("Off", "Off"),
                new ModOptionString("Rare", "Rare"),
                new ModOptionString("Default", "Default"),
                new ModOptionString("Frequent", "Frequent"),
                new ModOptionString("Always", "Always")
            };
        }

        public static ModOptionFloat[] DamageTypeMultiplierProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // 0.0x to 3.0x in 0.1 increments (31 options)
            for (int i = 0; i <= 30; i++)
            {
                float val = i / 10f;
                list.Add(new ModOptionFloat(val.ToString("0.0") + "x", val));
            }
            return list.ToArray();
        }

        public static ModOptionFloat[] ChanceProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // 0% to 100% in 5% increments (21 options)
            for (int i = 0; i <= 20; i++)
            {
                float val = i * 5f;
                list.Add(new ModOptionFloat(val.ToString("0") + "%", val));
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

        public static ModOptionFloat[] FrequencyProvider()
        {
            var list = new System.Collections.Generic.List<ModOptionFloat>();
            // Tick interval: 0.1s to 5.0s in 0.1s increments (50 options)
            for (int i = 1; i <= 50; i++)
            {
                float val = i / 10f;
                list.Add(new ModOptionFloat(val.ToString("0.0") + "s", val));
            }
            return list.ToArray();
        }

        public static ModOptionFloat[] DamageProvider()
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

        #region Category Orders

        private const int CategoryOrderPreset = 10;
        private const int CategoryOrderDamageTypeMult = 15;
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

        #endregion

        #region Global Settings (Preset Selection)

        [ModOption(name = OptionEnableMod, order = 0, defaultValueIndex = 1, tooltip = "Master switch for the entire mod")]
        public static bool EnableMod = true;

        [ModOption(name = OptionDamagePreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 10, defaultValueIndex = 2, valueSourceName = nameof(DamagePresetProvider), tooltip = "Damage per tick preset. Default is the balanced middle value.")]
        public static string DamagePresetSetting = "Default";

        [ModOption(name = OptionDurationPreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 20, defaultValueIndex = 2, valueSourceName = nameof(DurationPresetProvider), tooltip = "Bleed duration preset. Default is the balanced middle value.")]
        public static string DurationPresetSetting = "Default";

        [ModOption(name = OptionFrequencyPreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 30, defaultValueIndex = 2, valueSourceName = nameof(FrequencyPresetProvider), tooltip = "Tick frequency preset. Default is the balanced middle value.")]
        public static string FrequencyPresetSetting = "Default";

        [ModOption(name = OptionChancePreset, category = CategoryPresetSelection, categoryOrder = CategoryOrderPreset, order = 40, defaultValueIndex = 2, valueSourceName = nameof(ChancePresetProvider), tooltip = "Bleed chance preset. Default is the balanced middle value.")]
        public static string ChancePresetSetting = "Default";

        #endregion

        #region Damage Type Multipliers

        [ModOption(name = OptionPierceMultiplier, category = CategoryDamageTypeMultipliers, categoryOrder = CategoryOrderDamageTypeMult, order = 10, defaultValueIndex = 10, valueSourceName = nameof(DamageTypeMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Bleed damage multiplier for pierce attacks. 0.0x disables bleed from pierce entirely.")]
        public static float PierceMultiplier = 1.0f;

        [ModOption(name = OptionSlashMultiplier, category = CategoryDamageTypeMultipliers, categoryOrder = CategoryOrderDamageTypeMult, order = 20, defaultValueIndex = 10, valueSourceName = nameof(DamageTypeMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Bleed damage multiplier for slash attacks. 0.0x disables bleed from slash entirely.")]
        public static float SlashMultiplier = 1.0f;

        [ModOption(name = OptionBluntMultiplier, category = CategoryDamageTypeMultipliers, categoryOrder = CategoryOrderDamageTypeMult, order = 30, defaultValueIndex = 5, valueSourceName = nameof(DamageTypeMultiplierProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Bleed damage multiplier for blunt attacks. 0.0x disables bleed from blunt entirely.")]
        public static float BluntMultiplier = 0.5f;

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
        // Default values (preset index 2): Chance=60%, Damage=2.5, Duration=6.0s, Frequency=0.5s

        [ModOption(name = OptionThroatChance, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 10, defaultValueIndex = 12, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for throat wounds to cause bleeding")]
        public static float ThroatChance = 60f;

        [ModOption(name = OptionThroatDamage, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 20, defaultValueIndex = 9, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for throat wounds")]
        public static float ThroatDamage = 2.5f;

        [ModOption(name = OptionThroatDuration, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 30, defaultValueIndex = 11, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long throat bleeds last")]
        public static float ThroatDuration = 6.0f;

        [ModOption(name = OptionThroatFrequency, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for throat wounds")]
        public static float ThroatFrequency = 0.5f;

        [ModOption(name = OptionThroatStackLimit, category = CategoryZoneThroat, categoryOrder = CategoryOrderThroat, order = 40, defaultValueIndex = 2, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for throat wounds")]
        public static int ThroatStackLimit = 3;

        #endregion

        #region Head Zone
        // Default values (preset index 2): Chance=40%, Damage=1.5, Duration=5.0s, Frequency=0.5s

        [ModOption(name = OptionHeadChance, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 10, defaultValueIndex = 8, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for head wounds to cause bleeding")]
        public static float HeadChance = 40f;

        [ModOption(name = OptionHeadDamage, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 20, defaultValueIndex = 5, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for head wounds")]
        public static float HeadDamage = 1.5f;

        [ModOption(name = OptionHeadDuration, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 30, defaultValueIndex = 9, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long head bleeds last")]
        public static float HeadDuration = 5.0f;

        [ModOption(name = OptionHeadFrequency, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for head wounds")]
        public static float HeadFrequency = 0.5f;

        [ModOption(name = OptionHeadStackLimit, category = CategoryZoneHead, categoryOrder = CategoryOrderHead, order = 40, defaultValueIndex = 2, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for head wounds")]
        public static int HeadStackLimit = 3;

        #endregion

        #region Neck Zone
        // Default values (preset index 2): Chance=55%, Damage=2.0, Duration=5.5s, Frequency=0.5s

        [ModOption(name = OptionNeckChance, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 10, defaultValueIndex = 11, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for neck wounds to cause bleeding")]
        public static float NeckChance = 55f;

        [ModOption(name = OptionNeckDamage, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 20, defaultValueIndex = 7, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for neck wounds")]
        public static float NeckDamage = 2.0f;

        [ModOption(name = OptionNeckDuration, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 30, defaultValueIndex = 10, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long neck bleeds last")]
        public static float NeckDuration = 5.5f;

        [ModOption(name = OptionNeckFrequency, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for neck wounds")]
        public static float NeckFrequency = 0.5f;

        [ModOption(name = OptionNeckStackLimit, category = CategoryZoneNeck, categoryOrder = CategoryOrderNeck, order = 40, defaultValueIndex = 2, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for neck wounds")]
        public static int NeckStackLimit = 3;

        #endregion

        #region Torso Zone
        // Default values (preset index 2): Chance=35%, Damage=1.0, Duration=4.0s, Frequency=0.5s

        [ModOption(name = OptionTorsoChance, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 10, defaultValueIndex = 7, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for torso wounds to cause bleeding")]
        public static float TorsoChance = 35f;

        [ModOption(name = OptionTorsoDamage, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 20, defaultValueIndex = 3, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for torso wounds")]
        public static float TorsoDamage = 1.0f;

        [ModOption(name = OptionTorsoDuration, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 30, defaultValueIndex = 7, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long torso bleeds last")]
        public static float TorsoDuration = 4.0f;

        [ModOption(name = OptionTorsoFrequency, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for torso wounds")]
        public static float TorsoFrequency = 0.5f;

        [ModOption(name = OptionTorsoStackLimit, category = CategoryZoneTorso, categoryOrder = CategoryOrderTorso, order = 40, defaultValueIndex = 4, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for torso wounds")]
        public static int TorsoStackLimit = 5;

        #endregion

        #region Arm Zone
        // Default values (preset index 2): Chance=25%, Damage=0.5, Duration=3.0s, Frequency=0.5s

        [ModOption(name = OptionArmChance, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 10, defaultValueIndex = 5, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for arm wounds to cause bleeding")]
        public static float ArmChance = 25f;

        [ModOption(name = OptionArmDamage, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 20, defaultValueIndex = 1, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for arm wounds")]
        public static float ArmDamage = 0.5f;

        [ModOption(name = OptionArmDuration, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 30, defaultValueIndex = 5, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long arm bleeds last")]
        public static float ArmDuration = 3.0f;

        [ModOption(name = OptionArmFrequency, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for arm wounds")]
        public static float ArmFrequency = 0.5f;

        [ModOption(name = OptionArmStackLimit, category = CategoryZoneArm, categoryOrder = CategoryOrderArm, order = 40, defaultValueIndex = 3, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for arm wounds")]
        public static int ArmStackLimit = 4;

        #endregion

        #region Leg Zone
        // Default values (preset index 2): Chance=30%, Damage=0.75, Duration=3.5s, Frequency=0.5s

        [ModOption(name = OptionLegChance, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 10, defaultValueIndex = 6, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for leg wounds to cause bleeding")]
        public static float LegChance = 30f;

        [ModOption(name = OptionLegDamage, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 20, defaultValueIndex = 2, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for leg wounds")]
        public static float LegDamage = 0.75f;

        [ModOption(name = OptionLegDuration, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 30, defaultValueIndex = 6, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long leg bleeds last")]
        public static float LegDuration = 3.5f;

        [ModOption(name = OptionLegFrequency, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for leg wounds")]
        public static float LegFrequency = 0.5f;

        [ModOption(name = OptionLegStackLimit, category = CategoryZoneLeg, categoryOrder = CategoryOrderLeg, order = 40, defaultValueIndex = 3, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for leg wounds")]
        public static int LegStackLimit = 4;

        #endregion

        #region Dismemberment Zone
        // Default values (preset index 2): Chance=80%, Damage=3.0, Duration=8.0s, Frequency=0.5s

        [ModOption(name = OptionDismembermentChance, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 10, defaultValueIndex = 16, valueSourceName = nameof(ChanceProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Chance for dismemberment to cause bleeding")]
        public static float DismembermentChance = 80f;

        [ModOption(name = OptionDismembermentDamage, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 20, defaultValueIndex = 11, valueSourceName = nameof(DamageProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Base damage per tick for dismemberment")]
        public static float DismembermentDamage = 3.0f;

        [ModOption(name = OptionDismembermentDuration, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 30, defaultValueIndex = 15, valueSourceName = nameof(DurationProvider), interactionType = (ModOption.InteractionType)2, tooltip = "How long dismemberment bleeds last")]
        public static float DismembermentDuration = 8.0f;

        [ModOption(name = OptionDismembermentFrequency, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 35, defaultValueIndex = 4, valueSourceName = nameof(FrequencyProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Time between bleed ticks for dismemberment")]
        public static float DismembermentFrequency = 0.5f;

        [ModOption(name = OptionDismembermentStackLimit, category = CategoryZoneDismemberment, categoryOrder = CategoryOrderDismemberment, order = 40, defaultValueIndex = 0, valueSourceName = nameof(StackLimitProvider), interactionType = (ModOption.InteractionType)2, tooltip = "Max stacks for dismemberment (per limb)")]
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
            public float Chance;
            public float Damage;
            public float Duration;
            public float Frequency;
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
                    config.Chance = ThroatChance;
                    config.Damage = ThroatDamage;
                    config.Duration = ThroatDuration;
                    config.Frequency = ThroatFrequency;
                    config.StackLimit = ThroatStackLimit;
                    break;
                case BodyZone.Head:
                    config.Enabled = HeadEnabled;
                    config.Chance = HeadChance;
                    config.Damage = HeadDamage;
                    config.Duration = HeadDuration;
                    config.Frequency = HeadFrequency;
                    config.StackLimit = HeadStackLimit;
                    break;
                case BodyZone.Neck:
                    config.Enabled = NeckEnabled;
                    config.Chance = NeckChance;
                    config.Damage = NeckDamage;
                    config.Duration = NeckDuration;
                    config.Frequency = NeckFrequency;
                    config.StackLimit = NeckStackLimit;
                    break;
                case BodyZone.Torso:
                    config.Enabled = TorsoEnabled;
                    config.Chance = TorsoChance;
                    config.Damage = TorsoDamage;
                    config.Duration = TorsoDuration;
                    config.Frequency = TorsoFrequency;
                    config.StackLimit = TorsoStackLimit;
                    break;
                case BodyZone.Arm:
                    config.Enabled = ArmEnabled;
                    config.Chance = ArmChance;
                    config.Damage = ArmDamage;
                    config.Duration = ArmDuration;
                    config.Frequency = ArmFrequency;
                    config.StackLimit = ArmStackLimit;
                    break;
                case BodyZone.Leg:
                    config.Enabled = LegEnabled;
                    config.Chance = LegChance;
                    config.Damage = LegDamage;
                    config.Duration = LegDuration;
                    config.Frequency = LegFrequency;
                    config.StackLimit = LegStackLimit;
                    break;
                case BodyZone.Dismemberment:
                    config.Enabled = DismembermentEnabled;
                    config.Chance = DismembermentChance;
                    config.Damage = DismembermentDamage;
                    config.Duration = DismembermentDuration;
                    config.Frequency = DismembermentFrequency;
                    config.StackLimit = DismembermentStackLimit;
                    break;
                default:
                    config.Enabled = false;
                    config.Chance = 0f;
                    config.Damage = 1.0f;
                    config.Duration = 4.0f;
                    config.Frequency = 0.5f;
                    config.StackLimit = 3;
                    break;
            }
            return config;
        }

        public static DamagePreset GetDamagePreset()
        {
            switch (DamagePresetSetting)
            {
                case "Minimal": return DamagePreset.Minimal;
                case "Low": return DamagePreset.Low;
                case "High": return DamagePreset.High;
                case "Extreme": return DamagePreset.Extreme;
                default: return DamagePreset.Default;
            }
        }

        public static DurationPreset GetDurationPreset()
        {
            switch (DurationPresetSetting)
            {
                case "Very Short": return DurationPreset.VeryShort;
                case "Short": return DurationPreset.Short;
                case "Long": return DurationPreset.Long;
                case "Extended": return DurationPreset.Extended;
                default: return DurationPreset.Default;
            }
        }

        public static FrequencyPreset GetFrequencyPreset()
        {
            switch (FrequencyPresetSetting)
            {
                case "Very Slow": return FrequencyPreset.VerySlow;
                case "Slow": return FrequencyPreset.Slow;
                case "Fast": return FrequencyPreset.Fast;
                case "Rapid": return FrequencyPreset.Rapid;
                default: return FrequencyPreset.Default;
            }
        }

        public static ChancePreset GetChancePreset()
        {
            switch (ChancePresetSetting)
            {
                case "Off": return ChancePreset.Off;
                case "Rare": return ChancePreset.Rare;
                case "Frequent": return ChancePreset.Frequent;
                case "Always": return ChancePreset.Always;
                default: return ChancePreset.Default;
            }
        }

        public static float GetDamageTypeMultiplier(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Pierce: return PierceMultiplier;
                case DamageType.Slash: return SlashMultiplier;
                case DamageType.Blunt: return BluntMultiplier;
                default: return 1.0f;
            }
        }

        // Zone value setters for preset application
        public static void SetZoneChance(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatChance = value; break;
                case BodyZone.Head: HeadChance = value; break;
                case BodyZone.Neck: NeckChance = value; break;
                case BodyZone.Torso: TorsoChance = value; break;
                case BodyZone.Arm: ArmChance = value; break;
                case BodyZone.Leg: LegChance = value; break;
                case BodyZone.Dismemberment: DismembermentChance = value; break;
            }
        }

        public static void SetZoneDamage(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatDamage = value; break;
                case BodyZone.Head: HeadDamage = value; break;
                case BodyZone.Neck: NeckDamage = value; break;
                case BodyZone.Torso: TorsoDamage = value; break;
                case BodyZone.Arm: ArmDamage = value; break;
                case BodyZone.Leg: LegDamage = value; break;
                case BodyZone.Dismemberment: DismembermentDamage = value; break;
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

        public static void SetZoneFrequency(BodyZone zone, float value)
        {
            switch (zone)
            {
                case BodyZone.Throat: ThroatFrequency = value; break;
                case BodyZone.Head: HeadFrequency = value; break;
                case BodyZone.Neck: NeckFrequency = value; break;
                case BodyZone.Torso: TorsoFrequency = value; break;
                case BodyZone.Arm: ArmFrequency = value; break;
                case BodyZone.Leg: LegFrequency = value; break;
                case BodyZone.Dismemberment: DismembermentFrequency = value; break;
            }
        }

        public static float GetZoneFrequency(BodyZone zone)
        {
            switch (zone)
            {
                case BodyZone.Throat: return ThroatFrequency;
                case BodyZone.Head: return HeadFrequency;
                case BodyZone.Neck: return NeckFrequency;
                case BodyZone.Torso: return TorsoFrequency;
                case BodyZone.Arm: return ArmFrequency;
                case BodyZone.Leg: return LegFrequency;
                case BodyZone.Dismemberment: return DismembermentFrequency;
                default: return 0.5f;
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
