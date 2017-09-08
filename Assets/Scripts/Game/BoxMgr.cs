using UnityEngine;
using System.Collections;

/// <summary>
/// 箱子管理器
/// </summary>
public class BoxMgr : Singleton<BoxMgr>
{
	// 所有箱子
	private Box[] boxes;

	public void Init()
	{
		boxes = GameObject.FindObjectsOfType<Box>();
	}

	public Box GetBox(int id)
	{
		Box box = null;
		for (int i = 0; i < boxes.Length; i++)
		{
			if (boxes[i].id == id)
			{
				box = boxes[i];
				break;
			}
		}
		return box;
	}

	public Box GetBoxAround(Vector3 pos)
	{
		Box box = null;
		float distance = 0.0f;
		Box[] boxes = GameObject.FindObjectsOfType<Box>();
		for (int i = 0; i < boxes.Length; i++)
		{
			if (box == null)
			{
				box = boxes[i];
				distance = Vector3.Distance(pos, boxes[i].transform.position);
				continue;
			}
			float newdistance = Vector3.Distance(pos, boxes[i].transform.position);
			if (newdistance < distance)
			{
				box = boxes[i];
				distance = newdistance;
			}
		}
		return box;
	}
}
