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
            AgentList.Set(); // 初始化员工列表
            InitParams(); // 初始化参数
            RegisterNotice(); // 注册相关监听器
            animscript.StartCoroutine(DelaySetting4Log(0.5f)); // 其余初始化
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);
            CreatureUtils.TryUnlockRecover(); // 每次工作后解锁一次恢复机制（若锁定）
            AgentModel agent = skill.agent;
            if (agent.HasEquipment(83400))// 83400：原型武器装备ID - 工作时若配备原型武器，则分发装备
            {
                animscript.StartCoroutine(CreatureUtils.SpawnEquipmentsToInventory(CreatureUtils.EquipmentPlan));
                DialogueUtils.SendMessage(LocalTexts.EGO_DELIVERED);
            }
            if (agent.HasEquipment(83211))// 83211：特定批次步枪装备ID - 工作时如若配备步枪的特定批次(游戏中存在标识)，则分发饰品 & 统一员工发型
            {
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.DistributeGiftToAgent));
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.MakeBald));
                DialogueUtils.SendMessage(LocalTexts.ATTACHMENT_DELIVERED);
            }
            animscript.StartCoroutine(CreatureUtils.CreatureProcess(creatureModels)); //每次工作后增加所有异想体的计数器和 pebox
        }

        public override void OnStageEnd()
        {
            base.OnStageEnd();
            MoneyModel.instance.Add(creatureModels.Length);// 每天结束固定加lob，增加值为当天异想体数量。
            _deathFlag = false;
            DeregisterNotice();
            AgentList.Clear();
        }

        public void OnNotice(string notice, params object[] param)
        {
            if (notice == NoticeName.OnAgentDead)
            {
                AgentList.RemoveDeadAgents();
                _deathCounter++;
                if (_deathCounter >= 2 && !_deathFlag)
                {
                    _deathFlag = true;
                    animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.GetInvincibilityBuf));
                    DialogueUtils.SendMessage(LocalTexts.TOO_MUCH_CASUALTIES);
                }
            }
            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                // 仅 Malkuth 或 D47 才更新工作映射
                if (isMalkuth)
                {
                    DialogueUtils.SendMessage(LocalTexts.MALKUTH_ACTIVATE);
                    CreatureUtils.LogWorkMap();
                }

                // 仅 Yesod 或 D47 才销毁滤镜（且需要融毁等级 >= 2）
                if (isYesod)
                {
                    int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();
                    if (level >= 2)
                    {
                        animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed());
                        DialogueUtils.SendMessage(LocalTexts.YESOD_ACTIVATE);
                    }
                }

                // 每次融毁后解锁一次恢复机制（若锁定）
                CreatureUtils.TryUnlockRecover();
            }
        }


        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if (!CreatureTimer.started || !CreatureTimer.RunTimer()) // 计时器未启动/未到周期 → 跳过后续逻辑（周期为1s）
            {
                return;
            }
            if (_infectionCounter == 0)// 若感染移除协程未运行（计数器为0），则启动协程处理感染
            {
                animscript.StartCoroutine(RemoveInfectionShell());
            }
            EnergyModel.instance.AddEnergy(creatureModels.Length);// 每周期固定增加能量，增加值为当天异想体数量
            CreatureTimer.StartTimer(1f);
        }
        #endregion

        #region 私有方法

        private void InitParams()
        {
            CreatureTimer.StartTimer(1f); // 启动1s周期计时器
            _infectionCounter = 0;// 初始化感染协程计数器
            _deathCounter = 0;
            _deathFlag = false;
            _todayType = CreatureUtils.GetTodayType();// 获取当日业务类型
            creatureModels = CreatureManager.instance.GetCreatureList();// 取当日所有异想体
        }

        /// <summary>
        /// 注册相关监听器
        /// </summary>
        private void RegisterNotice()
        {
            Notice.instance.Observe(NoticeName.OnAgentDead, this);
            Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 注销相关监听器
        /// </summary>
        private void DeregisterNotice()
        {
            Notice.instance.Remove(NoticeName.OnAgentDead, this);
            Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        private IEnumerator DelaySetting4Log(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            string content = string.Format(LocalTexts.SYSTEM_ONLINE, _todayType.ToString());
            DialogueUtils.SendMessage(content);

            isD47 = (_todayType == CreatureUtils.DayType.D47);
            isMalkuth = (_todayType == CreatureUtils.DayType.MALKUTH) || isD47;
            isYesod = (_todayType == CreatureUtils.DayType.YESOD) || isD47;
            isNetzach = (_todayType == CreatureUtils.DayType.NETZACH) || isD47;

            if (isMalkuth)
            {
                DialogueUtils.SendMessage(LocalTexts.MALKUTH_INIT);
                CreatureUtils.LogWorkMap();
                DialogueUtils.SendMessage(LocalTexts.MALKUTH_ACTIVATE);
            }

            if (isYesod)
            {
                DialogueUtils.SendMessage(LocalTexts.YESOD_INIT);
                if (!isD47)  // D47 时不激活 YESOD
                {
                    animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed());
                    DialogueUtils.SendMessage(LocalTexts.YESOD_ACTIVATE);
                }
            }

            if (isNetzach)
            {
                DialogueUtils.SendMessage(LocalTexts.NETZACH_INIT);
            }

            yield break;
        }

        /// <summary>
        /// 感染移除协程的安全外壳：通过计数器控制并发，确保同一时间仅1个运行RemoveInfection的BatchProcess协程存在；
        /// 即使计时器1s触发一次，仍需计数器进行控制以避免RemoveInfection执行时长超过1s导致多协程冲突
        /// </summary>
        /// <param name="batch">单次处理的Agent数量</param>
        private IEnumerator RemoveInfectionShell(int batch = CreatureUtils.DEFAULT_BATCH_SIZE)
        {
            _infectionCounter++;
            try
            {
                // 直接等待 Enumerators 里的纯迭代器
                yield return CreatureUtils.AgentBatchProcess(CreatureUtils.RemoveInfection, batch);
            }
            finally
            {
                _infectionCounter--;
            }
        }

        #endregion

        #region 字段
        public EGODispatcherAnim animscript;

        private readonly Timer CreatureTimer = new Timer();

        // 感染移除协程计数器：确保同一时间仅1个RemoveInfection协程运行，避免并发冲突
        private int _infectionCounter = 0;

        private CreatureUtils.DayType _todayType;

        // 当日所有异想体
        private CreatureModel[] creatureModels;

        private int _deathCounter = 0;

        private bool _deathFlag = false;

        private bool isD47;

        private bool isMalkuth;

        private bool isYesod;

        private bool isNetzach;

        #endregion
    }
}
