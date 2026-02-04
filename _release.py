# -*- coding: utf-8 -*-
"""Build and release CDoT mod."""
import subprocess
import json
from pathlib import Path

BASE = Path(__file__).parent
MANIFEST = BASE / "manifest.json"

def get_version():
    with open(MANIFEST, 'r') as f:
        return json.load(f)['ModVersion']

def run(cmd, cwd=None):
    print(f"$ {cmd}")
    subprocess.run(cmd, shell=True, cwd=cwd or BASE, check=True)

def main():
    version = get_version()
    tag = f"v{version}"

    print(f"\n=== Building CDoT {version} ===\n")

    # Build both configurations
    run("dotnet build -c Release")
    run("dotnet build -c Nomad")

    # Create zips
    print("\n=== Creating release zips ===\n")
    run(f'powershell -Command "Compress-Archive -Path bin/Release/PCVR/CDoT -DestinationPath CDoT-PCVR.zip -Force"')
    run(f'powershell -Command "Compress-Archive -Path bin/Release/Nomad/CDoT -DestinationPath CDoT-Nomad.zip -Force"')

    # Git tag (if not exists)
    result = subprocess.run(f"git tag -l {tag}", shell=True, capture_output=True, text=True, cwd=BASE)
    if tag not in result.stdout:
        run(f"git tag {tag}")

    # Push tag
    run(f"git push origin {tag}")

    # Create GitHub release
    print("\n=== Creating GitHub release ===\n")
    run(f'gh release create {tag} CDoT-PCVR.zip CDoT-Nomad.zip --title "CDoT {version}" --notes "Release {version}"')

    print(f"\n=== CDoT {version} released! ===\n")

if __name__ == '__main__':
    main()
