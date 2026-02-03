# CDoT Development Reference (Agent-Only)

## Platform Differences
- PCVR: Mono runtime; Harmony patches OK; file I/O OK.
- Nomad: IL2CPP; Harmony unreliable; avoid file I/O and Newtonsoft.Json.
- Use `#if NOMAD` to route Nomad to `Hooks/EventHooks.cs` and PCVR to Harmony patches.

## ModOptions System
- Options live in `Configuration/CDoTModOptions.cs` as a **public static** class.
- Sliders require `valueSourceName` + `interactionType = (ModOption.InteractionType)2`.
- `defaultValueIndex` is the index into the provider array.
- Arrow lists are InteractionType `0` (default); buttons are `1`.

## EventManager Events (Nomad-safe)
- `EventManager.onCreatureHit` (Creature, CollisionInstance, EventTime)
- `EventManager.onCreatureKill` (Creature, Player, CollisionInstance, EventTime)
- Use `EventTime.OnEnd` for finalized data.
- No `onRagdollSlice` event; detect via ragdoll flags.

## Body Zone Detection
- Use `RagdollPart.Type` flags to determine hit location.
- Throat detection: `RagdollPart.Type.Neck` with additional position checks.
- Head: `RagdollPart.Type.Head`
- Neck: `RagdollPart.Type.Neck`
- Torso: `RagdollPart.Type.Torso`
- Arms: `RagdollPart.Type.LeftArm | RightArm | LeftHand | RightHand`
- Legs: `RagdollPart.Type.LeftLeg | RightLeg | LeftFoot | RightFoot`
- Dismemberment: Check `RagdollPart.isSliced` flag.

## Bleed Effect System
- `BleedEffect` tracks: target creature, zone, damage per tick, remaining duration, stack count.
- `BleedManager` maintains active effects per creature, handles tick updates.
- Damage is applied via `Creature.Damage()` with `DamageType.Unknown` or custom.
- Use `Time.unscaledTime` / `Time.unscaledDeltaTime` for consistency during slow motion.

## Project Structure (Key Files)
- `Configuration/CDoTModOptions.cs`: UI options + zone accessors.
- `Configuration/BodyZone.cs`: Zone enum and constants.
- `Core/CDoTModule.cs`: ThunderScript entry.
- `Core/BleedManager.cs`: Main bleed logic, effect tracking, damage application.
- `Core/BleedEffect.cs`: Individual bleed effect data structure.
- `Hooks/EventHooks.cs`: Event subscriptions.

## Build Configuration
- Nomad: `dotnet build -c Nomad`
- PCVR: `dotnet build -c Release`
- Nomad build excludes `Patches/**` and defines `NOMAD`.

## manifest.json (Critical)
- `GameVersion` must match the game's `minModVersion` **exactly** (`X.X.X.X`).
- Mismatch causes assemblies not to load (`Loaded 0 of Metadata Assemblies` in Player.log).

## Common Pitfalls (Actionable)
- Use `Time.unscaledTime` / `Time.unscaledDeltaTime` for timers (bleed effects must continue during slow motion mods).
- Avoid Newtonsoft.Json and file I/O on Nomad.
- Always unsubscribe before re-subscribing to events to prevent duplicates.
- Clean up bleed effects when creatures are killed or despawned.

## Known Limitations (Game Engine)
- **Slider drag doesn't commit values**: ThunderRoad's ModOption slider UI only commits values when arrow buttons are clicked. Dragging the slider moves it visually but does not fire the value change callback until an arrow is pressed. This is a game engine limitation affecting all mods using `InteractionType.Slider` - not fixable from mod code.
