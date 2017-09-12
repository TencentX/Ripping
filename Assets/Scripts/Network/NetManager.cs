using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 网络控制器
/// </summary>
public class NetManager : NetworkManager
{
	#region client
	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
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
		NetManager.singleton.ServerChangeScene(offlineScene);
		UIMgr.instance.CreatePanel("p_ui_result_panel");
	}
	#endregion client

	#region server

	#endregion server
}
