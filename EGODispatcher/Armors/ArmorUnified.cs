using Utils;

namespace Armors
{
    /// <summary>
    /// 护甲核心逻辑统一管理脚本
    /// 1. 根据员工当前战斗参数（CombatMode）动态调整恢复的周期与比例；
    /// 2. 生命值/精神值低于阈值时修改防御属性；
    /// 3. 武器准备/受击时触发屏障、移速加成等护甲特有效果；
    /// 4. 所有数值常量/结构体/工具方法依赖 ArmorConsts/ArmorStructs/ArmorMethods 定义。
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        #region 钩子
        public override void OnStageStart()
        {
            base.OnStageStart();
            // 绑定当前装备的所属员工
            owner = model.owner;
            worker = owner as WorkerModel;
            // 初始化战斗模式，避免空值逻辑异常
            currentMode = ArmorStructs.CombatMode.None;
            // 加载当前战斗模式的参数，并启动恢复计时器
            RestoreCombatParams(worker);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            // 若员工装备出现变化则重新加载战斗参数
            ArmorStructs.CombatMode combatMode = ArmorMethods.ResolveCombatMode(worker);
            if (combatMode != currentMode)
            {
                currentMode = combatMode;
                RestoreCombatParams(worker);
            }

            // 计时器未启动 / 未到执行周期 → 跳过本次恢复逻辑
            if (!HealTimer.started || !HealTimer.RunTimer())
            {
                return;
            }

            // 根据员工是否恐慌，获取对应模式下的生命/精神恢复比例
            float ratioHP;
            float ratioMental;
            ArmorStructs.CombatParams combatParams = ArmorStructs.ModeToValues[currentMode];
            if (worker.IsPanic())
            {
                ratioHP = combatParams.HpPanic;       // 恐慌状态下的生命恢复比例
                ratioMental = combatParams.MpPanic;   // 恐慌状态下的精神恢复比例
            }
            else
            {
                ratioHP = combatParams.HpNormal;      // 正常状态下的生命恢复比例
                ratioMental = combatParams.MpNormal;  // 正常状态下的精神恢复比例
            }

            // 执行恢复逻辑
            ArmorMethods.HealThisWorker(worker, ratioHP, ratioMental);
            // 重置恢复计时器，进入下一个周期
            HealTimer.StartTimer(timerInterval);
        }

        public override DefenseInfo GetDefense(UnitModel actor)
        {
            DefenseInfo defenseInfo = base.GetDefense(actor).Copy();

            // 计算生命/精神阈值（最大值 * 阈值比例）
            hpMark = actor.maxHp * ArmorConsts.DEFENSE_MARK_RATIO;
            mpMark = actor.maxMental * ArmorConsts.DEFENSE_MARK_RATIO;

            // 生命低于阈值 → R/P 抗性改为免疫
            if (actor.hp < hpMark)
            {
                defenseInfo.R = 0f;
                defenseInfo.P = 0f;
            }
            // 精神低于阈值 → W/B 抗性改为吸收（-0.1）
            if (actor.mental < mpMark)
            {
                defenseInfo.W = -0.1f;
                defenseInfo.B = -0.1f;
            }

            return defenseInfo;
        }

        public override void OnPrepareWeapon(UnitModel actor)
        {
            // 满足屏障添加条件 → 挂载准备阶段屏障Buff
            if (ArmorMethods.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorConsts.BARRIER_ON_PREPARE_VALUE,
                    ArmorConsts.BARRIER_ON_PREPARE_DURATION
                ));
            }
            // 挂载移速加成Buff
            actor.AddUnitBuf(CreateSpeedBuf());

            base.OnPrepareWeapon(actor);
        }

        public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            // 所属单位为空 → 直接返回
            if (owner == null) return false;

            // 满足屏障添加条件 → 挂载受击屏障Buff，并拦截本次伤害
            if (ArmorMethods.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorConsts.BARRIER_ON_HIT_VALUE,
                    ArmorConsts.BARRIER_ON_HIT_DURATION
                ));
                return false;
            }

            return base.OnTakeDamage(actor, ref dmg);
        }
        #endregion

        #region 私有工具方法
        /// <summary>
        /// 更新当前战斗模式对应的参数
        /// 包括：更新战斗模式、重置计时器间隔、启动恢复计时器
        /// </summary>
        private void RestoreCombatParams(WorkerModel worker)
        {
            // 重新解析并更新当前战斗模式
            currentMode = ArmorMethods.ResolveCombatMode(worker);
            // 根据战斗模式获取对应的计时器间隔
            timerInterval = ArmorStructs.ModeToValues[currentMode].TimerInterval;
            // 启动/重置恢复计时器
            if (HealTimer.started || HealTimer.RunTimer())
            {
                HealTimer.StopTimer();
            }
            HealTimer.StartTimer(timerInterval);
        }

        /// <summary>
        /// 创建移速加成Buff
        /// </summary>
        private UnitStatBuf CreateSpeedBuf()
        {
            return new UnitStatBuf(ArmorConsts.SPEED_BUF_DURATION, UnitBufType.ADD_SUPERARMOR)
            {
                duplicateType = BufDuplicateType.ONLY_ONE, 
                movementSpeed = ArmorConsts.SPEED_BUF_VALUE 
            };
        }
        #endregion

        #region 私有字段
        /// <summary>
        /// 回血计时器间隔（秒），由当前战斗模式（CombatMode）动态决定
        /// </summary>
        private float timerInterval;

        /// <summary>
        /// 生命阈值（maxHp * DEFENSE_MARK_RATIO），低于该值时修改防御抗性
        /// </summary>
        private float hpMark;

        /// <summary>
        /// 精神阈值（maxMental * DEFENSE_MARK_RATIO），低于该值时修改防御抗性
        /// </summary>
        private float mpMark;

        /// <summary>
        /// 恢复计时器，控制恢复逻辑的执行周期
        /// </summary>
        private readonly Timer HealTimer = new Timer();

        /// <summary>
        /// 当前绑定的员工，用于获取战斗状态/执行恢复逻辑
        /// </summary>
        private WorkerModel worker;
        private UnitModel owner;

        /// <summary>
        /// 当前所处的战斗模式（职位），用于匹配对应的恢复/计时器参数
        /// </summary>
        private ArmorStructs.CombatMode currentMode;
        #endregion
    }
}