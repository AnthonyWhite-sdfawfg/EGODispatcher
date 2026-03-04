using System;
using System.Collections.Generic;
using Bufs;
using Utils;

namespace Weapons
{
	public class WeaponChainsaw : EquipmentScriptBase
	{
		public override EquipmentScriptBase.WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			List<DamageInfo> list = new List<DamageInfo>();
			string animationName = string.Empty;
			if (WeaponMethods.HasImmuneDefense(target))
			{
				this.dmgFlag = true;
				this.dmgType = (RwbpType)WeaponMethods.GetWeakestDefenseType(target);
				for (int i = 0; i < 25; i++)
				{
					list.Add(base.model.metaInfo.damageInfos[0].Copy());
				}
				animationName = base.model.metaInfo.animationNames[1];
			}
			else
			{
				this.dmgFlag = false;
				for (int j = 0; j < 6; j++)
				{
					list.Add(base.model.metaInfo.damageInfos[0].Copy());
				}
				animationName = base.model.metaInfo.animationNames[0];
			}
			return new EquipmentScriptBase.WeaponDamageInfo(animationName, list.ToArray());
		}
		public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
		{
			if (this.dmgFlag)
			{
				dmg.type = this.dmgType;
			}
			return base.OnGiveDamage(actor, target, ref dmg);
		}
		private bool dmgFlag;
		private RwbpType dmgType;
	}
}
