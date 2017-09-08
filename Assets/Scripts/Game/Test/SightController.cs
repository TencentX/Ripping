using UnityEngine;
using System.Collections;

/// <summary>
/// 视野控制器
/// </summary>
public class SightController : MonoBehaviour
{
	private Renderer[] renderers;

	private bool lastVisible = true;

	void Start()
	{
		renderers = GetComponentsInChildren<Renderer>();
		SightMgr.instance.AddTarget(this);
	}

	public void BecameVisible()
	{
		if (lastVisible)
			return;
		lastVisible = true;
		foreach (Renderer renderer in renderers)
			renderer.enabled = true;
		gameObject.SendMessage("GetInSight", SendMessageOptions.DontRequireReceiver);
	}
	
	public void BecameInvisible()
	{
		if (!lastVisible)
			return;
		lastVisible = false;
		foreach (Renderer renderer in renderers)
			renderer.enabled = false;
		gameObject.SendMessage("OutOfSight", SendMessageOptions.DontRequireReceiver);
	}

	void OnDestroy()
	{
		SightMgr.instance.RemoveTarget(this);
	}
}
