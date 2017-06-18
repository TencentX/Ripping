
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 直接最大池创建策略，永久最大池。
/// eg: 创建2个，需要使用第三个，无法创建
/// </summary>
public class InstaiatePoolCreateFixedMaxSizeTactice: GamePoolBaseCreateTactice
{
    public override GameObject Give()
    {
        //看空闲项目
        LinkedListNode<GameObject> first = unUsedList.First;
        if (first != null)
        {
            unUsedList.RemoveFirst();
            usedList.AddLast(first.Value);
            return first.Value;
        }
        else
        {
            //是否满池
            int createdNum = unUsedList.Count + usedList.Count;
            if (createdNum < maxNum)
            {
                //没有满池。则创建1个
                CreateOne(createdNum);
                return Give();// 递归一次
            }
            else
            {
                LogMgr.instance.Log(LogLevel.WARNING, LogTag.GamePool, string.Format("devindzhang 固定大小池，无法取超过池的数据"));
                return null;
            }
        }
    }

   
}

