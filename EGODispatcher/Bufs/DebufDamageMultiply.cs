using System;

namespace Bufs
{
	public class DebufDamageMultiply : UnitBuf
	{
        /// <param name="reproducible">是否可以堆叠；</param>
        /// <param name="multiply">倍率，1.5对应着原伤害的150%；</param>
        /// <param name="duration">该buf的持续时间，单位为秒</param>
		public DebufDamageMultiply(bool reproducible = false, float multiply = 1.5f, float duration = 5f)
		{
            type = UnitBufType.ADD_SUPERARMOR;
            if (reproducible) {
                duplicateType = BufDuplicateType.UNLIMIT;
            } else {
                duplicateType = BufDuplicateType.ONLY_ONE;
            }
            this.multiply = multiply;
			
		}
		public override void Init(UnitModel model)
		{
			base.Init(model);
			remainTime = duration;
		}
		public override float OnTakeDamage(UnitModel attacker, DamageInfo damageInfo)
		{
			return multiply;
		}
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}

        private float multiply;
        private float duration;
	}
}
