using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{
	/// <summary>
	/// [霰弹枪] 高段数爆发型 EGO 武器脚本。
	/// <para>特点：5 段伤害叠加，命中后同时施加强减速、易伤（可叠加）并固定扣血，属于强控场武器。</para>
	/// <para>注意：其伤害类型<b>始终</b>改写为目标最弱抗性，而非仅在目标免疫时改写。</para>
	/// </summary>
	public class WeaponShotgun : EquipmentScriptBase
	{
		/// <summary>
		/// [攻击开始] 记录目标最弱抗性作为本次伤害类型，其余伤害信息沿用父类模板。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <returns>父类构造的武器伤害信息</returns>
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
			this.dmgType = WeaponTools.GetWeakestDefenseType(target);
            return base.OnAttackStart(actor, target);
		}

		/// <summary>
		/// [造成伤害] 改写伤害类型后，额外触发 5 次伤害结算。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <param name="dmg">本次伤害信息，可被就地修改</param>
		/// <returns>父类处理结果</returns>
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            dmg.type = dmgType;
            // 手动追加 5 次 TakeDamage，与父类的 1 次结算共同构成高段数爆发
            for (int i = 1; i <= 5; i++)
            {
                target.TakeDamage(dmg);
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }

		/// <summary>
		/// [伤害结算后] 施加 50% 减速、1.2 倍易伤（允许叠加），并额外固定扣除 5 点生命值。
		/// </summary>
		/// <param name="actor">攻击发起者</param>
		/// <param name="target">攻击目标</param>
		/// <param name="dmg">本次伤害信息</param>
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            // 目标已死亡则不再补刀与施加 Debuff
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufSlowDown(2f, 0.5f));
                // 第一个参数为 true：易伤可多层叠加，多把霰弹枪同时命中时层数会迅速累积
                target.AddUnitBuf(new DebufDamageMultiply(true, 1.2f, 5f));
                target.hp -= 5f;
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }

        /// <summary>本次攻击使用的伤害类型，由 <see cref="OnAttackStart"/> 依据目标抗性计算</summary>
        private RwbpType dmgType;

    }
}
