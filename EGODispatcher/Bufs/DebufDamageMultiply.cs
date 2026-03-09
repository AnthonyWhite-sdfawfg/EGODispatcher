using System;

namespace Bufs
{
	public class DebufDamageMultiply : UnitBuf
	{
		public DebufDamageMultiply(bool reproducible, float multiply = 1.5f)
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
			remainTime = 5f;
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
	}
}
