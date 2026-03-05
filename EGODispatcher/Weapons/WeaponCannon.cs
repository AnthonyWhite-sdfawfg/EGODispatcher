using Utils;
using Bufs;

namespace EGODispatcher.Weapons
{
    class WeaponCannon : EquipmentScriptBase
    {
        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            this.overrideDamageType = WeaponMethods.HasImmuneDefense(target);
            this.dmgType = (RwbpType)WeaponMethods.GetWeakestDefenseType(target);
            this.dotConfigCannon = new WeaponStructs.DotConfig(overrideDamageType, dmgType, 10f, 20f, 0.1f);
            return base.OnAttackStart(actor, target);
        }
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (overrideDamageType) {
                dmg.type = this.dmgType;
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufDotDamage(dotConfigCannon));
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }

        bool overrideDamageType;
        private RwbpType dmgType;
        private WeaponStructs.DotConfig dotConfigCannon;
    }
}
