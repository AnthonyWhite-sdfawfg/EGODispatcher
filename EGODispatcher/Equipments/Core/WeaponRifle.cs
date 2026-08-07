using System.Collections.Generic;
using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{
	public class WeaponRifle : EquipmentScriptBase
	{
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			dmgType = WeaponTools.GetWeakestDefenseType(target);
			List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i < 3; i++)
			{
				list.Add(model.metaInfo.damageInfos[0].Copy());
			}
			return new WeaponDamageInfo(model.metaInfo.animationNames[0], list.ToArray());
		}
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            dmg.type = dmgType;
            return base.OnGiveDamage(actor, target, ref dmg);
        }
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            if (target.hp > 0)
            {
                target.hp -= 1f;
            }
            target.AddUnitBuf(new DebufDamageMultiply(false, 2f, 5f));
            base.OnGiveDamageAfter(actor, target, dmg);
        }

        private RwbpType dmgType;
	}
}
