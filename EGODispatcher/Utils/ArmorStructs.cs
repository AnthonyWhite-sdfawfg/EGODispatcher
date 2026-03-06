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
			},
            {
                ArmorStructs.CombatMode.Prototype,
                new ArmorStructs.CombatParams(0.5f, 0.2f, 0.2f, 0.5f, 0.5f)
            }
		};
		public struct CombatParams
		{
            /// <param name="timerInterval">恢复的时间间隔，单位为秒</param>
            /// <param name="hpNormal">正常状态下的生命恢复比率，范围为0-1，0.1对应着总生命值的10%</param>
            /// <param name="mpNormal">正常状态下的精神恢复比率，范围为0-1，0.1对应着总精神值的10%</param>
            /// <param name="hpPanic">恐慌状态下的生命恢复比率，范围为0-1，0.1对应着总生命值的10%</param>
            /// <param name="mpPanic">恐慌状态下的精神恢复比率，范围为0-1，0.1对应着总精神值的10%</param>
			public CombatParams(float timerInterval, float hpNormal, float mpNormal, float hpPanic, float mpPanic)
			{
				this.TimerInterval = timerInterval;
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
            Prototype
		}
	}
}
