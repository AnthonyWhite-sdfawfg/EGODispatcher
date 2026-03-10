using Utils;

namespace Armors
{
    /// <summary>
    /// 护甲的统一脚本
    /// 所有数值由 ArmorConsts/ArmorStructs/ArmorMethods 提供。
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        // 初始化，启动计时器
        public override void OnStageStart()
        {
            base.OnStageStart();
            owner = model.owner;
            worker = owner as WorkerModel;
            currentMode = ArmorStructs.CombatMode.None;
            RestoreCombatParams(worker);// 取参，同时启动Timer
        }

        // 每个计时器周期的动作
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            // 判断所处职位
            ArmorStructs.CombatMode combatMode = ArmorMethods.ResolveCombatMode(worker);
            if (combatMode != currentMode)
            {
                currentMode = combatMode;
                RestoreCombatParams(worker);
            }
            // 从此处开始周期为timerInterval
            if (!HealTimer.started || !HealTimer.RunTimer())
            {
                return;
            }
            // 依据所处职位，取恢复 ratio 并进行恢复（如若需要恢复的话）
            float ratioHP;
            float ratioMental;
            ArmorStructs.CombatParams combatParams = ArmorStructs.ModeToValues[currentMode];
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
            ArmorMethods.HealThisWorker(worker, ratioHP, ratioMental);
            EnergyModel.instance.AddEnergy(1f);
            HealTimer.StartTimer(timerInterval);
        }

        // 实时取抗性，生命与精神值降低至阈值时修改
        public override DefenseInfo GetDefense(UnitModel actor)
        {
            DefenseInfo defenseInfo = base.GetDefense(actor).Copy();
            hpMark = actor.maxHp * ArmorConsts.DEFENSE_MARK_RATIO;
            mpMark = actor.maxMental * ArmorConsts.DEFENSE_MARK_RATIO;
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
            if (ArmorMethods.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(RwbpType.A, ArmorConsts.BARRIER_ON_PREPARE_VALUE, ArmorConsts.BARRIER_ON_PREPARE_DURATION));
            }
            actor.AddUnitBuf(CreateSpeedBuf());
            base.OnPrepareWeapon(actor);
        }

        public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
        {
            if (owner == null) return false;
            if (ArmorMethods.ShouldAddBarrier(actor))
            {
                actor.AddUnitBuf(new BarrierBuf(RwbpType.A, ArmorConsts.BARRIER_ON_HIT_VALUE, ArmorConsts.BARRIER_ON_HIT_DURATION));
                return false;
            }
            return base.OnTakeDamage(actor, ref dmg);
        }

        private void RestoreCombatParams(WorkerModel worker)
        {
            currentMode = ArmorMethods.ResolveCombatMode(worker);
            timerInterval = ArmorStructs.ModeToValues[currentMode].TimerInterval;
            HealTimer.StartTimer(timerInterval);
        }

        private UnitStatBuf CreateSpeedBuf()
        {
            return new UnitStatBuf(ArmorConsts.SPEED_BUF_DURATION, UnitBufType.ADD_SUPERARMOR)
            {
                duplicateType = BufDuplicateType.ONLY_ONE,
                movementSpeed = ArmorConsts.SPEED_BUF_VALUE
            };
        }


        private float timerInterval;
        private float hpMark;
        private float mpMark;
        private readonly Timer HealTimer = new Timer();
        private WorkerModel worker;
        private UnitModel owner;
        private ArmorStructs.CombatMode currentMode;
    }
}
