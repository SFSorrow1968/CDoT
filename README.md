# BDOT - Bleed Damage Over Time

A Blade & Sorcery mod that adds location-based bleeding damage over time effects.

## Features

- **Location-Based Bleeding** - Different body parts cause different bleed severity
- **Throat Wounds** - Very high damage multiplier for critical throat strikes
- **Head/Neck Wounds** - High damage for head and neck injuries
- **Torso Wounds** - Moderate bleeding from body hits
- **Limb Wounds** - Lower damage from arm and leg strikes
- **Dismemberment Bleeding** - High sustained damage when limbs are severed

All zones are fully configurable with individual settings for:
- Enable/disable
- Damage multiplier
- Duration
- Damage per tick
- Stack limit

## Body Zone Damage Multipliers

| Zone | Multiplier | Description |
|------|------------|-------------|
| Throat | 3.0x | Critical - rapid blood loss |
| Neck | 2.5x | Severe - major vessel damage |
| Head | 2.0x | High - significant trauma |
| Dismemberment | 2.5x | Severe - massive blood loss |
| Torso | 1.0x | Moderate - baseline damage |
| Leg | 0.6x | Minor - slower bleed |
| Arm | 0.5x | Minor - peripheral wound |

## Installation

### PCVR
1. Download the latest PCVR release
2. Extract to `[Blade & Sorcery]\BladeAndSorcery_Data\StreamingAssets\Mods\BDOT\`
3. Launch the game

### Nomad (Quest)
1. Download the latest Nomad release
2. Extract to `[Device]\Android\data\com.Warpfrog.BladeAndSorcery\files\Mods\BDOT\`
3. Launch the game

## Configuration

Edit `settings.json` in the mod folder to customize zones:

```json
{
  "Enabled": true,
  "GlobalDamageMultiplier": 1.0,
  "TickInterval": 0.5,
  "Zones": {
    "Throat": {
      "Enabled": true,
      "DamageMultiplier": 3.0,
      "Duration": 8.0,
      "DamagePerTick": 5.0,
      "StackLimit": 3
    },
    "Arm": {
      "Enabled": true,
      "DamageMultiplier": 0.5,
      "Duration": 4.0,
      "DamagePerTick": 1.0,
      "StackLimit": 4
    }
  }
}
```

## Building from Source

### Requirements
- .NET SDK 6.0+
- Blade & Sorcery game installation (for reference DLLs)

### Setup
1. Clone the repository
2. Copy required DLLs from your game installation to `libs/`:
   - `ThunderRoad.dll`
   - `Assembly-CSharp.dll`
   - `Assembly-CSharp-firstpass.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`

   From: `[Game Install]\BladeAndSorcery_Data\Managed\`

### Build Commands
```bash
# PCVR version
dotnet build -c Release

# Nomad version (IL2CPP compatible)
dotnet build -c Nomad
```

Output will be in `bin/Release/PCVR/BDOT/` or `bin/Release/Nomad/BDOT/`.

## Platform Differences

| Feature | PCVR | Nomad |
|---------|------|-------|
| Hook Method | Harmony Patches | EventManager |
| Dependencies | 0Harmony.dll | None |
| IL2CPP Compatible | N/A | Yes |

## Compatibility

- **Game Version**: 1.0.0.0+
- **PCVR**: Full support
- **Nomad**: Full support (IL2CPP compatible)

## License

MIT License - See [LICENSE](LICENSE) file.

## Credits

- **Author**: dkatz
- **Framework**: [ThunderRoad](https://github.com/KospY/BasSDK) by KospY/Warpfrog
