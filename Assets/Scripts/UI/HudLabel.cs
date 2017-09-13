using UnityEngine;
using System.Collections;

public class HudLabel : HudBase
{
	private UILabel label;
	private UISprite titleSprite;
	private UITexture titleTexture;

    new protected void Awake()
	{
		label = GetComponent<UILabel>();
        if (label == null)
        {
            label = GetComponentInChildren<UILabel>();
        }

		titleSprite = GetComponentInChildren<UISprite> ();
		titleTexture = GetComponentInChildren<UITexture>();
        base.Awake();
	}

	public void SetText(string text)
	{
		if( label != null )
			label.text = text;
	}

    public void SetTextVisible(bool vis)
    {
        if (label != null)
        {
            label.enabled = vis;
        }
    }

    public void SetSprite(string sprite)
    {
        if (titleSprite != null)
        {
            titleSprite.spriteName = sprite;
            titleSprite.Update();
        }        
    }

    public void SetSprite(int curNameCard)
    {
        SetSprite(curNameCard.ToString());
    }

    public void SetSpriteVisible(bool vis)
    {
        if (titleSprite != null)
        {
            titleSprite.gameObject.SetActive(vis);
        }
    }

	public void SetTexture(Texture tex)
	{
        if (titleTexture != null)
        {
            titleTexture.mainTexture = tex;
        }
	}

    public void SetTextureVisible(bool vis)
    {
        if (titleTexture != null)
        {
            titleTexture.gameObject.SetActive(vis);
        }
    }
}
