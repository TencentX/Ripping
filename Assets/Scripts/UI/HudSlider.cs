using UnityEngine;
using System.Collections;

public class HudSlider : HudBase
{
	public UISlider slider;
    static private Vector3 MaxVect= new Vector3(10000,0,0);

    override protected bool CanVisible()
    {
        return base.CanVisible() && slider.value > 0;
    }

	virtual public void SetValue(float value)
	{
        active = true;
		value = Mathf.Clamp(value, 0.0f, 1.0f);

		if( slider != null )
		{
			slider.value = value;
			slider.alpha = 1.0f;
		}
	}

    public void Hide()
    {
        if (!active) return;

        active = false;
        slider.cachedTransform.localPosition = MaxVect;
    }
}
