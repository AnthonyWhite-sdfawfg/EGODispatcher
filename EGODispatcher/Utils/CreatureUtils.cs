using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LobotomyBaseMod;
using UnityEngine;

namespace Utils
{
    public static class CreatureUtils
    {
        #region 数据结构

        // Attachment套装
        public static readonly int[] GiftWorker = new int[] { 82101 };
        public static readonly int[] GiftOperative = new int[] { 82201, 82202, 82203 };
        public static readonly int[] GiftKeterCrewMember = new int[] { 82301, 82302, 82303, 82202 };
        public static readonly int[] GiftDefault = new int[] { 82400 };

        // 感染Buf数组
        public static readonly UnitBufType[] InfectionBufTypes =
       {
            UnitBufType.SLIMEGIRL_LOVER,
            UnitBufType.VISCUSSNAAKE_INFESTED,
            UnitBufType.QUEENBEE_SPORE
        };

        // 装备生成清单（ID,数量）
        public static readonly Dictionary<int, int> EquipmentPlan = new Dictionary<int, int>
        {
            // 护甲 unified
            { 81111, 5 },{ 81112, 5 },{ 81113, 5 },{ 81114, 5 },{ 81115, 5 },
            { 81116, 5 },{ 81117, 5 },{ 81118, 5 },{ 81119, 5 },{ 81120, 5 },
            { 81121, 5 },
            // 武器
            { 83111, 5 },{ 83112, 5 },{ 83113, 5 },{ 83114, 5 },{ 83115, 5 },{ 83116, 5 },// 手枪
            { 83211, 5 },{ 83212, 5 },{ 83213, 5 },{ 83214, 5 },// 步枪
            { 83311, 5 },{ 83321, 1 }//keter crew：霰弹枪、链锯
        };

        // 每日类型判定
        public enum DayType
        {
            NONE,      // 普通日子
            MALKUTH,   // Malkuth 核心抑制
            YESOD,     // Yesod 核心抑制
            NETZACH,   // Netzach 核心抑制
            HOD,       // Hod 核心抑制
            D47        // Day 47 构筑部（Kether E1）
        }

        #endregion

        #region 方法

        /// <summary>
        /// [ExoSuit]迭代器，生成所有 EXOSuit 装备
        /// </summary>   
        public static IEnumerator SpawnEquipmentsToInventory(Dictionary<int, int> plan)
        {
            InventoryModel inv = InventoryModel.Instance;
            foreach (KeyValuePair<int, int> keyValuePair in plan)
            {
                int key = keyValuePair.Key;
                int value = keyValuePair.Value;
                LcId rhs = new LcId(key);
                int num = 0;
                for (int i = 0; i < inv.equipList.Count; i++)
                {
                    if (EquipmentTypeInfo.GetLcId(inv.equipList[i].metaInfo) == rhs)
                    {
                        num++;
                    }
                }
                int num2 = Mathf.Max(0, value - num);
                for (int j = 0; j < num2; j++)
                {
                    inv.CreateEquipment(key);
                }
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }

        /// <summary>
        /// [ExoSuit]遍历自建的员工 list ，依照装备的武器分发对应的 Attachment 套装
        /// </summary>
        public static void DistributeGiftToAgent(AgentModel ag)
        {
            int[] giftIds = ResolveID(ag);
            for (int j = 0; j < giftIds.Length; j++)
            {
                EGOgiftModel gift = EGOgiftModel.MakeGift(EquipmentTypeList.instance.GetData(giftIds[j]));
                if (!ag.HasEquipment(giftIds[j]))
                {
                    ag.AttachEGOgift(gift);
                }
            }
        }

        /// <summary>
        ///  [ExoSuit]复用ArmorUtils解析并通过映射取数组
        /// </summary>
        public static int[] ResolveID(AgentModel ag)
        {
            WorkerModel workerModel = ag as WorkerModel;
            ArmorUtils.CombatMode mode = ArmorUtils.ResolveCombatMode(workerModel);
            if (ArmorUtils.CombatModeToGiftMap.TryGetValue(mode, out int[] giftIds))
            {
                return giftIds;
            }
            return GiftDefault;
        }

        /// <summary>
        /// [移除感染]处理员工的感染（如有）
        /// </summary>
        public static void RemoveInfection(AgentModel agent)
        {
            if (agent == null) return;
            foreach (UnitBufType type in InfectionBufTypes)
            {
                UnitBuf buf = agent.GetUnitBufByType(type);
                if (buf != null)
                {
                    Notice.instance.Send("AddSystemLog", new object[] { string.Format("[EGODispatcher] {0}受到感染，正在清除感染……", agent.name) });
                    buf.Destroy();
                    agent.RemoveUnitBuf(buf);
                    agent.GetWorkerUnit().RemoveUnitBuf(buf);
                }
            }
        }

        /// <summary>
        /// [通用]取今日类型
        /// </summary>
        public static DayType GetTodayType()
        {
            var mgr = SefiraBossManager.Instance;

            // Day 47 构筑部（Kether-E1）
            if (mgr.IsKetherBoss(KetherBossType.E1))
                return DayType.D47;
            // 各核心抑制
            if (mgr.CheckBossActivation(SefiraEnum.MALKUT))
                return DayType.MALKUTH;
            if (mgr.CheckBossActivation(SefiraEnum.YESOD))
                return DayType.YESOD;
            if (mgr.CheckBossActivation(SefiraEnum.NETZACH))
                return DayType.NETZACH;
            if (mgr.CheckBossActivation(SefiraEnum.HOD))
                return DayType.HOD;

            return DayType.NONE;
        }

        /// <summary>
        /// [Netzach]解锁恢复机制
        /// </summary>
        public static void TryUnlockRecover(DayType TodayType)
        {
            var mgr = SefiraBossManager.Instance;
            if (mgr.IsRecoverBlocked)
            {
                mgr.SetRecoverBlockState(false);
                Notice.instance.Send(NoticeName.AddSystemLog, new object[] { $"[EGODispatcher] 已重新解锁恢复机制（{TodayType}）。" });
            }
        }

        /// <summary>
        /// [Malkuth]取 Malkuth 打乱的工作映射
        /// </summary>
        public static int[] GetWorkMap()
        {
            var mgr = SefiraBossManager.Instance;
            int[] map = new int[4];
            for (int i = 1; i <= 4; i++)
                map[i - 1] = mgr.GetWorkId(i);
            return map;
        }

        /// <summary>
        /// [Malkuth]在 systemLog 中显示工作映射
        /// </summary>
        public static void LogWorkMap()
        {
            int[] map = GetWorkMap();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[EGODispatcher] 工作映射已更新（当前过载等级 " + CreatureOverloadManager.instance.GetQliphothOverloadLevel() + "）:");
            string[] name = { "<color=red>本能</color>", "<color=white>洞察</color>", "<color=magenta>沟通</color>", "<color=cyan>压迫</color>" };
            for (int i = 0; i < 4; i++)
                sb.AppendLine(string.Format("  [{0}] {1} → {2}", i + 1, name[i], name[map[i] - 1]));
            Notice.instance.Send(NoticeName.AddSystemLog, new object[] { sb.ToString() });
        }

        /// <summary>
        /// [Yesod]核心方法，销毁主 Camera 和 UI Camera 的像素化滤镜
        /// </summary>
        public static void ClearYesodFilters()
        {
            // 销毁主 Camera
            Camera mainCam = Camera.main;
            if (mainCam)
            {
                var pix = mainCam.GetComponent<CameraFilterPack_Pixel_Pixelisation>();
                if (pix)
                {
                    UnityEngine.Object.DestroyImmediate(pix);
                    Notice.instance.Send("AddSystemLog", new object[] { "[EGODispatcher] 已销毁主Camera的像素化滤镜组件" });
                }
            }
            // 销毁 UI Camera
            Camera uiCam = UIActivateManager.instance?.GetCam();
            if (uiCam)
            {
                var pix = uiCam.GetComponent<CameraFilterPack_Pixel_Pixelisation>();
                if (pix)
                {
                    UnityEngine.Object.DestroyImmediate(pix);
                    Notice.instance.Send("AddSystemLog", new object[] { "[EGODispatcher] 已销毁UI Camera的像素化滤镜组件" });
                }
            }
        }

        /// <summary>
        /// [Yesod]迭代器外壳，因为未知原因，滤镜需要延迟一段时间后才能进行销毁
        /// </summary>
        public static IEnumerator ClearPixelDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            ClearYesodFilters();
        }

        /// <summary>
        /// [处理异想体]迭代器，异想体计数器+1，增加 pebox
        /// </summary>
        public static IEnumerator CreatureProcess(CreatureModel[] creatures)
        {
            for (int i = 0; i < creatures.Length; i++)
            {
                creatures[i].AddQliphothCounter();
                creatures[i].AddCreatureSuccessCube(10);
                yield return new WaitForEndOfFrame();
            }
        }


        /// <summary>
        /// [批处理协程]处理全体员工时使用：以5个员工为一组进行处理；
        /// </summary>
        public static IEnumerator BatchProcess(Action<AgentModel> processAction)
        {
            List<AgentModel> snapshot = new List<AgentModel>(AgentList.Agents);
            if (snapshot.Count == 0) yield break;

            for (int i = 0; i < snapshot.Count; i += 5)
            {
                int end = System.Math.Min(i + 5, snapshot.Count);
                for (int j = i; j < end; j++)
                {
                    AgentModel ag = snapshot[j];
                    if (ag == null || ag.IsDead()) continue;// 二次判断员工
                    processAction(ag);  // 执行传入的方法
                }

                yield return null;
            }
        }

        /// <summary>
        /// [EGODispatcher]更改员工发型
        /// </summary>
        /// <param name="target"></param>
        public static void MakeBald(WorkerModel target)
        {
            Sprite BALD_FRONT_SPRITE = Resources.Load<Sprite>("Sprites/Worker/Basic/Hair/Front/Bald");
            Sprite BALD_REAR_SPRITE = Resources.Load<Sprite>("Sprites/Worker/Basic/Hair/Rear/RearHair_Transparent");
            WorkerSprite.WorkerSpriteSaveData.Pair BALD_PAIR = new WorkerSprite.WorkerSpriteSaveData.Pair(0, 0);

            target.spriteData.FrontHair = BALD_FRONT_SPRITE;
            target.spriteData.RearHair = BALD_REAR_SPRITE;
            target.spriteData.saveData.FrontHair = BALD_PAIR;
            target.spriteData.saveData.RearHair = BALD_PAIR;

            target.GetWorkerUnit().spriteSetter.ChangeBasicSpriteData();
        }

        #endregion 
    }

}
