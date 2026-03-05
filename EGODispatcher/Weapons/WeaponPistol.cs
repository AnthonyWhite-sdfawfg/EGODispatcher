using System;
using System.Collections.Generic;
using Bufs;
using Utils;

namespace Weapons
{
	public class WeaponPistol : EquipmentScriptBase
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
		public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
		{
			dmg.type = this.dmgType;
			return base.OnGiveDamage(actor, target, ref dmg);
		}
		private RwbpType dmgType;
        private WeaponStructs.DotConfig DotDamageSettings4Pistol;
    }
}
