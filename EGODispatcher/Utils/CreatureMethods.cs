using System.Collections;
using System.Collections.Generic;
using System.Text;
using LobotomyBaseMod;
using UnityEngine;

namespace Utils
{
    public static class CreatureMethods
	{
        // [ExoSuit]迭代器，生成所有EXOSuit装备
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

        // [ExoSuit]迭代器，遍历自建的员工list，依照装备的武器分发对应的Attachment套装
        public static IEnumerator DistributeGiftsToAllAgents()
        {
            // 创建快照
            List<AgentModel> snapshot = new List<AgentModel>(AgentList.Agents);

            // 遍历快照
            foreach (AgentModel ag in snapshot)
            {
                if (ag == null || ag.IsDead())    // 二次排除已死亡员工
                    continue;

                int[] giftIds = ResolveID(ag);
                for (int j = 0; j < giftIds.Length; j++)
                {
                    EGOgiftModel gift = EGOgiftModel.MakeGift(EquipmentTypeList.instance.GetData(giftIds[j]));
                    ag.AttachEGOgift(gift);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        // [ExoSuit]与Armor体系同样的解析ID
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
			default:
				return CreatureConsts.GiftDefault;
			}
		}

        // [移除感染]核心方法
        public static void RemoveInfection(AgentModel agent)
        {
            if (agent == null) return;
            foreach (UnitBufType type in CreatureConsts.InfectionBufTypes)
            {
                UnitBuf buf = agent.GetUnitBufByType(type);
                if (buf != null) {
                    Notice.instance.Send("AddSystemLog", new object[] { string.Format("[EGODispatcher] {0}受到感染，正在清除……",agent.name) });
                    buf.Destroy();
                    agent.RemoveUnitBuf(buf);
                    agent.GetWorkerUnit().RemoveUnitBuf(buf);
                }
            }
        }

        // [移除感染]迭代器，以输入值为一组进行扫描并处理
        public static IEnumerator RemoveInfection(int batch)
        {
            List<AgentModel> snapshot = new List<AgentModel>(AgentList.Agents);
            if (snapshot.Count == 0) yield break;

            for (int i = 0; i < snapshot.Count; i += batch)
            {
                int end = System.Math.Min(i + batch, snapshot.Count);
                for (int j = i; j < end; j++)
                {
                    AgentModel ag = snapshot[j];
                    if (ag == null || ag.IsDead()) continue;    // 二次排除已死亡员工
                    CreatureMethods.RemoveInfection(ag);
                }

                yield return null;
            }
        }

        // [通用]取今日类型
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

        // [Netzach]解锁恢复机制
        public static void TryUnlockRecover(CreatureConsts.DayType TodayType)
        {
            var mgr = SefiraBossManager.Instance;
            // 只有被锁且当天类型匹配才解
            if (mgr.IsRecoverBlocked && TodayType != CreatureConsts.DayType.NONE)
            {
                mgr.SetRecoverBlockState(false);
                Notice.instance.Send(NoticeName.AddSystemLog, new object[] { $"[EGODispatcher] 已重新解锁恢复机制（{TodayType}）。" });
            }
        }

        // [Malkuth]取Malkuth打乱的工作映射
        public static int[] GetWorkMap()
        {
            var mgr = SefiraBossManager.Instance;
            int[] map = new int[4];
            for (int i = 1; i <= 4; i++)
                map[i - 1] = mgr.GetWorkId(i);
            return map;
        }

        // [Malkuth]在systemLog中显示工作映射
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

        // [Yesod]核心方法，销毁主Camera和UI Camera的像素化滤镜
        public static void ClearYesodFilters()
        {
            // 销毁主 Camera
            Camera mainCam = Camera.main;
            if (mainCam)
            {
                var pix = mainCam.GetComponent<CameraFilterPack_Pixel_Pixelisation>();
                if (pix)
                {
                    Object.DestroyImmediate(pix);
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
                    Object.DestroyImmediate(pix);
                    Notice.instance.Send("AddSystemLog", new object[] { "[EGODispatcher] 已销毁UI Camera的像素化滤镜组件" });
                }
            }
        }

        // [Yesod]迭代器外壳，延迟运行
        public static IEnumerator ClearPixelDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            ClearYesodFilters();
        }
    }
}
