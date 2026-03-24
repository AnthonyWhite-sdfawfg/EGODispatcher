using System;
using System.Collections.Generic;
using Bufs;
using Utils;

namespace Weapons
{
	public class WeaponChainsaw : EquipmentScriptBase
	{
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			List<DamageInfo> list = new List<DamageInfo>();
            
			if (WeaponUtils.HasImmuneDefense(target))
			{
				overrideDamageType = true;
				dmgType = WeaponUtils.GetWeakestDefenseType(target);
				for (int i = 0; i < 25; i++)
				{
					list.Add(model.metaInfo.damageInfos[0].Copy());
				}
				animationName = model.metaInfo.animationNames[1];
			}
			else
			{
				overrideDamageType = false;
				for (int j = 0; j < 6; j++)
				{
					list.Add(model.metaInfo.damageInfos[0].Copy());
				}
				animationName = model.metaInfo.animationNames[0];
			}
			return new WeaponDamageInfo(animationName, list.ToArray());
		}
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (overrideDamageType)
            {
                dmg.type = dmgType;
            }
            target.AddUnitBuf(new DebufDamageMultiply(true, 2f, 5f));
            return base.OnGiveDamage(actor, target, ref dmg);
        }
        private string animationName;
        private bool overrideDamageType;
		private RwbpType dmgType;
	}
}
