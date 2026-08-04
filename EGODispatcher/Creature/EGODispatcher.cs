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
            _infectionCounter = 0;
            _todayType = CreatureUtils.GetTodayType();
            creatureModels = CreatureManager.instance.GetCreatureList();
            addings = creatureModels.Length;
            lobCounter = 0;
            lobTrigger = false;
            RegisterNotice();
            AgentList.Set();
            infectionTimer.StartTimer(1f);
            animscript.StartCoroutine(InitDayTypeConfig(CreatureUtils.DEFAULT_DELAY_TIME));
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);

            string result = CreatureUtils.TryUnlockRecover(CreatureUtils.StatusType.Work);
            if (result != null) EnqueueMessage(result);

            AgentModel agent = skill.agent;

            if (agent.HasEquipment(83400))
            {
                animscript.StartCoroutine(CreatureUtils.SpawnEquipmentsToInventory(CreatureUtils.EquipmentPlan));
                EnqueueMessage(LocalTexts.EGO_DELIVERED);
            }

            if (Array.Exists(CreatureUtils.targetIds, id => agent.HasEquipment(id)))
            {
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.DistributeGiftToAgent));
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.MakeBald));
                EnqueueMessage(LocalTexts.ATTACHMENT_DELIVERED);
            }

            if (lobTrigger == false) // 每日首次工作后触发lobTrigger
            {
                lobTrigger = true;
                EnqueueMessage(LocalTexts.GENERATING_LOB);
            }


            animscript.StartCoroutine(CreatureUtils.CreatureProcess(creatureModels));
        }

        public override void OnStageEnd()
        {
            base.OnStageEnd();

            DeregisterNotice();
            AgentList.Clear();
            MoneyModel.instance.Add(creatureModels.Length);
        }

        public void OnNotice(string notice, params object[] param)
        {
            if (notice == NoticeName.OnAgentDead)
            {
                AgentList.RemoveDeadAgents();
            }

            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();

                if (isMalkuth)
                {
                    EnqueueMessage(LocalTexts.MALKUTH_ACTIVATE);
                    CreatureUtils.LogWorkMap();
                }

                if (isYesod && level >= 2)
                {
                    animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed(CreatureUtils.DEFAULT_DELAY_TIME));
                    EnqueueMessage(LocalTexts.YESOD_ACTIVATE);
                }

                string result = CreatureUtils.TryUnlockRecover(CreatureUtils.StatusType.Notice);
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

            if (_infectionCounter == 0)
            {
                animscript.StartCoroutine(RemoveInfectionShell());
            }

            if (lobTrigger && lobCounter < LOB_MAX_VALUE) // 如果lobTrigger被触发且没有达到每日限量，则每周期固定增加lob，增加值为当天异想体数量
            {
                MoneyModel.instance.Add(addings);
                lobCounter += addings;
            }


            EnergyModel.instance.AddEnergy(addings);
            infectionTimer.StartTimer(1f);
        }

        #endregion

        #region 私有方法

        private IEnumerator InitDayTypeConfig(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            isD47 = (_todayType == CreatureUtils.DayType.D47);
            isMalkuth = (_todayType == CreatureUtils.DayType.MALKUTH) || isD47;
            isYesod = (_todayType == CreatureUtils.DayType.YESOD) || isD47;
            isNetzach = (_todayType == CreatureUtils.DayType.NETZACH) || isD47;

            if (isD47 || isMalkuth || isYesod || isNetzach )
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
                CreatureUtils.LogWorkMap();
            }

            if (isYesod)
            {
                EnqueueMessage(LocalTexts.YESOD_INIT);
                if (!isD47)
                {
                    animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed(delayTime));
                    EnqueueMessage(LocalTexts.YESOD_ACTIVATE);
                }
            }

            if (isNetzach)
            {
                string result = CreatureUtils.TryUnlockRecover(CreatureUtils.StatusType.DayInit);
                if (result != null) EnqueueMessage(result);
            }

            yield break;
        }

        private IEnumerator RemoveInfectionShell()
        {
            _infectionCounter++;
            try
            {
                yield return CreatureUtils.AgentBatchProcess(CreatureUtils.RemoveInfection);
            }
            finally
            {
                _infectionCounter--;
            }
        }

        private void RegisterNotice()
        {
            Notice.instance.Observe(NoticeName.OnAgentDead, this);
            Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        private void DeregisterNotice()
        {
            Notice.instance.Remove(NoticeName.OnAgentDead, this);
            Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        private void EnqueueMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            // 如果队列已满，执行左移操作，丢弃最早的消息（FIFO）
            if (_messageCount >= MAX_MESSAGE_COUNT)
            {
                for (int i = 0; i < MAX_MESSAGE_COUNT - 1; i++)
                {
                    _messages[i] = _messages[i + 1];
                }
                _messageCount = MAX_MESSAGE_COUNT - 1;
            }

            _messages[_messageCount] = text;
            _messageCount++;

            if (!_isProcessingMessages)
            {
                animscript.StartCoroutine(ProcessMessages());
            }
        }

        private IEnumerator ProcessMessages()
        {
            if (_isProcessingMessages)
            {
                yield break;
            }

            _isProcessingMessages = true;

            try
            {
                int index = 0;
                while (index < _messageCount)
                {
                    string text = _messages[index];
                    if (!string.IsNullOrEmpty(text))
                    {
                        DialogueUtils.SendMessage(text);
                    }

                    index++;
                    yield return new WaitForSeconds(CreatureUtils.DEFAULT_DELAY_TIME);
                }
            }
            finally
            {
                // 无论是否发生异常，处理完成后都清空队列并重置处理标志
                _messageCount = 0;
                _isProcessingMessages = false;
            }
        }

        #endregion

        #region 字段

        public EGODispatcherAnim animscript;

        private readonly Timer infectionTimer = new Timer();

        private int _infectionCounter = 0;

        private CreatureUtils.DayType _todayType;

        private CreatureModel[] creatureModels;

        private bool isD47;
        private bool isMalkuth;
        private bool isYesod;
        private bool isNetzach;

        private bool lobTrigger = false;
        private int addings;
        private int lobCounter;
        private static readonly int LOB_MAX_VALUE = 300;

        private static readonly int MAX_MESSAGE_COUNT = 10;              // 最大消息数量
        private readonly string[] _messages = new string[MAX_MESSAGE_COUNT]; // 消息数组
        private int _messageCount = 0;                         // 当前消息数量
        private bool _isProcessingMessages = false;            // 是否正在处理

        #endregion
    }
}