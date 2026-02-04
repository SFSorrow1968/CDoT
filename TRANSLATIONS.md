# CDoT Translation System

## Overview

CDoT uses ThunderRoad's TextData localization system with a **safe group ID** (`CDoT_Options`) that differs from the mod name to avoid UI conflicts in the mod selector.

## File Structure

```
Texts/
  Text_English.json
  Text_French.json
  Text_German.json
  Text_Spanish.json
  Text_Italian.json
  Text_Portuguese.json
  Text_Japanese.json
  Text_Korean.json
  Text_ChineseSimplified.json
  Text_ChineseTraditional.json
  Text_Thai.json
_translations.csv          # Master translation spreadsheet
_generate_all_translations.py  # Regenerates all JSON from CSV
```

## Key Rules

1. **Group ID must NOT match mod name**: Use `CDoT_Options`, never `CDoT`
2. **All localization IDs use the prefix**: `CDoT_Options.SomethingHere`
3. **No emojis in text**: Quest's NotoSans font doesn't support them

## Adding New Translatable Text

### Step 1: Add to Code

In `CDoTModOptions.cs`, add localization ID parameters to ModOption attributes:

```csharp
[ModOption(name = "My Option",
           nameLocalizationId = LocalizationGroupId + ".MyOption",
           category = "My Category",
           categoryLocalizationId = LocalizationGroupId + ".CategoryMy",
           ...)]
```

For ModOptionString arrays:
```csharp
new ModOptionString("Label", LocalizationGroupId + ".LabelId", "Value")
```

### Step 2: Add to _translations.csv

Add a new row with the Text_ID (without prefix) and English text:
```
MyOption,My Option,=GOOGLETRANSLATE(B123,"en","fr"),...
```

### Step 3: Get Translations

Option A - Google Sheets:
1. Import CSV to Google Sheets
2. GOOGLETRANSLATE formulas auto-fill
3. Copy values (Ctrl+Shift+V to paste values only)
4. Export as CSV

Option B - Manual:
1. Add translations directly to each column in CSV

### Step 4: Regenerate JSON Files

```bash
python _generate_all_translations.py
```

This regenerates all 11 `Text_*.json` files from the CSV.

### Step 5: Build and Test

```bash
dotnet build -c Release
dotnet build -c Nomad
```

## Modifying Existing Translations

1. Edit `_translations.csv` directly
2. Run `python _generate_all_translations.py`
3. Rebuild

## JSON Format Reference

```json
{
  "$type": "ThunderRoad.TextData, ThunderRoad",
  "id": "CDoT_Options",
  "sensitiveContent": "None",
  "sensitiveFilterBehaviour": "None",
  "groupId": "CDoT_Options",
  "textList": [
    { "id": "CDoT_Options.EnableMod", "text": "Enable Mod" }
  ]
}
```

## Supported Languages

| CSV Column | JSON File | Language |
|------------|-----------|----------|
| English | Text_English.json | English |
| French | Text_French.json | French |
| German | Text_German.json | German |
| Spanish | Text_Spanish.json | Spanish |
| Italian | Text_Italian.json | Italian |
| Portuguese | Text_Portuguese.json | Portuguese |
| Japanese | Text_Japanese.json | Japanese |
| Korean | Text_Korean.json | Korean |
| Chinese_Simplified | Text_ChineseSimplified.json | Chinese (Simplified) |
| Chinese_Traditional | Text_ChineseTraditional.json | Chinese (Traditional) |
| Thai | Text_Thai.json | Thai |

## Troubleshooting

### Mod name cut off or unclickable in selector
- Ensure `groupId` in JSON is `CDoT_Options`, NOT `CDoT`
- Ensure `LocalizationGroupId` constant is `CDoT_Options`

### Text shows as ID instead of translated text
- Verify the ID in code matches the ID in JSON (including prefix)
- Check JSON file is in `Texts/` folder and properly formatted

### Font warnings on Quest/Nomad
- Remove any emoji characters from text values
