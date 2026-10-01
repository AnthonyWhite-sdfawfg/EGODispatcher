using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Creature
{
    /// <summary>
    /// [EGODispatcher] 异想体主逻辑脚本，本模组的核心。
    /// <para>定位：一台自律运行的设施管理终端。玩家对它完成工作后，它会执行一轮"系统服务"
    /// （下发装备与饰品、清除员工感染、生成 LOB 与 PE-box）；在核心抑制日还会上线对应的维护协议。</para>
    /// <para>关键机制：</para>
    /// <list type="bullet">
    /// <item><description><b>每日一次性初始化</b>：<see cref="OnStageStart"/> 中重置计数并启动协程
    /// <see cref="InitDayTypeConfig"/>，由它延迟判定当天类型并播报对应协议。</description></item>
    /// <item><description><b>周期轮询</b>：<see cref="OnFixedUpdate"/> 以 1 秒为周期清除感染、累积 LOB 并注入能量。</description></item>
    /// <item><description><b>事件订阅</b>：以 <see cref="IObserver"/> 身份监听员工死亡与逆卡巴拉过载等级变化。</description></item>
    /// <item><description><b>消息队列</b>：所有对话先进入定长环形队列，再由协程按固定间隔依次播报，避免同时弹出多条。</description></item>
    /// </list>
    /// </summary>
    public class EGODispatcher : CreatureBase, IObserver
    {
        #region 钩子

        /// <summary>
        /// [视图初始化] 取得动画脚本引用并完成互相绑定。
        /// </summary>
        /// <param name="unit">本异想体对应的单位</param>
        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            animscript = (EGODispatcherAnim)unit.animTarget;
            animscript.SetScript(this);
        }

        /// <summary>
        /// [关卡开始] 重置当日状态：清空计数器、判定当天是否有核心抑制、缓存异想体列表、
        /// 注册事件监听并启动感染轮询与当日协议初始化协程。
        /// </summary>
        public override void OnStageStart()
        {
            base.OnStageStart();
            infectionCounter = 0;
            todayType = CreatureTools.GetTodayType();
            // 缓存当日异想体列表，作为 LOB/能量增长量与每日结算的基数
            creatureModels = CreatureManager.instance.GetCreatureList();
            addings = creatureModels.Length;
            lobCounter = 0;
            lobTrigger = false;
            RegisterNotice();
            ActiveAgentManager.Set();
            infectionTimer.StartTimer(1f);
            // 延迟执行：等待关卡完全加载后再读取核心抑制状态并播报
            animscript.StartCoroutine(InitDayTypeConfig(CreatureTools.DEFAULT_DELAY_TIME));
        }

        /// <summary>
        /// [工作结束] 终端服务的主入口，按顺序发放奖励并推进当日进度。
        /// <para>依次处理：解除恢复机制封锁 → 装备库补给 → 饰品下发 → 首次工作启动 LOB →
        /// 推进全体异想体的计数器与 PE-box。</para>
        /// </summary>
        /// <param name="skill">本次工作的技能信息，可通过 <c>skill.agent</c> 取得执行者</param>
        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);

            string result = CreatureTools.TryUnlockRecover(CreatureTools.StatusType.Work);
            if (result != null) EnqueueMessage(result);

            AgentModel agent = skill.agent;

            // 携带 EGO 饰品 83400 时，向装备库补齐整套自建装备
            if (agent.HasEquipment(83400))
            {
                animscript.StartCoroutine(CreatureTools.SpawnEquipmentsToInventory(CreatureTools.EquipmentPlan));
                EnqueueMessage(LocalTexts.EGO_DELIVERED);
            }

            // 携带任一下发用饰品时，为全体员工按武器类型分发套装饰品，并统一改为光头
            if (Array.Exists(CreatureTools.getAttachmentIds, id => agent.HasEquipment(id)))
            {
                animscript.StartCoroutine(CreatureTools.AgentBatchProcess(CreatureTools.DistributeGiftToAgent, CreatureTools.DEFAULT_BATCH_SIZE));
                animscript.StartCoroutine(CreatureTools.AgentBatchProcess(CreatureTools.MakeBald, CreatureTools.DEFAULT_BATCH_SIZE));
                EnqueueMessage(LocalTexts.ATTACHMENT_DELIVERED);
            }

            if (lobTrigger == false) // 每日首次工作后触发lobTrigger
            {
                lobTrigger = true;
                EnqueueMessage(LocalTexts.GENERATING_LOB);
            }


            animscript.StartCoroutine(CreatureTools.CreatureProcess(creatureModels));
        }

        /// <summary>
        /// [关卡结束] 注销事件监听、清空员工表，并按异想体数量结算一次性资金收益。
        /// </summary>
        public override void OnStageEnd()
        {
            base.OnStageEnd();

            DeregisterNotice();
            ActiveAgentManager.Clear();
            MoneyModel.instance.Add(creatureModels.Length);
        }

        /// <summary>
        /// [事件回调] 处理订阅到的通知。
        /// <para>员工死亡：从存活员工表中移除，避免后续批量处理时访问到已死亡单位。</para>
        /// <para>逆卡巴拉过载等级变化：刷新 Malkuth 指令映射表；Yesod 模式下在过载等级 ≥ 2 时
        /// 重新清除像素滤镜；并尝试解除 Netzach 的恢复封锁。</para>
        /// </summary>
        /// <param name="notice">通知名称</param>
        /// <param name="param">通知附带的参数</param>
        public void OnNotice(string notice, params object[] param)
        {
            if (notice == NoticeName.OnAgentDead)
            {
                ActiveAgentManager.RemoveDeadAgents();
            }

            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();

                if (isMalkuth)
                {
                    EnqueueMessage(LocalTexts.MALKUTH_ACTIVATE);
                    CreatureTools.LogWorkMap();
                }

                // Yesod 的滤镜会在过载等级较高时重新出现，因此需要在等级 ≥ 2 时二次清除
                if (isYesod && level >= 2)
                {
                    animscript.StartCoroutine(CreatureTools.ClearPixelDelayed(CreatureTools.DEFAULT_DELAY_TIME));
                    EnqueueMessage(LocalTexts.YESOD_ACTIVATE);
                }

                string result = CreatureTools.TryUnlockRecover(CreatureTools.StatusType.Notice);
                if (result != null) EnqueueMessage(result);
            }
        }

        /// <summary>
        /// [周期性更新] 以 1 秒为周期执行终端服务：清除感染、累积 LOB 并注入能量。
        /// </summary>
        /// <param name="creature">本异想体模型</param>
        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);

            if (!infectionTimer.started || !infectionTimer.RunTimer())
            {
                return;
            }

            // 通过计数器实现一次性触发：感染清除本身是协程，执行期间不再重复启动
            if (infectionCounter == 0)
            {
                animscript.StartCoroutine(RemoveInfectionShell());
            }

            if (lobTrigger && lobCounter < CreatureTools.LOB_MAX_VALUE) // 如果lobTrigger被触发且没有达到每日限量，则每周期固定增加lob，增加值为当天异想体数量
            {
                MoneyModel.instance.Add(addings);
                lobCounter += addings;
            }


            EnergyModel.instance.AddEnergy(addings);
            infectionTimer.StartTimer(1f);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 注册监听器。
        /// <para>注意：此处额外订阅了 <see cref="NoticeName.AddSystemLog"/>，但 <see cref="OnNotice"/>
        /// 中并未处理该通知，属于无副作用的冗余订阅。</para>
        /// </summary>
        private void RegisterNotice()
        {
            Notice.instance.Observe(NoticeName.OnAgentDead, this);
            Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 注销监听器，须与 <see cref="RegisterNotice"/> 订阅的通知保持一致。
        /// </summary>
        private void DeregisterNotice()
        {
            Notice.instance.Remove(NoticeName.OnAgentDead, this);
            Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 将需要发送的文本加入队列（利用数组实现）。
        /// <para>队列为定长 FIFO：写满后整体左移一位，丢弃最早的消息。</para>
        /// <para>若当前没有正在播报的协程，则立即启动一个。</para>
        /// </summary>
        /// <param name="text">待播报的文本，为空时直接忽略</param>
        private void EnqueueMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // 如果队列已满，执行左移操作，丢弃最早的消息（FIFO）
            if (messageCount >= CreatureTools.MAX_MESSAGE_COUNT)
            {
                for (int i = 0; i < CreatureTools.MAX_MESSAGE_COUNT - 1; i++)
                {
                    messages[i] = messages[i + 1];
                }
                messageCount = CreatureTools.MAX_MESSAGE_COUNT - 1;
            }

            messages[messageCount] = text;
            messageCount++;

            if (!isProcessingMessages)
            {
                animscript.StartCoroutine(ProcessMessages());
            }
        }

        #endregion

        #region 迭代器
        
        /// <summary>
        /// 依序播报消息队列中的全部文本，每条之间间隔 <see cref="CreatureTools.DEFAULT_DELAY_TIME"/>。
        /// <para>通过 <see cref="isProcessingMessages"/> 保证同一时刻只有一条播报协程；
        /// 并在 <c>finally</c> 中清空队列，即使中途抛异常也不会残留待播消息。</para>
        /// <para>其他方法通过 <c>animscript.StartCoroutine</c> 启动本协程；重复启动时会在开头直接退出。</para>
        /// </summary>
        private IEnumerator ProcessMessages()
        {
            if (isProcessingMessages)
            {
                yield break;
            }

            isProcessingMessages = true;

            try
            {
                int index = 0;
                while (index < messageCount)
                {
                    string text = messages[index];
                    if (!string.IsNullOrEmpty(text))
                    {
                        DialogueSendings.SendMessage(text);
                    }

                    index++;
                    yield return new WaitForSeconds(CreatureTools.DEFAULT_DELAY_TIME);
                }
            }
            finally
            {
                // 无论是否发生异常，处理完成后都清空队列并重置处理标志
                messageCount = 0;
                isProcessingMessages = false;
            }
        }

        /// <summary>
        /// 初始化当日类型，并按此进行后续行动。
        /// <para>先延迟 <paramref name="delayTime"/> 秒，等待关卡完全加载后再读取核心抑制状态；
        /// 随后设置各模式标志（D47 会同时打开 Malkuth/Yesod/Netzach/Hod 四种模式）并播报对应协议。</para>
        /// <para>Hod 目前仅播报提示，尚未实现具体机制。</para>
        /// </summary>
        /// <param name="delayTime">初始化前的等待时间，单位为秒</param>
        private IEnumerator InitDayTypeConfig(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            isD47 = (todayType == CreatureTools.DayType.D47);
            isMalkuth = (todayType == CreatureTools.DayType.MALKUTH) || isD47;
            isYesod = (todayType == CreatureTools.DayType.YESOD) || isD47;
            isNetzach = (todayType == CreatureTools.DayType.NETZACH) || isD47;
            isHod = (todayType == CreatureTools.DayType.HOD) || isD47;

            if (isD47 || isMalkuth || isYesod || isNetzach)
            {
                EnqueueMessage(LocalTexts.SYSTEM_ONLINE_SUPPRESSION);
            }
            else
            {
                EnqueueMessage(LocalTexts.SYSTEM_ONLINE_REGULAR);
            }

            if (isMalkuth)
            {
                EnqueueMessage(LocalTexts.MALKUTH_INIT);
                CreatureTools.LogWorkMap();
            }

            if (isYesod)
            {
                EnqueueMessage(LocalTexts.YESOD_INIT);
                if (!isD47)
                {
                    animscript.StartCoroutine(CreatureTools.ClearPixelDelayed(delayTime));
                    EnqueueMessage(LocalTexts.YESOD_ACTIVATE);
                }
            }

            if (isNetzach)
            {
                string result = CreatureTools.TryUnlockRecover(CreatureTools.StatusType.DayInit);
                if (result != null) EnqueueMessage(result);
            }

            if (isHod)
            {
                EnqueueMessage(LocalTexts.HOD_INIT);
            }


            yield break;
        }

        /// <summary>
        /// 清除感染的计数器外壳。
        /// <para>以 <see cref="infectionCounter"/> 标记批处理正在执行，防止每秒重复启动；
        /// 并在 <c>finally</c> 中归零，保证协程被中断时计数不会泄漏。</para>
        /// </summary>
        private IEnumerator RemoveInfectionShell()
        {
            infectionCounter++;
            try
            {
                yield return CreatureTools.AgentBatchProcess(CreatureTools.RemoveInfection, CreatureTools.DEFAULT_BATCH_SIZE);
            }
            finally
            {
                infectionCounter--;
            }
        }

        #endregion

        #region 字段

        /// <summary>动画脚本引用，同时被用于启动各类协程</summary>
        public EGODispatcherAnim animscript;

        /// <summary>周期服务计时器（感染清除、LOB 与能量注入），固定 1 秒一个周期</summary>
        private readonly Timer infectionTimer = new Timer();

        /// <summary>当天判定出的日期类型，在 <see cref="OnStageStart"/> 中读取一次后不再变化</summary>
        private CreatureTools.DayType todayType;
        /// <summary>关卡开始时缓存的异想体列表，作为 LOB 与每日收益的基数</summary>
        private CreatureModel[] creatureModels;

        /// <summary>感染清除协程的运行计数，非 0 表示正在执行，用于避免重复启动</summary>
        private int infectionCounter = 0;

        /// <summary>当天是否为 Day 47 构筑部（Kether E1），会同时开启其余全部模式</summary>
        private bool isD47;
        /// <summary>是否启用 Malkuth 协议（指令映射表）</summary>
        private bool isMalkuth;
        /// <summary>是否启用 Yesod 协议（清除像素滤镜）</summary>
        private bool isYesod;
        /// <summary>是否启用 Netzach 协议（解除恢复封锁）</summary>
        private bool isNetzach;
        /// <summary>是否启用 Hod 协议（当前仅播报提示，机制未实现）</summary>
        private bool isHod;

        /// <summary>当日是否已执行过首次工作，用于只启动一次 LOB 生成</summary>
        private bool lobTrigger = false;
        /// <summary>每个周期增长的 LOB/能量数量，取自关卡开始时的异想体数量</summary>
        private int addings;
        /// <summary>当日已累计生成的 LOB 数量，达到 <see cref="CreatureTools.LOB_MAX_VALUE"/> 后停止增长</summary>
        private int lobCounter;

        /// <summary>消息数组，容量固定为 <see cref="CreatureTools.MAX_MESSAGE_COUNT"/>，作为环形队列使用</summary>
        private readonly string[] messages = new string[CreatureTools.MAX_MESSAGE_COUNT]; // 消息数组
        /// <summary>当前消息数量，同时作为队尾写入下标</summary>
        private int messageCount = 0;                         // 当前消息数量
        /// <summary>是否正在处理消息队列，用于保证同一时刻只有一个播报协程</summary>
        private bool isProcessingMessages = false;            // 是否正在处理

        #endregion
    }
}