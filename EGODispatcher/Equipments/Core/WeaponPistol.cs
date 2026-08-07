using System.Collections.Generic;
using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{ 
    public class WeaponPistol : EquipmentScriptBase
	{
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
            if (WeaponTools.HasImmuneDefense(target)) {
                overrideDamageType = true;
                dmgType = WeaponTools.GetWeakestDefenseType(target);
            }
            else {
                overrideDamageType = false;
            }
            List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i < 3; i++)
			{
				list.Add(base.model.metaInfo.damageInfos[0].Copy());
			}
			return new WeaponDamageInfo(base.model.metaInfo.animationNames[0], list.ToArray());
		}
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (overrideDamageType) {
                dmg.type = dmgType;
            }
            for (int i = 1; i <= 3; i++)
            {
                target.TakeDamage(dmg);
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufSlowDown(5f, 0.1f));
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }
        private RwbpType dmgType;
        private bool overrideDamageType;
    }
}
