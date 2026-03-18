using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LobotomyBaseMod;
using UnityEngine;

namespace Utils
{
    public static class CreatureMethods
    {
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
        ///  [ExoSuit]与 Armor 体系同样的解析ID
        /// </summary>
        public static int[] ResolveID(AgentModel ag)
        {
            switch (EquipmentTypeInfo.GetLcId(ag.Equipment.weapon.metaInfo).id / ArmorConsts.ID_DIGIT % 10)
            {
                case 1:
                    return CreatureConsts.GiftWorker;
                case 2:
                    return CreatureConsts.GiftOperative;
                case 3:
                    return CreatureConsts.GiftKeterCrewMember;
                case 4:
                    return CreatureConsts.GiftKeterCrewMember;
                default:
                    return CreatureConsts.GiftDefault;
            }
        }

        /// <summary>
        /// [移除感染]处理员工的感染（如有）
        /// </summary>
        public static void RemoveInfection(AgentModel agent)
        {
            if (agent == null) return;
            foreach (UnitBufType type in CreatureConsts.InfectionBufTypes)
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
        public static CreatureConsts.DayType GetTodayType()
        {
            var mgr = SefiraBossManager.Instance;

            // Day 47 构筑部（Kether-E1）
            if (mgr.IsKetherBoss(KetherBossType.E1))
                return CreatureConsts.DayType.D47;
            // 各核心抑制
            if (mgr.CheckBossActivation(SefiraEnum.MALKUT))
                return CreatureConsts.DayType.MALKUTH;
            if (mgr.CheckBossActivation(SefiraEnum.YESOD))
                return CreatureConsts.DayType.YESOD;
            if (mgr.CheckBossActivation(SefiraEnum.NETZACH))
                return CreatureConsts.DayType.NETZACH;
            if (mgr.CheckBossActivation(SefiraEnum.HOD))
                return CreatureConsts.DayType.HOD;

            return CreatureConsts.DayType.NONE;
        }

        /// <summary>
        /// [Netzach]解锁恢复机制
        /// </summary>
        public static void TryUnlockRecover(CreatureConsts.DayType TodayType)
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


    }
}
