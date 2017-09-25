using UnityEngine;
using System.Collections;

public class HudTexture : HudBase
{
	public UITexture texture;

	public float offsetX = 0f;

	static private Vector3 MaxVect= new Vector3(10000,0,0);

    new protected void Awake()
	{
        texture = GetComponentInChildren<UITexture>();
        base.Awake();
	}

	protected override void LateUpdate ()
	{
		if(!active)return;        
		if (gameCamera == null) gameCamera = Camera.main; 
		if (gameCamera == null) return;
		
		if (followTarget != null)
		{
			Vector3 pos = gameCamera.WorldToViewportPoint(followTarget.position);
			bool isVisible = (gameCamera.orthographic || pos.z > 0f) && (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
			if (isVisible)
			{
				thisTransform.position = uiCamera.ViewportToWorldPoint(pos);
				pos = thisTransform.localPosition;
				pos.x += offsetX;
				pos.y += offset;
				#if false
				pos.x = Mathf.FloorToInt(pos.x);
				pos.y = Mathf.FloorToInt(pos.y);
				#endif
				pos.z = 0f;
				thisTransform.localPosition = pos;
			}
			else 
			{
				thisTransform.localPosition = new Vector3(20000, 0, 0);
			}
		}
	}

	public void SetTexture(Texture2D tex)
	{
        texture.mainTexture = tex;
	}

	public void Show()
	{
		active = true;
	}

	public void Hide()
	{
		if (!active) return;
		
		active = false;
		texture.cachedTransform.localPosition = MaxVect;
	}

}
