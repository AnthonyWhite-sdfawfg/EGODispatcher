using Utils;

namespace Armors
{
    /// <summary>
    /// 周期恢复
    /// 修改抗性
    /// 受击、参战时获得护盾
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

        // 以计时器的周期对员工进行恢复
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

        // 员工生命值 / 精神值低于阈值则修改对应抗性
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

        // 进入战斗时添加护盾、添加增速buf
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

        // 受击时添加护盾
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

        private float timerInterval;

        private float hpMark;

        private float mpMark;

        private readonly Timer HealTimer = new Timer();

        private WorkerModel worker;

        private UnitModel owner;

        private ArmorUtils.CombatMode currentMode;

        #endregion
    }
}