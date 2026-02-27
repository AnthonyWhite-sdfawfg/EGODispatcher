

# EGODispatcher

Lobotomy Corporation 模组 - EGO 调度器

## 说明

**注意：此处为dll的源码，请编译后使用或直接下载dll文件。**


## 项目简介

该模组是一个为《脑叶公司》(Lobotomy Corporation)开发的游戏模组。模组添加了一个名为 "EGODispatcher" 的敌人实体，以及一系列 EGO 武器和护甲装备。

## 项目结构

```
EGODispatcher/
├── Armors/                   
│   ├── ArmorLazyDog.cs       
│   └── ArmorUnified.cs      
├── Bufs/                     
│   ├── DebufDamageMultiply.cs 
│   ├── DebufDotDamage.cs
│   └── DebufSlowDown.cs
├── Creature/
│   ├── EGODispatcher.cs  
│   └── EGODispatcherAnim.cs
├── Utils/           
│   ├── AgentList.cs          
│   ├── ArmorConsts.cs        
│   ├── ArmorMethods.cs       
│   ├── ArmorStructs.cs       
│   ├── CreatureConsts.cs     
│   ├── CreatureMethods.cs    
│   ├── WeaponMethods.cs      
│   └── WeaponStructs.cs     
└── Weapons/                  
    ├── WeaponChainsaw.cs     
    ├── WeaponLazyDog.cs    
    ├── WeaponPistol.cs       
    ├── WeaponRifle.cs        
    └── WeaponShotgun.cs      
```

## 主要功能

### EGO 调度器 (EGODispatcher)
### 武器系统
### 护甲系统
### Buff/Debuff 系统

因为篇幅原因无法在此详细说明，详见说明书.md（正在编纂）

## 技术栈

- **语言**: C#
- **框架**: Unity (游戏模组)
- **IDE**: Visual Studio

## 使用说明

通过特定操作，可以添加 EGO 调度器敌人以及相关武器装备。玩家可以在游戏中体验这些新内容。

## 注意事项

- 本模组仅供学习交流使用
- 请确保游戏版本与模组兼容
- 备份游戏存档后再使用模组

## 许可证

本项目遵循相关开源许可证。