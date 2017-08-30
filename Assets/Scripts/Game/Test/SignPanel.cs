using UnityEngine;
using System.Collections;

/// <summary>
/// 标识界面
/// </summary>
public class SignPanel : PanelBase
{
	/// <summary>
	/// 标识按钮
	/// </summary>
	public UIButton signBtn;

	// 拥有者
	private GameObject owner;

	public void SetOwner(GameObject owner)
	{
		this.owner = owner;
		FixedUpdate();
	}

	public void OnSignBtnPress()
	{
		EventMgr.instance.TriggerEvent<GameObject>("OnSignPress", owner);
		gameObject.SetActive(false);
	}

	void FixedUpdate()
	{
		Vector3 pos = Camera.main.WorldToScreenPoint(owner.transform.position);
		pos.z = 0;
		pos = UIMgr.instance.uiCamera.transform.InverseTransformPoint(UIMgr.instance.uiCamera.ScreenToWorldPoint(pos));
		transform.localPosition = pos;
	}
}
