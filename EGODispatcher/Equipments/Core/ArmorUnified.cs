using Equipments.Tools;

namespace Equipments.Core
{
    /// <summary>
    /// 护甲核心逻辑统一管理脚本。
    /// <para>本模组所有护甲共用此脚本，差异全部由所携带武器的 ID 推导出的战斗模式决定，
    /// 因此新增护甲通常无需新建脚本，只需登记 ID 与数值表。</para>
    /// <para>职责：</para>
    /// <list type="number">
    /// <item><description>根据员工战斗参数（CombatMode）设定恢复的周期与比例，战斗参数由该员工携带的武器类型决定；</description></item>
    /// <item><description>生命值/精神值低于阈值时修改对应的指定伤害类型的承伤修正比；</description></item>
    /// <item><description>参战/受击时触发屏障、移速加成等护甲特有效果。</description></item>
    /// </list>
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        #region 钩子

        /// <summary>
        /// [关卡开始] 缓存所属单位与员工模型，并按武器推导出的战斗模式初始化恢复节奏。
        /// </summary>
        public override void OnStageStart()
        {
            base.OnStageStart();
            owner = model.owner;
            worker = owner as WorkerModel;
            currentMode = ArmorTools.CombatMode.None;
            SetCombatParams(worker);
        }

        /// <summary>
        /// [每帧更新] 按战斗模式设定的间隔为员工恢复生命值与精神值。
        /// <para>处于恐慌状态时改用表中对应的恐慌恢复比率（通常更高，用于自救）。</para>
        /// </summary>
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            // 计时器未启动 / 未到执行周期 → 跳过本次恢复逻辑
            if (!HealTimer.started || !HealTimer.RunTimer())
            {
                return;
            }

            float ratioHP;
            float ratioMental;
            ArmorTools.CombatParams combatParams = ArmorTools.ModeToValues[currentMode];
            if (worker.IsPanic())
            {
                ratioHP = combatParams.HpPanic;       
                ratioMental = combatParams.MpPanic;   
            }
            else
            {
                ratioHP = combatParams.HpNormal;      
                ratioMental = combatParams.MpNormal;  
            }
            ArmorTools.HealThisWorker(worker, ratioHP, ratioMental);
            HealTimer.StartTimer(timerInterval);
        }

        /// <summary>
        /// [承伤修正] 按当前生命值/精神值相对阈值的高低改写承伤修正比。
        /// <para>注意：这里的 Defense 实际语义为"承伤修正比"，数值越大受到的伤害越高：
        /// 置为 0 表示完全免疫该类型伤害，负值表示吸收该类型伤害并转化为治疗。</para>
        /// <para>触发条件：生命值低于最大值的 <see cref="ArmorTools.DEFENSE_MARK_RATIO"/> 时，
        /// R、P 两系变为免疫；精神值低于阈值时，W、B 两系变为 10% 吸收。</para>
        /// </summary>
        /// <param name="actor">被查询承伤修正比的单位</param>
        /// <returns>改写后的承伤修正比副本</returns>
        public override DefenseInfo GetDefense(UnitModel actor) // 这里的Defence其实应该理解为承伤修正比
        {
            // 复制一份再修改，避免直接改动父类返回的原始数值
            DefenseInfo defenseInfo = base.GetDefense(actor).Copy();

            hpMark = actor.maxHp * ArmorTools.DEFENSE_MARK_RATIO;
            mpMark = actor.maxMental * ArmorTools.DEFENSE_MARK_RATIO;

            if (actor.hp < hpMark)
            {
                defenseInfo.R = 0f; // 免疫
                defenseInfo.P = 0f;
            }
            if (actor.mental < mpMark)
            {
                defenseInfo.W = -0.1f; // 以10%吸收
                defenseInfo.B = -0.1f;
            }

            return defenseInfo;
        }

        /// <summary>
        /// [准备武器] 员工参战时附加屏障，并无条件附加移速加成 Buff。
        /// <para>与 <see cref="OnTakeDamage"/> 的区别：此处是主动出击时的屏障，受击屏障另有数值。</para>
        /// </summary>
        /// <param name="actor">参战的单位</param>
        public override void OnPrepareWeapon(UnitModel actor)
        {
            if (ArmorTools.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorTools.BARRIER_ON_PREPARE_VALUE,
                    ArmorTools.BARRIER_ON_PREPARE_DURATION
                ));
            }
            actor.AddUnitBuf(CreateSpeedBuf(ArmorTools.SPEED_BUF_DURATION, ArmorTools.SPEED_BUF_VALUE));
            base.OnPrepareWeapon(actor);
        }

        /// <summary>
        /// [受击] 在尚未持有屏障时补上一层受击屏障。
        /// <para>因为父类 <c>OnTakeDamage</c> 的返回值语义为"是否取消本次伤害"，
        /// 这里在补屏障后返回 false，仅表示不拦截该次伤害，屏障会在后续结算中生效。</para>
        /// </summary>
        /// <param name="actor">受击单位</param>
        /// <param name="dmg">本次伤害信息</param>
        /// <returns>未持有屏障时交由父类判定；持有屏障时返回 false</returns>
        public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            if (owner == null) return false;
            if (ArmorTools.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorTools.BARRIER_ON_HIT_VALUE,
                    ArmorTools.BARRIER_ON_HIT_DURATION
                ));
                return false;
            }
            return base.OnTakeDamage(actor, ref dmg);
        }
        #endregion

        #region 私有工具方法

        /// <summary>
        /// 读取战斗参数，设定恢复周期与比例。
        /// <para>战斗模式由武器 ID 推导（见 <see cref="ArmorTools.ResolveCombatMode"/>），
        /// 并据此查表取得恢复间隔；比例在每次恢复时再按恐慌状态选择。</para>
        /// </summary>
        /// <param name="worker">护甲所属的员工</param>
        private void SetCombatParams(WorkerModel worker)
        {
            currentMode = ArmorTools.ResolveCombatMode(worker);
            timerInterval = ArmorTools.ModeToValues[currentMode].TimerInterval;
            HealTimer.StartTimer(timerInterval);
        }

        /// <summary>
        /// 创建加速buf。
        /// </summary>
        /// <param name="duration">Buff 持续时间，单位为秒</param>
        /// <param name="value">移动速度加成数值</param>
        /// <returns>配置好的移速加成 Buff</returns>
        private UnitStatBuf CreateSpeedBuf(float duration, float value)
        {
            // ONLY_ONE：同一目标上只保留一份移速 Buff，重复参战会刷新而非叠加速度
            return new UnitStatBuf(duration, UnitBufType.ADD_SUPERARMOR)
            {
                duplicateType = BufDuplicateType.ONLY_ONE, 
                movementSpeed = value 
            };
        }
        #endregion

        #region 私有字段
        // 单位：秒；值由当前 CombatMode 决定
        /// <summary>恢复间隔，单位为秒，由当前战斗模式查表得出</summary>
        private float timerInterval;

        // 低于此阈值时修改防御抗性，计算方式：maxHp * DEFENSE_MARK_RATIO
        /// <summary>生命值阈值，低于它时 R、P 两系承伤修正比变为免疫</summary>
        private float hpMark;
        /// <summary>精神值阈值，低于它时 W、B 两系承伤修正比变为吸收</summary>
        private float mpMark;

        /// <summary>恢复计时器，控制 <see cref="OnFixedUpdate"/> 中的治疗节奏</summary>
        private readonly Timer HealTimer = new Timer();

        // 当前战斗中的员工及其所属单位
        /// <summary>护甲所属的员工模型</summary>
        private WorkerModel worker;
        /// <summary>护甲所属的单位模型，<see cref="OnTakeDamage"/> 中用作空值保护</summary>
        private UnitModel owner;

        // 当前模式，用于匹配恢复参数
        /// <summary>当前战斗模式，用于匹配 <see cref="ArmorTools.ModeToValues"/> 中的恢复参数</summary>
        private ArmorTools.CombatMode currentMode;
        #endregion
    }
}