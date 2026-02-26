using System;
using System.Collections.Generic;

namespace Utils
{
    // Token: 0x02000006 RID: 6
    public static class CreatureConsts
    {
        //Attachment套装
        public static readonly int[] GiftWorker = new int[] { 82101 };
        public static readonly int[] GiftOperative = new int[] { 82201, 82202, 82203 };
        public static readonly int[] GiftKeterCrewMember = new int[] { 82301, 82302, 82303, 82202 };
        public static readonly int[] GiftDefault = new int[] { 82400 };

        //感染Buf数组
        public static readonly UnitBufType[] InfectionBufTypes =
       {
            UnitBufType.SLIMEGIRL_LOVER,
            UnitBufType.VISCUSSNAAKE_INFESTED,
            UnitBufType.QUEENBEE_SPORE
        };

        //装备生成清单（ID,数量）
        public static readonly Dictionary<int, int> EquipmentPlan = new Dictionary<int, int>
        {
            { 81111, 5 },{ 81112, 5 },{ 81113, 5 },{ 81114, 5 },{ 81115, 5 },{ 81116, 5 },
            { 81211, 5 },{ 81212, 5 },{ 81213, 5 },{ 81214, 5 },{ 81215, 5 },
            { 83111, 5 },{ 83112, 5 },{ 83113, 5 },{ 83114, 5 },{ 83115, 5 },{ 83116, 5 },
            { 83211, 5 },{ 83212, 5 },{ 83213, 5 },{ 83214, 5 },
            { 83311, 5 }
        };

        //每日类型判定
        public enum DayType
        {
            NONE,      // 普通日子
            MALKUTH,   // Malkuth 核心抑制
            YESOD,     // Yesod 核心抑制
            NETZACH,   // Netzach 核心抑制
            HOD,       // Hod 核心抑制
            D47        // Day 47 构筑部（Kether E1）
        }

    }
  
}
