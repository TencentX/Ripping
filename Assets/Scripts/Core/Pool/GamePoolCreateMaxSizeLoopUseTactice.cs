
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 永久最大池，使用上，循环使用。 
/// eg: 初始化2个，当需要使用第三个的时候，强制回收第一个做为第三个
/// </summary>
public class GamePoolCreateMaxSizeLoopUseTactice : GamePoolBaseCreateTactice
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
            int createdNum = unUsedList.Count + usedList.Count;
            if (createdNum < maxNum)
            {
                //没有满池。则创建1个
                CreateOne(createdNum);
                return Give();// 递归一次
            }
            else
            {
                //强制回收第一个作为
                Back(usedList.First.Value);
                return Give();
            }
           
        }
    }
}

