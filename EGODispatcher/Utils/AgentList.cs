using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Utils
{
	public static class AgentList
	{
		public static ReadOnlyCollection<AgentModel> Agents
		{
			get
			{
				return new ReadOnlyCollection<AgentModel>(AgentList._agents);
			}
		}
		public static void Set()
		{
			AgentList._agents.Clear();
			IList<AgentModel> agentList = AgentManager.instance.GetAgentList();
			for (int i = 0; i < agentList.Count; i++)
			{
				AgentList._agents.Add(agentList[i]);
			}
        }
		public static void Clear()
		{
			AgentList._agents.Clear();
		}
		public static void Update()
		{
			AgentList._agents.Clear();
			IList<AgentModel> agentList = AgentManager.instance.GetAgentList();
			for (int i = 0; i < agentList.Count; i++)
			{
				AgentModel agentModel = agentList[i];
				if (agentModel != null && !agentModel.IsDead())
				{
					AgentList._agents.Add(agentModel);
				}
			}
		}
		public static void LogAgents()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[AgentList] 员工列表：");
			for (int i = 0; i < AgentList._agents.Count; i++)
			{
				AgentModel agentModel = AgentList._agents[i];
				stringBuilder.AppendLine(string.Format("[{0}] {1}  ({2})", i, agentModel.name, agentModel.GetType().Name));
			}
			Notice.instance.Send("AddSystemLog", new object[] { stringBuilder.ToString() });
		}
		public static void LogAgentsBySefira()
		{
			Dictionary<string, List<AgentModel>> dictionary = new Dictionary<string, List<AgentModel>>();
			for (int i = 0; i < AgentList._agents.Count; i++)
			{
				AgentModel agentModel = AgentList._agents[i];
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
		private static List<AgentModel> _agents = new List<AgentModel>();
	}
}
