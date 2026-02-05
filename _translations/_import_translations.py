# -*- coding: utf-8 -*-
"""Import DOT translations from CSV back into JSON files."""
import json
import csv
from pathlib import Path

BASE = Path(__file__).parent
INPUT = BASE / '_translations.csv'

LANG_MAP = {
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
    with open(INPUT, 'r', encoding='utf-8') as f:
        rows = list(csv.DictReader(f))

    translations = {}
    for row in rows:
        text_id = row.get('Text_ID', '')
        if not text_id:
            continue
        translations[text_id] = {}
        for csv_col, json_suffix in LANG_MAP.items():
            val = row.get(csv_col, '')
            if val and not val.startswith('=GOOGLETRANSLATE'):
                translations[text_id][json_suffix] = val

    for json_suffix in LANG_MAP.values():
        json_path = BASE / 'Texts' / f'Text_{json_suffix}.json'
        if not json_path.exists():
            continue
        with open(json_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        count = 0
        for group in data.get('textGroups', []):
            for text_item in group.get('texts', []):
                text_id = text_item.get('id', '')
                if text_id in translations and json_suffix in translations[text_id]:
                    text_item['text'] = translations[text_id][json_suffix]
                    count += 1
        with open(json_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4, ensure_ascii=False)
        if count > 0:
            print(f"{json_suffix}: {count} translations applied")

if __name__ == '__main__':
    main()
