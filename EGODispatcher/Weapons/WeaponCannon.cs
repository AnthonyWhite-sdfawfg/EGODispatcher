using Utils;

namespace EGODispatcher.Weapons
{
    class WeaponCannon : EquipmentScriptBase
    {
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (WeaponMethods.HasImmuneDefense(target)) {
                dmg.type = (RwbpType)WeaponMethods.GetWeakestDefenseType(target);
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            base.OnGiveDamageAfter(actor, target, dmg);
        }
    }
}
