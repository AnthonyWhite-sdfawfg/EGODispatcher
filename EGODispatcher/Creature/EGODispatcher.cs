using System.Collections;
using System.Text;
using UnityEngine;
using Utils;

namespace Creature
{
	public class EGODispatcher : CreatureBase, IObserver
	{
		public override void OnViewInit(CreatureUnit unit)
		{
			base.OnViewInit(unit);
			animscript = (EGODispatcherAnim)unit.animTarget;
			animscript.SetScript(this);
		}

		public override void OnStageStart()
		{
			base.OnStageStart();
            Timer.StartTimer(1f);// 启动timer
            registerNotice();
            _infectionCounter = 0;// 初始化判定清除感染协程的计数器
            _todayType = CreatureMethods.GetTodayType();// 取当日类型
            AgentList.Set();
            this.animscript.StartCoroutine(DelaySetting4Log());
        }

        public override void OnFinishWork(UseSkill skill)
        {
            base.OnFinishWork(skill);
            CreatureMethods.TryUnlockRecover(_todayType);
            AgentModel agent = skill.agent;
            if (agent.HasEquipment(83400))// 如若配备原型武器，则分发装备
            {
                animscript.StartCoroutine(CreatureMethods.SpawnEquipmentsToInventory(CreatureConsts.EquipmentPlan));
            }
            if (agent.HasEquipment(83211))// 如若配备步枪的特定批次(可以从描述中得知)，则分发饰品
            {
                animscript.StartCoroutine(CreatureMethods.DistributeGiftsToAllAgents());
            }
            animscript.StartCoroutine(CreatureMethods.CreatureProcess(CreatureManager.instance.GetCreatureList()));
        }

		public override void OnStageEnd()
		{
			base.OnStageEnd();
            deregisterNotice();
            AgentList.Clear();// 清理员工名单
		}

		public void OnNotice(string notice, params object[] param)
		{
			if (notice == NoticeName.OnAgentDead)
			{
				AgentList.Update();
			}
            if (notice == NoticeName.OnQliphothOverloadLevelChanged)
            {
                // 仅 Malkuth 或 Day47 才打印
                if (_todayType == CreatureConsts.DayType.MALKUTH || _todayType == CreatureConsts.DayType.D47)
                {
                    CreatureMethods.LogWorkMap();
                }
                if (_todayType == CreatureConsts.DayType.D47 || _todayType == CreatureConsts.DayType.YESOD)
                {
                    int level = CreatureOverloadManager.instance.GetQliphothOverloadLevel();
                    if (level >= 2)
                    {
                        animscript.StartCoroutine(CreatureMethods.ClearPixelDelayed());
                    }
                }
                CreatureMethods.TryUnlockRecover(_todayType);
            }
        }


        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if (!Timer.started || !Timer.RunTimer()) // 从此处开始为计时器周期
            {
                return;
            }
            if (_infectionCounter == 0)// 检测移除感染协程是否正在运行，如果未运行则激活
            {
                this.animscript.StartCoroutine(RemoveInfectionShell(5));
            }
            this.Timer.StartTimer(1f);
        }

        /// <summary>
        /// [通用] OnStageStart() 处为保证 SystemLog 出现设置的延时协程
        /// </summary>
        private IEnumerator DelaySetting4Log()
        {
            yield return new WaitForSeconds(0.5f);
            Notice.instance.Send("AddSystemLog", new object[] { string.Format("[EGODispatcher]今日类型:{0}", _todayType.ToString()) });
            if (_todayType == CreatureConsts.DayType.MALKUTH || _todayType == CreatureConsts.DayType.D47) {
                CreatureMethods.LogWorkMap();
            }
            if (_todayType == CreatureConsts.DayType.YESOD || _todayType == CreatureConsts.DayType.D47)
            {
                this.animscript.StartCoroutine(CreatureMethods.ClearPixelDelayed());
            }
            yield break;
        }


        /// <summary>
        /// [移除感染]移除感染协程计数外壳
        /// </summary>
        /// <param name="batch">内部协程一个批次处理的人数</param>
        // 只做「+1 / -1」& 启动
        // 计数是确保同一时间最多只有一个 RemoveInfection 协程在运行，
        // 避免多个协程并发执行 RemoveInfection 操作从而出现冲突。
        // 虽然timer已经是以1s为单位运作，但是这样避免了后续 RemoveInfection 运行时长超出1s出现的问题。
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
        /// 注册监听器
        /// </summary>
        private void registerNotice()
        {
            Notice.instance.Observe(NoticeName.OnAgentDead, this);
            Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        /// <summary>
        /// 注销监听器
        /// </summary>
        private void deregisterNotice()
        {
            Notice.instance.Remove(NoticeName.OnAgentDead, this);
            Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
        }

        public EGODispatcherAnim animscript;
        private readonly Timer Timer = new Timer();
        private int _infectionCounter = 0;
        private CreatureConsts.DayType _todayType;
  
    }
}
