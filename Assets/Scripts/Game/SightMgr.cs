using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 视野管理器
/// </summary>
public class SightMgr : Singleton<SightMgr>
{
	// 所有目标
	private List<SightController> targetList = new List<SightController>();

	private int layerMask = 1 << LayerMask.NameToLayer("Wall");

	public void AddTarget(SightController controller)
	{
		if (!targetList.Contains(controller))
		{
			targetList.Add(controller);
		}
	}

	public void RemoveTarget(SightController controller)
	{
		if (targetList.Contains(controller))
			targetList.Remove(controller);
	}

	public void Check(SightController center, float sightRange, float angle, float selfRadius, ref List<SightController> targetsInSight, ref List<SightController> targetsOutSight)
	{
		targetsInSight.Clear();
		targetsOutSight.Clear();
		for (int i = 0; i < targetList.Count; i++)
		{
			if (Check(center, sightRange, angle, selfRadius, targetList[i].gameObject))
				targetsInSight.Add(targetList[i]);
			else
				targetsOutSight.Add(targetList[i]);
		}
	}
	public bool Check(SightController center, float sightRange, float angle, float selfRadius, GameObject checkTarget)
	{
		bool inSight = false;
		// 坐标和朝向去除y坐标影响
		Vector3 pos = center.GetSource().position;
		pos.y = 0;
		Vector3 forward = center.GetSource().forward;
		forward.y = 0;
		// 自身看得见
		if (checkTarget == center.gameObject)
			return true;
		Vector3 targetPos = checkTarget.transform.position;
		targetPos.y = 0;
		Vector3 delta = targetPos - pos;
		// 与自身圆是否相交
		if (Vector3.Distance(targetPos, pos) <= selfRadius)
			return true;
		// 与扇形圆是否相交
		float distance = delta.magnitude;
		if (distance >= sightRange)
			return inSight;
		// 判断方向与点的夹角是否大于angle的一半
		if (Vector3.Angle(forward, delta) > angle / 2)
			return inSight;
		// 判断视野中有没有遮挡
		if (Physics.Raycast(center.GetSource().position, delta, distance, layerMask))
		    return inSight;
		inSight = true;
		return inSight;
	}
}
