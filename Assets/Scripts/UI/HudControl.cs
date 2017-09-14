using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HudControl
{
    private HudLabel hudTextName = null;
	private HudLabel hutTextScore = null;
	private HudLabel hudTextTip = null;
    private HudSlider hudSliderTime = null;
	private HudSlider hudSliderEnergy = null;
    private Transform hudAnchor;

	private int tipFloatAction = -1;
	private int sliderTimeAction = -1;

	const float UNIFIED_OFFSET = 30f;
	const float NAME_OFFSET = UNIFIED_OFFSET + 30f;
	const float SCORE_OFFSET = UNIFIED_OFFSET + 60f;
	const float TIP_OFFSET = UNIFIED_OFFSET + 80f;
	const float SLIDER_OFFSET = UNIFIED_OFFSET + 70f;
	const float ENERGY_OFFSET = UNIFIED_OFFSET + 0f;
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
		if (hutTextScore != null)
			Object.Destroy(hutTextScore.gameObject);
		if (hudTextTip != null)
			Object.Destroy(hudTextTip.gameObject);
		if (hudSliderTime != null)
			Object.Destroy(hudSliderTime.gameObject);
		if (hudSliderEnergy != null)
			Object.Destroy(hudSliderEnergy.gameObject);
    }

	public void Show()
	{
		Scheduler.RemoveSchedule(tipFloatAction);
		Scheduler.RemoveSchedule(sliderTimeAction);
		if (hudTextName != null)
			hudTextName.gameObject.SetActive(true);
		if (hutTextScore != null)
			hutTextScore.gameObject.SetActive(true);
		if (hudTextTip != null)
			hudTextTip.gameObject.SetActive(true);
		if (hudSliderTime != null)
			hudSliderTime.gameObject.SetActive(true);
		if (hudSliderEnergy != null)
			hudSliderEnergy.gameObject.SetActive(true);
	}

	public void Hide()
	{
		if (hudTextName != null)
			hudTextName.gameObject.SetActive(false);
		if (hutTextScore != null)
			hutTextScore.gameObject.SetActive(false);
		if (hudTextTip != null)
			hudTextTip.gameObject.SetActive(false);
		if (hudSliderTime != null)
			hudSliderTime.gameObject.SetActive(false);
		if (hudSliderEnergy != null)
			hudSliderEnergy.gameObject.SetActive(false);
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

	public void CreateHudScore(int score)
	{
		if (hutTextScore == null)
		{
			string _path = "prefabs/uis/p_hud_score";
			hutTextScore = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, SCORE_OFFSET) as HudLabel;
			UIMgr.instance.SetHudVisible(true, false);
		}
		hutTextScore.SetText(score.ToString());
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

    public void ShowSliderTime(float start, float max, float zeroToMaxTime, System.Action callback)
    {
		if (start >= max)
		{
			if (hudSliderTime != null)
				hudSliderTime.Hide();
			if (callback != null)
				callback();
		}
		else
		{
			if (hudSliderTime == null)
	        {
	            string _path = "prefabs/uis/p_hud_time_slider";
				hudSliderTime = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, SLIDER_OFFSET) as HudSlider;
	            UIMgr.instance.SetHudVisible(true, false);
	        }
			hudSliderTime.SetValue(start / max);
			float delta = max - start;
			float totalTime = zeroToMaxTime * delta / max;
			sliderTimeAction = Scheduler.Create(this, (sche, t, s) => {
				if (hudSliderTime == null)
					return;
				if (t >= s)
				{
					hudSliderTime.Hide();
					if (callback != null)
						callback();
				}
				else
				{
					hudSliderTime.SetValue((start + t / s * delta) / max);
				}
			}, 0f, totalTime, 0f).actionId;
		}
    }

	public void HideSliderTime()
	{
		if (hudSliderTime != null)
			hudSliderTime.Hide();
		Scheduler.RemoveSchedule(sliderTimeAction);
	}

	public void ShowSliderEnergy(float start, float max)
	{
		if (hudSliderEnergy == null)
		{
			string _path = "prefabs/uis/p_hud_energy_slider";
			hudSliderEnergy = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, ENERGY_OFFSET) as HudSlider;
			UIMgr.instance.SetHudVisible(true, false);
		}
		hudSliderEnergy.SetValue(start / max);
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