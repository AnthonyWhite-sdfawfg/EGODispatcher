using System;

namespace Utils
{
    public static class ArmorMethods
    {
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
            ArmorMethods.HealThisWorker(worker, ratio, ratio);
        }
        public static bool IsHostile(UnitModel target, UnitModel owner, WorkerModel worker)
        {
            return target != null && owner != null && target.hp > 0f && target.IsAttackTargetable() && target != owner && (owner.IsHostile(target) || (worker != null && worker.IsPanic()) || target is CreatureModel);
        }
        public static ArmorStructs.CombatMode ResolveCombatMode(WorkerModel worker)
        {
            if (worker == null)
            {
                return ArmorStructs.CombatMode.None;
            }
            switch (EquipmentTypeInfo.GetLcId(worker.Equipment.weapon.metaInfo).id / ArmorConsts.ID_DIGIT % 10)
            {
                case 1:
                    return ArmorStructs.CombatMode.Worker;
                case 2:
                    return ArmorStructs.CombatMode.Operative;
                case 3:
                    return ArmorStructs.CombatMode.KeterCrewMember;
                case 4:
                    return ArmorStructs.CombatMode.Prototype;
                default:
                    return ArmorStructs.CombatMode.None;
            }
        }
        public static bool HasNoHostileTargets(PassageObjectModel passage, UnitModel owner, WorkerModel worker)
        {
            if (passage == null) return true;
            foreach (MovableObjectNode target in passage.GetEnteredTargets(owner.GetMovableNode()))
            {
                if (ArmorMethods.IsHostile(target.GetUnit(), owner, worker))
                    return false;
            }
            return true;
        }
        public static float GetCombatTimerInterval( ArmorStructs.CombatMode mode)
        {
            return ArmorStructs.ModeToValues[mode].TimerInterval;
        }
    }
}
