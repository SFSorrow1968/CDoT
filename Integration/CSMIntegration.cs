using System;
using System.Reflection;
using DOT.Configuration;
using ThunderRoad;
using UnityEngine;

namespace DOT.Integration
{
    /// <summary>
    /// Optional integration with CSM (Conditional Slow Motion) mod.
    /// Uses reflection to avoid hard dependency - works whether CSM is installed or not.
    /// </summary>
    public static class CSMIntegration
    {
        private static bool _initialized = false;
        private static bool _csmAvailable = false;
        private static Type _csmManagerType = null;
        private static PropertyInfo _instanceProperty = null;
        private static MethodInfo _triggerSlowMethod = null;

        // Cache for TriggerType enum if CSM is available
        private static Type _triggerTypeEnum = null;
        private static object _basicKillTrigger = null;

        /// <summary>
        /// Attempts to initialize the CSM integration by finding CSM types via reflection.
        /// Call this during mod initialization.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                // Try to find CSM assembly
                Assembly csmAssembly = null;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "CSM")
                    {
                        csmAssembly = assembly;
                        break;
                    }
                }

                if (csmAssembly == null)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] CSM integration: CSM assembly not found (mod not installed)");
                    return;
                }

                // Find CSMManager type
                _csmManagerType = csmAssembly.GetType("CSM.Core.CSMManager");
                if (_csmManagerType == null)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] CSM integration: CSMManager type not found");
                    return;
                }

                // Find Instance property
                _instanceProperty = _csmManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (_instanceProperty == null)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] CSM integration: Instance property not found");
                    return;
                }

                // Find TriggerType enum
                _triggerTypeEnum = csmAssembly.GetType("CSM.Configuration.TriggerType");
                if (_triggerTypeEnum != null)
                {
                    _basicKillTrigger = Enum.Parse(_triggerTypeEnum, "BasicKill");
                }

                // Find TriggerSlow method with full signature
                // TriggerSlow(TriggerType type, float damageDealt, Creature targetCreature, DamageType damageType, float intensity, bool isQuickTest, bool isStatusKill, bool isThrown)
                _triggerSlowMethod = _csmManagerType.GetMethod("TriggerSlow", new Type[]
                {
                    _triggerTypeEnum,
                    typeof(float),
                    typeof(Creature),
                    typeof(DamageType),
                    typeof(float),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool)
                });

                if (_triggerSlowMethod == null)
                {
                    if (DOTModOptions.DebugLogging)
                        Debug.Log("[DOT] CSM integration: TriggerSlow method not found");
                    return;
                }

                _csmAvailable = true;
                Debug.Log("[DOT] CSM integration: Successfully connected to CSM");
            }
            catch (Exception ex)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log($"[DOT] CSM integration: Failed to initialize - {ex.Message}");
                _csmAvailable = false;
            }
        }

        /// <summary>
        /// Returns true if CSM is available and integration is active.
        /// </summary>
        public static bool IsAvailable => _csmAvailable;

        /// <summary>
        /// Notifies CSM that a creature was killed by bleed damage.
        /// CSM can then trigger slow motion for the kill if configured to do so.
        /// </summary>
        /// <param name="creature">The creature that was killed</param>
        /// <param name="damageType">The damage type of the bleed (Slash, Pierce, Fire, Lightning)</param>
        /// <param name="bleedDamage">The final damage that killed the creature</param>
        /// <returns>True if CSM was notified and triggered slow motion</returns>
        public static bool NotifyBleedKill(Creature creature, DamageType damageType, float bleedDamage)
        {
            if (!_csmAvailable || creature == null)
                return false;

            try
            {
                // Get CSMManager instance
                object csmManager = _instanceProperty.GetValue(null);
                if (csmManager == null)
                    return false;

                // Call TriggerSlow with isStatusKill=true to indicate DOT kill
                // Parameters: TriggerType.BasicKill, damageDealt, creature, damageType, intensity, isQuickTest=false, isStatusKill=true, isThrown=false
                object[] parameters = new object[]
                {
                    _basicKillTrigger,  // TriggerType.BasicKill
                    bleedDamage,        // damageDealt
                    creature,           // targetCreature
                    damageType,         // damageType
                    0.5f,               // intensity (moderate)
                    false,              // isQuickTest
                    true,               // isStatusKill - THIS IS THE KEY FLAG
                    false               // isThrown
                };

                object result = _triggerSlowMethod.Invoke(csmManager, parameters);
                bool triggered = result is bool b && b;

                if (DOTModOptions.DebugLogging && triggered)
                    Debug.Log($"[DOT] CSM integration: Bleed kill triggered slow motion for {creature.name}");

                return triggered;
            }
            catch (Exception ex)
            {
                if (DOTModOptions.DebugLogging)
                    Debug.Log($"[DOT] CSM integration: Error notifying bleed kill - {ex.Message}");
                return false;
            }
        }
    }
}
