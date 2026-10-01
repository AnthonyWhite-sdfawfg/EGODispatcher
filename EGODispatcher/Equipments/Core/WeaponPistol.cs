using System.Collections.Generic;
using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{ 
    /// <summary>
    /// [手枪] 点射型 EGO 武器脚本，也是本模组最基础的武器。
    /// <para>特点：固定 3 段伤害，命中后施加减速（持续 5 秒、幅度 10%）。</para>
    /// <para>破防策略：仅在目标存在免疫/吸收抗性时（<see cref="WeaponTools.HasImmuneDefense"/>）才改写伤害类型为目标最弱抗性，否则保留武器原本的伤害类型。</para>
    /// </summary>
    public class WeaponPistol : EquipmentScriptBase
	{
        /// <summary>
        /// [攻击开始] 按目标抗性决定是否改写伤害类型，并构造 3 段伤害信息。
        /// </summary>
        /// <param name="actor">攻击发起者</param>
        /// <param name="target">攻击目标</param>
        /// <returns>使用动画 0 与 3 段伤害的武器伤害信息</returns>
		public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
		{
            if (WeaponTools.HasImmuneDefense(target)) {
                overrideDamageType = true;
                dmgType = WeaponTools.GetWeakestDefenseType(target);
            }
            else {
                overrideDamageType = false;
            }
            List<DamageInfo> list = new List<DamageInfo>();
			for (int i = 0; i < 3; i++)
			{
				// 复制模板伤害，避免多段伤害共用同一实例而互相影响
				list.Add(base.model.metaInfo.damageInfos[0].Copy());
			}
			return new WeaponDamageInfo(base.model.metaInfo.animationNames[0], list.ToArray());
		}

        /// <summary>
        /// [造成伤害] 必要时改写伤害类型，并额外触发 3 次伤害结算。
        /// </summary>
        /// <param name="actor">攻击发起者</param>
        /// <param name="target">攻击目标</param>
        /// <param name="dmg">本次伤害信息，可被就地修改</param>
        /// <returns>父类处理结果</returns>
        public override bool OnGiveDamage(UnitModel actor, UnitModel target, ref DamageInfo dmg)
        {
            if (overrideDamageType) {
                dmg.type = dmgType;
            }
            // 手动追加 3 次 TakeDamage，与父类的 1 次结算共同构成点射效果
            for (int i = 1; i <= 3; i++)
            {
                target.TakeDamage(dmg);
            }
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        /// <summary>
        /// [伤害结算后] 为目标施加 10% 减速，持续 5 秒。
        /// </summary>
        /// <param name="actor">攻击发起者</param>
        /// <param name="target">攻击目标</param>
        /// <param name="dmg">本次伤害信息</param>
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            // 目标已死亡则不再施加 Debuff
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufSlowDown(5f, 0.1f));
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }

        /// <summary>本次攻击使用的伤害类型，仅在 <see cref="overrideDamageType"/> 为 true 时有效</summary>
        private RwbpType dmgType;
        /// <summary>是否需要强制改写本次伤害类型（目标存在免疫/吸收抗性时为 true）</summary>
        private bool overrideDamageType;
    }
}
