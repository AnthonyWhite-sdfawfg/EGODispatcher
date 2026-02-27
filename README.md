

# EGODispatcher

Lobotomy Corporation 模组代码部分源码 - EGO 调度器

## 项目简介

这是一个为《脑叶公司》(Lobotomy Corporation)开发的游戏模组的代码部分的源码。模组添加了一个名为 "EGODispatcher" 的敌人实体，以及一系列 EGO 武器和护甲装备。

这是一个为《脑叶公司》(Lobotomy Corporation)开发的游戏模组的代码部分的源码。模组添加了一个名为 "EGODispatcher" 的敌人实体，以及一系列 EGO 武器和护甲装备。

## 项目结构

```
EGODispatcher/
├── Armors/                    # 护甲
│   ├── ArmorLazyDog.cs       # EGODispatcher异想体本身对应的EGO护甲
│   └── ArmorUnified.cs       # EXO Suit护甲，统一脚本
├── Bufs/                      # Buff/Debuff 模块
│   ├── DebufDamageMultiply.cs # 伤害倍率Debuff
│   ├── DebufDotDamage.cs      # 持续伤害Debuff
│   └── DebufSlowDown.cs       # 减速Debuff
├── Creature/                  # 生物实体
│   ├── EGODispatcher.cs      # EGO调度器主体
│   └── EGODispatcherAnim.cs  # 动画脚本
├── Utils/                     # 工具类
│   ├── AgentList.cs          
│   ├── ArmorConsts.cs        
│   ├── ArmorMethods.cs       
│   ├── ArmorStructs.cs       
│   ├── CreatureConsts.cs     
│   ├── CreatureMethods.cs    
│   ├── WeaponMethods.cs      
│   └── WeaponStructs.cs     
└── Weapons/                   # 武器模块
    ├── WeaponChainsaw.cs     
    ├── WeaponLazyDog.cs      # EGODispatcher异想体本身对应的EGO武器
    ├── WeaponPistol.cs       
    ├── WeaponRifle.cs        
    └── WeaponShotgun.cs      
```

## 主要功能

### EGO 调度器 (EGODispatcher)

- 在每个阶段开始时执行初始化逻辑
- 处理工作完成后的特殊事件
- 支持多种日期类型 (DayType) 的特殊行为

### 武器系统

| 武器 | 功能描述 |
|------|----------|
| WeaponChainsaw | 电锯攻击，造成持续伤害 |
| WeaponLazyDog | EGODispatcher |
| WeaponPistol | 手枪攻击，带有持续伤害效果 |
| WeaponRifle | 步枪攻击，远程精确打击 |
| WeaponShotgun | 霰弹枪攻击，范围伤害 |

### 护甲系统

| 护甲 | 功能描述 |
|------|----------|
| ArmorLazyDog | EGODispatcher护甲，战斗开始时触发特殊效果 |
| ArmorUnified | 统一护甲，支持多种战斗模式切换，自动恢复生命值和能量值 |

### Buff/Debuff 系统

- **伤害倍率 Debuff**: 修改目标受到的伤害倍率
- **持续伤害 (DoT)**: 周期性造成伤害
- **减速效果**: 降低目标的移动速度

## 工具类功能

- **AgentList**: 管理员工列表，提供查询和更新功能
- **CreatureMethods**: 生物相关操作，包括装备分配、天赋授予、感染清除等
- **ArmorMethods**: 护甲相关判断逻辑，如是否添加护盾、战斗模式解析等
- **WeaponMethods**: 武器相关工具方法

## 技术栈

- **语言**: C#
- **框架**: Unity (游戏模组)
- **IDE**: Visual Studio

## 说明

此处为dll的源码，请编译后使用或直接下载dll文件。

## 使用说明

通过特定操作，可以添加 EGO 调度器敌人以及相关武器装备。玩家可以在游戏中体验这些新内容。

## 注意事项

- 本模组仅供学习交流使用
- 请确保游戏版本与模组兼容
- 备份游戏存档后再使用模组

## 许可证

本项目遵循相关开源许可证。