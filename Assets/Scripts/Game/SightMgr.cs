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

	public void Check(SightController center, float radius, float angle, ref List<SightController> targetsInSight, ref List<SightController> targetsOutSight)
	{
		targetsInSight.Clear();
		targetsOutSight.Clear();
		for (int i = 0; i < targetList.Count; i++)
		{
			if (Check(center, radius, angle, targetList[i]))
				targetsInSight.Add(targetList[i]);
			else
				targetsOutSight.Add(targetList[i]);
		}
	}
	public bool Check(SightController center, float radius, float angle, SightController checkTarget)
	{
		bool inSight = false;
		// 坐标和朝向去除y坐标影响
		Vector3 pos = center.transform.position;
		pos.y = 0;
		Vector3 forward = center.transform.forward;
		forward.y = 0;
		// 自身看得见
		if (checkTarget == center)
			return true;
		Vector3 targetPos = checkTarget.transform.position;
		targetPos.y = 0;
		// 先判断是否相交
		Vector3 delta = targetPos - pos;
		float distance = delta.magnitude;
		if (distance >= radius)
			return inSight;
		// 判断方向与点的夹角是否大于angle的一半
		if (Vector3.Angle(forward, delta) > angle / 2)
			return inSight;
		// 判断视野中有没有遮挡
		if (Physics.Raycast(pos, delta, distance, layerMask))
		    return inSight;
		inSight = true;
		return inSight;
	}
}
