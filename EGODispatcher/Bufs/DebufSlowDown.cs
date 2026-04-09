using System;

namespace Bufs
{
    /// <summary>
    /// Debuff，为敌对目标施加一个减速效果。
    /// 此Debuff借用了尸山EGO“笑靥”的减速debuff的UnitBufType，
    /// 因此会导致与尸山EGO“笑靥”的减速debuff冲突，
    /// 从而出现该武器及“笑靥”两者的debuff同时出现时，
    /// 其中一方创建的debuff会被另一方覆盖。
    /// </summary>
    public class DebufSlowDown : UnitBuf
	{
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
				model.RemoveUnitBuf(unitBufByType); // 此处会移除原有的debuff
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
