using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class GamePoolMgr : Singleton<GamePoolMgr>
{
    private static Dictionary<string, GamePool> pools = new Dictionary<string, GamePool>();

    private Transform poolRootTransform;

    /// <summary>
    /// 记录每个创建的 GameObject 属于的 池
    /// </summary>
    private Dictionary<GameObject,string>  objectBelongtoPoolPath = new Dictionary<GameObject, string>(); 

     

    public void AddGameObjectBelongToPath(GameObject gObject , string belongtoPath)
    {
        if (gObject == null) return;

        if(!objectBelongtoPoolPath.ContainsKey(gObject))
        {
            objectBelongtoPoolPath[gObject] = belongtoPath;
        }
    }

    public void RemoveGameObjectBelongToPath(GameObject gObject)
    {
        if (gObject == null) return;

        if (objectBelongtoPoolPath.ContainsKey(gObject))
        {
            objectBelongtoPoolPath.Remove(gObject);
        }
    }

    public void CreatePool(string path, Object asset, PoolCreateTactice tactice, int poolNum, int poolSize)
    {
        if (pools.ContainsKey(path)) return;

        if (asset != null)
        {
            pools.Add(path, new GamePool(path,asset, tactice, poolNum, poolSize,path.Contains(PanelNameDefine.UIROOT)?null:GetpoolRootTransform()));
        }
        else
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool, string.Format("devindzhang create pool fail : check path-->{0}", path));
        }
    }
   
    public GameObject Give(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool, string.Format("devindzhang path is null"));
            return null;
        }

        GamePool pool;
        pools.TryGetValue(path, out pool);

        if (pool == null)
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool, string.Format("devindzhang path has no pool : {0}",path));
            return null;
        }else
        {
            return pool.Give();
        }
    }




    private Transform GetpoolRootTransform()
    {
       if(poolRootTransform == null)
       {
            // 全局对象
            GameObject coreRoot = GameObject.Find("CoreRoot");
            if(coreRoot != null)
            {
                GameObject poolRoot = new GameObject("PoolRoot");
                poolRootTransform = poolRoot.transform;
                poolRootTransform.parent = coreRoot.transform;
                return poolRootTransform;
            }
            else
            {
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool,string.Format("devindzhang cannot get coreRoot ?"));
                return null;
            }
        }
       else
       {
           return poolRootTransform;
       }
    }


    public override void Dispose()
    {
        if(objectBelongtoPoolPath!= null)
        {
            objectBelongtoPoolPath.Clear();
        }

        if (pools != null)
        {
            foreach (KeyValuePair<string, GamePool> kv in pools)
            {
                kv.Value.Dispose();
            }
            pools.Clear();
        }
        pools = null;
    }
}

public enum PoolCreateTactice
{
    FixedMaxSize = 1 , //固定上限
    MaxSizeLoopUse=2, //固定上限循环使用
    DynamicMaxSize =3,//动态上限
    ImmediatelyDestory = 4, //立即销毁策略
}

