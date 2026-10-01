using System.Collections.Generic;
using Creature;


namespace Equipments.Tools
{
    /// <summary>
    /// 护甲公共工具类：集中存放护甲的数值常量、战斗模式判定与治疗/敌对关系等辅助方法。
    /// <para>战斗中"职业"由员工携带武器的 ID 推导得出（见 <see cref="ResolveCombatMode"/>），
    /// 并据此查表得到恢复节奏、饰品套装等差异化的护甲效果。</para>
    /// </summary>
    public static class ArmorTools
    {

        #region 静态字段

        /// <summary>参战（OnPrepareWeapon）时附加屏障的持续时间，单位为秒</summary>
        public static readonly float BARRIER_ON_PREPARE_DURATION = 65f;
        /// <summary>参战（OnPrepareWeapon）时附加屏障的护盾值</summary>
        public static readonly float BARRIER_ON_PREPARE_VALUE = 1800f;
        /// <summary>受击（OnTakeDamage）时附加屏障的持续时间，单位为秒</summary>
        public static readonly float BARRIER_ON_HIT_DURATION = 65f;
        /// <summary>受击（OnTakeDamage）时附加屏障的护盾值</summary>
        public static readonly float BARRIER_ON_HIT_VALUE = 1800f;
        /// <summary>移速加成 Buff 的持续时间，单位为秒</summary>
        public static readonly float SPEED_BUF_DURATION = 20f;
        /// <summary>移速加成 Buff 的数值</summary>
        public static readonly float SPEED_BUF_VALUE = 180f;
        /// <summary>从武器 ID 中解析职业标识的取位基数，即取 ID 的百位数字</summary>
        public static readonly int ID_DIGIT = 100;
        /// <summary>改写承伤修正比的血量/精神阈值比例，如 0.3 表示低于最大值的 30% 时触发</summary>
        public static readonly float DEFENSE_MARK_RATIO = 0.3f;

        #endregion

        #region 数据结构

        /// <summary>
        /// 各战斗模式对应的恢复参数表，由 <see cref="Equipments.Core.ArmorUnified"/> 在战斗开始时查表使用。
        /// <para>构造参数顺序为：恢复间隔、正常状态生命恢复比率、正常状态精神恢复比率、恐慌状态生命恢复比率、恐慌状态精神恢复比率。</para>
        /// </summary>
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

        /// <summary>
        /// 战斗模式到饰品套装 ID 的映射表，供 <c>CreatureTools.DistributeGiftToAgent</c> 查表下发饰品。
        /// <para>注意：<see cref="CombatMode.Prototype"/> 与 <see cref="CombatMode.KeterCrewMember"/> 共用同一套饰品。</para>
        /// </summary>
        public static readonly Dictionary<CombatMode, int[]> CombatModeToGiftMap = new Dictionary<CombatMode, int[]>
        {
            { CombatMode.Worker, CreatureTools.GiftWorker },
            { CombatMode.Operative, CreatureTools.GiftOperative },
            { CombatMode.KeterCrewMember, CreatureTools.GiftKeterCrewMember },
            { CombatMode.Prototype, CreatureTools.GiftKeterCrewMember }, 
            { CombatMode.None, CreatureTools.GiftDefault }
        };


        /// <summary>
        /// 单个战斗模式下的恢复参数集合（只读结构体）。
        /// <para>所有恢复比率均以最大值的百分比表示，0.1 表示恢复总生命值/精神值的 10%。</para>
        /// </summary>
        public struct CombatParams
        {
            /// <summary>
            /// 构造恢复参数。
            /// </summary>
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

            /// <summary>恢复的时间间隔，单位为秒</summary>
            public readonly float TimerInterval;
            /// <summary>正常状态下的生命恢复比率</summary>
            public readonly float HpNormal;
            /// <summary>正常状态下的精神恢复比率</summary>
            public readonly float MpNormal;
            /// <summary>恐慌状态下的生命恢复比率</summary>
            public readonly float HpPanic;
            /// <summary>恐慌状态下的精神恢复比率</summary>
            public readonly float MpPanic;
        }

        /// <summary>
        /// 员工战斗模式，由所在武器的 ID 推导（见 <see cref="ResolveCombatMode"/>），
        /// 决定护甲的恢复参数与下发的饰品套装。
        /// </summary>
        public enum CombatMode
        {
            /// <summary>未识别或未持有武器，使用默认参数与默认饰品</summary>
            None,
            /// <summary>特工（武器 ID 百位为 2）</summary>
            Operative,
            /// <summary>普通员工（武器 ID 百位为 1）</summary>
            Worker,
            /// <summary>构筑部员工（武器 ID 百位为 3）</summary>
            KeterCrewMember,
            /// <summary>原型武器（武器 ID 百位为 4）</summary>
            Prototype
        }

        #endregion

        #region 方法

        /// <summary>
        /// 判断当前是否应为该单位附加屏障 Buff。
        /// <para>条件：目标是员工模型、未处于恐慌状态、且尚未持有任意屏障。</para>
        /// </summary>
        /// <param name="model">待判定的单位</param>
        /// <returns>满足条件返回 true</returns>
        public static bool ShouldAddBarrier(UnitModel model)
        {
            WorkerModel workerModel = model as WorkerModel;
            return workerModel != null && !workerModel.IsPanic() && !model.HasUnitBuf(UnitBufType.BARRIER_ALL);
        }

        /// <summary>
        /// 判断员工是否处于"可正常行动"的状态。
        /// <para>条件：非空、未死亡、位于移动路径上（currentPassage 不为空）、未恐慌、
        /// 无失控行为且可被玩家控制。</para>
        /// <para>说明：该判定当前未被仓库内其它代码调用，保留为通用状态检查工具。</para>
        /// </summary>
        /// <param name="worker">待判定的员工</param>
        /// <returns>处于正常状态返回 true</returns>
        public static bool IsNormal(WorkerModel worker)
        {
            return worker != null && !worker.IsDead() && worker.GetMovableNode().currentPassage != null && !worker.IsPanic() && worker.unconAction == null && !worker.CannotControll();
        }

        /// <summary>
        /// 按最大值的比率治疗员工的生命值与精神值。
        /// <para>已满的一项不会被治疗，避免浪费恢复量。</para>
        /// </summary>
        /// <param name="worker">待治疗的员工</param>
        /// <param name="ratioHP">生命恢复比率，0.1 表示最大生命值的 10%</param>
        /// <param name="ratioMental">精神恢复比率，0.1 表示最大精神值的 10%</param>
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

        /// <summary>
        /// 以同一比率治疗员工的生命值与精神值。
        /// </summary>
        /// <param name="worker">待治疗的员工</param>
        /// <param name="ratio">同时应用于生命值与精神值的恢复比率</param>
        public static void HealThisWorker(WorkerModel worker, float ratio)
        {
            HealThisWorker(worker, ratio, ratio);
        }

        /// <summary>
        /// 判断目标是否应被视为当前单位的敌人。
        /// <para>成立条件：目标可被攻击、非自身，且满足以下任一：拥有者本就将目标视为敌对、
        /// 员工处于恐慌状态（无差别攻击）、或目标本身是异想体。</para>
        /// <para>说明：该判定当前未被仓库内其它代码调用，保留为通用攻击目标筛选工具。</para>
        /// </summary>
        /// <param name="target">待判定的目标</param>
        /// <param name="owner">发起判定的单位</param>
        /// <param name="worker">与该单位关联的员工，可为 null</param>
        /// <returns>目标应被视为敌人时返回 true</returns>
        public static bool IsHostile(UnitModel target, UnitModel owner, WorkerModel worker)
        {
            return target != null && owner != null && target.IsAttackTargetable() && target != owner && (owner.IsHostile(target) || (worker != null && worker.IsPanic()) || target is CreatureModel);
        }

        /// <summary>
        /// 由员工当前携带的武器 ID 推导其战斗模式。
        /// <para>解析方式：取武器 ID 的百位数字，1=Worker、2=Operative、3=KeterCrewMember、4=Prototype，
        /// 其余情况返回 <see cref="CombatMode.None"/>。</para>
        /// </summary>
        /// <param name="worker">待判定的员工，可为 null</param>
        /// <returns>对应的战斗模式</returns>
        public static CombatMode ResolveCombatMode(WorkerModel worker)
        {
            if (worker == null)
            {
                return CombatMode.None;
            }
            switch (EquipmentTypeInfo.GetLcId(worker.Equipment.weapon.metaInfo).id / ArmorTools.ID_DIGIT % 10)
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
