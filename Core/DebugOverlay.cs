using System;
using CDoT.Configuration;
using UnityEngine;

namespace CDoT.Core
{
    /// <summary>
    /// Debug logging helper for CDoT state information.
    /// Logs to console when DebugOverlay option is enabled.
    /// </summary>
    public class DebugOverlay
    {
        private static DebugOverlay _instance;
        public static DebugOverlay Instance => _instance ??= new DebugOverlay();

        private float _lastLogTime;
        private const float LOG_INTERVAL = 3f; // Log every 3 seconds max

        public void Initialize()
        {
            _lastLogTime = 0f;
            if (CDoTModOptions.DebugLogging)
                Debug.Log("[CDoT] DebugOverlay initialized (console mode)");
        }

        /// <summary>
        /// Called periodically - logs state when overlay is enabled.
        /// </summary>
        public void Draw()
        {
            if (!CDoTModOptions.DebugOverlay) return;

            // Rate-limit logging to avoid spam
            if (Time.unscaledTime - _lastLogTime < LOG_INTERVAL) return;
            _lastLogTime = Time.unscaledTime;

            try
            {
                var manager = BleedManager.Instance;
                int activeEffects = manager?.GetActiveEffectCount() ?? 0;

                if (activeEffects > 0)
                {
                    // Only log when there are active effects
                    var perf = PerformanceMetrics.Instance;
                    Debug.Log($"[CDoT] Overlay: Effects={activeEffects} | {perf.GetOverlaySummary()}");
                }
            }
            catch (Exception ex)
            {
                if (CDoTModOptions.DebugLogging)
                    Debug.LogError($"[CDoT] DebugOverlay error: {ex.Message}");
            }
        }

        public void Shutdown()
        {
            _instance = null;
        }
    }
}
