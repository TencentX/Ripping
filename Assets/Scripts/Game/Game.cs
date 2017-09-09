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
	/// 查看箱子
	/// </summary>
	public Button lookBoxBtn;

	/// <summary>
	/// 跳出箱子
	/// </summary>
	public Button outBoxBtn;

	/// <summary>
	/// 玩家蹦跑信息
	/// </summary>
	public Text runText;

	/// <summary>
	/// 玩家分数信息
	/// </summary>
	public Text scoreText;

	/// <summary>
	/// 倒计时
	/// </summary>
	public Text countDownTime;

	/// <summary>
	/// 一局总时间
	/// </summary>
	public const float ONE_GAME_TIME = 600f;

	// 出生点
	private List<Transform> bornPoses = new List<Transform>();

	public static string inputName = "";

	void Start()
	{
		GameDataMgr.instance.LoadAllData();
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
		lookBoxBtn.gameObject.SetActive(false);
		outBoxBtn.gameObject.SetActive(false);
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
		NickNameMgr.instance.Init();
		playerName.text = NickNameMgr.instance.GetRandomName();
	}

	void Update()
	{
		countDownTime.text = GlobalFunctions.GetFormatTimeString((int)RoundMgr.instance.leftTime);
	}

	void OnDestroy()
	{
		EventMgr.instance.RemoveListener(this);
		foreach (Transform pos in bornPoses)
			NetManager.UnRegisterStartPosition(pos);
	}

	private void OnSwitchHide(string gameEvent, bool hide)
	{
		outBoxBtn.gameObject.SetActive(hide);
		if (hide)
			lookBoxBtn.gameObject.SetActive(false);
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
		runText.text = string.Concat("奔跑时间：", runTime.ToString());
	}

	private void OnAddScore(string gameEvent, int score, int delta)
	{
		scoreText.text = string.Concat("分数：", score.ToString());
	}

	private void OnCloseToBox(string gameEvent, bool close)
	{
		lookBoxBtn.gameObject.SetActive(close);
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
		{
			CoinMgr.instance.Init();
			RoundMgr.instance.Start();
		}
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
