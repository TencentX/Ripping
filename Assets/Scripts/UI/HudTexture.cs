using UnityEngine;
using System.Collections;

public class HudTexture : HudBase
{
	public UITexture texture;

    new protected void Awake()
	{
        texture = GetComponentInChildren<UITexture>();
        base.Awake();
	}

	public void SetTexture(Texture2D tex)
	{
        texture.mainTexture = tex;
	}
}
