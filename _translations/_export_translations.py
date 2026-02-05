# -*- coding: utf-8 -*-
"""Export DOT translation strings to CSV with GOOGLETRANSLATE formulas."""
import json
import csv
from pathlib import Path

BASE = Path(__file__).parent
OUTPUT = BASE / '_translations.csv'

LANGUAGES = [
    ('en', 'English'),
    ('fr', 'French'),
    ('de', 'German'),
    ('es', 'Spanish'),
    ('it', 'Italian'),
    ('pt', 'Portuguese'),
    ('ja', 'Japanese'),
    ('ko', 'Korean'),
    ('zh-CN', 'Chinese_Simplified'),
    ('zh-TW', 'Chinese_Traditional'),
    ('th', 'Thai'),
]

def load_english_texts():
    path = BASE / 'Texts' / 'Text_English.json'
    with open(path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    entries = []
    for group in data.get('textGroups', []):
        for text_item in group.get('texts', []):
            entries.append({
                'id': text_item.get('id', ''),
                'text': text_item.get('text', '')
            })
    return entries

def main():
    entries = load_english_texts()
    with open(OUTPUT, 'w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        headers = ['Text_ID', 'English'] + [lang[1] for lang in LANGUAGES[1:]]
        writer.writerow(headers)
        for row_num, entry in enumerate(entries, start=2):
            row = [entry['id'], entry['text']]
            for lang_code, _ in LANGUAGES[1:]:
                row.append(f'=GOOGLETRANSLATE(B{row_num},"en","{lang_code}")')
            writer.writerow(row)
    print(f"Exported {len(entries)} strings to {OUTPUT}")

if __name__ == '__main__':
    main()
