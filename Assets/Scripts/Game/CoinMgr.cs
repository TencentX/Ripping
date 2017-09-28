using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

/// <summary>
/// 金币管理器
/// </summary>
public class CoinMgr : Singleton<CoinMgr>
{
	// 所有金币
	private Dictionary<string, GameObject> coins = new Dictionary<string, GameObject>();

	// 没有金币的位置
	private List<Transform> nonCoinPos = new List<Transform>();

	// 上一个移除的金币位置
	private Transform lastRemoveCoinPos;

	GameObject bigCoinPrefab;
	GameObject smallCoinPrefab;

	// 金币个数
	const int COIN_NUM = 20;

	public void Init()
	{
		coins.Clear();
		nonCoinPos.Clear();
		lastRemoveCoinPos = null;
		for (int i = 0; i < NetManager.singleton.spawnPrefabs.Count; i++)
		{
			GameObject prefab = NetManager.singleton.spawnPrefabs[i];
			if ("coin_big".Equals(prefab.name))
				bigCoinPrefab = prefab;
			else if ("coin_small".Equals(prefab.name))
				smallCoinPrefab = prefab;
		}
		GameObject coinPos = GameObject.Find("CoinPos");
		for (int i = 0; i < coinPos.transform.childCount; i++)
		{
			Transform pos = coinPos.transform.GetChild(i);
			if (!pos.gameObject.activeSelf)
				continue;
			nonCoinPos.Add(pos);
		}
		for (int i = 0; i < COIN_NUM; i++)
		{
			Transform pos = GetOneCoinTransform(null);
			GameObject coin = CreateCoinAtPos(pos);
			if (NetworkServer.active)
				NetworkServer.Spawn(coin);
		}
	}

	public bool RemoveCoin(Coin coin)
	{
		bool success = false;
		Transform parent = coin.transform.parent;
		if (parent != null)
		{
			string name = coin.parentName;
			if (coins.ContainsKey(name))
			{
				coins.Remove(name);
				nonCoinPos.Add(parent);
				lastRemoveCoinPos = parent;
				success = true;
			}
		}
		NetworkServer.Destroy(coin.gameObject);
		return success;
	}

	public Coin AddCoin()
	{
		Transform pos = GetOneCoinTransform(lastRemoveCoinPos);
		Coin coin = CreateCoinAtPos(pos).GetComponent<Coin>();
		NetworkServer.Spawn(coin.gameObject);
		return coin;
	}

	private Transform GetOneCoinTransform(Transform distinctPos)
	{
		int i = 0;
		Transform pos;
		do
		{
			i = Random.Range(0, nonCoinPos.Count - 1);
			pos = nonCoinPos[i];
		}
		while (pos!= null && pos == distinctPos);
		return pos;
	}

	private GameObject CreateCoinAtPos(Transform pos)
	{
		GameObject prefab;
		GameObject coin;
		float rate = Random.Range(0.0f, 1.0f);
		if (rate >= 0.7)
			prefab = bigCoinPrefab;
		else
			prefab = smallCoinPrefab;
		coin = GameObject.Instantiate(prefab, pos.position, Quaternion.identity) as GameObject;
		coin.transform.parent = pos;
		string name = pos.name;
		coin.GetComponent<Coin>().parentName = name;
		nonCoinPos.Remove(pos);
		coins.Add(name, coin);
		return coin;
	}
}
