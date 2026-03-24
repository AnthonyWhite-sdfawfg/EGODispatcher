using System;
using System.Collections.Generic;


namespace Utils
{
    public static class ArmorUtils
    {

        #region 静态字段

        public static readonly float BARRIER_ON_PREPARE_DURATION = 65f;
        public static readonly float BARRIER_ON_PREPARE_VALUE = 1800f;
        public static readonly float BARRIER_ON_HIT_DURATION = 65f;
        public static readonly float BARRIER_ON_HIT_VALUE = 1800f;
        public static readonly float SPEED_BUF_DURATION = 20f;
        public static readonly float SPEED_BUF_VALUE = 180f;
        public static readonly int ID_DIGIT = 100;
        public static readonly float DEFENSE_MARK_RATIO = 0.3f;

        #endregion

        #region 数据结构

        public static readonly Dictionary<CombatMode, CombatParams> ModeToValues = new Dictionary<CombatMode, CombatParams>
        {
            {
                CombatMode.Operative,
                new CombatParams(0.5f, 0.1f, 0.2f, 0.2f, 0.5f)
            },
            {
                CombatMode.Worker,
                new CombatParams(1f, 0.1f, 0.2f, 0.2f, 0.5f)
            },
            {
                CombatMode.KeterCrewMember,
                new CombatParams(0.1f, 0.1f, 0.2f, 0.2f, 0.5f)
            },
            {
                CombatMode.None,
                new CombatParams(1f, 0.1f, 0.1f, 0.2f, 0.2f)
            },
            {
                CombatMode.Prototype,
                new CombatParams(0.5f, 0.2f, 0.2f, 0.5f, 0.5f)
            }
        };

        public struct CombatParams
        {
            /// <param name="timerInterval">恢复的时间间隔，单位为秒</param>
            /// <param name="hpNormal">正常状态下的生命恢复比率，范围为0-1，0.1对应着总生命值的10%</param>
            /// <param name="mpNormal">正常状态下的精神恢复比率，范围为0-1，0.1对应着总精神值的10%</param>
            /// <param name="hpPanic">恐慌状态下的生命恢复比率，范围为0-1，0.1对应着总生命值的10%</param>
            /// <param name="mpPanic">恐慌状态下的精神恢复比率，范围为0-1，0.1对应着总精神值的10%</param>
			public CombatParams(float timerInterval, float hpNormal, float mpNormal, float hpPanic, float mpPanic)
            {
                TimerInterval = timerInterval;
                HpNormal = hpNormal;
                MpNormal = mpNormal;
                HpPanic = hpPanic;
                MpPanic = mpPanic;
            }
            public readonly float TimerInterval;
            public readonly float HpNormal;
            public readonly float MpNormal;
            public readonly float HpPanic;
            public readonly float MpPanic;
        }

        public enum CombatMode
        {
            None,
            Operative,
            Worker,
            KeterCrewMember,
            Prototype
        }

        #endregion

        #region 方法

        public static bool ShouldAddBarrier(UnitModel model)
        {
            WorkerModel workerModel = model as WorkerModel;
            return workerModel != null && !workerModel.IsPanic() && !model.HasUnitBuf(UnitBufType.BARRIER_ALL);
        }

        public static bool IsNormal(WorkerModel worker)
        {
            return worker != null && !worker.IsDead() && worker.GetMovableNode().currentPassage != null && !worker.IsPanic() && worker.unconAction == null && !worker.CannotControll();
        }

        public static void HealThisWorker(WorkerModel worker, float ratioHP, float ratioMental)
        {
            if (worker != null && !worker.IsDead())
            {
                float num = (float)worker.maxHp * ratioHP;
                float num2 = (float)worker.maxMental * ratioMental;
                if (worker.hp < (float)worker.maxHp)
                {
                    worker.RecoverHP(num);
                }
                if (worker.mental < (float)worker.maxMental)
                {
                    worker.RecoverMental(num2);
                }
            }
        }

        public static void HealThisWorker(WorkerModel worker, float ratio)
        {
            HealThisWorker(worker, ratio, ratio);
        }

        public static bool IsHostile(UnitModel target, UnitModel owner, WorkerModel worker)
        {
            return target != null && owner != null && target.IsAttackTargetable() && target != owner && (owner.IsHostile(target) || (worker != null && worker.IsPanic()) || target is CreatureModel);
        }

        public static CombatMode ResolveCombatMode(WorkerModel worker)
        {
            if (worker == null)
            {
                return CombatMode.None;
            }
            switch (EquipmentTypeInfo.GetLcId(worker.Equipment.weapon.metaInfo).id / ArmorUtils.ID_DIGIT % 10)
            {
                case 1:
                    return CombatMode.Worker;
                case 2:
                    return CombatMode.Operative;
                case 3:
                    return CombatMode.KeterCrewMember;
                case 4:
                    return CombatMode.Prototype;
                default:
                    return CombatMode.None;
            }
        }

        #endregion

    }
}
