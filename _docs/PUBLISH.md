# Publish Checklist

Use this when the user says "publish".

## AI Disclaimer

This mod was developed with AI assistance (GitHub Copilot / Claude). All code is original work - no code was stolen or used without consent. Only official, open-source references and publicly available ThunderRoad documentation were used during development.

## Steps

1) Confirm version
- Update `Configuration/DOTModOptions.cs` VERSION (and any other version fields if present).
- Ensure the Git tag/release name matches the version.

2) Build + artifacts
- `dotnet build DOT.csproj -c Release`
- `dotnet build DOT.csproj -c Nomad`
- Copy artifacts:
  - `bin/Release/PCVR/DOT/DOT.dll` -> `builds/DOT-PCVR/DOT/DOT.dll`
  - `bin/Release/Nomad/DOT/DOT.dll` -> `builds/DOT-Nomad/DOT/DOT.dll`
- Log results in `_agent/verification_results.md`.

3) Documentation updates
- If UI/options changed, regenerate `MENU_MOCK.xlsx` using `_agent/build_menu_mock_xlsx.py`.
- Update `Description.md` overview/detailed text if needed.

4) Commit + push
- Commit all changes and push to `main`.

5) GitHub release
- Create a GitHub release with the version tag.
- Attach build artifacts (zip `builds/DOT-PCVR` and `builds/DOT-Nomad`).
- Release notes: concise summary of changes + any known issues.
