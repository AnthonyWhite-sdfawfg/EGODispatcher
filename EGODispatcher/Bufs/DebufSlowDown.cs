using System;

namespace Bufs
{
    public class DebufSlowDown : UnitBuf
	{
        /// <summary>
        /// 此处借用了尸山EGO“笑靥”的减速debuf的UnitBufType，
        /// 因此会导致与尸山EGO“笑靥”的减速debuf冲突从而出现该武器及“笑靥”两者的debuf同时出现时，
        /// 其中一方创建的debuf会被另一方覆盖。（因为两者的duplicateType均为ONLY_ONE）
        /// </summary>

        /// <param name="remainTime">该buf的持续时间，单位为秒</param>
        /// <param name="movementScale">该buf的减速效果，取值范围0-1，0.3表示30%</param>
        public DebufSlowDown(float remainTime = 1f, float movementScale = 0.3f)
		{
			this.remainTime = remainTime;
            _movementScale = movementScale;
			duplicateType = BufDuplicateType.ONLY_ONE;
			type = UnitBufType.DANGO_CREATURE_WEAPON_SLOW_NORMAL;
           
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
				creature = model as CreatureModel;
				creature.movementScale *= _movementScale;
			}
		}
		
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (creature != null)
			{
				creature.movementScale /= _movementScale;
			}
		}
		private CreatureModel creature;
        private readonly float _movementScale;
	}
}
