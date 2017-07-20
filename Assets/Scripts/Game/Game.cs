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

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		foreach (Transform pos in bornPoses)
			NetworkManager.RegisterStartPosition(pos);
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
}
