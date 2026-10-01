using Equipments.Bufs;
using Equipments.Tools;

namespace Equipments.Core
{
    /// <summary>
    /// [火炮] 持续伤害型 EGO 武器脚本。
    /// <para>特点：单次直接伤害不高，但命中后为目标附加可观的 DOT（持续伤害），适合对付高血量目标。</para>
    /// <para>DOT 参数见 <see cref="WeaponTools.DotConfig"/>：持续 10 秒，每 0.1 秒结算一次，每次 20 点。</para>
    /// </summary>
    class WeaponCannon : EquipmentScriptBase
    {
        /// <summary>
        /// [攻击开始] 判定是否需要改写伤害类型，并按目标最弱抗性构建本次的 DOT 配置。
        /// </summary>
        /// <param name="actor">攻击发起者</param>
        /// <param name="target">攻击目标</param>
        /// <returns>父类默认构造的武器伤害信息</returns>
        public override WeaponDamageInfo OnAttackStart(UnitModel actor, UnitModel target)
        {
            overrideDamageType = WeaponTools.HasImmuneDefense(target);
            dmgType = WeaponTools.GetWeakestDefenseType(target);
            dotConfigCannon = new WeaponTools.DotConfig(overrideDamageType, dmgType, 10f, 20f, 0.1f);
            return base.OnAttackStart(actor, target);
        }

        /// <summary>
        /// [造成伤害] 若目标存在免疫/吸收抗性，则把伤害类型替换为其最弱抗性。
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
            return base.OnGiveDamage(actor, target, ref dmg);
        }

        /// <summary>
        /// [伤害结算后] 为目标附加 DOT Debuff，其伤害类型沿用本次攻击的判定结果。
        /// </summary>
        /// <param name="actor">攻击发起者</param>
        /// <param name="target">攻击目标</param>
        /// <param name="dmg">本次伤害信息</param>
        public override void OnGiveDamageAfter(UnitModel actor, UnitModel target, DamageInfo dmg)
        {
            // 目标已死亡则不再附加 DOT
            if (target.hp > 0f)
            {
                target.AddUnitBuf(new DebufDotDamage(dotConfigCannon));
            }
            base.OnGiveDamageAfter(actor, target, dmg);
        }

        /// <summary>是否需要强制改写本次伤害类型（目标存在免疫/吸收抗性时为 true）</summary>
        bool overrideDamageType;
        /// <summary>本次攻击使用的伤害类型，由 <see cref="OnAttackStart"/> 依据目标抗性计算</summary>
        private RwbpType dmgType;
        /// <summary>本次攻击所使用的 DOT 配置，作为构造参数传给 <see cref="DebufDotDamage"/></summary>
        private WeaponTools.DotConfig dotConfigCannon;
    }
}
