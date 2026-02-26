using System;
using Utils;

namespace Bufs
{
	public class DebufDotDamage : UnitBuf
	{
		public override void Init(UnitModel model)
		{
			base.Init(model);
			this.remainTime = _remainTime;
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
				if (_needSpecificDamageType)
				{
					for (int j = 0; j < 4; j++)
					{
						this.model.TakeDamage(new DamageInfo(this._dmgType, _dmgValue));
					}
				}
				else
				{
					this.model.TakeDamage(new DamageInfo(RwbpType.R, _dmgValue));
					this.model.TakeDamage(new DamageInfo(RwbpType.W, _dmgValue));
					this.model.TakeDamage(new DamageInfo(RwbpType.B, _dmgValue));
					this.model.TakeDamage(new DamageInfo(RwbpType.P, _dmgValue));
				}
				this.tickTimer.StartTimer(_timeInterval);
			}
		}
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			this.Destroy();
		}
        public DebufDotDamage(WeaponStructs.DebufDotDamageSettings settings)
		{
            this._needSpecificDamageType = settings.needSpecificDamageType;
            this._remainTime = settings.remainTime;
            this._timeInterval = settings.timeInterval;
            this._dmgType = settings.dmgType;
            this._dmgValue = settings.dmgValue;

            this.tickTimer.StartTimer(_timeInterval);
			this.duplicateType = BufDuplicateType.ONLY_ONE;
			this.type = UnitBufType.ADD_SUPERARMOR;

		}
		private Timer tickTimer = new Timer();
        private bool _needSpecificDamageType;
        private float _remainTime;
        private float _dmgValue;
        private float _timeInterval;
        private RwbpType _dmgType;


	}
}
