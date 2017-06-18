
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// init 部分， 无上限
/// eg： 初始化2个，当使用第3个的时候，初始化第3个
/// </summary>
public  class GamePoolCreateDynamicMaxTactice: GamePoolBaseCreateTactice
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
                //LargePool
                LargePoolSize(maxNum+1);
                return Give();// 递归一次
            }
        }
    }

   
}

