# -*- coding: utf-8 -*-
"""Generate all language JSON files from _translations.csv with new localization group ID."""
import json
import csv
from pathlib import Path

BASE = Path(__file__).parent
INPUT = BASE / '_translations.csv'
TEXTS_DIR = BASE / 'Texts'
GROUP_ID = 'CDoT_Options'

LANG_MAP = {
    'English': 'English',
    'French': 'French',
    'German': 'German',
    'Spanish': 'Spanish',
    'Italian': 'Italian',
    'Portuguese': 'Portuguese',
    'Japanese': 'Japanese',
    'Korean': 'Korean',
    'Chinese_Simplified': 'ChineseSimplified',
    'Chinese_Traditional': 'ChineseTraditional',
    'Thai': 'Thai',
}

def main():
    TEXTS_DIR.mkdir(exist_ok=True)

    with open(INPUT, 'r', encoding='utf-8') as f:
        rows = list(csv.DictReader(f))

    # Build translation dict: old_id -> {lang: text}
    translations = {}
    for row in rows:
        old_id = row.get('Text_ID', '')
        if not old_id:
            continue
        translations[old_id] = {}
        for csv_col in LANG_MAP.keys():
            val = row.get(csv_col, '')
            if val and not val.startswith('=GOOGLETRANSLATE'):
                translations[old_id][csv_col] = val

    # Generate JSON for each language
    for csv_col, json_suffix in LANG_MAP.items():
        text_list = []

        # Add all translations with the new group prefix
        for old_id in translations:
            new_id = f"{GROUP_ID}.{old_id}"
            if csv_col in translations[old_id]:
                text = translations[old_id][csv_col]
                text_list.append({"id": new_id, "text": text})

        # Create the JSON structure
        data = {
            "$type": "ThunderRoad.TextData, ThunderRoad",
            "id": GROUP_ID,
            "sensitiveContent": "None",
            "sensitiveFilterBehaviour": "None",
            "groupId": GROUP_ID,
            "textList": text_list
        }

        json_path = TEXTS_DIR / f'Text_{json_suffix}.json'
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        print(f"Generated {json_path.name}: {len(text_list)} entries")

if __name__ == '__main__':
    main()
