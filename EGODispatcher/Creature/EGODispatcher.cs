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

        //开局注册监听器，初始化员工名单，初始化协程计数器
		public override void OnStageStart()
		{
			base.OnStageStart();
            Timer.StartTimer(1f);//启动timer
			Notice.instance.Observe(NoticeName.OnAgentDead, this);//注册监听器
			Notice.instance.Observe(NoticeName.AddSystemLog, this);
            Notice.instance.Observe(NoticeName.OnQliphothOverloadLevelChanged, this);
            _infectionCounter = 0;//初始化判定清除感染协程的计数器
            _todayType = CreatureMethods.GetTodayType();//取当日类型
            AgentList.Set();
            this.animscript.StartCoroutine(DelaySetting4Log());//其余操作，为了让系统提示出现，用协程套了个壳子延迟0.5秒后运行
        }

        //结束工作后
		public override void OnFinishWork(UseSkill skill)
		{
			base.OnFinishWork(skill);
            CreatureMethods.TryUnlockRecover(_todayType);
            AgentModel agent = skill.agent;
			if (agent.HasEquipment(81400))//如若穿着懒狗套装，则分发装备
			{
				animscript.StartCoroutine(CreatureMethods.SpawnEquipmentsToInventory(CreatureConsts.EquipmentPlan));
			}
			if (agent.HasEquipment(81211))//如若穿着EXOSuit的特定批次(可以从描述中得知)，则分发饰品
			{
				animscript.StartCoroutine(CreatureMethods.DistributeGiftsToAllAgents());
			}
		}

        //当日结束，注销监听器，清空员工列表
		public override void OnStageEnd()
		{
			base.OnStageEnd();
			Notice.instance.Remove(NoticeName.OnAgentDead, this);//注销监听器
			Notice.instance.Remove(NoticeName.AddSystemLog, this);
            Notice.instance.Remove(NoticeName.OnQliphothOverloadLevelChanged, this);
            AgentList.Clear();//清理员工名单
		}

        //响应员工死亡，更新员工名单，将死亡员工剔除
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

        //以1秒为单位运作
        public override void OnFixedUpdate(CreatureModel creature)
        {
            base.OnFixedUpdate(creature);
            if (!Timer.started || !Timer.RunTimer())
            {
                return;
            }
            if (_infectionCounter == 0)//每秒检测一次移除感染协程是否正在运行，如果未运行则激活
            {
                this.animscript.StartCoroutine(RemoveInfectionShell(5));
            }
            this.Timer.StartTimer(1f);
        }

        //[通用]OnStageStart()最后延时协程，保证SystemLog出现
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

        //[移除感染]移除感染协程计数外壳：只做「+1 / -1」& 启动
        //计数是确保同一时间最多只有一个 RemoveInfectionShell 协程在运行，
        //避免多个协程并发执行 RemoveInfection 操作可能导致的资源竞争或状态混乱。
        //虽然timer已经是以1s为单位运作，但是这样避免了后续协程运行时长超出1s出现的问题。
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

        public EGODispatcherAnim animscript;
        private readonly Timer Timer = new Timer();
        private int _infectionCounter = 0;
        private CreatureConsts.DayType _todayType;
  
    }
}
