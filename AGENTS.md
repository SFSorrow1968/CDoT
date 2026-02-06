# Agent Instructions

When making changes in this repo:
- Always build (Release and Nomad).
- Only regenerate `_design/MENU_MOCK.xlsx` if user says "Mock" or explicitly requests it.
- Only update translations if user says "Translations" or explicitly requests it.
- Always create a new branch before making any changes; never commit directly to `main`/`master`.
- Ensure the working tree is clean before making changes; if not, ask the user how to proceed.
- Always commit your changes.
- After changes, run `_agent/snapshot.ps1 -Message "<short>"` to create a snapshot commit and a tag `snapshot-YYYYMMDD-HHMMSS`. Add `-Zip` when an archive is desired.
- After the snapshot commit, push the branch and tags (`git push -u origin HEAD` and `git push origin --tags`).
- `DEVELOPMENT.md` contains platform/ModOptions notes; consult it when needed.
- Quick refs: `DEVELOPMENT.md` sections "Platform Differences", "ModOptions System", "EventManager Events".
- User does not want learning-oriented content; optimize for agent clarity over human prose (except MENU_MOCK, PUBLISH, and build steps).
- Build artifacts: `bin/Release/PCVR/DOT/` and `bin/Release/Nomad/DOT/`.
- `_design/MENU_MOCK.xlsx` is the current UI reference.
- Check `References/` for new logs/screenshots before starting; screenshots live in `References/Screenshots/`.
- Common edit points: UI options in `Configuration/DOTModOptions.cs`, bleed logic in `Core/BleedManager.cs`, zone config in `Configuration/BodyZone.cs`.
- **QUIRKS.md**: Check this file before tackling complex problems. Add new entries if a redundant task cost significant time and the note would help future agents avoid it.
- If the user says "publish", follow `_docs/PUBLISH.md`.
