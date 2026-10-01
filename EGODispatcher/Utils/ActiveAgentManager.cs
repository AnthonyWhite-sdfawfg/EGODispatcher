using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Utils
{
    /// <summary>
    /// AgentManager.instance.GetAgentList() 取得结果既包括存活员工也包括死亡员工，因此建表来单独管理员工。
    /// <para>本表在关卡开始时通过 <see cref="Set"/> 建立快照，并监听员工死亡事件实时剔除，
    /// 供批量下发饰品、清除感染等操作安全遍历。</para>
    /// </summary>
	public static class ActiveAgentManager
	{
		/// <summary>
		/// 当前存活员工的只读视图。
		/// <para>每次访问都会新建包装对象，因此可安全地在遍历过程中断言集合不会被外部修改。</para>
		/// </summary>
		public static ReadOnlyCollection<AgentModel> Agents
		{
			get
			{
				return new ReadOnlyCollection<AgentModel>(activeAgents);
			}
		}

		/// <summary>
		/// 重建员工表：清空后从游戏接口重新载入全部员工。
		/// <para>此时可能包含已死亡员工，需依靠 <see cref="RemoveDeadAgents"/> 后续清理。</para>
		/// </summary>
		public static void Set()
		{
			activeAgents.Clear();
			IList<AgentModel> agentList = AgentManager.instance.GetAgentList();
			for (int i = 0; i < agentList.Count; i++)
			{
				activeAgents.Add(agentList[i]);
			}
        }

		/// <summary>
		/// 清空员工表，通常在关卡结束时调用以释放引用。
		/// </summary>
		public static void Clear()
		{
			activeAgents.Clear();
		}

        /// <summary>
        /// 移除已死亡或引用为空的员工。
        /// <para>由 <c>OnAgentDead</c> 通知触发，保证批量处理时不会访问到已死亡单位。</para>
        /// </summary>
        public static void RemoveDeadAgents()
        {
            // 从后向前遍历，安全移除死亡或空引用
            for (int i = activeAgents.Count - 1; i >= 0; i--)
            {
                AgentModel agent = activeAgents[i];
                if (agent == null || agent.IsDead())
                {
                    activeAgents.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 将当前员工表输出到系统日志，用于调试。
        /// </summary>
        public static void LogAgents()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[AgentList] 员工列表：");
			for (int i = 0; i < activeAgents.Count; i++)
			{
				AgentModel agentModel = activeAgents[i];
				stringBuilder.AppendLine(string.Format("[{0}] {1}  ({2})", i, agentModel.name, agentModel.GetType().Name));
			}
			Notice.instance.Send("AddSystemLog", new object[] { stringBuilder.ToString() });
		}

        /// <summary>
        /// 将当前员工表按所属部门分组输出到系统日志，用于调试。
        /// <para>部门名称取自 SefiraManager，取不到时归入"未知部门"；分组按名称排序。</para>
        /// </summary>
        public static void LogAgentsBySefira()
		{
			Dictionary<string, List<AgentModel>> dictionary = new Dictionary<string, List<AgentModel>>();
			for (int i = 0; i < activeAgents.Count; i++)
			{
				AgentModel agentModel = activeAgents[i];
				string currentSefira = agentModel.currentSefira;
				Sefira sefira = SefiraManager.instance.GetSefira(currentSefira);
				string key = ((sefira != null) ? sefira.name : "未知部门");
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new List<AgentModel>();
				}
				dictionary[key].Add(agentModel);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[AgentList] 按部门分类");
			List<string> list = new List<string>(dictionary.Keys);
			list.Sort();
			for (int j = 0; j < list.Count; j++)
			{
				string text = list[j];
				stringBuilder.AppendLine(string.Format("--- {0} ---", text));
				List<AgentModel> list2 = dictionary[text];
				for (int k = 0; k < list2.Count; k++)
				{
					AgentModel agentModel2 = list2[k];
					stringBuilder.AppendLine(string.Format("  [{0}] {1}  ({2})", k, agentModel2.name, agentModel2.GetType().Name));
				}
			}
			Notice.instance.Send("AddSystemLog", new object[] { stringBuilder.ToString() });
		}

		/// <summary>存活员工表本体，仅通过上述方法增删</summary>
		private static readonly List<AgentModel> activeAgents = new List<AgentModel>();
	}
}
