using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// 排名管理器
/// </summary>
public class RankMgr : NetworkBehaviour
{
	public static RankMgr instance;

	private Dictionary<uint, RankInfo> rankDic = new Dictionary<uint, RankInfo>();

	private Dictionary<uint, bool> rankKey = new Dictionary<uint, bool>();
	public SyncListRankInfo rankInfos = new SyncListRankInfo();

	public const int RANK_NUM = 10;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		rankInfos.Callback = OnRankListChange;
	}

	public override void OnStartClient ()
	{
		base.OnStartClient ();
		rankKey.Clear();
		rankInfos.Callback = OnRankListChange;
	}

	public override void OnStartServer ()
	{
		base.OnStartServer ();
		rankInfos.Clear();
		rankKey.Clear();
		EventMgr.instance.AddListener<NetworkInstanceId>("RefreshScore", OnScoreRefresh);
		EventMgr.instance.AddListener<NetworkInstanceId>("NetworkDestroy", NetworkDestroy);
	}

	private void OnRankListChange(SyncListRankInfo.Operation op, int index)
	{
		EventMgr.instance.TriggerEvent("RefreshRank");
	}

	private void OnScoreRefresh(string gameEvent, NetworkInstanceId id)
	{
		GameObject go = NetworkServer.FindLocalObject(id);
		if (go == null)
			return;
		TestController player = go.GetComponent<TestController>();
		if (player == null)
			return;
		RankInfo rankInfo;
		if (!rankDic.ContainsKey(id.Value))
		{
			rankInfo = new RankInfo();
			rankInfo.id = id.Value;
			rankInfo.name = player.playerName;
			rankInfo.score = player.score;
			rankDic.Add(rankInfo.id, rankInfo);
		}
		else
		{
			rankInfo = rankDic[id.Value];
			rankInfo.score = player.score;
			rankInfo.name = player.playerName;
		}
		CheckChange(rankInfo);
	}

	private void NetworkDestroy(string gameEvent, NetworkInstanceId id)
	{
		RankInfo rankInfo;
		if (rankDic.ContainsKey(id.Value))
		{
			rankInfo = rankDic[id.Value];
			rankInfo.modifyType = 2;
			rankDic.Remove(id.Value);
			CheckChange(rankInfo);
		}
	}

	private void CheckChange(RankInfo rankInfo)
	{
		if (rankInfo.modifyType == 2)
		{
			int index = 0;
			for (int i = 0; i < rankInfos.Count; i++)
			{
				if (rankInfos[i].id == rankInfo.id)
				{
					if (rankKey.ContainsKey(rankInfo.id))
						rankKey.Remove(rankInfo.id);
					rankInfos.RemoveAt(i);
					index = i;
					break;
				}
			}
			for (int i = index + 1; i < rankInfos.Count; i++)
			{
				RankInfo info = rankInfos[i];
				info.rank = info.rank - 1;
				rankInfos.Dirty(i);
			}
			FillChangeRankInfo();
		}
		else if (rankInfos.Count > 0)
		{
			int index = -1;
			for (int i = 0; i < rankInfos.Count; i++)
			{
				RankInfo info = rankInfos[i];
				if (info.score < rankInfo.score)
				{
					rankInfo.rank = info.rank;
					rankInfo.modifyType = 1;
					rankInfos.Insert(i, rankInfo);
					if (!rankKey.ContainsKey(rankInfo.id))
						rankKey.Add(rankInfo.id, true);
					index = i;
					break;
				}
			}
			if (index != -1)
			{
				// 插入了新的排名
				for (int i = index + 1; i < rankInfos.Count; i++)
				{
					RankInfo info = rankInfos[i];
					info.rank = info.rank + 1;
					rankInfos.Dirty(i);
				}
				// 移除重复的变化值
				index = -1;
				for (int i = rankInfos.Count - 1; i >= 0; i--)
				{
					RankInfo info = rankInfos[i];
					if (info.id == rankInfo.id)
					{
						info.modifyType = 2;
						rankInfos.RemoveAt(i);
						index = i;
						if (rankKey.Count != rankInfos.Count)
						{
							var a = 1;
						}
						break;
					}
				}
				if (index != -1)
				{
					// 删除了排名
					for (int i = index; i < rankInfos.Count; i++)
					{
						RankInfo info = rankInfos[i];
						info.rank = info.rank + 1;
						rankInfos.Dirty(i);
					}
					if (rankKey.Count != rankInfos.Count)
					{
						var a = 1;
					}
				}
			}
			if (rankInfos.Count > RANK_NUM)
			{
				// 删除过长的排名
				RankInfo info = rankInfos[rankInfos.Count - 1];
				info.modifyType = 2;
				rankInfos.RemoveAt(rankInfos.Count - 1);
				if (rankKey.ContainsKey(info.id))
					rankKey.Remove(info.id);
			}
			else
			{
				FillChangeRankInfo();
			}
		}
		else
		{
			rankInfo.rank = 0;
			rankInfo.modifyType = 1;
			rankInfos.Add(rankInfo);
			if (!rankKey.ContainsKey(rankInfo.id))
				rankKey.Add(rankInfo.id, true);
		}
	}

	private void FillChangeRankInfo()
	{
		while (rankInfos.Count < rankDic.Count && rankInfos.Count < RANK_NUM)
		{
			// 补足排名
			RankInfo max = new RankInfo();
			max.rank = -1;
			foreach (RankInfo info in rankDic.Values)
			{
				if (rankKey.ContainsKey(info.id))
					continue;
				if (max.rank == -1)
					max = info;
				if (max.score < info.score)
					max = info;
			}
			if (max.rank == -1)
				break;
			max.rank = rankInfos.Count;
			max.modifyType = 1;
			rankInfos.Add(max);
			rankKey.Add(max.id, true);
		}
	}
	
	void OnDestroy()
	{
		instance = null;
		EventMgr.instance.RemoveListener(this);
	}
}

public struct RankInfo
{
	public uint id;
	public int rank;
	public string name;
	public int score;
	public int modifyType;//1为增加，2为删除
}
public class SyncListRankInfo : SyncListStruct<RankInfo>
{
	
}
