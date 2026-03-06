using System;
using Bufs;
using Utils;

namespace Weapons
{
	public class WeaponShotgun : EquipmentScriptBase
	{
		public override EquipmentScriptBase.WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			this.dmgType = (RwbpType)WeaponMethods.GetWeakestDefenseType(target);
            this.DotDamageSettings4Shotgun = new WeaponStructs.DotConfig(WeaponMethods.HasImmuneDefense(target), dmgType, 10f, 20f, 0.2f);
            return base.OnAttackStart(actor, target);
		}
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            dmg.type = this.dmgType;
            for (int i = 1; i <= 5; i++)
            {
                target.TakeDamage(dmg);
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufSlowDown(2f, 0.5f));
                target.AddUnitBuf(new DebufDamageMultiply(true));
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }
        private RwbpType dmgType;
        private WeaponStructs.DotConfig DotDamageSettings4Shotgun;

    }
}
