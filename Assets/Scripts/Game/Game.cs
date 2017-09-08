using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 游戏逻辑
/// </summary>
public class Game : MonoBehaviour
{
	/// <summary>
	/// 房间按钮
	/// </summary>
	public Button host;
	public Button client;

	/// <summary>
	/// 我的ip
	/// </summary>
	public Text myIp;

	/// <summary>
	/// ip输入框
	/// </summary>
	public InputField input;

	/// <summary>
	/// 昵称输入框
	/// </summary>
	public InputField playerName;

	/// <summary>
	/// 蹦跑按钮
	/// </summary>
	public Button runBtn;

	/// <summary>
	/// 箱子按钮
	/// </summary>
	public Button boxBtn;

	/// <summary>
	/// 箱子按钮文字
	/// </summary>
	public Text boxText;

	/// <summary>
	/// 玩家蹦跑信息
	/// </summary>
	public Text runText;

	/// <summary>
	/// 玩家分数信息
	/// </summary>
	public Text scoreText;

	// 出生点
	private List<Transform> bornPoses = new List<Transform>();

	public static string inputName = "";

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		GameObject bornPos = GameObject.Find("BornPos");
		for (int i = 0; i < bornPos.transform.childCount; i++)
		{
			Transform pos = bornPos.transform.GetChild(i);
			if (!pos.gameObject.activeSelf)
				continue;
			bornPoses.Add(pos);
			NetworkManager.RegisterStartPosition(pos);
		}
		myIp.text = Network.player.ipAddress;
		OnPlayerNameChange();
		boxBtn.gameObject.SetActive(false);
		BoxMgr.instance.Init();
		EventTriggerListener.Get(runBtn.gameObject).onDown = OnDownRun;
		EventTriggerListener.Get(runBtn.gameObject).onUp = OnUpRun;
		EventMgr.instance.AddListener<bool>("SwitchHide", OnSwitchHide);
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener("OnClientDisconnect", OnClientDisconnect);
		EventMgr.instance.AddListener<float>("RefreshRunTime", OnRunTimeRefresh);
		EventMgr.instance.AddListener<int, int>("AddScore", OnAddScore);
		EventMgr.instance.AddListener<bool>("CloseToBox", OnCloseToBox);
		playerName.onValueChanged.AddListener(delegate {OnPlayerNameChange();});
	}

	void OnDestroy()
	{
		foreach (Transform pos in bornPoses)
			NetManager.UnRegisterStartPosition(pos);
	}

	private void OnSwitchHide(string gameEvent, bool hide)
	{
		if (hide)
		{
			boxText.text = "跳出箱子";
			boxBtn.gameObject.SetActive(true);
		}
		else
		{
			boxText.text = "查看箱子";
		}
	}

	private void OnClientConnect(string gameEvent)
	{
		host.gameObject.SetActive(false);
		client.gameObject.SetActive(false);
		input.gameObject.SetActive(false);
		playerName.gameObject.SetActive(false);
	}

	private void OnClientDisconnect(string gameEvent)
	{
		host.gameObject.SetActive(true);
		client.gameObject.SetActive(true);
		input.gameObject.SetActive(true);
		playerName.gameObject.SetActive(true);
	}

	private void OnRunTimeRefresh(string gameEvent, float runTime)
	{
		runText.text = string.Concat("蹦跑时间：", runTime.ToString());
	}

	private void OnAddScore(string gameEvent, int score, int delta)
	{
		scoreText.text = string.Concat("分数：", score.ToString());
	}

	private void OnCloseToBox(string gameEvent, bool close)
	{
		boxBtn.gameObject.SetActive(close);
	}

	private void OnPlayerNameChange()
	{
		inputName = playerName.text;
	}

	/// <summary>
	/// 创建主机
	/// </summary>
	public void OnClickHost()
	{
		if (string.IsNullOrEmpty(inputName))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		if (NetManager.singleton.StartHost() != null && NetworkServer.active)
			CoinMgr.instance.Init();
	}

	/// <summary>
	/// 连接客户端
	/// </summary>
	public void OnClickClient()
	{
		if (string.IsNullOrEmpty(inputName))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		NetManager.singleton.networkAddress = input.text;
		NetManager.singleton.StartClient();
	}

	/// <summary>
	/// 奔跑
	/// </summary>
	public void OnDownRun(GameObject go)
	{
		EventMgr.instance.TriggerEvent<bool>("runPress", true);
	}
	public void OnUpRun(GameObject go)
	{
		EventMgr.instance.TriggerEvent<bool>("runPress", false);
	}

	/// <summary>
	/// 撕名牌
	/// </summary>
	public void OnCatch()
	{
		EventMgr.instance.TriggerEvent("jumpPress");
	}

	public void OnBox()
	{
		EventMgr.instance.TriggerEvent("boxPress");
	}
}
