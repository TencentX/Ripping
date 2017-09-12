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
	/// 我的ip
	/// </summary>
	public Text myIp;

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

	// 出生点
	private List<Transform> bornPoses = new List<Transform>();

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
		Canvas canvas = GameObject.FindObjectOfType<Canvas>();
		canvas.gameObject.SetActive(false);
		BoxMgr.instance.Init();
		EventTriggerListener.Get(runBtn.gameObject).onDown = OnDownRun;
		EventTriggerListener.Get(runBtn.gameObject).onUp = OnUpRun;
		EventMgr.instance.AddListener<bool>("SwitchHide", OnSwitchHide);
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener("OnClientDisconnect", OnClientDisconnect);
		EventMgr.instance.AddListener<float>("RefreshRunTime", OnRunTimeRefresh);
		EventMgr.instance.AddListener<int, int>("AddScore", OnAddScore);
		EventMgr.instance.AddListener<bool>("CloseToBox", OnCloseToBox);
		NickNameMgr.instance.Init();
		PanelBase loginPanel = UIMgr.instance.GetOrCreatePanel("p_ui_login_panel");
		GameObject resultPanel = UIMgr.instance.GetPanel("p_ui_result_panel");
		if (resultPanel != null)
			loginPanel.gameObject.SetActive(false);
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
		Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
		foreach (Canvas canvas in canvases)
			canvas.gameObject.SetActive(true);
	}

	private void OnClientDisconnect(string gameEvent)
	{
		Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
		foreach (Canvas canvas in canvases)
			canvas.gameObject.SetActive(false);
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

	/// <summary>
	/// 箱子按钮
	/// </summary>
	public void OnBox()
	{
		EventMgr.instance.TriggerEvent("boxPress");
	}
}
