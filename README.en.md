# EGODispatcher

Lobotomy Corporation Mod - EGO Equipment Dispatch System

## Project Overview

EGODispatcher is a game mod designed for Lobotomy Corporation, providing an enhanced EGO equipment dispatch system, agent management system, and multiple combat assistance features. The mod implements special effects for weapons and armor through a unified equipment framework and introduces a flexible buff/debuff system to enrich the combat experience.

## Main Features

### Equipment System

- **Unified Armor System (ArmorUnified)**: Intelligent armor supporting multiple combat modes, automatically switching defense patterns based on agent status
  - Automatic detection and switching of combat modes
  - Automatic shield application upon taking damage
  - Continuous recovery of HP and SP

- **Weapon System**: Implementation of multiple special weapons
  - Cannon (WeaponCannon): Area-of-effect damage + Damage-over-Time (DoT)
  - Chainsaw (WeaponChainsaw): High-damage output
  - Pistol (WeaponPistol), Rifle (WeaponRifle), Shotgun (WeaponShotgun)

### Buff/Debuff System

- **BufInvincible**: Invincibility buff
- **DebufDamageMultiply**: Damage multiplier debuff
- **DebufDotDamage**: Damage-over-Time debuff
- **DebufSlowDown**: Slow debuff

### Agent Management

- Maintenance of active agent list
- Automatic cleanup of deceased agents
- Log recording grouped by Sephira

### Combat Assistance

- Weak point detection
- Damage type immunity checks
- Weapon-armor compatibility calculations

## Project Structure

```
EGODispatcher/
├── Armors/           # Armor System
│   └── ArmorUnified.cs
├── Bufs/             # Buff/Debuff System
│   ├── BufInvincible.cs
│   ├── DebufDamageMultiply.cs
│   ├── DebufDotDamage.cs
│   └── DebufSlowDown.cs
├── Creature/         # Creature/Agent Core Logic
│   ├── EGODispatcher.cs
│   └── EGODispatcherAnim.cs
├── Utils/            # Utility Classes
│   ├── AgentList.cs
│   ├── ArmorUtils.cs
│   ├── CreatureUtils.cs
│   ├── DialogueUtils.cs
│   ├── LocalTexts.cs
│   ├── LogUtils.cs
│   └── WeaponUtils.cs
└── Weapons/          # Weapon System
    ├── WeaponCannon.cs
    ├── WeaponChainsaw.cs
    ├── WeaponPistol.cs
    ├── WeaponRifle.cs
    └── WeaponShotgun.cs
```

## Usage Instructions

This mod requires the Lobotomy Corporation game executable. After installation, the following features are automatically loaded:

1. **Combat Mode Switching**: Armor automatically switches between defense and attack modes based on current agent status.
2. **Passive Recovery**: Agents automatically regenerate HP and SP during non-combat states.
3. **Special Weapon Effects**: Using specific weapons triggers additional effects (e.g., DoT, slow).

## Technical Details

### Combat Modes

The mod defines four combat modes:
- **Default**: Default mode
- **Combat**: Combat mode
- **Escape**: Escape mode
- **Work**: Work mode

### Damage Types

Supports multiple damage types (RwbpType) for weak point exploitation and attribute advantage calculations.

## Dependencies

- .NET Framework (based on game version)
- Lobotomy Corporation Game Core

## License

MIT License

---

*This mod is a fan-made project intended solely for learning and exchange purposes. Do not use for commercial purposes.*