

# EGODispatcher

Lobotomy Corporation 模组 - EGO 装备调度系统

## 项目简介

## 主要功能

### 装备系统

- **统一护甲 (ArmorUnified)**：支持多种战斗模式的智能护甲，根据员工状态自动切换防御模式
  - 战斗模式自动识别与切换
  - 受到攻击时自动施加护盾
  - 持续生命值与精神值恢复

- **武器系统**：多种特殊武器实现
  - 加农炮 (WeaponCannon)：范围伤害 + 持续伤害 (DoT)
  - 电锯 (WeaponChainsaw)：高伤害输出
  - 手枪 (WeaponPistol)、步枪 (WeaponRifle)、霰弹枪 (WeaponShotgun)

### Buff/Debuff 系统

### 员工管理

### 战斗辅助

## 项目结构

```
EGODispatcher/
├── Armors/           # 护甲系统
│   └── ArmorUnified.cs
├── Bufs/             # Buff/Debuff 系统
│   ├── BufInvincible.cs
│   ├── DebufDamageMultiply.cs
│   ├── DebufDotDamage.cs
│   └── DebufSlowDown.cs
├── Creature/         # Creature核心逻辑
│   ├── EGODispatcher.cs
│   └── EGODispatcherAnim.cs
├── Utils/            # 工具类
│   ├── AgentList.cs
│   ├── ArmorUtils.cs
│   ├── CreatureUtils.cs
│   ├── DialogueUtils.cs
│   ├── LocalTexts.cs
│   ├── LogUtils.cs
│   └── WeaponUtils.cs
└── Weapons/         # 武器系统
    ├── WeaponCannon.cs
    ├── WeaponChainsaw.cs
    ├── WeaponPistol.cs
    ├── WeaponRifle.cs
    └── WeaponShotgun.cs
```

## 使用说明

本模组需配合 Lobotomy Corporation 游戏主程序使用。该模组包含以下功能：

1. **战斗模式切换**：护甲会根据当前员工状态自动切换防御/攻击模式
2. **持续恢复**：员工在非战斗状态下会自动恢复生命值与精神值
3. **特殊武器效果**：使用特定武器时将触发额外效果（如持续伤害、减速等）

## 技术细节

### 战斗模式

## 依赖项

- .NET Framework (根据游戏版本)
- Lobotomy Corporation 游戏本体

## 许可证

MIT License

---

*本模组为粉丝作品，仅供学习交流使用。请勿用于商业用途。*