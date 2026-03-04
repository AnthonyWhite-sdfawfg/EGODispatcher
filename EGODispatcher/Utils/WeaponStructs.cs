

namespace Utils
{
    public static class WeaponStructs
    {
        public struct DebufDotDamageSettings{
            /// <param name="needSpecificDamageType">是否需要指定伤害类型</param>
            /// <param name="dmgType">伤害类型（如需要）</param>
            /// <param name="remainTime">该buf的持续时间，单位为秒</param>
            /// <param name="dmgValue">每次伤害的伤害值</param>
            /// <param name="timeInterval">每次伤害的间隔时间，单位为秒</param>
            public DebufDotDamageSettings(bool needSpecificDamageType = false,
                RwbpType dmgType = RwbpType.R,
                float remainTime = 10f,
                float dmgValue = 20f,
                float timeInterval = 0.1f
                ) {
                this.needSpecificDamageType = needSpecificDamageType;
                this.dmgType = dmgType;
                this.remainTime = remainTime;
                this.dmgValue = dmgValue;
                this.timeInterval = timeInterval;
            }
            public float remainTime;
            public float dmgValue;
            public float timeInterval;
            public RwbpType dmgType;
            public bool needSpecificDamageType;
        }
    }
}

