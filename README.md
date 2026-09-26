# EGODispatcher

《脑叶公司》(Lobotomy Corporation) 的自定义异想体模组。

模组向游戏中加入一个名为 **EGODispatcher** 的异想体，外观是一台自律运行的设施管理终端。玩家收容它之后，每完成一次对它本人的工作，终端就会触发一轮"系统服务"：下发 EGO 装备与饰品、清除员工感染、持续生成 LOB 能量；若当天处于核心抑制日，它还会上线对应的"维护协议"，用对话的形式向主管汇报设施状态。

- 语言：C# / .NET Framework 4.6.1（类库）
- 依赖：游戏本体 `Assembly-CSharp.dll`、`UnityEngine`、`0Harmony`，以及外部 mod 框架 `LobotomyBaseMod`
- 许可：[MIT](LICENSE)

---

## 功能一览

### 终端服务（每次工作后触发）

| 功能 | 说明 |
| --- | --- |
| **EGO 装备下发** | 员工携带 EGO 饰品 `83400` 对终端完成工作时，向装备库补齐一整套自建装备（护甲 11 种 + 武器 12 种，按清单逐项补足数量） |
| **饰品下发** | 员工携带 `83211`–`83214` 中任意一件时，为设施内全体员工按武器类型分发对应套装饰品，并强制改为光头（头盔贴图无法完全覆盖发型贴图） |
| **感染清除** | 每秒轮询一次，清除员工身上的溶解之爱 / 裸巢 / 蜂后感染 |
| **LOB 能量** | 当天首次工作后启动 Lob Generator，按当天异想体数量持续增长 LOB，每日上限 300，跨日自动关闭 |

### 核心抑制协议

终端会识别当天的核心抑制类型，并执行对应行为：

| 模式 | 触发条件 | 行为 |
| --- | --- | --- |
| `MALKUTH` | Malkuth 核心抑制 | 输出 Malkuth 打乱后的工作指令映射表；过载等级变化时刷新映射表 |
| `YESOD` | Yesod 核心抑制 | 延迟销毁Yesod的UI滤镜(位于主相机与 UI 相机上) |
| `NETZACH` | Netzach 核心抑制 | 解除恢复机制封锁，重启医疗系统 |
| `HOD` | Hod 核心抑制 | 仅输出系统启动提示；对应机制**尚未实现** |
| `D47` | Day 47 构筑部（Kether E1） | 以上全部模式同时开启 |

### 自建 EGO 装备

**护甲（统一脚本 `ArmorUnified`）**

- 按员工携带武器推导出的战斗模式（`CombatMode`）决定回血周期与比例，武器 ID 的百位数字即职业标识：`1` = Worker、`2` = Operative、`3` = Keter 组员、`4` = 原型
- 生命值 / 精神值低于 30% 阈值时改写承伤修正比：HP 低 → R、P 免疫；MP 低 → W、B 以 10% 吸收
- 参战与受击时自动挂上屏障 Buff，并附带移速加成

**武器**

| 脚本 | 特点 |
| --- | --- |
| `WeaponPistol` | 多发点射，命中后施加减速 |
| `WeaponRifle` | 多发点射，命中后施加易伤并固定扣血 |
| `WeaponShotgun` | 高段数射击，强减速 + 易伤 + 固定扣血 |
| `WeaponChainsaw` | 常规段数攻击；目标存在免疫抗性时切换为高频多段并改用其最弱抗性 |
| `WeaponCannon` | 附带持续伤害（DOT） |

所有武器都具备**自适应破防**：命中前检测目标是否拥有免疫/吸收抗性（承伤修正比 ≤ 0），若有则改用目标承伤修正比最高的伤害类型，避免被免疫完全抵消。

**Debuff（`Equipments/Bufs/`）**

| 脚本 | 效果 |
| --- | --- |
| `DebufSlowDown` | 降低目标移动速度 |
| `DebufDamageMultiply` | 目标受到伤害的倍率修正，可选择唯一或无限叠加 |
| `DebufDotDamage` | 持续伤害，可指定单一伤害类型，也可四色齐打 |

---

## 目录结构

```
EGODispatcher/
├── Creature/                 异想体本体
│   ├── EGODispatcher.cs          主类：生命周期钩子、核心抑制协议、消息队列
│   ├── CreatureTools.cs          装备清单、批处理协程、核心抑制处理
│   └── EGODispatcherAnim.cs      Spine 动画绑定
├── Equipments/
│   ├── Core/                     护甲与武器脚本
│   ├── Bufs/                     自定义 Debuff
│   └── Tools/                    数值表与解析工具
├── Utils/
│   ├── ActiveAgentManager.cs     存活员工表（游戏接口会连死亡员工一起返回）
│   ├── DialogueSendings.cs       头像对话发送
│   ├── LocalTexts.cs             中文文案常量
│   └── LogSendings.cs            系统日志颜色封装
├── Lib/                          游戏与 Harmony 依赖 DLL（不纳入版本控制）
└── Properties/AssemblyInfo.cs
```

---

## 构建

### 前置条件

- Visual Studio 2019 或更高版本（含 .NET Framework 4.6.1 开发工具）
- 一份已安装的《脑叶公司》，用于取得以下程序集：

| 程序集 | 放置位置 |
| --- | --- |
| `Assembly-CSharp.dll` | `EGODispatcher/Lib/` |
| `UnityEngine.dll` | `EGODispatcher/Lib/` |
| `UnityEngine.CoreModule.dll` | `EGODispatcher/Lib/` |
| `UnityEngine.ImageConversionModule.dll` | `EGODispatcher/Lib/` |
| `0Harmony.dll` | `EGODispatcher/Lib/` |

其中 `Assembly-CSharp.dll` 通常位于游戏目录的 `LobotomyCorp_Data/Managed/` 下。

> `Lib/` 与 `bin/`、`obj/` 已写入 `.gitignore`，克隆仓库后需自行放入上述 DLL。

### 编译

用 Visual Studio 打开 `EGODispatcher.sln` 并生成 `Release`，或使用命令行：

```powershell
msbuild EGODispatcher.sln /p:Configuration=Release
```

产物为 `EGODispatcher/bin/Release/EGODispatcher.dll`。

### 部署

略，该仓库储存的为该模组的代码部分。

---

## 关键参数速查

以下常量可在 `Creature/CreatureTools.cs` 与 `Equipments/Tools/ArmorTools.cs` 中调整：

| 常量 | 值 | 含义 |
| --- | --- | --- |
| `DEFAULT_BATCH_SIZE` | `5` | 批量处理员工时每批人数 |
| `DEFAULT_DELAY_TIME` | `0.5` 秒 | 通用延迟与消息播报间隔 |
| `LOB_MAX_VALUE` | `300` | 每日 LOB 生成上限 |
| `MAX_MESSAGE_COUNT` | `10` | 待播报消息队列长度（FIFO，超出丢弃最旧） |
| `BARRIER_ON_PREPARE_VALUE` / `DURATION` | `1800` / `65` 秒 | 参战屏障数值与持续时间 |
| `BARRIER_ON_HIT_VALUE` / `DURATION` | `1800` / `65` 秒 | 受击屏障数值与持续时间 |
| `SPEED_BUF_VALUE` / `DURATION` | `180` / `20` 秒 | 移速加成数值与持续时间 |
| `DEFENSE_MARK_RATIO` | `0.3` | 改写承伤修正比的血量 / 精神阈值比例 |
| `ID_DIGIT` | `100` | 从武器 ID 解析职业的取位基数 |

武器 ID 编号段：护甲 `811xx`、手枪 `8311x`、步枪 `8321x`、霰弹枪 `8331x`、链锯 `8332x`、饰品 `821xx`–`824xx`。

---

## 已知问题与注意事项

1. **编码规范**：仓库内全部 `.cs` / `.csproj` / `.sln` 文件统一为「带 BOM 的 UTF-8」+ CRLF 行尾，请勿新增无 BOM 或 GBK 编码的源文件。注意 `Equipments/Core/ArmorUnified.cs` 在**历史提交中仍以 GBK 存放**，工作区版本已完成 UTF-8 转换但尚未提交；在提交该转换前，对它的 `git diff` 会把所有中文注释行显示为乱码变更（属正常现象，提交后即恢复）。
2. `Equipments/Core/ArmorUnified.cs` 内部命名空间声明为 `Equipments.Tools`，与所在目录 `Core` 不一致。
3. `DebufSlowDown` 借用了尸山 EGO"笑靥"的减速 Buff 类型槽位，且 `duplicateType` 为 `ONLY_ONE`，因此两者同时生效时会互相覆盖。
4. HOD 对应机制尚未实现：`isHod` 仅用于输出 `LocalTexts.HOD_INIT` 提示，未修改任何实际游戏逻辑。

---

## 许可

本项目基于 [MIT License](LICENSE) 开源。
