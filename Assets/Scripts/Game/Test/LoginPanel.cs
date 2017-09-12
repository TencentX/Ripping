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

	public static string inputName = "";

	protected override void Init (GameObject obj)
	{
		nameText.text = NickNameMgr.instance.GetRandomName();
		OnNameChange();
	}

	public void OnClickCreateBtn()
	{
		if (string.IsNullOrEmpty(nameText.text))
		{
			UIMgr.instance.ShowTipString("昵称不能为空！");
			return;
		}
		if (NetManager.singleton.StartHost() != null && NetworkServer.active)
		{
			CoinMgr.instance.Init();
//			GameObject rank = GameObject.Instantiate(NetManager.singleton.spawnPrefabs[2]);
//			NetworkServer.Spawn(rank);
			RankPanel panel = UIMgr.instance.GetOrCreatePanel("p_ui_rank_panel") as RankPanel;
			panel.InitList();
		}
		Exit();
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
