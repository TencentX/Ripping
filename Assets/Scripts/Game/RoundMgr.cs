using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.ObjectModel;

/// <summary>
/// 回合管理器
/// </summary>
public class RoundMgr : NetworkBehaviour
{
	public static RoundMgr instance;

	// 剩余时间
	[SyncVar]
	private float totalTime;

	// 开始时间
	[SyncVar]
	private float startTime;

	// 上次同步时间
	private float lastSyncTime = 0f;

	// 时间差
	private float deltaTime = 0;

	/// <summary>
	/// 一局的时间
	/// </summary>
	public const float ONE_ROUND_TIME = 3000f;

	void Awake()
	{
		instance = this;
	}

	void OnDestroy()
	{
		instance = null;
	}

	void Start()
	{
		totalTime = ONE_ROUND_TIME;
		if (isServer)
			startTime = Time.realtimeSinceStartup;
	}

	void Update()
	{
		if (!isServer)
			return;
		float now = Time.realtimeSinceStartup;
		if (now - lastSyncTime > 1.0f)
		{
			// 每1s同步一次时间
			lastSyncTime = now;
			RpcSetCurrentTime(lastSyncTime);
		}
		if (leftTime <= 0)
			End();
	}

	void End()
	{
		NetManager.singleton.StopHost();
	}

	[ClientRpc]
	void RpcSetCurrentTime(float currentTime)
	{
		if (!hasAuthority)
		{
			deltaTime = Time.realtimeSinceStartup - currentTime;
		}
	}

	public float leftTime
	{
		get
		{
			return Mathf.Max(totalTime - (Time.realtimeSinceStartup - deltaTime - startTime), 0);
		}
	}
}
