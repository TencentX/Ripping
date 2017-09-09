using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 金币
/// </summary>
public class Coin : NetworkBehaviour
{
	public enum CoinType
	{
		Big = 1,
		Small = 2,
	}

	/// <summary>
	/// 金币类型
	/// </summary>
	public CoinType type = CoinType.Big;

	[System.NonSerialized]
	public string parentName = "";

	const int BIG_COIN_SCORE = 4;
	const int SMALL_COIN_SCORE = 1;

	void Start()
	{
		if (isClient)
			gameObject.AddMissingComponent<SightController>();
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (!isServer)
			return;
		if (!collider.tag.Equals("Player"))
			return;
		NetworkIdentity identity = collider.GetComponent<NetworkIdentity>();
		if (identity == null)
			return;
		TestController player = collider.GetComponent<TestController>();
		if (player == null)
			return;
		if (CoinMgr.instance.RemoveCoin(this))
		{
			Coin coin = CoinMgr.instance.AddCoin();
			coin.RpcSpawn(coin.GetComponent<NetworkIdentity>().netId, coin.parentName);
		}
		if (type == CoinType.Big)
			player.AddScore(BIG_COIN_SCORE);
		else
			player.AddScore(SMALL_COIN_SCORE);
	}

	[ClientRpc]
	void RpcSpawn(NetworkInstanceId id, string parentName)
	{
		GameObject coin = ClientScene.FindLocalObject(netId);
		if (coin == null)
			return;
		GameObject coinPos = GameObject.Find("CoinPos");
		coin.transform.parent = coinPos.transform.FindChild(parentName);
	}
}
