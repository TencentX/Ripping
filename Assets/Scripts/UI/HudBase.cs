using UnityEngine;
using System.Collections;

public class HudBase : MonoBehaviour {

    protected Transform thisTransform;
    public Camera gameCamera;
    protected Camera uiCamera;
    public Transform followTarget;
    public float offset = 0f;

    public bool active = true;

    protected void Awake(){
        thisTransform = transform;

		if (uiCamera == null)
			uiCamera = UIMgr.instance.uiCamera;//NGUITools.FindCameraForLayer(gameObject.layer);

        if (!UIMgr.instance.MountToHud(thisTransform))
            thisTransform.parent = uiCamera.transform.parent;
		
        thisTransform.localScale = Vector3.one;
    }

	protected void Start () {        
        OnSetup(null, true);
	}

   

    virtual protected bool CanVisible()
    {
        return followTarget != null ;//&& CoreEntry.uiMgr.showHud;
    }

    protected virtual void LateUpdate(){

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

    protected void OnSetup(string gameEvent, bool isSetup)
    {
        //gameObject.SetActive(isSetup);
    }
}
