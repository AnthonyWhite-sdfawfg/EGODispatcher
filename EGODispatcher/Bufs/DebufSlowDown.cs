using System;

namespace Bufs
{
	public class DebufSlowDown : UnitBuf
	{
		public DebufSlowDown()
		{
			this.remainTime = 1f;
			this.duplicateType = BufDuplicateType.ONLY_ONE;
			this.type = UnitBufType.DANGO_CREATURE_WEAPON_SLOW_NORMAL;
		}
		public override void Init(UnitModel model)
		{
			base.Init(model);
			this.remainTime = 1f;
			UnitBuf unitBufByType = model.GetUnitBufByType(UnitBufType.DANGO_CREATURE_WEAPON_SLOW_SPECIAL);
			if (unitBufByType != null)
			{
				model.RemoveUnitBuf(unitBufByType);
			}
			if (model is CreatureModel)
			{
				this.creature = model as CreatureModel;
				this.creature.movementScale = this.creature.movementScale * this.MovementScale();
			}
		}
		public override float MovementScale()
		{
			return 0.3f;
		}
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			this.Destroy();
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this.creature != null)
			{
				this.creature.movementScale = this.creature.movementScale / this.MovementScale();
			}
		}
		private CreatureModel creature;
	}
}
