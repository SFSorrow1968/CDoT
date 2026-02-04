using System;
using System.Collections.Generic;
using CDoT.Configuration;
using UnityEngine;

namespace CDoT.Core
{
    /// <summary>
    /// Tracks performance metrics for bleed effect processing.
    /// Monitors tick processing times and effect counts.
    /// </summary>
    public class PerformanceMetrics
    {
        private static PerformanceMetrics _instance;
        public static PerformanceMetrics Instance => _instance ??= new PerformanceMetrics();

        // Tracking state
        private bool _isInitialized;
        private float _lastTickTime;
        private float _worstTickTime;
        private float _totalTickTime;
        private int _tickCount;
        private int _peakActiveEffects;
        private int _currentActiveEffects;
        private int _totalDamageApplied;

        // Warning thresholds (ms)
        private const float TICK_WARN_THRESHOLD_MS = 2f;  // Warn if tick takes over 2ms
        private const float TICK_LOG_INTERVAL = 30f;      // Log summary every 30 seconds

        private float _lastLogTime;

        public void Initialize()
        {
            Reset();
            _isInitialized = true;
            if (CDoTModOptions.DebugLogging)
                Debug.Log("[CDoT] PerformanceMetrics initialized");
        }

        public void Reset()
        {
            _lastTickTime = 0f;
            _worstTickTime = 0f;
            _totalTickTime = 0f;
            _tickCount = 0;
            _peakActiveEffects = 0;
            _currentActiveEffects = 0;
            _totalDamageApplied = 0;
            _lastLogTime = Time.unscaledTime;
        }

        /// <summary>
        /// Record the start of a bleed tick processing cycle.
        /// </summary>
        public void StartTickProcessing()
        {
            if (!_isInitialized) return;
            _lastTickTime = Time.unscaledTime;
        }

        /// <summary>
        /// Record the end of a bleed tick processing cycle.
        /// </summary>
        public void EndTickProcessing(int activeEffects, float damageThisTick)
        {
            if (!_isInitialized || _lastTickTime == 0f) return;

            float tickDuration = (Time.unscaledTime - _lastTickTime) * 1000f; // Convert to ms
            _tickCount++;
            _totalTickTime += tickDuration;
            _currentActiveEffects = activeEffects;
            _totalDamageApplied += (int)damageThisTick;

            if (tickDuration > _worstTickTime)
                _worstTickTime = tickDuration;

            if (activeEffects > _peakActiveEffects)
                _peakActiveEffects = activeEffects;

            // Warn on slow ticks
            if (tickDuration > TICK_WARN_THRESHOLD_MS && CDoTModOptions.DebugLogging)
            {
                Debug.LogWarning($"[CDoT] Slow tick: {tickDuration:F2}ms ({activeEffects} effects)");
            }

            // Periodic logging
            if (CDoTModOptions.DebugLogging && Time.unscaledTime - _lastLogTime >= TICK_LOG_INTERVAL)
            {
                LogSummary();
                _lastLogTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Record when a new bleed effect is applied.
        /// </summary>
        public void RecordEffectApplied()
        {
            // Tracked through activeEffects in EndTickProcessing
        }

        /// <summary>
        /// Record when a bleed effect ends (expires or creature dies).
        /// </summary>
        public void RecordEffectEnded()
        {
            // Tracked through activeEffects in EndTickProcessing
        }

        private void LogSummary()
        {
            if (_tickCount == 0) return;

            float avgTick = _totalTickTime / _tickCount;
            Debug.Log($"[CDoT] Performance: {_tickCount} ticks, avg={avgTick:F2}ms, worst={_worstTickTime:F2}ms, peak={_peakActiveEffects} effects, total dmg={_totalDamageApplied}");
        }

        public string GetOverlaySummary()
        {
            if (_tickCount == 0)
                return "No ticks processed";

            float avgTick = _totalTickTime / _tickCount;
            return $"Ticks={_tickCount} | Avg={avgTick:F1}ms | Peak={_peakActiveEffects} | Active={_currentActiveEffects}";
        }

        public void Shutdown()
        {
            if (CDoTModOptions.DebugLogging)
                LogSummary();
            _isInitialized = false;
            _instance = null;
        }
    }
}
