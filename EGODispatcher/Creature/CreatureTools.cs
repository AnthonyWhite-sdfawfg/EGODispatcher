using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LobotomyBaseMod;
using UnityEngine;
using Utils;
using Equipments.Tools;

namespace Creature
{
    /// <summary>
    /// [EGODispatcher] 的逻辑工具库：集中存放装备/饰品清单、批处理协程与核心抑制相关处理。
    /// <para>与 <see cref="EGODispatcher"/> 的分工：主类只负责"何时触发"，本类负责"具体怎么做"。</para>
    /// </summary>
    public static class CreatureTools
    {
        #region 常量字段

        /// <summary>批处理员工时每批的数量，用于把耗时的遍历摊到多帧，避免集中卡顿</summary>
        public const int DEFAULT_BATCH_SIZE = 5;
        /// <summary>通用延迟与消息播报间隔，单位为秒</summary>
        public static readonly float DEFAULT_DELAY_TIME = 0.5F;
        /// <summary>每日 LOB 生成上限，达到后当日不再增长</summary>
        public static readonly int LOB_MAX_VALUE = 300;
        /// <summary>待播报消息队列的长度，超出后丢弃最早的消息</summary>
        public static readonly int MAX_MESSAGE_COUNT = 10;

        #endregion

        #region 数据结构

        // Attachment套装
        /// <summary>武器 ID 百位为 1（Worker）时下发的饰品套装</summary>
        public static readonly int[] GiftWorker = new int[] { 82101 };
        /// <summary>武器 ID 百位为 2（Operative）时下发的饰品套装</summary>
        public static readonly int[] GiftOperative = new int[] { 82201, 82202, 82203 };
        /// <summary>武器 ID 百位为 3（Keter 组员）与 4（原型）时下发的饰品套装</summary>
        public static readonly int[] GiftKeterCrewMember = new int[] { 82301, 82302, 82303, 82202 };
        /// <summary>未持有武器或无法识别战斗模式时下发的默认饰品</summary>
        public static readonly int[] GiftDefault = new int[] { 82400 };

        /// <summary>四种工作类型的中文名（已带游戏内富文本颜色），下标与工作 ID - 1 对应</summary>
        public static readonly string[] WorkType = { "<color=#D92B3B>本能</color>", "<color=#F2F0D0>洞察</color>", "<color=#A057A0>沟通</color>", "<color=#4ECDC4>压迫</color>" };

        /// <summary>触发饰品下发的饰品 ID：员工携带其中任意一件完成工作时即向全员分发套装</summary>
        public static readonly int[] getAttachmentIds = { 83211, 83212, 83213, 83214 };

        // 感染Buf数组
        /// <summary>需要被清除的感染类 Buff（溶解之爱、裸巢、蜂后）</summary>
        public static readonly UnitBufType[] InfectionBufTypes =
       {
            UnitBufType.SLIMEGIRL_LOVER,
            UnitBufType.VISCUSSNAAKE_INFESTED,
            UnitBufType.QUEENBEE_SPORE
        };

        // 装备生成清单（ID,数量）
        /// <summary>
        /// 装备生成清单（ID,数量）。
        /// <para>下发时按"补齐差额"处理：若装备库中已有若干件，则只补足到目标数量。</para>
        /// </summary>
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
        /// <summary>当日类型：普通日或某一种核心抑制日</summary>
        public enum DayType
        {
            /// <summary>普通日子，无核心抑制</summary>
            NONE,      // 普通日子
            /// <summary>Malkuth 核心抑制：工作指令被随机打乱</summary>
            MALKUTH,   // Malkuth 核心抑制
            /// <summary>Yesod 核心抑制：画面被像素化滤镜覆盖</summary>
            YESOD,     // Yesod 核心抑制
            /// <summary>Netzach 核心抑制：员工恢复机制被封锁</summary>
            NETZACH,   // Netzach 核心抑制
            /// <summary>Hod 核心抑制：员工属性维持系统异常</summary>
            HOD,       // Hod 核心抑制
            /// <summary>Day 47 构筑部（Kether E1），会同时开启上述全部模式</summary>
            D47        // Day 47 构筑部（Kether E1）
        }

        /// <summary>调用 <see cref="TryUnlockRecover"/> 的场景，用于区分返回的提示文案</summary>
        public enum StatusType
        {
            /// <summary>当日初始化阶段</summary>
            DayInit,
            /// <summary>逆卡巴拉过载等级变化时的通知阶段</summary>
            Notice,
            /// <summary>工作结束阶段</summary>
            Work
        }

        #endregion

        #region 方法

        /// <summary>
        /// [处理异想体]迭代器，异想体计数器+1，增加 pebox。
        /// <para>每个异想体处理完后等待一帧，避免一次性操作大量异想体造成卡顿。</para>
        /// </summary>
        /// <param name="creatures">待处理的异想体列表（通常为关卡开始时缓存的全体异想体）</param>
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
        /// [ExoSuit]迭代器，生成所有 EXOSuit 装备。
        /// <para>按清单逐项补齐：先统计装备库中已有的数量，只创建差额部分；
        /// 每处理完一项装备等待一帧。</para>
        /// </summary>
        /// <param name="plan">装备生成清单，键为装备 ID、值为目标数量</param>
        public static IEnumerator SpawnEquipmentsToInventory(Dictionary<int, int> plan)
        {
            InventoryModel inv = InventoryModel.Instance;
            foreach (KeyValuePair<int, int> keyValuePair in plan)
            {
                int key = keyValuePair.Key;
                int value = keyValuePair.Value;
                LcId rhs = new LcId(key);
                int num = 0;
                // 统计装备库中已有的同 ID 装备数量
                for (int i = 0; i < inv.equipList.Count; i++)
                {
                    if (EquipmentTypeInfo.GetLcId(inv.equipList[i].metaInfo) == rhs)
                    {
                        num++;
                    }
                }
                // 只补齐差额，已在库中的装备不会重复生成
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
        /// [ExoSuit]遍历自建的员工 list ，依照装备的武器分发对应的 Attachment 套装。
        /// <para>已持有的饰品会被跳过，因此可重复调用而不会重复发放。</para>
        /// </summary>
        /// <param name="ag">待发放饰品的员工</param>
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
        /// [ExoSuit]复用 ArmorTools 解析战斗模式，并通过映射表取得对应的饰品 ID 数组。
        /// </summary>
        /// <param name="ag">待解析的员工</param>
        /// <returns>该员工应获得的饰品 ID 数组；无法匹配时返回默认套装</returns>
        public static int[] ResolveID(AgentModel ag)
        {
            WorkerModel workerModel = ag as WorkerModel;
            ArmorTools.CombatMode mode = ArmorTools.ResolveCombatMode(workerModel);
            if (ArmorTools.CombatModeToGiftMap.TryGetValue(mode, out int[] giftIds))
            {
                return giftIds;
            }
            return GiftDefault;
        }

        /// <summary>
        /// [移除感染]处理员工的感染（如有）。
        /// <para>同时从员工模型与其对应的 WorkerUnit 上移除 Buff，并输出一条系统日志。</para>
        /// </summary>
        /// <param name="agent">待处理的员工</param>
        public static void RemoveInfection(AgentModel agent)
        {
            if (agent == null) return;
            foreach (UnitBufType type in InfectionBufTypes)
            {
                UnitBuf buf = agent.GetUnitBufByType(type);
                if (buf != null)
                {
                    string content = string.Format(LocalTexts.REMOVING_INFECTION, agent.name);
                    string colorizedContent = LogSendings.Colorize(LogSendings.ColorType.Notice,content);
                    LogSendings.SendLog(colorizedContent);
                    buf.Destroy();
                    agent.RemoveUnitBuf(buf);
                    agent.GetWorkerUnit().RemoveUnitBuf(buf);
                }
            }
        }

        /// <summary>
        /// [批处理协程]处理全体员工时使用：依据传参数量分组进行处理。
        /// <para>先对员工表做一次快照，避免处理过程中列表被修改；每批之间等待一帧。</para>
        /// </summary>
        /// <param name="processAction">对单个员工执行的操作</param>
        /// <param name="batch">每批处理的员工数量</param>
        public static IEnumerator AgentBatchProcess(Action<AgentModel> processAction, int batch)
        {
            List<AgentModel> snapshot = new List<AgentModel>(ActiveAgentManager.Agents);
            if (snapshot.Count == 0) yield break;

            for (int i = 0; i < snapshot.Count; i += batch)
            {
                int end = System.Math.Min(i + batch, snapshot.Count);
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
        /// [EGODispatcher]头盔贴图无法完全覆盖发型贴图，所以固定修改为光头。
        /// <para>同时改写正面/背面发型贴图与其存档数据，最后刷新角色基础外观。</para>
        /// </summary>
        /// <param name="target">待修改外观的员工</param>
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

        #region 核心抑制

        /// <summary>
        /// [通用]取今日类型。
        /// <para>判定顺序：先判断是否为 Day 47 构筑部，再依次检查各 Sefira 的核心抑制是否激活。</para>
        /// </summary>
        /// <returns>当日的类型；均未命中时返回 <see cref="DayType.NONE"/></returns>
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
        /// [Malkuth]取 Malkuth 打乱的工作映射。
        /// </summary>
        /// <returns>长度为 4 的数组，下标为原始工作序号（0 起），值为打乱后实际对应的工作 ID（1 起）</returns>
        public static int[] GetWorkMap()
        {
            var mgr = SefiraBossManager.Instance;
            int[] map = new int[4];
            for (int i = 1; i <= 4; i++)
                map[i - 1] = mgr.GetWorkId(i);
            return map;
        }

        /// <summary>
        /// [Malkuth]在 systemLog 中显示工作映射。
        /// <para>输出内容包含当前过载等级，并逐行列出"原始指令 → 实际指令"的对照关系。</para>
        /// </summary>
        public static void LogWorkMap()
        {
            int[] map = GetWorkMap();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[EGODispatcher] 指令映射表（当前过载等级 " + CreatureOverloadManager.instance.GetQliphothOverloadLevel() + "）:");
            for (int i = 0; i < 4; i++)
            {
                sb.AppendLine(string.Format("  [{0}] {1} → {2}", i + 1, WorkType[i], WorkType[map[i] - 1]));
            }

            Notice.instance.Send(NoticeName.AddSystemLog, new object[] { sb.ToString() });
        }

        /// <summary>
        /// [Yesod]核心方法，销毁主 Camera 和 UI Camera 的像素化滤镜。
        /// <para>使用 <c>DestroyImmediate</c> 立即销毁组件，两处相机分别处理。</para>
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
                }
            }
        }

        /// <summary>
        /// [Yesod]迭代器外壳，因为未知原因，滤镜需要延迟一段时间后才能进行销毁。
        /// </summary>
        /// <param name="delayTime">销毁前的等待时间，单位为秒</param>
        public static IEnumerator ClearPixelDelayed(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            ClearYesodFilters();
        }

        /// <summary>
        /// [Netzach]解锁恢复机制。
        /// <para>先读取并解除恢复封锁（若原本未被封锁则记录为 false），
        /// 再依据调用场景返回不同的提示文案；工作场景下仅在确实解除封锁时才提示。</para>
        /// </summary>
        /// <param name="statusType">调用场景（当日初始化 / 过载通知 / 工作结束）</param>
        /// <returns>需要播报的文案；无需播报时返回 null</returns>
        public static string TryUnlockRecover(StatusType statusType)
        {
            var mgr = SefiraBossManager.Instance;
            bool wasBlocked = mgr.IsRecoverBlocked;

            // 如果需要解锁，先执行解锁
            if (wasBlocked)
            {
                mgr.SetRecoverBlockState(false);
            }

            // 根据状态类型和是否解锁返回对应的消息
            switch (statusType)
            {
                case StatusType.DayInit:
                    return LocalTexts.NETZACH_INIT;
                case StatusType.Notice:
                    return wasBlocked ? LocalTexts.NETZACH_ACTIVATE : LocalTexts.NETZACH_SELF_TEST_CLEAR;
                case StatusType.Work:
                    return wasBlocked ? LocalTexts.NETZACH_MANUAL_OVERRIDE : null;
                default:
                    return "unnoticed situation!";
            }
        }


        #endregion
    }

}
