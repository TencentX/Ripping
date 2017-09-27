using UnityEngine;
using System.Collections;

/// <summary>
/// 结果界面
/// </summary>
public class ResultPanel : PanelBase
{
	/// <summary>
	/// 排名
	/// </summary>
	public UILabel rank;

	/// <summary>
	/// 分数
	/// </summary>
	public UILabel score;

	protected override void Init (GameObject obj)
	{
		if (TestController.mySelf == null)
		{
			OnContinueClick();
			return;
		}
		int rank = 0;
		int score = 0;
		for (int i = 0; i < RankMgr.instance.rankInfos.Count; i++)
		{
			if (RankMgr.instance.rankInfos[i].id == TestController.mySelf.netId.Value)
			{
				rank = RankMgr.instance.rankInfos[i].rank;
				score = RankMgr.instance.rankInfos[i].score;
				break;
			}
		}
		this.rank.text = string.Format("第{0}名", rank + 1);
		this.score.text = "我的积分：" + score;
		UIMgr.instance.DestroyPanel("p_ui_help_panel");
	}

	public void OnContinueClick()
	{
		Exit();
		LoginPanel loginPanel = UIMgr.instance.GetOrCreatePanel("p_ui_login_panel") as LoginPanel;
		if (loginPanel != null)
			loginPanel.gameObject.SetActive(true);
	}
}
