using System.Collections.Generic;
using UnityEngine;

public  abstract class GamePoolBaseCreateTactice:GamePoolCreateTactice
{

    //当前池的总数量
    protected GameObject[] pool;

    //存正在用的GameObject
    protected LinkedList<GameObject> usedList = new LinkedList<GameObject>();
    //存未使用的GameObject
    protected LinkedList<GameObject> unUsedList = new LinkedList<GameObject>();

    protected GameObject poolParent;
    protected int maxNum;
    protected int initNum;
    protected Object asset;

    protected string belongtopath;

    public void Create(string belongtopath,Object asset, int initNum, int maxNum, GameObject poolParent)
    {
        this.poolParent = poolParent;
        this.asset = asset;
        this.initNum = initNum;
        this.maxNum = maxNum;
        this.belongtopath = belongtopath;

        pool = new GameObject[maxNum];

        for (int i = 0; i < initNum; ++i)
        {
            CreateOne(i);
        }
    }

    protected void CreateOne(int i)
    {
        pool[i] = UnityEngine.Object.Instantiate(asset) as GameObject;
        pool[i].transform.parent = poolParent==null ? null :poolParent.transform;
        pool[i].SetActive(false);
        GamePoolMgr.instance.AddGameObjectBelongToPath(pool[i],belongtopath);
        unUsedList.AddLast(pool[i]);
    }

    protected int GetCreatedNum()
    {
        return usedList.Count + unUsedList.Count;
    }

    protected void LargePoolSize(int poolSize)
    {
        if (pool.Length < poolSize)
        {
            GameObject[] newPool = new GameObject[poolSize];
            for (int i = 0; i < maxNum; ++i)
            {
                newPool[i] = pool[i];
            }
            maxNum = poolSize;
            pool = newPool;
        }
    }

    public virtual  GameObject Give()
    {
       LogMgr.instance.Log(LogLevel.ERROR, LogTag.GamePool, "devindzhang the function must be override");
       return null;
    }

    public virtual void Back(GameObject item)
    {
        if (item == null) return;
        if (usedList.Contains(item))
        {
            usedList.Remove(item);
            unUsedList.AddLast(item);
            if (item)
            {
                item.SetActive(false);
                item.transform.parent = poolParent == null ? null : poolParent.transform;
            }
        }
    }

    public bool IsEmpty()
    {
        return maxNum==0;
    }

    public int GetCount()
    {
        return maxNum;
    }


    public void Dispose()
    {
        for (int i = 0; i < pool.Length; ++i)
        {
            UnityEngine.Object.Destroy(pool[i]);
        }
        usedList.Clear(); usedList = null;
        unUsedList.Clear(); unUsedList = null;
        pool = null;
        poolParent = null;
        asset = null;
    }
}

