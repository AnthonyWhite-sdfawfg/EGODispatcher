using System;

namespace Bufs
{
    public class DebufSlowDown : UnitBuf
	{
        /// <summary>
        /// 此处只能借用尸山EGO“笑靥”的减速debuf的UnitBufType，
        /// 因此会导致与尸山EGO“笑靥”的减速debuf冲突从而出现该武器及“笑靥”两者的debuf同时出现时，
        /// 其中一方创建的debuf会被另一方覆盖。（因为两者的duplicateType均为ONLY_ONE）
        /// </summary>

        /// <param name="remainTime">该buf的持续时间，单位为秒</param>
        /// <param name="MovementScale">该buf的减速效果，取值范围0-1，0.3表示30%</param>
        public DebufSlowDown(float remainTime = 1f, float movementScale = 0.3f)
		{
			this.remainTime = remainTime;
            this._movementScale = movementScale;
			this.duplicateType = BufDuplicateType.ONLY_ONE;
			this.type = UnitBufType.DANGO_CREATURE_WEAPON_SLOW_NORMAL;
           
        }
        public override void Init(UnitModel model)
		{
			base.Init(model);
			UnitBuf unitBufByType = model.GetUnitBufByType(UnitBufType.DANGO_CREATURE_WEAPON_SLOW_SPECIAL);
			if (unitBufByType != null)
			{
				model.RemoveUnitBuf(unitBufByType);
			}
			if (model is CreatureModel)
			{
				this.creature = model as CreatureModel;
				this.creature.movementScale = this.creature.movementScale * this._movementScale;
			}
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
				this.creature.movementScale = this.creature.movementScale / this._movementScale;
			}
		}
		private CreatureModel creature;
        private float _movementScale;
	}
}
