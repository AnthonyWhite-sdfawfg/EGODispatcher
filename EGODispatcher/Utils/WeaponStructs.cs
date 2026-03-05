

namespace Utils
{
    public static class WeaponStructs
    {
        public struct DotConfig{
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
                ) {
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
    }
}

