#!/usr/bin/env python3
"""
Generates Presets.xlsx showing zone configurations across intensity presets.
"""

import os
import sys

try:
    import openpyxl
    from openpyxl.styles import Font, PatternFill, Alignment
except ImportError:
    print("Error: openpyxl not installed. Run: pip install openpyxl")
    sys.exit(1)

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(SCRIPT_DIR)
OUTPUT_PATH = os.path.join(PROJECT_ROOT, "Presets.xlsx")

# Zone configurations - default values
ZONES = {
    'Throat': {'multiplier': 3.0, 'duration': 8.0, 'damage_per_tick': 5.0, 'stack_limit': 3},
    'Head': {'multiplier': 2.0, 'duration': 6.0, 'damage_per_tick': 3.0, 'stack_limit': 3},
    'Neck': {'multiplier': 2.5, 'duration': 7.0, 'damage_per_tick': 4.0, 'stack_limit': 3},
    'Torso': {'multiplier': 1.0, 'duration': 5.0, 'damage_per_tick': 2.0, 'stack_limit': 5},
    'Arm': {'multiplier': 0.5, 'duration': 4.0, 'damage_per_tick': 1.0, 'stack_limit': 4},
    'Leg': {'multiplier': 0.6, 'duration': 4.5, 'damage_per_tick': 1.5, 'stack_limit': 4},
    'Dismemberment': {'multiplier': 2.5, 'duration': 10.0, 'damage_per_tick': 6.0, 'stack_limit': 1},
}

# Intensity preset multipliers
INTENSITY_PRESETS = {
    'Light': 0.5,
    'Default': 1.0,
    'Heavy': 1.5,
    'Brutal': 2.5,
}


def create_xlsx(output_path):
    """Create Presets.xlsx with zone configurations."""
    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = "Zone Presets"

    # Headers
    header_fill = PatternFill(start_color="366092", end_color="366092", fill_type="solid")
    header_font = Font(color="FFFFFF", bold=True)

    # Row 1: Preset names
    ws.cell(row=1, column=1, value="Zone")
    ws.cell(row=1, column=2, value="Property")
    col = 3
    for preset in INTENSITY_PRESETS:
        cell = ws.cell(row=1, column=col, value=preset)
        cell.fill = header_fill
        cell.font = header_font
        cell.alignment = Alignment(horizontal='center')
        col += 1

    # Data rows
    row = 2
    for zone_name, zone_config in ZONES.items():
        # Zone header
        zone_cell = ws.cell(row=row, column=1, value=zone_name)
        zone_cell.font = Font(bold=True)

        # Multiplier row
        ws.cell(row=row, column=2, value="Multiplier")
        col = 3
        for preset, mult in INTENSITY_PRESETS.items():
            effective_mult = zone_config['multiplier'] * mult
            ws.cell(row=row, column=col, value=f"{effective_mult:.2f}x")
            col += 1
        row += 1

        # Duration row
        ws.cell(row=row, column=2, value="Duration")
        col = 3
        for preset, mult in INTENSITY_PRESETS.items():
            ws.cell(row=row, column=col, value=f"{zone_config['duration']:.1f}s")
            col += 1
        row += 1

        # Damage per tick row
        ws.cell(row=row, column=2, value="Dmg/Tick")
        col = 3
        for preset, mult in INTENSITY_PRESETS.items():
            effective_dmg = zone_config['damage_per_tick'] * mult
            ws.cell(row=row, column=col, value=f"{effective_dmg:.1f}")
            col += 1
        row += 1

        # Stack limit row
        ws.cell(row=row, column=2, value="Stack Limit")
        col = 3
        for preset in INTENSITY_PRESETS:
            ws.cell(row=row, column=col, value=zone_config['stack_limit'])
            col += 1
        row += 1

        # Empty row between zones
        row += 1

    # Adjust column widths
    ws.column_dimensions['A'].width = 15
    ws.column_dimensions['B'].width = 12
    for i in range(3, 3 + len(INTENSITY_PRESETS)):
        ws.column_dimensions[openpyxl.utils.get_column_letter(i)].width = 12

    wb.save(output_path)
    print(f"Generated: {output_path}")


def main():
    create_xlsx(OUTPUT_PATH)


if __name__ == "__main__":
    main()
