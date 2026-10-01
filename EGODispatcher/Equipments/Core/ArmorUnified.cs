using Equipments.Tools;

namespace Equipments.Core
{
    /// <summary>
    /// 护甲核心逻辑统一管理脚本
    /// 1. 根据员工战斗参数（CombatMode）设定恢复的周期与比例，战斗参数由该员工携带的武器类型决定；
    /// 2. 生命值/精神值低于阈值时修改对应的指定伤害类型的承伤修正比；
    /// 3. 参战/受击时触发屏障、移速加成等护甲特有效果；
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        #region 钩子
        public override void OnStageStart()
        {
            base.OnStageStart();
            owner = model.owner;
            worker = owner as WorkerModel;
            currentMode = ArmorTools.CombatMode.None;
            SetCombatParams(worker);
        }

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

        public override DefenseInfo GetDefense(UnitModel actor) // 这里的Defence其实应该理解为承伤修正比
        {
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
        /// 读取战斗参数，设定恢复周期与比例
        /// </summary>
        private void SetCombatParams(WorkerModel worker)
        {
            currentMode = ArmorTools.ResolveCombatMode(worker);
            timerInterval = ArmorTools.ModeToValues[currentMode].TimerInterval;
            HealTimer.StartTimer(timerInterval);
        }

        /// <summary>
        /// 创建加速buf
        /// </summary>
        private UnitStatBuf CreateSpeedBuf(float duration, float value)
        {
            return new UnitStatBuf(duration, UnitBufType.ADD_SUPERARMOR)
            {
                duplicateType = BufDuplicateType.ONLY_ONE, 
                movementSpeed = value 
            };
        }
        #endregion

        #region 私有字段
        // 单位：秒；值由当前 CombatMode 决定
        private float timerInterval;

        // 低于此阈值时修改防御抗性，计算方式：maxHp * DEFENSE_MARK_RATIO
        private float hpMark;
        private float mpMark;

        private readonly Timer HealTimer = new Timer();

        // 当前战斗中的员工及其所属单位
        private WorkerModel worker;
        private UnitModel owner;

        // 当前模式，用于匹配恢复参数
        private ArmorTools.CombatMode currentMode;
        #endregion
    }
}