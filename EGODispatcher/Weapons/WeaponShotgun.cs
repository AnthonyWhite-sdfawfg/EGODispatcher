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
            this.DotDamageSettings4Shotgun = new WeaponStructs.DebufDotDamageSettings(WeaponMethods.HasImmuneDefense(target), dmgType, 10f, 20f, 0.2f);
            return base.OnAttackStart(actor, target);
		}
		public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
		{
			dmg.type = this.dmgType;
            return base.OnGiveDamage(actor, target, ref dmg);
		}
		private RwbpType dmgType;
        private WeaponStructs.DebufDotDamageSettings DotDamageSettings4Shotgun;

    }
}
