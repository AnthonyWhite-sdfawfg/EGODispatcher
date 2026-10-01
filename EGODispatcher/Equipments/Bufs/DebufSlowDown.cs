namespace Equipments.Bufs
{
    /// <summary>
    /// Debuff：为敌对目标施加一个减速效果。
    /// <para>此 Debuff 借用了尸山 EGO"笑靥"的减速 debuff 的 UnitBufType，
    /// 因此会与尸山 EGO"笑靥"的减速 debuff 冲突：当该武器与"笑靥"的 debuff 同时出现时，
    /// 其中一方创建的 debuff 会被另一方覆盖（因为两者的 duplicateType 均为 ONLY_ONE）。</para>
    /// <para>作用对象仅限 <see cref="CreatureModel"/>：对非异想体单位不会产生任何减速效果。</para>
    /// </summary>
    public class DebufSlowDown : UnitBuf
	{
        /// <summary>
        /// 构造减速 Debuff。
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

        /// <summary>
        /// [初始化] 挂载到单位时生效：移除同源的"特殊减速"以避免叠加，
        /// 并对异想体直接缩放其移动速度倍率。
        /// </summary>
        /// <param name="model">Debuff 所挂载的单位</param>
        public override void Init(UnitModel model)
		{
			base.Init(model);
			// 与"笑靥"的特殊减速互斥，先移除已有的一份再施加本 Debuff
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
		
        /// <summary>
        /// [单位死亡] 目标死亡时自行销毁，避免残留无效 Debuff。
        /// </summary>
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}

        /// <summary>
        /// [销毁] 按相反方向还原移速倍率，避免减速效果永久残留。
        /// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (creature != null)
			{
				creature.movementScale /= _movementScale;
			}
		}

        /// <summary>被减速的异想体，非异想体目标时为 null（<see cref="OnDestroy"/> 依赖它还原倍率）</summary>
		private CreatureModel creature;
        /// <summary>减速幅度，取值范围 0-1，作用于 movementScale 的乘数</summary>
        private readonly float _movementScale;
	}
}
