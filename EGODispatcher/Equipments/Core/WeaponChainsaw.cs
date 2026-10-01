using System.Collections.Generic;
using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{
	/// <summary>
	/// [链锯] 高段数近战型 EGO 武器脚本。
	/// <para>特点：命中时施加 2 倍易伤（可叠加）并固定扣除 10 点生命值，是单体爆发最高的武器。</para>
	/// <para>双形态：目标<b>无</b>免疫抗性时使用 6 段伤害与动画 0；目标<b>存在</b>免疫抗性时切换为 25 段伤害、
	/// 改用目标最弱抗性并播放动画 1，用于强行突破高抗性目标。</para>
	/// </summary>
	public class WeaponChainsaw : EquipmentScriptBase
	{
		/// <summary>
		/// [攻击开始] 检测目标抗性并据此在"常规形态"与"破防形态"之间切换。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <returns>对应形态的动画名与伤害段数</returns>
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{

			List<DamageInfo> list = new List<DamageInfo>();
			if (WeaponTools.HasImmuneDefense(target))
			{
				// 破防形态：25 段 + 最弱抗性 + 动画 1
				overrideDamageType = true;
				dmgType = WeaponTools.GetWeakestDefenseType(target);
				for (int i = 0; i < 25; i++)
				{
					list.Add(model.metaInfo.damageInfos[0].Copy());
				}
				animationName = model.metaInfo.animationNames[1];
			}
			else
			{
				// 常规形态：6 段 + 武器原伤害类型 + 动画 0
				overrideDamageType = false;
				for (int j = 0; j < 6; j++)
				{
					list.Add(model.metaInfo.damageInfos[0].Copy());
				}
				animationName = model.metaInfo.animationNames[0];
			}
			return new WeaponDamageInfo(animationName, list.ToArray());
		}

		/// <summary>
		/// [造成伤害] 施加易伤、固定扣血，并在破防形态下改写伤害类型。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <param name="dmg">本次伤害信息，可被就地修改</param>
		/// <returns>父类处理结果</returns>
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (overrideDamageType)
            {
                dmg.type = dmgType;
            }
            
            // 2 倍易伤、持续 5 秒，可叠加：配合 25 段破防形态时收益极高
            target.AddUnitBuf(new DebufDamageMultiply(true, 2f, 5f));

            // 目标已死亡则不再补刀
            if (target.hp>0) {
                target.hp -= 10f;
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        /// <summary>本次攻击使用的动画名，由 <see cref="OnAttackStart"/> 依据目标抗性选择</summary>
        private string animationName;
        /// <summary>是否需要强制改写本次伤害类型（目标存在免疫/吸收抗性时为 true）</summary>
        private bool overrideDamageType;
		/// <summary>本次攻击使用的伤害类型，仅在 <see cref="overrideDamageType"/> 为 true 时有效</summary>
		private RwbpType dmgType;
	}
}
