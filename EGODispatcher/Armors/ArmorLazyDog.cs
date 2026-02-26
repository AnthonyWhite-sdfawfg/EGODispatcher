using System;

namespace Armors
{
    /// <summary>
    /// 很简单，单纯的把数值归零，时刻保持满血满精神，简单粗暴。
    /// </summary>
	public class ArmorLazyDog : EquipmentScriptBase
	{
		public override void OnStageStart()
		{
			base.OnStageStart();
		}
		public override bool OnTakeDamage(UnitModel actor, ref DamageInfo dmg)
		{
			dmg.min *= 0f;
			dmg.max *= 0f;
			base.model.owner.hp = (float)base.model.owner.maxHp;
			base.model.owner.mental = (float)base.model.owner.maxMental;
			return base.OnTakeDamage(actor, ref dmg);
		}
		public override void OnFixedUpdate()
		{
			base.model.owner.hp = (float)base.model.owner.maxHp;
			base.model.owner.mental = (float)base.model.owner.maxMental;
			base.OnFixedUpdate();
		}
	}
}
