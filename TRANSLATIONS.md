# CDoT Translation System

## Quick Reference

- **Group ID**: `CDoT_Options` (must NOT match mod name `CDoT`)
- **ID format**: `CDoT_Options.SomethingHere`
- **No emojis** in text (Quest font doesn't support them)

## Workflow

### When New Translatable Text is Added

**Agent does:**
1. Add localization IDs to code (see Code Examples below)
2. Add new rows to `_translations.csv` with:
   - `Text_ID` column: the key (without prefix)
   - `English` column: the English text
   - Other columns: GOOGLETRANSLATE formulas, e.g. `=GOOGLETRANSLATE(B123,"en","fr")`
3. Tell user the CSV is ready for translation

**User does:**
1. Open `_translations.csv` in Google Sheets
2. Wait for GOOGLETRANSLATE formulas to complete
3. Select all cells with formulas, Copy, then Paste Values Only (Ctrl+Shift+V)
4. Save/export as `_translations.csv` back to project directory
5. Tell agent translations are ready

**Agent does:**
1. Run `python _generate_all_translations.py`
2. Build: `dotnet build -c Release && dotnet build -c Nomad`
3. Commit changes

## Code Examples

Adding localization to ModOption attributes:
```csharp
[ModOption(name = "My Option",
           nameLocalizationId = LocalizationGroupId + ".MyOption",
           category = "My Category",
           categoryLocalizationId = LocalizationGroupId + ".CategoryMy",
           ...)]
```

Adding localization to ModOptionString arrays:
```csharp
new ModOptionString("Label", LocalizationGroupId + ".LabelId", "Value")
```

## File Structure

```
_translations.csv              <- Master file (agent adds keys/formulas, user resolves translations)
_generate_all_translations.py  <- Generates JSON from CSV
Texts/
  Text_*.json                  <- Generated (don't edit directly)
```

## CSV Format

```csv
Text_ID,English,French,German,Spanish,Italian,Portuguese,Japanese,Korean,Chinese_Simplified,Chinese_Traditional,Thai
MyOption,My Option,=GOOGLETRANSLATE(B2,"en","fr"),=GOOGLETRANSLATE(B2,"en","de"),...
```

After user processes in Google Sheets:
```csv
Text_ID,English,French,German,...
MyOption,My Option,Mon option,Meine Option,...
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Mod name cut off/unclickable | Ensure groupId is `CDoT_Options`, not `CDoT` |
| Text shows as ID | Check ID in code matches ID in JSON (with prefix) |
| Font warnings on Quest | Remove emoji characters from text |
