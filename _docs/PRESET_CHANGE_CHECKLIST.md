# Preset/Feature Change Checklist

Use this when adding or renaming preset labels, enum options, or ModOption strings.

- If a label differs from its stored value (e.g., "Off (Disabled)" -> "Off"), add the label to the matching `Get*Preset()` switch so it never falls back unexpectedly.
- If you change or add a `ModOptionString` label/value, update any parsing in `DOTModOptions` and any UI sync in `DOTModOptionVisibility`.
- If you rename option labels in custom sections, ensure UI sync keys still resolve (category + name) so presets can push values.
- If you add/rename presets, update provider arrays, enum options, default indices, and any mappings in `BleedManager.GetZoneConfig()`.
- If you add/rename body zones, update `BodyZone` enum, zone detection in `EventHooks.cs`, and all related UI options.
- If UI/options change: regenerate `MENU_MOCK.xlsx`.
- Always build Release + Nomad and copy outputs to `builds/DOT-PCVR/DOT/DOT.dll` and `builds/DOT-Nomad/DOT/DOT.dll`, then commit.
