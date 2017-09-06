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
	/// ip输入框
	/// </summary>
	public InputField input;

	/// <summary>
	/// 蹦跑按钮
	/// </summary>
	public Button runBtn;

	/// <summary>
	/// 箱子按钮文字
	/// </summary>
	public Text boxText;

	/// <summary>
	/// 玩家信息
	/// </summary>
	public Text infoText;

	// 出生点
	private List<Transform> bornPoses = new List<Transform>();

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
		EventTriggerListener.Get(runBtn.gameObject).onDown = OnDownRun;
		EventTriggerListener.Get(runBtn.gameObject).onUp = OnUpRun;
		EventMgr.instance.AddListener<bool>("SwitchHide", OnSwitchHide);
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener("OnClientDisconnect", OnClientDisconnect);
		EventMgr.instance.AddListener<float>("RefreshRunTime", OnRunTimeRefresh);
	}

	void OnDestroy()
	{
		foreach (Transform pos in bornPoses)
			NetManager.UnRegisterStartPosition(pos);
	}

	private void OnSwitchHide(string gameEvent, bool hide)
	{
		if (hide)
			boxText.text = "跳出箱子";
		else
			boxText.text = "查看箱子";
	}

	private void OnClientConnect(string gameEvent)
	{
		host.gameObject.SetActive(false);
		client.gameObject.SetActive(false);
		input.gameObject.SetActive(false);
	}

	private void OnClientDisconnect(string gameEvent)
	{
		host.gameObject.SetActive(true);
		client.gameObject.SetActive(true);
		input.gameObject.SetActive(true);
	}

	private void OnRunTimeRefresh(string gameEvent, float runTime)
	{
		infoText.text = string.Concat("蹦跑时间：", runTime.ToString());
	}

	/// <summary>
	/// 创建主机
	/// </summary>
	public void OnClickHost()
	{
		NetManager.singleton.StartHost();
	}

	/// <summary>
	/// 连接客户端
	/// </summary>
	public void OnClickClient()
	{
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
