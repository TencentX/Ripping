using UnityEngine;
using System.Collections;

/// <summary>
/// 复活界面
/// </summary>
public class RelivePanel : PanelBase
{
	/// <summary>
	/// 复活提示
	/// </summary>
	public UILabel tip;

	private float startTime;

	public const float RELIVE_TIME = 4f;

	protected override void Init (GameObject obj)
	{
		startTime = Time.unscaledTime;
		Update();
	}

	void Update()
	{
		float delta = Time.unscaledTime - startTime;
		if (delta < 5f)
			tip.text = "你将在[FFFF00]" + (RELIVE_TIME - delta).ToString("F0") + "[-]秒后复活";
		else
			Exit();
	}
}
