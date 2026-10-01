using Equipments.Tools;

namespace Equipments.Bufs
{
    /// <summary>
    /// Debuff：为敌对目标施加一个 DOT（持续伤害）效果，以 config 结构体作为参数传入。
    /// <para>结算方式：每 <c>tickRate</c> 秒触发一次，直到 <c>totalDuration</c> 结束。</para>
    /// <para>指定伤害类型时，每次触发造成 2 段该类型的伤害并额外直接扣除等量生命值；
    /// 未指定时，每次触发对 R/W/B/P 四种伤害类型各结算一次。</para>
    /// </summary>
	public class DebufDotDamage : UnitBuf
	{
        /// <summary>
        /// 构造 DOT Debuff。
        /// </summary>
        /// <param name="config">DOT 伤害相关参数，以 struct 的形式传入</param>
        public DebufDotDamage(WeaponTools.DotConfig config)
        {
            _overrideDamageType = config.overrideDamageType;
            _totalDuration = config.totalDuration;
            _tickRate = config.tickRate;
            _damageType = config.damageType;
            _tickDamage = config.tickDamage;

            tickTimer.StartTimer(_tickRate);
            // ONLY_ONE：同一目标上只保留一份 DOT，重复命中会覆盖而非叠层
            duplicateType = BufDuplicateType.ONLY_ONE;
            // 复用 ADD_SUPERARMOR 槽位承载本 Debuff（游戏原版无对应的独立枚举位）
            type = UnitBufType.ADD_SUPERARMOR;

        }

        /// <summary>
        /// [初始化] 由父类在挂载到单位时调用，用总持续时间初始化剩余时间。
        /// </summary>
        /// <param name="model">Debuff 所挂载的单位</param>
        public override void Init(UnitModel model)
		{
			base.Init(model);
			remainTime = _totalDuration;
		}

        /// <summary>
        /// [每帧结算] 按 tickRate 周期性地对目标造成伤害。
        /// <para>目标已死亡时直接跳过，避免对尸体继续结算。</para>
        /// </summary>
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (model.hp <= 0f)
			{
				return;
			}
			if (tickTimer.RunTimer())
			{
				if (_overrideDamageType)
				{
					// 已指定伤害类型：每次触发结算 2 段，并额外直接扣血以强化 DOT 收益
					for (int j = 0; j < 2; j++)
					{
                        model.TakeDamage(new DamageInfo(_damageType, _tickDamage));
                        if (model.hp>0) {
                            model.hp -= _tickDamage;
                        }

					}
				}
				else
				{
					// 未指定伤害类型：四色齐打，用于绕过单一抗性较高的目标
					model.TakeDamage(new DamageInfo(RwbpType.R, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.W, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.B, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.P, _tickDamage));
				}
				tickTimer.StartTimer(_tickRate);
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

        /// <summary>伤害结算计时器，用于控制每段伤害的间隔</summary>
		private readonly Timer tickTimer = new Timer();
        /// <summary>是否已指定固定伤害类型（false 时四色齐打）</summary>
        private readonly bool _overrideDamageType;
        /// <summary>Debuff 总持续时间，单位为秒</summary>
        private readonly float _totalDuration;
        /// <summary>每次触发结算的伤害值</summary>
        private readonly float _tickDamage;
        /// <summary>两次伤害结算之间的间隔，单位为秒</summary>
        private readonly float _tickRate;
        /// <summary>指定的伤害类型，仅在 <see cref="_overrideDamageType"/> 为 true 时有效</summary>
        private readonly RwbpType _damageType;

	}
}
