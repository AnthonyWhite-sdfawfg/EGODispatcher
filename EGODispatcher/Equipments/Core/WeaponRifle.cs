using System.Collections.Generic;
using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{
	/// <summary>
	/// [步枪] 点射型 EGO 武器脚本。
	/// <para>特点：固定 3 段伤害，命中后附加"易伤"与固定扣血，属于持续压制向武器。</para>
	/// <para>与手枪的区别：步枪的伤害类型<b>始终</b>改写为目标最弱抗性；手枪仅在目标存在免疫抗性时才改写。</para>
	/// </summary>
	public class WeaponRifle : EquipmentScriptBase
	{
		/// <summary>
		/// [攻击开始] 取目标最弱抗性作为本次攻击的伤害类型，并构造 3 段伤害信息。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <returns>使用动画 0 与 3 段伤害的武器伤害信息</returns>
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			dmgType = WeaponTools.GetWeakestDefenseType(target);
			List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i < 3; i++)
			{
				// 复制模板伤害，避免多段伤害共用同一实例而互相影响
				list.Add(model.metaInfo.damageInfos[0].Copy());
			}
			return new WeaponDamageInfo(model.metaInfo.animationNames[0], list.ToArray());
		}

		/// <summary>
		/// [造成伤害] 将伤害类型替换为目标最弱抗性，避免被免疫或吸收抵消。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <param name="dmg">本次伤害信息，可被就地修改</param>
		/// <returns>父类处理结果</returns>
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            dmg.type = dmgType;
            return base.OnGiveDamage(actor, target, ref dmg);
        }

		/// <summary>
		/// [伤害结算后] 施加易伤 Debuff，并额外固定扣除 1 点生命值。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <param name="dmg">本次伤害信息</param>
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            // 目标已死亡则不再补刀
            if (target.hp > 0)
            {
                target.hp -= 1f;
            }
            // 2 倍易伤、持续 5 秒；duplicateType 为 ONLY_ONE，重复命中会刷新而非叠加
            target.AddUnitBuf(new DebufDamageMultiply(false, 2f, 5f));
            base.OnGiveDamageAfter(actor, target, dmg);
        }

		/// <summary>本次攻击使用的伤害类型，由 <see cref="OnAttackStart"/> 依据目标抗性计算</summary>
        private RwbpType dmgType;
	}
}
