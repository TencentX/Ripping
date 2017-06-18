
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象的池管理
/// </summary>
public class GamePool : IDisposable
{
    //池的应用策略
    private GamePoolCreateTactice tactics;

    private GameObject poolParent;

    public GamePool(string itemPath, UnityEngine.Object prefab, PoolCreateTactice tactice, int poolNum, int poolSize, Transform rooTransform)
    {
        poolSize = Mathf.Max(poolNum, poolSize);

        if (rooTransform == null)
        {
            poolParent = null;
        }
        else
        {
            poolParent = new GameObject(prefab.name + "_pool");

            poolParent.transform.parent = rooTransform;
        }


        switch (tactice)
        {
            case PoolCreateTactice.DynamicMaxSize:
                tactics = new GamePoolCreateDynamicMaxTactice();
                break;
            case PoolCreateTactice.FixedMaxSize:
                tactics = new InstaiatePoolCreateFixedMaxSizeTactice();
                break;
            case PoolCreateTactice.MaxSizeLoopUse:
                tactics = new GamePoolCreateMaxSizeLoopUseTactice();
                break;
            case PoolCreateTactice.ImmediatelyDestory:
                tactics = new GamePoolCreateImmediatelyDestoryTactice();
                break;
            default:
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool, string.Format("devindzhang un catch case : {0}", tactice));
                break;
        }
        tactics.Create(itemPath, prefab, poolNum, poolSize, poolParent);
    }

    #region give and back

    public GameObject Give()
    {
        return tactics.Give();
    }


    public void Back(GameObject item)
    {
        tactics.Back(item);
    }


    public bool IsEmpty()
    {
        return tactics.IsEmpty();
    }

    #endregion


    public void Dispose()
    {
        tactics.Dispose();
        if (poolParent != null) UnityEngine.Object.Destroy(poolParent);
        poolParent = null;
    }
}