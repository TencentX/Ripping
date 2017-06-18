using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 立即销毁策略
/// </summary>
public class GamePoolCreateImmediatelyDestoryTactice: GamePoolCreateDynamicMaxTactice
{

    public override void Back(GameObject item)
    {
        if (item == null) return;
        if (usedList.Contains(item))
        {
            usedList.Remove(item);
            if (item)
            {
                int numsize = maxNum - 1;
                List<GameObject> newPool = new List<GameObject>();
                for (int i = 0; i < maxNum; ++i)
                {
                    GameObject oldItem = pool[i];
                    if(oldItem == item) continue;
                    newPool.Add(oldItem);
                }
                maxNum = numsize;
                pool = newPool.ToArray();
                GamePoolMgr.instance.RemoveGameObjectBelongToPath(item);
                Object.Destroy(item);
            }
        }
    }
}

