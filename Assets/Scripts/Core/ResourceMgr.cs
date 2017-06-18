using System;
using UnityEngine;
using Object = UnityEngine.Object;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class ResourceMgr : Singleton<ResourceMgr>
{
    public void LoadResource(string path, ResourceType rType, System.Action<Object> callback)
    {

    }

    public GameObject InstantiateUIObject(string path)
    {
        Object obj = Resources.Load(path);
        return Object.Instantiate(obj) as GameObject;
    }

    public GameObject InstantiateGameObject(string path)
    {
        Object obj = Resources.Load(path);
        return Object.Instantiate(obj) as GameObject;
    }

    public void InstantiateObject(string path, ResourceType rType, System.Action<GameObject> callback)
    {
        Action<Object> action = o =>
        {
            callback(Object.Instantiate(o) as GameObject);
        };

        LoadResource(path, rType, action);
    }

    public TextAsset LoadConfig(string path)
    {
        return LoadConfig(path, false, null);
    }

    public TextAsset LoadConfig(string path, bool isAync)
    {
        return LoadConfig(path, isAync, null);
    }

    public TextAsset LoadConfig(string path, bool isAync, System.Action<TextAsset> callback)
    {
        if (!isAync)
        {
            TextAsset textAsset = LoadResourceSync(path, ResourceType.Config) as TextAsset;
            if (callback != null) callback(textAsset);
            return textAsset;
        }
        else
        {
            Action<Object> actions = (textasset) =>
            {
                if (callback != null)
                {
                    callback(textasset as TextAsset);
                }
                else
                {
                    LogMgr.instance.Log(LogLevel.ERROR, LogTag.ResourceMgr, string.Format("devindzhang LoadConfig use Aync but no callback check code ?"));
                }

            };


            LoadResourceAsyn(path, ResourceType.Config, actions);
            return null;
        }
    }

    /// <summary>
    /// 同步加载接口  同步加载是适用于  txt.xml uiprefab 等资源量比较少的加载，不适合 英雄，宠物等大资源的加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rType"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadResourceSync(string path, ResourceType rType)
    {
        return Resources.Load(path);
    }
    /// <summary>
    /// 异步记载接口  用于 大批量资源的更新和加载。 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rType"></param>
    /// <param name="callback"></param>
    public void LoadResourceAsyn(string path, ResourceType rType, System.Action<Object> callback)
    {
        Object obj = Resources.Load(path);
        if (callback != null)
        {
            callback(obj);
        }
    }

    /// <summary>
    /// 同步加载图片资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Texture LoadTexture(string path)
    {
        if ( null == path || "" == path )
        {
            return null;
        }
        
        return LoadResourceSync(path, ResourceType.RawTexure) as Texture;
    }

    /// <summary>
    /// GameObject prfab 加载 默认 异步
    /// TODO: 资源加载字符串提取一下，做成类似stringlist那样
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isAync"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public GameObject InstantiateGameObject(string path, Action<GameObject> callback)
    {
        return InstantiateGameObject(path, callback, true, PoolCreateTactice.ImmediatelyDestory, 1, 1);
    }
    /// <summary>
    /// 同步Get对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rType"></param>
    /// <param name="tactice"></param>
    /// <param name="poolNum"></param>
    /// <param name="poolSize"></param>
    /// <returns></returns>
    private GameObject InstantiateObjectSync(string path, ResourceType rType, PoolCreateTactice tactice = PoolCreateTactice.DynamicMaxSize, int poolNum = 1, int poolSize = 1)
    {
        Object asset = LoadResourceSync(path, rType);
        GamePoolMgr.instance.CreatePool(path, asset, tactice, poolNum, poolSize);

        GameObject gameObject = GamePoolMgr.instance.Give(path);
        if (gameObject != null) gameObject.SetActive(true);
        return gameObject;
    }

    public GameObject InstantiateGameObject(string path, Action<GameObject> callback, bool isAync, PoolCreateTactice tactice, int poolNum, int poolSize)
    {
        if (!isAync)
        {
            GameObject obj = InstantiateObjectSync(path, ResourceType.Prefab, tactice, poolNum, poolSize);
            if (callback != null)
            {
                callback(obj);
            }
            return obj;
        }
        else
        {
            Action<GameObject> actions = (gObject) =>
            {
                if (callback != null)
                {
                    callback(gObject);
                }
                else
                {
                    LogMgr.instance.Log(LogLevel.ERROR, LogTag.ResourceMgr, string.Format("devindzhang InstantiateGameObject use Aync but no callback check code ?"));
                }

            };

            InstantiateObjectAync(path, ResourceType.Prefab, actions, tactice, poolNum, poolSize);
            return null;
        }
    }

    /// <summary>
    /// 异步Get资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rType"></param>
    /// <param name="callback"></param>
    /// <param name="tactice"></param>
    /// <param name="poolNum"></param>
    /// <param name="poolSize"></param>
    private void InstantiateObjectAync(string path, ResourceType rType, System.Action<GameObject> callback, PoolCreateTactice tactice = PoolCreateTactice.DynamicMaxSize, int poolNum = 1, int poolSize = 1)
    {
        LoadResourceAsyn(path, rType, (asset) =>
        {
            GamePoolMgr.instance.CreatePool(path, asset, tactice, poolNum, poolSize);

            GameObject gameObject = GamePoolMgr.instance.Give(path);
            if (gameObject != null) gameObject.SetActive(true);
            callback(gameObject);
        });
    }

    public override void Dispose()
    {
      
    }
}


/// <summary>
/// 资源类型
/// </summary>
public enum ResourceType
{
    Prefab = 1, // prefab
    RawTexure = 2, //动态贴图
    UIPrefab = 3,//UI的Prefab
    Config = 4,//配置文件
    Sound = 5, // 声音文件
    Shader = 6,//直接加载shader
    Mat = 7, //material
    Font = 8, //字体
}