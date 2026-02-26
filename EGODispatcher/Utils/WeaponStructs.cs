

namespace Utils
{
    public static class WeaponStructs
    {
        public struct DebufDotDamageSettings{
            public DebufDotDamageSettings(bool needSpecificDamageType,
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

