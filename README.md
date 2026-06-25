

# EGODispatcher

Lobotomy Corporation 模组 - EGO 装备调度系统

## 项目简介

EGODispatcher 是一个为 Lobotomy Corporation 设计的游戏模组，提供了增强的 EGO 装备调度系统、 agent 管理系统以及多种战斗辅助功能。该模组通过统一的装备框架实现了武器、护甲的特殊效果，并引入了灵活的 buff/debuff 系统来增强游戏战斗体验。

## 主要功能

### 装备系统

- **统一护甲系统 (ArmorUnified)**：支持多种战斗模式的智能护甲，根据 agent 状态自动切换防御模式
  - 战斗模式自动识别与切换
  - 受到攻击时自动施加护盾
  - 持续生命值与精神值恢复

- **武器系统**：多种特殊武器实现
  - 加农炮 (WeaponCannon)：范围伤害 + 持续伤害 (DoT)
  - 电锯 (WeaponChainsaw)：高伤害输出
  - 手枪 (WeaponPistol)、步枪 (WeaponRifle)、霰弹枪 (WeaponShotgun)

### Buff/Debuff 系统

- **BufInvincible**：无敌状态 buff
- **DebufDamageMultiply**：伤害倍率 debuff
- **DebufDotDamage**：持续伤害 debuff
- **DebufSlowDown**：减速 debuff

### Agent 管理

- 活跃 agent 列表维护
- 死亡 agent 自动清理
- 按 Sephira 分组日志记录

### 战斗辅助

- 弱点击破检测
- 伤害类型免疫判断
- 武器与防御相性计算

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
├── Creature/         # 生物/Agent 核心逻辑
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

本模组需配合 Lobotomy Corporation 游戏主程序使用。安装模组后，游戏将自动加载以下功能：

1. **战斗模式切换**：护甲会根据当前 agent 状态自动切换防御/攻击模式
2. **持续恢复**：Agent 在非战斗状态下会自动恢复生命值与精神值
3. **特殊武器效果**：使用特定武器时将触发额外效果（如持续伤害、减速等）

## 技术细节

### 战斗模式

模组定义了四种战斗模式：
- **Default**：默认模式
- **Combat**：战斗模式
- **Escape**：撤退模式
- **Work**：工作模式

### 伤害类型

支持多种伤害类型 (RwbpType)，可用于弱点击破与属性相克计算。

## 依赖项

- .NET Framework (根据游戏版本)
- Lobotomy Corporation 游戏本体

## 许可证

MIT License

---

*本模组为粉丝作品，仅供学习交流使用。请勿用于商业用途。*