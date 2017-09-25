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
	private HudTexture hudWarnIcon = null;
    private Transform hudAnchor;

	private List<GameObject> hudGos = new List<GameObject>();
	private int tipFloatAction = -1;
	private int sliderTimeAction = -1;
	private int warnTimeAction = -1;

	const float UNIFIED_OFFSET = 30f;
	const float NAME_OFFSET = UNIFIED_OFFSET + 30f;
	const float SCORE_OFFSET = UNIFIED_OFFSET + 60f;
	const float TIP_OFFSET = UNIFIED_OFFSET + 80f;
	const float SLIDER_OFFSET = UNIFIED_OFFSET + 70f;
	const float ENERGY_OFFSET = UNIFIED_OFFSET + 0f;
	const float WARN_ICON_OFFSET = UNIFIED_OFFSET - 20;
	const float TIP_TIME = 1.0f;
	const float TIP_FLY_HEIGHT = 30f;

    public void Init(Transform parent)
    {
		CharacterController characterController = parent.GetComponent<CharacterController>();
		hudAnchor = CreateAnchor(parent, "HudAnchor", Vector3.up * characterController.height);
    }

    public void Release()
    {
		foreach (GameObject go in hudGos)
			Object.Destroy(go);
    }

	public void Show()
	{
		Scheduler.RemoveSchedule(this);
		foreach (GameObject go in hudGos)
			go.SetActive(true);
	}

	public void Hide()
	{
		foreach (GameObject go in hudGos)
			go.SetActive(false);
	}

    public void CreateHudName(string name)
    {
        if (hudTextName == null)
        {
            string _path = "prefabs/uis/p_hud_name";
			hudTextName = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, NAME_OFFSET) as HudLabel;
            UIMgr.instance.SetHudVisible(true, false);
			hudGos.Add(hudTextName.gameObject);
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
			hudGos.Add(hutTextScore.gameObject);
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
			hudGos.Add(hudTextTip.gameObject);
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
				hudGos.Add(hudSliderTime.gameObject);
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
			hudGos.Add(hudSliderEnergy.gameObject);
		}
		hudSliderEnergy.SetValue(start / max);
	}

	public void ShowWarnIcon(float showTime = -1f)
	{
		if (hudWarnIcon == null)
		{
			string _path = "prefabs/uis/p_hud_warn_texture";
			hudWarnIcon = UIMgr.instance.CreateHud(_path, Camera.main, hudAnchor, WARN_ICON_OFFSET) as HudTexture;
			hudWarnIcon.offsetX = 50f;
			UIMgr.instance.SetHudVisible(true, false);
			hudGos.Add(hudWarnIcon.gameObject);
		}
		hudWarnIcon.Show();
		warnTimeAction = Scheduler.RemoveSchedule(warnTimeAction);
		if (showTime > 0)
		{
			warnTimeAction = Scheduler.Create(this, (sche, t, s) => {
				warnTimeAction = -1;
				hudWarnIcon.Hide();
			}, 0, 0, showTime).actionId;
		}
	}

	public void HideWarnIcon()
	{
		if (warnTimeAction != -1)
			return;
		if (hudWarnIcon != null)
			hudWarnIcon.Hide();
	}

	public bool IsWarnShow()
	{
		if (hudWarnIcon == null)
			return false;
		return hudWarnIcon.active;
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