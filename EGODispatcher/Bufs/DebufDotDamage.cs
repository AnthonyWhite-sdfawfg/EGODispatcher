using System;
using Utils;

namespace Bufs
{
	public class DebufDotDamage : UnitBuf
	{
        public DebufDotDamage(WeaponStructs.DotConfig config)
        {
            this._overrideDamageType = config.overrideDamageType;
            this._totalDuration = config.totalDuration;
            this._tickRate = config.tickRate;
            this._damageType = config.damageType;
            this._tickDamage = config.tickDamage;

            this.tickTimer.StartTimer(_tickRate);
            this.duplicateType = BufDuplicateType.ONLY_ONE;
            this.type = UnitBufType.ADD_SUPERARMOR;

        }

        public override void Init(UnitModel model)
		{
			base.Init(model);
			this.remainTime = _totalDuration;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.model.hp <= 0f)
			{
				return;
			}
			if (this.tickTimer.RunTimer())
			{
				if (_overrideDamageType)
				{
					for (int j = 0; j < 4; j++)
					{
						this.model.TakeDamage(new DamageInfo(this._damageType, _tickDamage));
					}
				}
				else
				{
					this.model.TakeDamage(new DamageInfo(RwbpType.R, _tickDamage));
					this.model.TakeDamage(new DamageInfo(RwbpType.W, _tickDamage));
					this.model.TakeDamage(new DamageInfo(RwbpType.B, _tickDamage));
					this.model.TakeDamage(new DamageInfo(RwbpType.P, _tickDamage));
				}
				this.tickTimer.StartTimer(_tickRate);
			}
		}

		public override void OnUnitDie()
		{
			base.OnUnitDie();
			this.Destroy();
		}

		private Timer tickTimer = new Timer();
        private bool _overrideDamageType;
        private float _totalDuration;
        private float _tickDamage;
        private float _tickRate;
        private RwbpType _damageType;

	}
}
