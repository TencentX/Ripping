using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 登录界面
/// </summary>
public class LoginPanel : PanelBase
{
	/// <summary>
	/// 创建房间
	/// </summary>
	public UIButton createBtn;

	/// <summary>
	/// 加入房间
	/// </summary>
	public UIButton joinBtn;

	/// <summary>
	/// 玩家名称
	/// </summary>
	public UILabel nameText;

	public const string PLAYER_NAME = "player_name";

	public static string inputName = "";

	protected override void Init (GameObject obj)
	{
		if (PlayerPrefs.HasKey(PLAYER_NAME))
			inputName = PlayerPrefs.GetString(PLAYER_NAME);
		else
			inputName = NickNameMgr.instance.GetRandomName();
		nameText.text = inputName;
	}

	public void OnClickCreateBtn()
	{
		if (string.IsNullOrEmpty(nameText.text))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		if (NetManager.singleton.client != null)
		{
			// 走到这里，说明点击了加入房间，再点击了创建房间
			// 需要先删除client
			NetManager.singleton.StopClient();
		}
		if (NetManager.singleton.StartHost() != null && NetworkServer.active)
		{
			CoinMgr.instance.Init();
			RankPanel panel = UIMgr.instance.GetOrCreatePanel("p_ui_rank_panel") as RankPanel;
			panel.InitList();
			PlayerPrefs.SetString(PLAYER_NAME, inputName);
			Exit();
		}
		else
		{
			UIMgr.instance.ShowTipString("创建主机失败!");
		}
	}

	public void OnClickJoinBtn()
	{
		UIMgr.instance.CreatePanel("p_ui_join_panel");
		Exit();
	}

	public void OnNameChange()
	{
		inputName = nameText.text;
	}
}
