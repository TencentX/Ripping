using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 加入界面
/// </summary>
public class JoinPanel : PanelBase
{
	/// <summary>
	/// 加入房间
	/// </summary>
	public UIButton joinBtn;
	
	/// <summary>
	/// ip地址
	/// </summary>
	public UILabel ipText;

	// 客户端
	private NetworkClient client;

	public void OnClickJoinBtn()
	{
		if (string.IsNullOrEmpty(LoginPanel.inputName))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		if (client != null)
		{
			UIMgr.instance.ShowTipString("正在连接主机，请稍后！");
			return;
		}
		if (string.IsNullOrEmpty(ipText.GetComponent<UIInput>().value))
		{
			UIMgr.instance.ShowTipString("请输入创建人的房间ID！");
			return;
		}
		NetManager.singleton.networkAddress = ipText.text;
		client = NetManager.singleton.StartClient();
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener("OnClientDisconnect", OnClientError);
	}

	public void OnCloseBtnClick()
	{
		Exit();
		UIMgr.instance.CreatePanel("p_ui_login_panel");
	}

	private void OnClientConnect(string gameEvent)
	{
		ipText.GetComponent<UIInput>().SaveValue();
		client = null;
		Exit();
		RankPanel panel = UIMgr.instance.GetOrCreatePanel("p_ui_rank_panel") as RankPanel;
		panel.InitList();
	}

	private void OnClientError(string gameEvent)
	{
		client = null;
		UIMgr.instance.ShowTipString("查找房间" + ipText.text + "失败!");
	}
}
