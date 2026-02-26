using System.Collections.Generic;

namespace Weapons
{
	public class WeaponLazyDog : EquipmentScriptBase
	{
		public override EquipmentScriptBase.WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i <= 10; i++)
			{
				list.Add(base.model.metaInfo.damageInfos[0].Copy());
			}
			return new EquipmentScriptBase.WeaponDamageInfo(base.model.metaInfo.animationNames[0], list.ToArray());
		}
		public override void OnStageStart()
		{
			base.OnStageStart();
		}
		public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
		{
			switch (UnityEngine.Random.Range(1, 5))
			{
			case 1:
				dmg.type = RwbpType.R;
				break;
			case 2:
				dmg.type = RwbpType.W;
				break;
			case 3:
				dmg.type = RwbpType.B;
				break;
			case 4:
				dmg.type = RwbpType.P;
				break;
			}
			return base.OnGiveDamage(actor, target, ref dmg);
		}
		public override void OnFixedUpdate()
		{
			base.model.owner.hp = (float)base.model.owner.maxHp;
			base.model.owner.mental = (float)base.model.owner.maxMental;
			base.OnFixedUpdate();
		}
	}
}
