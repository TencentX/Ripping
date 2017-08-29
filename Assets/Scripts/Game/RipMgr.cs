using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 撕扯管理器
/// </summary>
public class RipMgr : Singleton<RipMgr>
{
	/// <summary>
	/// 撕扯目标对象
	/// 场景中存活的地方对象
	/// </summary>
	private List<RipTargetInfo> targetList = new List<RipTargetInfo>();

	/// <summary>
	/// 添加目标
	/// </summary>
	public void AddTarget(GameObject go, float radius)
	{
		RipTargetInfo info = null;
		for (int i = 0; i < targetList.Count; i++)
		{
			if (targetList[i].target == go)
			{
				info = targetList[i];
				break;
			}
		}
		if (info == null)
		{
			info = new RipTargetInfo();
			info.target = go;
			targetList.Add(info);
		}
		info.radius = radius;
	}

	/// <summary>
	/// 移除目标
	/// </summary>
	public void RemoveTarget(GameObject go)
	{
		for (int i = 0; i < targetList.Count; i++)
		{
			if (targetList[i].target == go)
			{
				targetList.RemoveAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// 检测是否抓到
	/// </summary>
	public bool Check(GameObject ripCenter, float radius, float angle, ref GameObject ripTarget)
	{
		bool hit = false;
		ripTarget = null;
		List<RipTargetInfo> targets = targetList;
		// 坐标和朝向去除y坐标影响
		Vector3 pos = ripCenter.transform.position;
		pos.y = 0;
		Vector3 forward = ripCenter.transform.forward;
		forward.y = 0;
		// 算出扇形两边的端点
		float h = angle / 2;
		Vector3 left = Quaternion.AngleAxis(-h, Vector3.up) * (pos + forward * radius);
		Vector3 right = Quaternion.AngleAxis(h, Vector3.up) * (pos + forward * radius);
		for (int i = 0; i < targets.Count; i ++)
		{
			RipTargetInfo targetInfo = targets[i];
			if (targetInfo.target == null)
				continue;
			// 自身不是目标
			if (targetInfo.target == ripCenter)
				continue;
			Vector3 targetPos = targetInfo.target.transform.position;
			targetPos.y = 0;
			// 先判断是否相交
			if (Vector3.Distance(pos, targetPos) >= radius + targetInfo.radius)
				continue;
			// 如果相交，再判断目标圆心到两点的距离是否小于目标圆半径
			if (Vector3.Distance(pos, left) < radius || Vector3.Distance(pos, right) < radius)
			{
				hit = true;
				ripTarget = targetInfo.target;
				break;
			}
		}
		return hit;
	}
}

/// <summary>
/// 撕扯目标信息
/// </summary>
public class RipTargetInfo
{
	/// <summary>
	/// 目标
	/// </summary>
	public GameObject target;

	/// <summary>
	/// 目标的半径
	/// </summary>
	public float radius;

	public RipTargetInfo()
	{

	}
}
