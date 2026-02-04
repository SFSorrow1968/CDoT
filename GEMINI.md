# Agent Instructions

When making changes in this repo:
- Always build (Release and Nomad).
- If any UI/options change, regenerate `MENU_MOCK.xlsx` using `_agent/build_menu_mock_xlsx.py`.
- If new UI text/labels are added, update translations: run `python _export_translations.py`, translate new rows in Google Sheets using GOOGLETRANSLATE formulas, paste values, save as `_translations.csv`, then run `python _import_translations.py`.
- Always commit your changes.
- `DEVELOPMENT.md` contains platform/ModOptions notes; consult it when needed.
- Quick refs: `DEVELOPMENT.md` sections "Platform Differences", "ModOptions System", "EventManager Events".
- User does not want learning-oriented content; optimize for agent clarity over human prose (except MENU_MOCK, PUBLISH, and build steps).
- When adding/renaming presets or ModOption labels, follow `PRESET_CHANGE_CHECKLIST.md`.
- Build artifacts: `bin/Release/PCVR/CDoT/` and `bin/Release/Nomad/CDoT/`.
- `MENU_MOCK.xlsx` is the current UI reference.
- Check `References/` for new logs/screenshots before starting; screenshots live in `References/Screenshots/`.
- Common edit points: UI options in `Configuration/CDoTModOptions.cs`, bleed logic in `Core/BleedManager.cs`, zone config in `Configuration/BodyZone.cs`.
- Only add new pointers to these agent files if a redundant task cost significant time and the note would materially help onboarding agents avoid it.
- If the user says "publish", follow `PUBLISH.md`.
