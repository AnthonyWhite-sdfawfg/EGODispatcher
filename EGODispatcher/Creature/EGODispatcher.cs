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
            infectionTimer.StartTimer(1f); // 启动1s周期计时器
            RegisterNotice(); // 注册相关监听器
            _infectionCounter = 0;// 初始化感染协程计数器
            _deathCounter = 0;
            _deathFlag = false;
            _todayType = CreatureUtils.GetTodayType();// 获取当日业务类型
            AgentList.Set();// 初始化员工列表
            creatureModels = CreatureManager.instance.GetCreatureList();// 取当日所有异想体
            animscript.StartCoroutine(DelaySetting4Log(0.5f));// 打印相关log，注册监听器需要时间，为保证log得以出现，故延时运行
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);
            CreatureUtils.TryUnlockRecover(_todayType); // 每次工作后解锁一次恢复机制（若锁定）
            AgentModel agent = skill.agent;
            if (agent.HasEquipment(83400))// 83400：原型武器装备ID - 工作时若配备原型武器，则分发装备
            {
                animscript.StartCoroutine(CreatureUtils.SpawnEquipmentsToInventory(CreatureUtils.EquipmentPlan));
            }
            if (agent.HasEquipment(83211))// 83211：特定批次步枪装备ID - 工作时如若配备步枪的特定批次(游戏中存在标识)，则分发饰品 & 统一员工发型
            {
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.DistributeGiftToAgent));
                animscript.StartCoroutine(CreatureUtils.AgentBatchProcess(CreatureUtils.MakeBald));
            }
            animscript.StartCoroutine(CreatureUtils.CreatureProcess(creatureModels)); //每次工作后增加所有异想体的计数器和 pebox
        }

        public override void OnStageEnd()
        {
            base.OnStageEnd();
            MoneyModel.instance.Add(creatureModels.Length);// 每天结束固定加lob，增加值为当天异想体数量。
            _deathFlag = false;
            DeregisterNotice();// 注销相关监听器
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
                    LogUtils.SendLog(LogUtils.Colorize(LogUtils.ColorType.Notice, "[EGODispatcher] 伤亡人数超出阈值，贝利撒留熔炉已启动。"));
                }
            }
            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                // 仅 Malkuth 或 Day47 才更新工作映射
                if (_todayType == CreatureUtils.DayType.MALKUTH || _todayType == CreatureUtils.DayType.D47)
                {
                    CreatureUtils.LogWorkMap();
                }
                // 仅 Yesod 或 Day47 才销毁滤镜
                if (_todayType == CreatureUtils.DayType.D47 || _todayType == CreatureUtils.DayType.YESOD)
                {
                    int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();
                    if (level >= 2)
                    {
                        animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed());
                    }
                }
                CreatureUtils.TryUnlockRecover(_todayType); // 每次融毁后解锁一次恢复机制（若锁定）
            }
        }


        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if (!infectionTimer.started || !infectionTimer.RunTimer()) // 计时器未启动/未到周期 → 跳过后续逻辑（周期为1s）
            {
                return;
            }
            if (_infectionCounter == 0)// 若感染移除协程未运行（计数器为0），则启动协程处理感染
            {
                animscript.StartCoroutine(RemoveInfectionShell());
            }
            EnergyModel.instance.AddEnergy(creatureModels.Length);// 每周期固定增加能量，增加值为当天异想体数量
            infectionTimer.StartTimer(1f);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// [通用] OnStageStart() 处为保证 SystemLog 出现设置的延时协程
        /// </summary>
        private IEnumerator DelaySetting4Log(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            string content = string.Format("[EGODispatcher]今日类型:{0}", _todayType.ToString());
            LogUtils.SendLog(LogUtils.Colorize(LogUtils.ColorType.Notice, content));
            if (_todayType == CreatureUtils.DayType.MALKUTH || _todayType == CreatureUtils.DayType.D47)
            {
                CreatureUtils.LogWorkMap();
            }
            if (_todayType == CreatureUtils.DayType.YESOD || _todayType == CreatureUtils.DayType.D47)
            {
                animscript.StartCoroutine(CreatureUtils.ClearPixelDelayed());
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

        #endregion

        #region 字段
        public EGODispatcherAnim animscript;

        private readonly Timer infectionTimer = new Timer();

        // 感染移除协程计数器：确保同一时间仅1个RemoveInfection协程运行，避免并发冲突
        private int _infectionCounter = 0;

        private CreatureUtils.DayType _todayType;

        // 当日所有异想体
        private CreatureModel[] creatureModels;

        private int _deathCounter = 0;

        private bool _deathFlag = false;

        #endregion
    }
}
