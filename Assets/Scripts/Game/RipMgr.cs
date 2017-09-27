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
		for (int i = 0; i < targetList.Count; i ++)
		{
			RipTargetInfo targetInfo = targetList[i];
			if (Check(ripCenter, radius, angle, targetInfo.target, targetInfo.radius))
			{
				hit = true;
				ripTarget = targetInfo.target;
				break;
			}
		}
		return hit;
	}
	public bool Check(GameObject ripCenter, float radius, float angle, GameObject checkTarget, float targetRadius)
	{
		bool hit = false;
		// 坐标和朝向去除y坐标影响
		Vector3 pos = ripCenter.transform.position;
		pos.y = 0;
		Vector3 forward = ripCenter.transform.forward;
		forward.y = 0;
		// 算出扇形两边的端点
		float h = angle / 2;
		Vector3 left = Quaternion.AngleAxis(-h, Vector3.up) * (pos + forward * radius);
		Vector3 right = Quaternion.AngleAxis(h, Vector3.up) * (pos + forward * radius);
		if (checkTarget == null)
			return hit;
		// 自身不是目标
		if (checkTarget == ripCenter)
			return hit;
		Vector3 targetPos = checkTarget.transform.position;
		targetPos.y = 0;
		// 先判断是否相交
		if (Vector3.Distance(pos, targetPos) >= radius + targetRadius)
			return hit;
		// 判断目标是否在前方
		if (Vector3.Dot(forward, targetPos - pos) <= 0)
			return hit;
		// 判断朝向是否基本一致
		if (Vector3.Dot(forward, checkTarget.transform.forward) < 0.342f)
			return hit;
		hit = true;
		return hit;
	}

	/// <summary>
	/// 检测是否碰到
	/// 正向碰到，不包含抓到
	/// </summary>
	public bool CheckHit(GameObject ripCenter, float radius, float angle, ref GameObject hitTarget)
	{
		bool hit = false;
		hitTarget = null;
		for (int i = 0; i < targetList.Count; i ++)
		{
			RipTargetInfo targetInfo = targetList[i];
			if (CheckHit(ripCenter, radius, angle, targetInfo.target, targetInfo.radius))
			{
				hit = true;
				hitTarget = targetInfo.target;
				break;
			}
		}
		return hit;
	}
	public bool CheckHit(GameObject ripCenter, float radius, float angle, GameObject checkTarget, float targetRadius)
	{
		bool hit = false;
		// 坐标和朝向去除y坐标影响
		Vector3 pos = ripCenter.transform.position;
		pos.y = 0;
		Vector3 forward = ripCenter.transform.forward;
		forward.y = 0;
		// 算出扇形两边的端点
		float h = angle / 2;
		Vector3 left = Quaternion.AngleAxis(-h, Vector3.up) * (pos + forward * radius);
		Vector3 right = Quaternion.AngleAxis(h, Vector3.up) * (pos + forward * radius);
		if (checkTarget == null)
			return hit;
		// 自身不是目标
		if (checkTarget == ripCenter)
			return hit;
		Vector3 targetPos = checkTarget.transform.position;
		targetPos.y = 0;
		// 先判断是否相交
		if (Vector3.Distance(pos, targetPos) >= radius + targetRadius)
			return hit;
		// 判断目标是否在前方
		if (Vector3.Dot(forward, targetPos - pos) <= 0)
			return hit;
		// 判断朝向是否基本一致
		if (Vector3.Dot(forward, checkTarget.transform.forward) >= 0.342f)
			return hit;
		hit = true;
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
