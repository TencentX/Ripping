using UnityEngine;
using System.Collections;

public class HudSlider : HudBase
{
	public enum SliderColor
	{
		Red,
		White
	}

	public UISlider slider;
	public UISprite overlay;
	public UISprite overlayLight;
	public GameObject fullEffect;
	public UISprite heroIcon;

	// 控制显示的计时器
	public float showTimer = 0.0f;
	public float showTime = 5.0f;

	public string fullSpriteName;
	public string lightSpriteName;

    static private Vector3 MaxVect= new Vector3(10000,0,0);

    public System.Action HideHudSlider;
	private bool shouldShowHeroIcon;

	public UILabel txtSkillName;
	public UISprite iconSkillBg;

    new protected void Awake()
	{
        base.Awake();
	}
	
	void Update()
	{
        if (!active)return;
	}

    override protected bool CanVisible()
    {
        return base.CanVisible() && slider.value > 0;
    }

	public void ShowHeroIcon(bool shouldShowHeroIcon)
	{
		this.shouldShowHeroIcon = shouldShowHeroIcon;
		if (heroIcon != null)
		{
			heroIcon.gameObject.SetActive(this.shouldShowHeroIcon);
		}
	}

	public void SetValue(float value)
	{
        active = true;
		value = Mathf.Clamp(value, 0.0f, 1.0f);

		if (!string.IsNullOrEmpty(fullSpriteName) && !string.IsNullOrEmpty(lightSpriteName))
		{
			if (value > 0.95f)
			{
				if (slider.foregroundWidget != overlay)
				{
					slider.foregroundWidget = overlay;
				}
				if (overlayLight.gameObject.activeSelf)
				{
					overlayLight.gameObject.SetActive(false);
				}
				if (!overlay.gameObject.activeSelf)
				{
					overlay.gameObject.SetActive(true);
				}
			}
			else
			{
				if (slider.foregroundWidget != overlayLight)
				{
					slider.foregroundWidget = overlayLight;
				}

				if (!overlayLight.gameObject.activeSelf)
				{
					overlayLight.gameObject.SetActive(true);
				}
				if (overlay.gameObject.activeSelf)
				{
					overlay.gameObject.SetActive(false);
				}
			}
		}

		if( slider != null )
		{
			slider.value = value;
			slider.alpha = 1.0f;
		}

		showTimer = showTime;

		if (fullEffect != null)
		{
			fullEffect.SetActive(value >= 1.0f);
		}
	}

    public void Hide()
    {
        if (!active) return;

        active = false;
        showTimer = 0.0f;
        slider.cachedTransform.localPosition = MaxVect;
    }
}
