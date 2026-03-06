

# EGODispatcher

Lobotomy Corporation 模组 - EGO 调度器

## 说明

**注意：此处为 DLL 文件的源码，并非模组本体。**

本模组为《脑叶公司》(Lobotomy Corporation) 开发的功能扩展模组，添加了全新的敌人实体、武器装备和战斗机制。

## 项目简介

EGODispatcher 模组为《脑叶公司》游戏添加了以下内容：

- **EGO 调度器 (EGODispatcher)**：一个具有独特行为模式的敌人实体
- **EGO 武器系统**：包含多种特殊武器，每种武器都有独特的攻击方式和伤害机制
- **EGO 护甲系统**：提供不同的防御模式和战斗加成
- **Buff/Debuff 系统**：丰富的状态效果系统，包括伤害倍率、持续伤害和减速效果

## 项目结构

```
EGODispatcher/
├── Armors/                      # 护甲系统
│   ├── ArmorLazyDog.cs          # 懒狗护甲
│   └── ArmorUnified.cs          # 统一护甲
├── Bufs/                        # Buff/Debuff 系统
│   ├── DebufDamageMultiply.cs   # 伤害倍率减益
│   ├── DebufDotDamage.cs        # 持续伤害减益
│   └── DebufSlowDown.cs         # 减速减益
├── Creature/                    # 敌人实体
│   ├── EGODispatcher.cs         # EGO 调度器核心逻辑
│   └── EGODispatcherAnim.cs     # EGO 调度器动画脚本
├── Utils/                       # 工具类
│   ├── AgentList.cs             # 员工列表管理
│   ├── ArmorConsts.cs           # 护甲常量定义
│   ├── ArmorMethods.cs          # 护甲方法集
│   ├── ArmorStructs.cs          # 护甲数据结构
│   ├── CreatureConsts.cs        # 敌人常量定义
│   ├── CreatureMethods.cs       # 敌人方法集
│   ├── WeaponMethods.cs         # 武器方法集
│   └── WeaponStructs.cs         # 武器数据结构
├── Weapons/                     # 武器系统
│   ├── WeaponChainsaw.cs        # 电锯
│   ├── WeaponLazyDog.cs         # 懒狗武器
│   ├── WeaponPistol.cs          # 手枪
│   ├── WeaponRifle.cs           # 步枪
│   └── WeaponShotgun.cs         # 霰弹枪
└── Properties/
    └── AssemblyInfo.cs          # 程序集信息
```

## 主要功能

### EGO 调度器 (EGODispatcher)

EGO 调度器是一个具有复杂行为模式的敌人实体，具备以下特性：

- **工作行为**：根据不同日期类型执行不同的工作逻辑
- **感染机制**：能够对员工施加感染效果
- **动画控制**：完整的工作、逃跑和死亡动画系统
- **装备分发**：自动为员工分配 EGO 武器和护甲

### 武器系统

模组包含多种 EGO 武器，每种武器都有独特的攻击机制：

| 武器类型 | 特性 |
|---------|------|
| 手枪 (Pistol) | 基础远程武器 |
| 步枪 (Rifle) | 高精度射击 |
| 霰弹枪 (Shotgun) | 范围伤害，带有持续伤害效果 |
| 电锯 (Chainsaw) | 持续近战伤害 |
| 链炮 (Cannon) | 强力攻击，可附加持续伤害 |

### 护甲系统

护甲系统提供多种防御模式和战斗加成：

- **统一护甲 (ArmorUnified)**：根据战斗情况自动切换防御模式
  - 常规模式：提供屏障和生命值恢复
  - 战斗模式：提供速度加成和额外的防御能力
- **懒狗护甲 (ArmorLazyDog)**：特殊防御机制

### Buff/Debuff 系统

- **伤害倍率减益 (DebufDamageMultiply)**：降低目标受到的伤害倍数
- **持续伤害减益 (DebufDotDamage)**：周期性造成伤害
- **减速减益 (DebufSlowDown)**：降低目标移动速度

## 技术栈

- **语言**：C#
- **框架**：Unity (游戏模组)
- **IDE**：Visual Studio

## 使用说明

1. **安装前提**：确保已安装《脑叶公司》游戏本体
2. **模组安装**：将编译后的 DLL 文件放入游戏的模组目录
3. **游戏启动**：启动游戏后，模组内容将自动加载

具体操作请参考游戏模组安装指南。

## 注意事项

- 本模组仅供学习交流使用
- 请确保游戏版本与模组兼容
- 建议在使用模组前备份游戏存档
- 部分功能可能需要特定的游戏进度才能解锁

## 许可证

本项目遵循相关开源许可证。具体许可证信息请参阅 LICENSE 文件。