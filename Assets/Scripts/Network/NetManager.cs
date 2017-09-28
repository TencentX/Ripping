using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 网络控制器
/// </summary>
public class NetManager : NetworkManager
{
	/// <summary>
	/// 是否服务器
	/// </summary>
	public static bool isServer;

	/// <summary>
	/// 是否客户端
	/// </summary>
	public static bool isClient;

	#region client
	public override void OnClientConnect(NetworkConnection conn)
	{
		if (conn == null)
			return;
		base.OnClientConnect(conn);
		isClient = true;
		EventMgr.instance.TriggerEvent("OnClientConnect");
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		EventMgr.instance.TriggerEvent("OnClientDisconnect");
	}

	public override void OnStopClient ()
	{
		base.OnStopClient ();
		bool lastConnected = isClient;
		isClient = false;
		if (lastConnected)
		{
			NetManager.singleton.ServerChangeScene(offlineScene);
			UIMgr.instance.CreatePanel("p_ui_result_panel");
		}
	}
	#endregion client

	#region server
	public override void OnStartServer ()
	{
		base.OnStartServer ();
		isServer = true;
	}

	public override void OnStopServer ()
	{
		base.OnStopServer ();
		isServer = false;
	}
	#endregion server
}
