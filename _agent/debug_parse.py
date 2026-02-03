#!/usr/bin/env python3
"""
Debug utility for parsing and validating CDoT configuration files.
"""

import os
import json
import sys

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(SCRIPT_DIR)


def validate_manifest():
    """Validate manifest.json format."""
    manifest_path = os.path.join(PROJECT_ROOT, "manifest.json")

    if not os.path.exists(manifest_path):
        print(f"ERROR: manifest.json not found at {manifest_path}")
        return False

    try:
        with open(manifest_path, 'r', encoding='utf-8') as f:
            manifest = json.load(f)

        required_fields = ['Name', 'Description', 'Author', 'ModVersion', 'GameVersion']
        missing = [f for f in required_fields if f not in manifest]

        if missing:
            print(f"ERROR: manifest.json missing fields: {missing}")
            return False

        print("manifest.json: VALID")
        print(f"  Name: {manifest['Name']}")
        print(f"  Version: {manifest['ModVersion']}")
        print(f"  GameVersion: {manifest['GameVersion']}")
        return True

    except json.JSONDecodeError as e:
        print(f"ERROR: manifest.json parse error: {e}")
        return False


def validate_settings():
    """Validate settings.json format."""
    settings_path = os.path.join(PROJECT_ROOT, "settings.json")

    if not os.path.exists(settings_path):
        print(f"ERROR: settings.json not found at {settings_path}")
        return False

    try:
        with open(settings_path, 'r', encoding='utf-8') as f:
            settings = json.load(f)

        if 'Enabled' not in settings:
            print("WARNING: settings.json missing 'Enabled' field")

        if 'Zones' not in settings:
            print("ERROR: settings.json missing 'Zones' field")
            return False

        zones = settings['Zones']
        expected_zones = ['Throat', 'Head', 'Neck', 'Torso', 'Arm', 'Leg', 'Dismemberment']
        missing_zones = [z for z in expected_zones if z not in zones]

        if missing_zones:
            print(f"WARNING: settings.json missing zones: {missing_zones}")

        print("settings.json: VALID")
        print(f"  Zones defined: {list(zones.keys())}")
        return True

    except json.JSONDecodeError as e:
        print(f"ERROR: settings.json parse error: {e}")
        return False


def main():
    print("CDoT Configuration Validator")
    print("=" * 40)

    all_valid = True
    all_valid &= validate_manifest()
    print()
    all_valid &= validate_settings()

    print()
    if all_valid:
        print("All configurations valid!")
        sys.exit(0)
    else:
        print("Validation failed - see errors above")
        sys.exit(1)


if __name__ == "__main__":
    main()
