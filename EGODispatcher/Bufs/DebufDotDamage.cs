using Utils;

namespace Bufs
{
	public class DebufDotDamage : UnitBuf
	{
        public DebufDotDamage(WeaponUtils.DotConfig config)
        {
            _overrideDamageType = config.overrideDamageType;
            _totalDuration = config.totalDuration;
            _tickRate = config.tickRate;
            _damageType = config.damageType;
            _tickDamage = config.tickDamage;

            tickTimer.StartTimer(_tickRate);
            duplicateType = BufDuplicateType.ONLY_ONE;
            type = UnitBufType.ADD_SUPERARMOR;

        }

        public override void Init(UnitModel model)
		{
			base.Init(model);
			remainTime = _totalDuration;
		}

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
					for (int j = 0; j < 4; j++)
					{
						model.TakeDamage(new DamageInfo(_damageType, _tickDamage));
					}
				}
				else
				{
					model.TakeDamage(new DamageInfo(RwbpType.R, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.W, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.B, _tickDamage));
					model.TakeDamage(new DamageInfo(RwbpType.P, _tickDamage));
				}
				tickTimer.StartTimer(_tickRate);
			}
		}

		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}

		private readonly Timer tickTimer = new Timer();
        private readonly bool _overrideDamageType;
        private readonly float _totalDuration;
        private readonly float _tickDamage;
        private readonly float _tickRate;
        private readonly RwbpType _damageType;

	}
}
