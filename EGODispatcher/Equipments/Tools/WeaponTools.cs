namespace Equipments.Tools
{
    /// <summary>
    /// 武器公共工具类：提供"自适应破防"所需的抗性查询与 DOT 参数定义。
    /// <para>本模组所有武器脚本都通过这里的两个方法实现"目标免疫时改用其最弱抗性"的策略。</para>
    /// </summary>
    public static class WeaponTools
    {
        #region 方法

        /// <summary>
        /// 取目标的最高承伤修正比的伤害类型。
        /// <para>承伤修正比越高，代表该类型对目标造成的伤害越高，因此返回值即该目标"最弱"的抗性。</para>
        /// <para>比较顺序固定为 R → W → B → P，且采用严格大于（<c>&gt;</c>）比较，
        /// 因此数值相同时取排在前面的伤害类型。</para>
        /// </summary>
        /// <param name="target">待查询的目标单位</param>
        /// <returns>目标承伤修正比最高的伤害类型</returns>
        public static RwbpType GetWeakestDefenseType(UnitModel target)
        {
            float r = target.defense.R;
            float w = target.defense.W;
            float b = target.defense.B;
            float p = target.defense.P;
            int num = 1;
            float num2 = float.MinValue;
            if (r > num2)
            {
                num2 = r;
                num = 1;
            }
            if (w > num2)
            {
                num2 = w;
                num = 2;
            }
            if (b > num2)
            {
                num2 = b;
                num = 3;
            }
            if (p > num2)
            {
                num = 4;
            }
            return (RwbpType)num; // 游戏源码中R、W、B、P对应的枚举值为1-4
        }

        /// <summary>
        /// 检测目标是否有数值低于 0（免疫或吸收）的承伤修正比。
        /// <para>修正比为 0 表示完全免疫该类型伤害，小于 0 表示吸收并转化为治疗，
        /// 这两种情况下继续使用该伤害类型会造成伤害被抵消，需改用其它类型。</para>
        /// </summary>
        /// <param name="target">待查询的目标单位</param>
        /// <returns>四种伤害类型中只要有任意一种的承伤修正比 ≤ 0 即返回 true</returns>
		public static bool HasImmuneDefense(UnitModel target)
        {
            return target.defense.R <= 0f || target.defense.W <= 0f || target.defense.B <= 0f || target.defense.P <= 0f;
        }

        #endregion

        #region 数据结构

        /// <summary>
        /// DOT（持续伤害）参数结构体，作为构造 <c>DebufDotDamage</c> 的入参使用。
        /// <para>默认值对应"持续 10 秒、每 0.1 秒结算 20 点、使用 R 类型伤害"。</para>
        /// </summary>
        public struct DotConfig
        {
            /// <summary>
            /// 构造 DOT 参数。
            /// </summary>
            /// <param name="overrideDamageType">是否需要指定伤害类型</param>
            /// <param name="damageType">伤害类型（如需要）</param>
            /// <param name="totalDuration">该buf的持续时间，浮点数，单位为秒</param>
            /// <param name="tickDamage">每次伤害的伤害值，浮点数</param>
            /// <param name="tickRate">每次伤害的间隔时间，浮点数，单位为秒</param>
            public DotConfig(bool overrideDamageType = false,
                RwbpType damageType = RwbpType.R,
                float totalDuration = 10f,
                float tickDamage = 20f,
                float tickRate = 0.1f
                )
            {
                this.overrideDamageType = overrideDamageType;
                this.damageType = damageType;
                this.totalDuration = totalDuration;
                this.tickDamage = tickDamage;
                this.tickRate = tickRate;
            }

            /// <summary>DOT 的总持续时间，单位为秒</summary>
            public float totalDuration;
            /// <summary>每次结算造成的伤害值</summary>
            public float tickDamage;
            /// <summary>两次伤害结算之间的间隔，单位为秒</summary>
            public float tickRate;
            /// <summary>指定的伤害类型，仅在 <see cref="overrideDamageType"/> 为 true 时生效</summary>
            public RwbpType damageType;
            /// <summary>是否使用 <see cref="damageType"/> 指定伤害类型（false 时对四色伤害各结算一次）</summary>
            public bool overrideDamageType;
        }

        #endregion
    }
}
