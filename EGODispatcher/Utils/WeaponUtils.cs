namespace Utils
{
    public static class WeaponUtils
    {

        #region 方法

        /// <summary>
        /// 取目标的最弱抗性(数值越大越弱，越小越强，0代表免疫，负值代表吸收)
        /// </summary>
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
        /// 检测目标是否有数值低于0（免疫或吸收）的抗性
        /// </summary>
		public static bool HasImmuneDefense(UnitModel target)
        {
            return target.defense.R <= 0f || target.defense.W <= 0f || target.defense.B <= 0f || target.defense.P <= 0f;
        }

        #endregion

        #region 数据结构

        public struct DotConfig
        {
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
            public float totalDuration;
            public float tickDamage;
            public float tickRate;
            public RwbpType damageType;
            public bool overrideDamageType;
        }

        #endregion
    }
}
