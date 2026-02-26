using System;
using System.Collections.Generic;
using Bufs;
using Utils;

namespace Weapons
{
	public class WeaponRifle : EquipmentScriptBase
	{
		public override EquipmentScriptBase.WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			this.dmgType = (RwbpType)WeaponMethods.GetWeakestDefenseType(target);
			List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i < 3; i++)
			{
				list.Add(base.model.metaInfo.damageInfos[0].Copy());
			}
			return new EquipmentScriptBase.WeaponDamageInfo(base.model.metaInfo.animationNames[0], list.ToArray());
		}
		public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
		{
			base.OnGiveDamageAfter(actor, target, dmg);
            target.AddUnitBuf(new DebufDamageMultiply(false));
            if (target.hp > 0f)
			{
				target.AddUnitBuf(new DebufDotDamage(new WeaponStructs.DebufDotDamageSettings()));
			}
		}
		private RwbpType dmgType;
	}
}
