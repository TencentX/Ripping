using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HudControl
{
    private HudLabel hudTextName = null;
	private HudLabel hudTextTip = null;
    private HudSlider hudSliderTime = null;
    private Transform hudAnchor;

	private int tipFloatAction = -1;

	const float NAME_OFFSET = 5f;
	const float TIP_OFFSET = 25f;
	const float SLIDER_OFFSET = 20f;
	const float TIP_TIME = 1.0f;
	const float TIP_FLY_HEIGHT = 30f;

    public void Init(Transform parent)
    {
		CharacterController characterController = parent.GetComponent<CharacterController>();
		hudAnchor = CreateAnchor(parent, "HudAnchor", Vector3.up * characterController.height);
    }

    public void Release()
    {
        if (hudTextName != null)
            Object.Destroy(hudTextName.gameObject);
		if (hudTextTip != null)
			Object.Destroy(hudTextTip.gameObject);
		if (hudSliderTime != null)
			Object.Destroy(hudSliderTime.gameObject);      
    }

	public void Show()
	{
		if (hudTextName != null)
			hudTextName.gameObject.SetActive(true);
		if (hudTextTip != null)
			hudTextTip.gameObject.SetActive(true);
		if (hudSliderTime != null)
			hudSliderTime.gameObject.SetActive(true);
	}

	public void Hide()
	{
		if (hudTextName != null)
			hudTextName.gameObject.SetActive(false);
		if (hudTextTip != null)
			hudTextTip.gameObject.SetActive(false);
		if (hudSliderTime != null)
			hudSliderTime.gameObject.SetActive(false);
	}

    public void CreateHudName(string name)
    {
        if (hudTextName == null)
        {
            string _path = "prefabs/uis/p_hud_name";
			hudTextName = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, NAME_OFFSET) as HudLabel;
            UIMgr.instance.SetHudVisible(true, false);
        }
		hudTextName.SetText(name);
    }

	public void ShoweHudTip(string tip)
	{
		if (hudTextTip == null)
		{
			string _path = "prefabs/uis/p_hud_name";
			hudTextTip = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, TIP_OFFSET) as HudLabel;
			UIMgr.instance.SetHudVisible(true, false);
		}
		hudTextTip.offset = TIP_OFFSET;
		hudTextTip.SetText(tip);
		hudTextTip.SetTextVisible(true);
		Scheduler.RemoveSchedule(tipFloatAction);
		tipFloatAction = Scheduler.Create(this, (sche, t, s) => {
			if (hudTextTip == null)
				return;
			if (t >= s)
				hudTextTip.SetTextVisible(false);
			else
				hudTextTip.offset = TIP_OFFSET + t / s * TIP_FLY_HEIGHT;
		}, 0f, TIP_TIME, 0f).actionId;
	}

    public void ShowSliderTime(float current, float max)
    {
		if (current >= max)
		{
			if (hudSliderTime != null)
				UIMgr.instance.SetHudVisible(false, false);
		}
		else
		{
			if (hudSliderTime == null)
	        {
	            string _path = "prefabs/uis/p_hud_hp_slider";
				hudSliderTime = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, SLIDER_OFFSET) as HudSlider;
	            UIMgr.instance.SetHudVisible(true, false);
	        }
			hudSliderTime.SetValue(current / max);
		}
    }

	private Transform CreateAnchor(Transform parent, string name, Vector3 localPosition)
	{
		GameObject go = new GameObject(name);
		Transform goTransform = go.transform;
		goTransform.parent = parent;
		goTransform.localPosition = localPosition;
		goTransform.localScale = Vector3.one;
		goTransform.localRotation = Quaternion.identity;
		return goTransform;
	}
}