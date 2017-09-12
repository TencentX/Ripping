using UnityEngine;
using System.Collections;

/// <summary>
/// 结果界面
/// </summary>
public class ResultPanel : PanelBase
{
	/// <summary>
	/// 分数
	/// </summary>
	public UILabel score;

	/// <summary>
	/// 背景
	/// </summary>
	public GameObject bg;

	protected override void Init (GameObject obj)
	{
		int score = TestController.mySelf != null ? TestController.mySelf.score : 0;
		this.score.text = "我的积分：" + score;
		UIEventListener.Get(bg).onClick = OnBgClick;
	}

	void OnBgClick(GameObject go)
	{
		Exit();
		LoginPanel loginPanel = UIMgr.instance.GetOrCreatePanel("p_ui_login_panel") as LoginPanel;
		if (loginPanel != null)
			loginPanel.gameObject.SetActive(true);
	}
}
