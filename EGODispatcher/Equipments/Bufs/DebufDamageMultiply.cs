namespace Equipments.Bufs
{
    /// <summary>
    /// Debuff：使目标受到的伤害按倍率放大（"易伤"效果）。
    /// <para>实现方式为在 <see cref="OnTakeDamage"/> 中返回倍率，由父类完成最终运算。</para>
    /// </summary>
	public class DebufDamageMultiply : UnitBuf
	{
        /// <summary>
        /// 构造易伤 Debuff。
        /// </summary>
        /// <param name="reproducible">是否可以堆叠</param>
        /// <param name="multiply">倍率，1.5对应着原伤害的150%</param>
        /// <param name="duration">该buf的持续时间，单位为秒</param>
		public DebufDamageMultiply(bool reproducible = false, float multiply = 1.5f, float duration = 5f)
		{
            // 复用 ADD_SUPERARMOR 槽位承载本 Debuff（游戏原版无对应的独立枚举位）
            type = UnitBufType.ADD_SUPERARMOR;
            if (reproducible) {
                // UNLIMIT：允许多层共存，倍率会随层数累积，需谨慎使用
                duplicateType = BufDuplicateType.UNLIMIT;
            } else {
                duplicateType = BufDuplicateType.ONLY_ONE;
            }
            this.multiply = multiply;
            this.duration = duration;
		}

        /// <summary>
        /// [初始化] 由父类在挂载到单位时调用，用持续时间初始化剩余时间。
        /// </summary>
        /// <param name="model">Debuff 所挂载的单位</param>
		public override void Init(UnitModel model)
		{
			base.Init(model);
			remainTime = duration;
		}

        /// <summary>
        /// [受伤修正] 返回伤害倍率，交由父类与原始伤害相乘。
        /// </summary>
        /// <param name="attacker">伤害来源单位</param>
        /// <param name="damageInfo">本次伤害信息</param>
        /// <returns>伤害倍率，1.5 表示放大为原伤害的 150%</returns>
		public override float OnTakeDamage(UnitModel attacker, DamageInfo damageInfo)
		{
			return multiply; // 直接返回伤害的倍率至父类进行运算
		}

        /// <summary>
        /// [单位死亡] 目标死亡时自行销毁，避免残留无效 Debuff。
        /// </summary>
		public override void OnUnitDie()
		{
			base.OnUnitDie();
			Destroy();
		}

        /// <summary>伤害倍率，1.5 表示放大为原伤害的 150%</summary>
        private readonly float multiply;
        /// <summary>Debuff 持续时间，单位为秒</summary>
        private readonly float duration;
	}
}
