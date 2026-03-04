using System;
using Utils;

namespace Armors
{
    /// <summary>
    /// 护甲的统一脚本，游戏中两个颜色的护甲只有数值上的差异，机制相同
    /// 所有数值由 ArmorConsts/ArmorStructs/ArmorMethods 提供。
    /// </summary>
    public class ArmorUnified : EquipmentScriptBase
    {
        // 初始化，启动计时器
        public override void OnStageStart()
        {
            base.OnStageStart();
            this.owner = base.model.owner;
            this.worker = this.owner as WorkerModel;
            this.currentMode = ArmorStructs.CombatMode.None;
            this.RestoreCombatParams(this.worker);// 取参
        }

        // 每个计时器周期的动作
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            // 判断所处职位
            ArmorStructs.CombatMode combatMode = ArmorMethods.ResolveCombatMode(this.worker);
            if (combatMode != this.currentMode)
            {
                this.currentMode = combatMode;
                this.RestoreCombatParams(this.worker);
            }
            // 从此处开始为计时器周期
            if (!this.HealTimer.started || !this.HealTimer.RunTimer())
            {
                return;
            }
            // 依据所处职位，取恢复 ratio 并进行恢复（如若需要恢复的话）
            float ratio;
            float ratio2;
            ArmorStructs.CombatParams combatParams = ArmorStructs.ModeToValues[this.currentMode];
            if (this.worker.IsPanic())
            {
                ratio = combatParams.HpPanic;
                ratio2 = combatParams.MpPanic;
            }
            else
            {
                ratio = combatParams.HpNormal;
                ratio2 = combatParams.MpNormal;
            }
            ArmorMethods.HealThisWorker(this.worker, ratio, ratio2);
            EnergyModel.instance.AddEnergy(1f);
            this.HealTimer.StartTimer(this.timerInterval);
        }

        // 实时取抗性，生命与精神值降低至阈值时修改
        public override DefenseInfo GetDefense(UnitModel actor)
        {
            DefenseInfo defenseInfo = base.GetDefense(actor).Copy();
            this.hpMark = (float)actor.maxHp * ArmorConsts.DEFENSE_MARK_RATIO;
            this.mpMark = (float)actor.maxMental * ArmorConsts.DEFENSE_MARK_RATIO;
            if (actor.hp < this.hpMark)
            {
                defenseInfo.R = 0f;
                defenseInfo.P = 0f;
            }
            if (actor.mental < this.mpMark)
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
            this.currentMode = ArmorMethods.ResolveCombatMode(worker);
            this.timerInterval = ArmorMethods.GetCombatTimerInterval(this.currentMode);
            this.HealTimer.StartTimer(this.timerInterval);
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
