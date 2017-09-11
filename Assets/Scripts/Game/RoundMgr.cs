using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.ObjectModel;

/// <summary>
/// 回合管理器
/// </summary>
public class RoundMgr : Singleton<RoundMgr>
{
	// 剩余时间
	private float totalTime;

	// 开始时间
	private float startTime;

	// 是否正在游戏
	private bool isPlaying = false;

	// 回合管理对象
	private GameObject go;

	/// <summary>
	/// 一局的时间
	/// </summary>
	public const float ONE_ROUND_TIME = 600f;

	class Round : MonoBehaviour
	{
		void Update()
		{
			RoundMgr.instance.Update();
		}
	}

	public void Start()
	{
		totalTime = ONE_ROUND_TIME;
		startTime = Time.realtimeSinceStartup;
		isPlaying = true;
		go = GameObject.Find("RoundMgr");
		if (go == null)
			go = new GameObject("RoundMgr");
		go.AddMissingComponent<Round>();
	}

	public void Start(float leftTime)
	{
		totalTime = leftTime;
	}

	public void End()
	{
		isPlaying = false;
//		NetworkServer.DisconnectAll();
		NetManager.singleton.StopHost();
	}

	public void Update()
	{
		if (!isPlaying)
			return;
		if (leftTime <= 0)
			End();
	}

	public float leftTime
	{
		get
		{
			return Mathf.Max(totalTime - (Time.realtimeSinceStartup - startTime), 0);
		}
	}
}
