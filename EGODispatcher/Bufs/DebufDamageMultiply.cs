using System;

namespace Bufs
{
	public class DebufDamageMultiply : UnitBuf
	{
		public DebufDamageMultiply(bool reproducible)
		{
            type = UnitBufType.ADD_SUPERARMOR;
            if (reproducible) {
                duplicateType = BufDuplicateType.UNLIMIT;
            } else {
                duplicateType = BufDuplicateType.ONLY_ONE;
            }
			
		}
		public override void Init(UnitModel model)
		{
			base.Init(model);
			remainTime = 5f;
		}
		public override float OnTakeDamage(UnitModel attacker, DamageInfo damageInfo)
		{
			return 1.5f;
		}
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}
	}
}
