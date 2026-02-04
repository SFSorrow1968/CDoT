using System;
using CDoT.Configuration;
using UnityEngine;

namespace CDoT.Core
{
    /// <summary>
    /// Visual in-game debug overlay using Unity IMGUI.
    /// Displays CDoT bleed effect status and performance metrics.
    /// </summary>
    public class DebugOverlay
    {
        private static DebugOverlay _instance;
        public static DebugOverlay Instance => _instance ??= new DebugOverlay();

        // GUI styling
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;
        private bool _stylesInitialized;

        // Display state
        private Rect _windowRect = new Rect(10, 170, 280, 130);
        private const int WINDOW_ID = 91828; // Unique ID for CDoT overlay

        public void Initialize()
        {
            _stylesInitialized = false;
            if (CDoTModOptions.DebugLogging)
                Debug.Log("[CDoT] DebugOverlay initialized (IMGUI mode)");
        }

        private void InitializeStyles()
        {
            if (_stylesInitialized) return;

            try
            {
                // Create semi-transparent background (darker red tint for blood theme)
                _backgroundTexture = new Texture2D(1, 1);
                _backgroundTexture.SetPixel(0, 0, new Color(0.15f, 0.08f, 0.08f, 0.85f));
                _backgroundTexture.Apply();

                // Box style for window background
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = _backgroundTexture;
                _boxStyle.padding = new RectOffset(10, 10, 10, 10);

                // Label style
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = 12;
                _labelStyle.normal.textColor = Color.white;
                _labelStyle.richText = true;

                _stylesInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CDoT] DebugOverlay style init failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Called from OnGUI - draws the debug overlay when enabled.
        /// </summary>
        public void Draw()
        {
            if (!CDoTModOptions.DebugOverlay) return;

            try
            {
                InitializeStyles();
                if (!_stylesInitialized) return;

                _windowRect = GUILayout.Window(WINDOW_ID, _windowRect, DrawWindow, "", _boxStyle);
            }
            catch (Exception ex)
            {
                if (CDoTModOptions.DebugLogging)
                    Debug.LogError($"[CDoT] DebugOverlay Draw error: {ex.Message}");
            }
        }

        private void DrawWindow(int windowId)
        {
            try
            {
                var manager = BleedManager.Instance;
                int activeEffects = manager?.GetActiveEffectCount() ?? 0;
                int affectedCreatures = manager?.GetAffectedCreatureCount() ?? 0;

                // Header
                GUILayout.Label("<color=#ff6666><b>CDoT Debug</b></color>", _labelStyle);
                GUILayout.Space(4);

                // Active effects
                string effectColor = activeEffects > 0 ? "#ff4444" : "#888888";
                string statusIcon = activeEffects > 0 ? "●" : "○";
                GUILayout.Label($"<color={effectColor}>{statusIcon} {activeEffects} Active Bleeds</color>", _labelStyle);

                // Affected creatures
                GUILayout.Label($"Creatures: {affectedCreatures}", _labelStyle);

                // Profile
                GUILayout.Label($"Profile: {CDoTModOptions.ProfilePresetSetting}", _labelStyle);

                GUILayout.Space(4);

                // Performance metrics summary
                var perf = PerformanceMetrics.Instance;
                if (perf != null)
                {
                    GUILayout.Label("<color=#ffaaaa>Performance</color>", _labelStyle);
                    GUILayout.Label(perf.GetOverlaySummary(), _labelStyle);
                }

                // Make window draggable
                GUI.DragWindow();
            }
            catch (Exception ex)
            {
                GUILayout.Label($"Error: {ex.Message}", _labelStyle);
            }
        }

        public void Shutdown()
        {
            if (_backgroundTexture != null)
            {
                UnityEngine.Object.Destroy(_backgroundTexture);
                _backgroundTexture = null;
            }
            _stylesInitialized = false;
            _instance = null;
        }
    }
}
