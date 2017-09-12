using UnityEngine;
using System.Collections;

/// <summary>
/// 排名界面
/// </summary>
public class RankPanel : PanelBase
{
	/// <summary>
	/// 排名项
	/// </summary>
	public GameObject itemPrefab;

	/// <summary>
	/// 列表
	/// </summary>
	public UIGrid grid;

	protected override void Init (GameObject obj)
	{
		EventMgr.instance.AddListener("RefreshRank", OnRankRefresh);
	}

	public void InitList()
	{
		grid.transform.DestroyChildren();
		for (int i = 0; i < RankMgr.RANK_NUM; i++)
		{
			GameObject go = GameObject.Instantiate(itemPrefab) as GameObject;
			grid.AddChild(go.transform, false);
			go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			RankInfo rankInfo = new RankInfo();
			rankInfo.rank = -1;
			go.GetComponent<RankItem>().SetData(rankInfo);
		}
		itemPrefab.SetActive(false);
		grid.repositionNow = true;
	}

	private void OnRankRefresh(string gameEvent)
	{
		for (int i = 0; i < RankMgr.RANK_NUM; i++)
		{
			GameObject go = grid.transform.GetChild(i).gameObject;
			RankItem item = go.GetComponent<RankItem>();
			if (i < RankMgr.instance.rankInfos.Count)
			{
				item.SetData(RankMgr.instance.rankInfos[i]);
			}
			else
			{
				go.SetActive(false);
			}
		}
		grid.repositionNow = true;
	}
}
