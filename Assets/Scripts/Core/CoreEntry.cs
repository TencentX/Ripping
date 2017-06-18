//#define DEBG_SERVICE
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


// 注意：
// 1. 新枚举必须在后面加
// 2. 尽量把Stage的逻辑都放在一起，通过尽量少的逻辑推导得到更多的结果
public enum GameStage
{
    Nothing,
    Logo,
    Login,
    Main,
    CreatePlayer,	//创建角色    

}

// 程序入口，要保证Exe Order最高
// 管理程序生命周期
public class CoreEntry : MonoBehaviour
{
    public GameStage stage = GameStage.Nothing;

    // 全局对象
    private static GameStage gameStage = GameStage.Nothing;
    private static bool corePreInited = false;
    private static bool coreInited = false;
    public static GameObject coreRoot;
    private static CoreRootObj coroutObj;
    public static bool isApplicationQuit = false;

    private static int m_screenWidth;
    private static int m_screenHeight;

    // 保存管理器单例，再次初始化时使用
    private static List<IDisposable> globalMgrs = new List<IDisposable>();
    private static List<Module> gameplayModules = new List<Module>();

    static bool pauseState = false;
    static public bool GetPauseState() { return pauseState; }
    static bool focusState = true;
    static public bool GetFocuseState() { return focusState; }
    static bool isQuit = false;
    static public bool IsQuit() { return isQuit; }
    static bool resetCore = false;
    static public bool IsResetCore() { return resetCore; }

    // 是否从正常流程进入当前场景，登录->选关->战斗
    static bool isFullGame = false;


    public static bool CHECK_UPDATE = 
#if UNITY_EDITOR
 false;
#else
       false ;
#endif

    static public bool checkFullGame
    {
        get
        {
            return isFullGame || GetGameStage() == GameStage.Logo || GetGameStage() == GameStage.Login || GetGameStage() == GameStage.CreatePlayer;
        }
    }


    public static int screenWidth
    {
        get
        {
            return m_screenWidth;
        }
    }

    public static int screenHeight
    {
        get
        {
            return m_screenHeight;
        }
    }

    static public void DetermineFullGame()
    {
        isFullGame = true;
    }

    // 这个对象作为常驻内存的全局对象
    // 对各个Mgr进行tick
    // 给单例提供Coroutine接口
    class CoreRootObj : MonoBehaviour
    {
        public void DoCoroutine(System.Collections.IEnumerator rou)
        {
            StartCoroutine(rou);
        }

        /// <summary>
        /// 增加可顺序执行的coroutine
        /// </summary>
        /// <param name="rou"></param>
        /// <returns></returns>
        public IEnumerator DoCoroutineQueuer(System.Collections.IEnumerator rou)
        {
            yield return StartCoroutine(rou);
        }

        void Update()
        {
            CoreEntry.CoreTick();

            LogMgr.instance.WriteLog();
        }

        void OnApplicationFocus(bool focus)
        {
            CoreEntry.OnFocus(focus);
        }
        void OnApplicationPause(bool pause)
        {
            CoreEntry.OnPause(pause);
        }
        void OnApplicationQuit()
        {
            CoreEntry.OnQuit();
        }
    }

    public static void CallCorotinue(System.Collections.IEnumerator rou)
    {
        coroutObj.DoCoroutine(rou);
    }

    /// <summary>
    /// 多协程 顺序执行接口
    /// </summary>
    /// <param name="rou"></param>
    /// <returns></returns>
    public static IEnumerator CallCorotinueQueue(System.Collections.IEnumerator rou)
    {
        yield return coroutObj.DoCoroutineQueuer(rou);
    }

    #region GameLifeCyfle

    // 游戏由多个Scene构成，每次loadScene都会走以下流程(重复load同一个scene除外)
    // 游戏生命周期：loadSceneBegin->GameObject::Awake->GameObject::Start->SetGameStage->ExitStage->EnterStage(Prepare->Enter->AllDone)[->gameStart]

    public static void SetGameStage(GameStage stage)
    {
        var oldState = gameStage;
        gameStage = stage;

        EventMgr.instance.TriggerEvent<GameStage>("onGameStageExit", oldState);
        EventMgr.instance.TriggerEvent<GameStage>("onGameStageInitEnter", stage);

        // 这里保证所有loadscene之后，所有gameobj都执行过start                
        Scheduler.Create(null, (sche, t, s) =>
        {
            // 用于准备数据、控制条件等等
            EventMgr.instance.TriggerEvent<GameStage>("onGameStageEnter", stage);
        });
    }

    public static GameStage GetGameStage()
    {
        return gameStage;
    }

    // 程序入口
    void Awake()
    {
        gameStage = stage;
        // 初始化全局管理器
        CorePreInit();

        if (!CHECK_UPDATE)
        {
            OnUpdateComplete();//editor 不走更新流程
        }
        else
        {
            OnUpdateComplete();
        }
    }

    private void OnUpdateComplete()
    {
        // 加载资源，初始化各个业务模块
        CoreInitAfterResUpdate(stage == GameStage.Login);

        // 通知业务启动
        SetGameStage(stage);
    }

    // 不依赖增量更新
    public static void CorePreInit()
    {
        if (corePreInited || isApplicationQuit)
            return;

        corePreInited = true;

		// 全局对象
		GameObject go = GameObject.Find("CoreRoot");
		if (go == null)
		{
			coreRoot = new GameObject("CoreRoot");
			coroutObj = coreRoot.AddComponent<CoreRootObj>();
			DontDestroyOnLoad(coreRoot);
		}

        // 设置手机不睡眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_EDITOR
        m_screenWidth = Screen.width;
        m_screenHeight = Screen.height;
#else
		float ratio4_3 = 4.0f / 3.0f;
		float ratio = Screen.width / (float)Screen.height;
		
		if( ratio < ratio4_3 + 0.01f )
		{
			// 4：3的屏幕			
			m_screenHeight = 1080;                
			m_screenWidth = (int)(1080.0f * ratio);
		}
		else
		{
			m_screenHeight = Screen.height;
			if (m_screenHeight > 1080)	m_screenHeight = 1080;
			m_screenWidth = (int)(m_screenHeight * ratio);
		}
		
		Screen.SetResolution(m_screenWidth, m_screenHeight, true);
#endif

        // 设置为30fps
        Application.targetFrameRate = 60;

        //强制设置为不能左右旋转
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // Init Global Mgr
        Scheduler.Init();
		EventMgr.instance.Init();

        globalMgrs.Add(EventMgr.instance);
		globalMgrs.Add(InputMgr.instance);
		globalMgrs.Add(UIMgr.instance);
		globalMgrs.Add(LogMgr.instance);
        InputMgr.instance.Init();
        UIMgr.instance.FetchRootAndCamera();
    }

    // 依赖增量更新，正常流程走在增量更新后面
    public static void CoreInitAfterResUpdate(bool forceInit = false)
    {
        if ((!forceInit && coreInited) || isApplicationQuit)
            return;

        coreInited = true;

        // 先清理各个业务一遍
        CoreReset();
		
        for (int i = 0; i < gameplayModules.Count; ++i)
        {
            try
            {
                gameplayModules[i].Init();
            }
            catch (System.Exception ex)
            {
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.CoryEntry, "init module exception:" + ex.StackTrace);
            }
        }
    }

    static void CoreReset()
    {
        resetCore = true;

        // 清理各个业务模块
        for (int i = 0; i < gameplayModules.Count; ++i)
        {
            try
            {
                gameplayModules[i].Exit();
                gameplayModules[i].Release();
            }
            catch (System.Exception ex)
            {
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.CoryEntry, "release module exception:" + ex.StackTrace);
            }
        }
        gameplayModules.Clear();

        resetCore = false;
    }

    static void CoreTick()
    {
        Scheduler.UpdateSche();        
        InputMgr.instance.UpdateTick();
    }

    static void OnPause(bool pauseStatus)
    {
        CoreEntry.pauseState = pauseStatus;
        EventMgr.instance.TriggerEvent<bool>("onPause", pauseStatus);
        EventMgr.instance.TriggerEvent("onAppStateChange");

        //系统暂停的时候写日志
        LogMgr.instance.WriteLog();
    }

    static void OnFocus(bool focusStatus)
    {
        CoreEntry.focusState = focusStatus;
        EventMgr.instance.TriggerEvent<bool>("onFocus", focusStatus);
        EventMgr.instance.TriggerEvent("onAppStateChange");
    }

    static void OnQuit()
    {
        isQuit = true;
        EventMgr.instance.TriggerEvent("onQuit");
        EventMgr.instance.TriggerEvent("onAppStateChange");
    }

    public static bool IsInit()
    {
        return coreInited;
    }

    void OnApplicationQuit()
    {
        isApplicationQuit = true;
    }
    #endregion
}
