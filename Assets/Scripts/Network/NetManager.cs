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
	#endregion client

	#region server

	#endregion server
}
