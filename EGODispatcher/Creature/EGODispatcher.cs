using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Creature
{
    public class EGODispatcher : CreatureBase, IObserver
    {
        #region 钩子

        public override void OnViewInit(CreatureUnit unit)
        {
            base.OnViewInit(unit);
            animscript = (EGODispatcherAnim)unit.animTarget;
            animscript.SetScript(this);
        }

        public override void OnStageStart()
        {
            base.OnStageStart();
            infectionCounter = 0;
            todayType = CreatureTools.GetTodayType();
            creatureModels = CreatureManager.instance.GetCreatureList();
            addings = creatureModels.Length;
            lobCounter = 0;
            lobTrigger = false;
            RegisterNotice();
            ActiveAgentManager.Set();
            infectionTimer.StartTimer(1f);
            animscript.StartCoroutine(InitDayTypeConfig(CreatureTools.DEFAULT_DELAY_TIME));
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);

            string result = CreatureTools.TryUnlockRecover(CreatureTools.StatusType.Work);
            if (result != null) EnqueueMessage(result);

            AgentModel agent = skill.agent;

            if (agent.HasEquipment(83400))
            {
                animscript.StartCoroutine(CreatureTools.SpawnEquipmentsToInventory(CreatureTools.EquipmentPlan));
                EnqueueMessage(LocalTexts.EGO_DELIVERED);
            }

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

        public override void OnStageEnd()
        {
            base.OnStageEnd();

            DeregisterNotice();
            ActiveAgentManager.Clear();
            MoneyModel.instance.Add(creatureModels.Length);
        }

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

                if (isYesod && level >= 2)
                {
                    animscript.StartCoroutine(CreatureTools.ClearPixelDelayed(CreatureTools.DEFAULT_DELAY_TIME));
                    EnqueueMessage(LocalTexts.YESOD_ACTIVATE);
                }

                string result = CreatureTools.TryUnlockRecover(CreatureTools.StatusType.Notice);
                if (result != null) EnqueueMessage(result);
            }
        }

        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);

            if (!infectionTimer.started || !infectionTimer.RunTimer())
            {
                return;
            }

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
        /// 注册监听器
        /// </summary>
        private void RegisterNotice()
        {
            Notice.instance.Observe(NoticeName.OnAgentDead, this);
            Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 注销监听器
        /// </summary>
        private void DeregisterNotice()
        {
            Notice.instance.Remove(NoticeName.OnAgentDead, this);
            Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 将需要发送的文本加入队列（利用数组实现）
        /// </summary>
        /// <param name="text"></param>
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
        /// 发送文本
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
        /// 初始化当日类型，并按此进行后续行动
        /// </summary>
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
        /// 清除感染的计数器外壳
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

        public EGODispatcherAnim animscript;
        private readonly Timer infectionTimer = new Timer();

        private CreatureTools.DayType todayType;
        private CreatureModel[] creatureModels;

        private int infectionCounter = 0;

        private bool isD47;
        private bool isMalkuth;
        private bool isYesod;
        private bool isNetzach;
        private bool isHod;

        private bool lobTrigger = false;
        private int addings;
        private int lobCounter;

        private readonly string[] messages = new string[CreatureTools.MAX_MESSAGE_COUNT]; // 消息数组
        private int messageCount = 0;                         // 当前消息数量
        private bool isProcessingMessages = false;            // 是否正在处理

        #endregion
    }
}