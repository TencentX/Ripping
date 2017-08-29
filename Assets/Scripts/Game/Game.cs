using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 游戏逻辑
/// </summary>
public class Game : MonoBehaviour
{
	/// <summary>
	/// 出生点
	/// </summary>
	public Transform[] bornPoses;

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

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		foreach (Transform pos in bornPoses)
			NetworkManager.RegisterStartPosition(pos);
		EventTriggerListener.Get(runBtn.gameObject).onDown = OnDownRun;
		EventTriggerListener.Get(runBtn.gameObject).onUp = OnUpRun;
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener("OnClientDisconnect", OnClientDisconnect);
	}

	void OnDestroy()
	{
		foreach (Transform pos in bornPoses)
			NetManager.UnRegisterStartPosition(pos);
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
}
