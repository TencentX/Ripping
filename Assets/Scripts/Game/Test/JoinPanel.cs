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

	public void OnClickJoinBtn()
	{
		if (string.IsNullOrEmpty(LoginPanel.inputName))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		NetManager.singleton.networkAddress = ipText.text;
		NetworkClient client = NetManager.singleton.StartClient();
		if (client != null)
		{
			Exit();
			RankPanel panel = UIMgr.instance.GetOrCreatePanel("p_ui_rank_panel") as RankPanel;
			panel.InitList();
		}
		else
		{
			UIMgr.instance.ShowTipString("加入房间" + ipText.text + "失败!");
		}
	}

	public void OnCloseBtnClick()
	{
		Exit();
		UIMgr.instance.CreatePanel("p_ui_login_panel");
	}
}
