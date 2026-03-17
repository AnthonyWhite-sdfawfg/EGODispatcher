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
            Timer.StartTimer(1f); // 启动1s周期计时器
            RegisterNotice(); // 注册相关监听器
            _infectionCounter = 0;// 初始化感染协程计数器
            _todayType = CreatureMethods.GetTodayType();// 获取当日业务类型
            AgentList.Set();// 初始化当日参与EGO分发的员工列表
            creatureModels = CreatureManager.instance.GetCreatureList();// 取当日所有异想体
            this.animscript.StartCoroutine(DelaySetting4Log(0.5f));// 打印相关log，注册监听器需要时间，为保证log得以出现，故延时运行
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);
            CreatureMethods.TryUnlockRecover(_todayType); // 每次工作后解锁一次恢复机制（若锁定）
            AgentModel agent = skill.agent;
            if (agent.HasEquipment(83400))// 83400：原型武器装备ID - 工作时若配备原型武器，则分发装备
            {
                animscript.StartCoroutine(CreatureMethods.SpawnEquipmentsToInventory(CreatureConsts.EquipmentPlan));
            }
            if (agent.HasEquipment(83211))// 83211：特定批次步枪装备ID - 工作时如若配备步枪的特定批次(游戏中存在标识)，则分发饰品
            {
                animscript.StartCoroutine(CreatureMethods.DistributeGiftsToAllAgents());
            }
            animscript.StartCoroutine(CreatureMethods.CreatureProcess(creatureModels));
        }

        public override void OnStageEnd()
        {
            base.OnStageEnd();
            MoneyModel.instance.Add(creatureModels.Length * 10);// 每天结束固定加lob,，增加值为当天异想体数量*10。
            DeregisterNotice();// 注销相关监听器
            AgentList.Clear();// 清空当日参与EGO分发的员工列表
        }

        public void OnNotice(string notice, params object[] param)
        {
            if (notice == NoticeName.OnAgentDead)
            {
                AgentList.Update();// 更新当日参与EGO分发的员工列表
            }
            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                // 仅 Malkuth 或 Day47 才更新工作映射
                if (_todayType == CreatureConsts.DayType.MALKUTH || _todayType == CreatureConsts.DayType.D47)
                {
                    CreatureMethods.LogWorkMap();
                }
                // 仅 Yesod 或 Day47 才销毁滤镜
                if (_todayType == CreatureConsts.DayType.D47 || _todayType == CreatureConsts.DayType.YESOD)
                {
                    int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();
                    if (level >= 2)
                    {
                        animscript.StartCoroutine(CreatureMethods.ClearPixelDelayed());
                    }
                }
                CreatureMethods.TryUnlockRecover(_todayType); // 每次融毁后解锁一次恢复机制（若锁定）
            }
        }


        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if (!Timer.started || !Timer.RunTimer()) // 计时器未启动/未到周期 → 跳过后续逻辑（每1s执行一次感染检测）
            {
                return;
            }
            if (_infectionCounter == 0)// 若感染移除协程未运行（计数器为0），则启动协程处理感染
            {
                this.animscript.StartCoroutine(RemoveInfectionShell(5));
            }
            EnergyModel.instance.AddEnergy(creatureModels.Length);// 每秒固定增加1能量
            this.Timer.StartTimer(1f);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// [通用] OnStageStart() 处为保证 SystemLog 出现设置的延时协程
        /// </summary>
        private IEnumerator DelaySetting4Log(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Notice.instance.Send("AddSystemLog", new object[] { string.Format("[EGODispatcher]今日类型:{0}", _todayType.ToString()) });
            if (_todayType == CreatureConsts.DayType.MALKUTH || _todayType == CreatureConsts.DayType.D47)
            {
                CreatureMethods.LogWorkMap();
            }
            if (_todayType == CreatureConsts.DayType.YESOD || _todayType == CreatureConsts.DayType.D47)
            {
                this.animscript.StartCoroutine(CreatureMethods.ClearPixelDelayed());
            }
            yield break;
        }


        /// <summary>
        /// 感染移除协程的安全外壳：通过计数器控制并发，确保同一时间仅1个RemoveInfection协程运行；
        /// 即使计时器1s触发一次，仍需计数器兜底，避免RemoveInfection执行时长超过1s导致多协程冲突
        /// </summary>
        /// <param name="batch">单次处理的Agent数量</param>
        private IEnumerator RemoveInfectionShell(int batch)
        {
            _infectionCounter++;
            try
            {
                // 直接等待 Enumerators 里的纯迭代器
                yield return CreatureMethods.RemoveInfection(batch);
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
        /// <summary>
        /// 计时器：控制感染检测的周期
        /// </summary>
        private readonly Timer Timer = new Timer();
        /// <summary>
        /// 感染移除协程计数器：确保同一时间仅1个RemoveInfection协程运行，避免并发冲突
        /// </summary>
        private int _infectionCounter = 0;
        /// <summary>
        /// 当日业务类型，决定不同场景的分支逻辑
        /// </summary>
        private CreatureConsts.DayType _todayType;

        private CreatureModel[] creatureModels;

        #endregion
    }
}
