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
	}

	void OnDestroy()
	{
		foreach (Transform pos in bornPoses)
			NetworkManager.UnRegisterStartPosition(pos);
	}

	/// <summary>
	/// 创建主机
	/// </summary>
	public void OnClickHost()
	{
		NetworkManager.singleton.StartHost();
	}

	/// <summary>
	/// 连接客户端
	/// </summary>
	public void OnClickClient()
	{
		NetworkManager.singleton.networkAddress = input.text;
		NetworkManager.singleton.StartClient();
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
}
