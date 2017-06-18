using UnityEngine;
using System.Collections;

/// <summary>
/// 跟随对象
/// </summary>
public class FollowPosition : MonoBehaviour 
{
    public const float DEFAULT_VSPEED = 10.0f;
    public const float DEFAULT_HSPEED = 15.0f;
    public const float PLAYER_HEIGHT = 1.25f;

	[Tooltip("跟随的目标")]
	public Transform targetTransform;

	[Tooltip("跟随的偏移")]
	public Vector3 offset = Vector3.zero;

    private Transform thisTransform;

    void Awake()
    {
	    thisTransform = transform;
    }

	void LateUpdate() 
	{
		if (targetTransform != null)
		{
			Vector3 targetPos = targetTransform.position + offset;

			Vector3 newPos = thisTransform.position;

			float changeDis = (thisTransform.position - targetPos).magnitude;
			if (changeDis > 0.3f)
			{
				newPos = targetPos + (thisTransform.position - targetPos) * 0.3f / changeDis;
			}

			thisTransform.position = newPos;
		}
	}
}
