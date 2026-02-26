using System;
using System.Collections.Generic;

namespace Utils
{
	public static class ArmorStructs
	{
		public static readonly Dictionary<ArmorStructs.CombatMode, ArmorStructs.CombatParams> ModeToValues = new Dictionary<ArmorStructs.CombatMode, ArmorStructs.CombatParams>
		{
			{
				ArmorStructs.CombatMode.Operative,
				new ArmorStructs.CombatParams(0.5f, 0.1f, 0.2f, 0.2f, 0.5f)
			},
			{
				ArmorStructs.CombatMode.Worker,
				new ArmorStructs.CombatParams(1f, 0.1f, 0.2f, 0.2f, 0.5f)
			},
			{
				ArmorStructs.CombatMode.KeterCrewMember,
				new ArmorStructs.CombatParams(0.1f, 0.1f, 0.2f, 0.2f, 0.5f)
			},
			{
				ArmorStructs.CombatMode.None,
				new ArmorStructs.CombatParams(1f, 0.1f, 0.1f, 0.2f, 0.2f)
			}
		};
		public struct CombatParams
		{
			public CombatParams(float timerInterval, float hpNormal, float mpNormal, float hpPanic, float mpPanic)
			{
				this.TimerInterval = timerInterval;//单位为秒，下面四条单位均为血量/精神的比值（0-1为0%-100%）
				this.HpNormal = hpNormal;
				this.MpNormal = mpNormal;
				this.HpPanic = hpPanic;
				this.MpPanic = mpPanic;
			}
			public readonly float TimerInterval;
			public readonly float HpNormal;
			public readonly float MpNormal;
			public readonly float HpPanic;
			public readonly float MpPanic;
		}
		public enum CombatMode
		{
			None,
			Operative,
			Worker,
			KeterCrewMember,
		}
	}
}
