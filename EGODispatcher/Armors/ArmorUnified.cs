using Utils;

namespace Armors
{
    /// <summary>
    /// 护甲核心逻辑统一管理脚本
    /// 1. 根据员工战斗参数（CombatMode）设定恢复的周期与比例；
    /// 2. 生命值/精神值低于阈值时修改防御属性；
    /// 3. 武器准备/受击时触发屏障、移速加成等护甲特有效果；
    /// 4. 所有数值常量/结构体/工具方法依赖 ArmorUtils 定义。
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        #region 钩子
        public override void OnStageStart()
        {
            base.OnStageStart();
            owner = model.owner;
            worker = owner as WorkerModel;
            currentMode = ArmorUtils.CombatMode.None;
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
            ArmorUtils.CombatParams combatParams = ArmorUtils.ModeToValues[currentMode];
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
            ArmorUtils.HealThisWorker(worker, ratioHP, ratioMental);
            HealTimer.StartTimer(timerInterval);
        }

        public override DefenseInfo GetDefense(UnitModel actor)
        {
            DefenseInfo defenseInfo = base.GetDefense(actor).Copy();

            hpMark = actor.maxHp * ArmorUtils.DEFENSE_MARK_RATIO;
            mpMark = actor.maxMental * ArmorUtils.DEFENSE_MARK_RATIO;

            if (actor.hp < hpMark)
            {
                defenseInfo.R = 0f;
                defenseInfo.P = 0f;
            }
            if (actor.mental < mpMark)
            {
                defenseInfo.W = -0.1f;
                defenseInfo.B = -0.1f;
            }

            return defenseInfo;
        }

        public override void OnPrepareWeapon(UnitModel actor)
        {
            if (ArmorUtils.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorUtils.BARRIER_ON_PREPARE_VALUE,
                    ArmorUtils.BARRIER_ON_PREPARE_DURATION
                ));
            }
            actor.AddUnitBuf(CreateSpeedBuf(ArmorUtils.SPEED_BUF_DURATION, ArmorUtils.SPEED_BUF_VALUE));
            base.OnPrepareWeapon(actor);
        }

        public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            if (owner == null) return false;
            if (ArmorUtils.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(
                    RwbpType.A,
                    ArmorUtils.BARRIER_ON_HIT_VALUE,
                    ArmorUtils.BARRIER_ON_HIT_DURATION
                ));
                return false;
            }
            return base.OnTakeDamage(actor, ref dmg);
        }
        #endregion

        #region 私有工具方法

        private void SetCombatParams(WorkerModel worker)
        {
            currentMode = ArmorUtils.ResolveCombatMode(worker);
            timerInterval = ArmorUtils.ModeToValues[currentMode].TimerInterval;
            HealTimer.StartTimer(timerInterval);
        }

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
        private ArmorUtils.CombatMode currentMode;
        #endregion
    }
}