# CDoT Translation System

## Quick Reference

- **Group ID**: `CDoT_Options` (must NOT match mod name `CDoT`)
- **ID format**: `CDoT_Options.SomethingHere`
- **No emojis** in text (Quest font doesn't support them)

## Workflow

### For Users

1. Edit `_translations.csv`:
   - Add new row with `Text_ID` and `English` columns filled in
   - Add GOOGLETRANSLATE formulas for other languages: `=GOOGLETRANSLATE(B2,"en","fr")`
2. Open CSV in Google Sheets, wait for translations to complete
3. Select all translated cells, copy, then Paste Values (Ctrl+Shift+V) to replace formulas with text
4. Save/export as `_translations.csv` back to project directory
5. Tell the agent to regenerate translations

### For Agents

When user says translations are ready or asks to regenerate:

```bash
python _generate_all_translations.py
dotnet build -c Release
dotnet build -c Nomad
```

When adding new localizable options to code, use:

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

## File Structure

```
_translations.csv              <- Master file (user edits this)
_generate_all_translations.py  <- Generates JSON from CSV
Texts/
  Text_English.json           <- Generated (don't edit directly)
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
```

## CSV Format

```csv
Text_ID,English,French,German,Spanish,Italian,Portuguese,Japanese,Korean,Chinese_Simplified,Chinese_Traditional,Thai
MyOption,My Option,Mon option,Meine Option,...
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Mod name cut off/unclickable | Ensure groupId is `CDoT_Options`, not `CDoT` |
| Text shows as ID | Check ID in code matches ID in JSON (with prefix) |
| Font warnings on Quest | Remove emoji characters from text |
