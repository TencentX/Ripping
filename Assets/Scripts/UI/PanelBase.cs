#pragma warning disable 0618
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{

    [System.NonSerialized]
    public string panelName;

    /// <summary>
    /// 面板唯一标识id
    /// </summary>
    public uint panelId
    {
        get
        {
            return _panelId;
        }
    }
    private uint _panelId = 0;
    private static uint _panelIdSeq = 1;

    public enum TransitionStyle
    {
        None,               // 过度效果
        Fade,               // 淡入淡出
        FadeSlowly,         // 慢慢地淡入淡出
        FadeAtExitSlowly,   // 只淡出
        FadeQuickly,        // 快速地淡入淡出
    }


    public enum DepthType
    {        
        Dynamic = 0,        // 普通高度，绝大多数UI都用这层
        High = 1,
        Highest = 2,
        None = 3,
        Low = 4,
    }

    // 同一个depthtype面板，所拥有的深度间隔
    static public int PanelDepthInteval = 200;
    static public int PanelDepthPerInteval = 20;

    static public int[] DepthValue = {
        1000,
        500000,
        600000,
        100,
        0,
        0,
    };

    public enum BackgroundType
    {
        BlackBlock,                // 黑色模糊遮挡层，遮挡层的深度总是比需要遮挡层的面板当中，深度最大的面板小1
        None,                        // 不需要遮挡
		BlurBlock,                  // 模糊遮挡层
        BlackBlockNoBlur,       // 黑色底没有模糊效果
    }

    public enum ToyAction
    {
        Exit,           // 玩偶进入离开，退出界面，比如系统界面
        Stay,           // 玩偶进入离开，保持界面，比如主界面
        Custom,         // 玩偶进入离开，界面特殊处理，比如英雄界面，会通过OnToyEnter/OnToyExit通知
    }

    public enum SceneExitAction
    {
        None,           // 场景退出时什么都不做
        Destroy,        // 场景退出时关闭
    }


    [Tooltip("UI过度效果")]
    public TransitionStyle transitionStyle = TransitionStyle.Fade;
    [Tooltip("UI深度")]
    public DepthType depthType = DepthType.Dynamic;
    [System.NonSerialized]
    public bool isMarkDestroy = false;
    [Tooltip("UI是否全屏，全屏信息用于优化场景")]
    public bool isFullScreen = true;

    [HideInInspector]
    public bool isSuportEffect = true; //是否支持动效

#region atlas 处理
    [Tooltip("Atlas")]
    public Texture[] m_atlasTexture;

    [Tooltip("弹出框遮挡层样式")]
    public BackgroundType backgroundType = BackgroundType.None;
    [Tooltip("点击遮挡层响应关闭")]
    public bool clickBackgroundToExit = false;


    //atlas 的名称和数量
    private static readonly Dictionary<string/*atlas name*/, int/**atlas num*/> mAtlasShowNum = new Dictionary<string, int>();
    
    private static void AddAtlasShow(PanelBase panelBase)
    {
        if (panelBase == null) return;
        Texture[] texs = panelBase.m_atlasTexture;
        if (texs == null) return;
        foreach (Texture tex in texs)
        {
            if (tex != null)
            {
                string atlasName = tex.name;
                if (!mAtlasShowNum.ContainsKey(atlasName))
                {
                    mAtlasShowNum.Add(atlasName, 1);
                }
                else
                {
                    mAtlasShowNum[atlasName] += 1;
                }
            }
        }
    }

    private static void RemoveAtlasShow(PanelBase panelBase)
    {
        if (panelBase == null) return;
        //remove atlas name
        Texture[] texs = panelBase.m_atlasTexture;
        if(texs == null) return;
        foreach (Texture tex in texs)
        {
            if (tex != null)
            {
                string atlasName = tex.name;
                if (mAtlasShowNum.ContainsKey(atlasName))
                {
                    mAtlasShowNum[atlasName] -= 1;

                    if (mAtlasShowNum[atlasName] <= 0)
                    {
                        mAtlasShowNum.Remove(atlasName);
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

    public static bool HasTexShow(string texName)
    {
        return mAtlasShowNum.ContainsKey(texName) && mAtlasShowNum[texName] > 0;
    }
#endregion

    public void InitPanel(GameObject obj)
    {
        // 设置唯一标识
        _panelId = _panelIdSeq++;

        //set bg
        //UIMgr.instance.AdjustSizeAndPosition(GlobalFunctions.GetComponent<UITexture>(transform, "fullscenebg"));

        AddAtlasShow(this);
        Init(obj);
    }

    // 在创建的一帧进行初始化
    virtual protected void Init(GameObject obj)
    {
        
    }

    // 在确定退出的这一帧进行释放，而不是在OnDestroy中，因为OnDestroy会由于过度动画的原因滞后
    virtual public void Release()
    {
        if (isMarkDestroy || gameObject == null)
            return;

        isMarkDestroy = true;

        // 有时候Exit还在渐变，这时候需要把响应都关闭
        var colliders = GetComponentsInChildren<Collider>();
        if (colliders != null)
        {
            foreach (var c in colliders)
            {
                c.enabled = false;
            }
        }

        // 如果有粒子，那么也要把粒子关了，避免残影
        if (transitionStyle != TransitionStyle.None)
        {
            var partlcles = GetComponentsInChildren<ParticleSystem>();
            if (partlcles != null)
            {
                for (int i = 0; i < partlcles.Length; ++i)
                {
                    partlcles[i].enableEmission = false;
					partlcles[i].gameObject.SetActive(false);
                }
            }

			//隐藏所有特效
			var uiParticles = GetComponentsInChildren<UIParticle>();
			if ( uiParticles != null)
			{
				for (int i = 0; i < uiParticles.Length; ++i)
				{
					if( uiParticles[i].gameObject )
					{
						uiParticles[i].gameObject.SetActive(false);
					}
				}
			}

        }
    }

    virtual public void OnToyEnter(int resId)
    {

    }

    virtual public void OnToyExit()
    {

    }

    virtual public void ClickToExit()
    {
        Exit();
    }

    virtual protected void OnDestroy()
    {
        Scheduler.RemoveSchedule(this);
        EventMgr.instance.RemoveListener(this);
        RemoveAtlasShow(this);
        DisposeAtlas();
    }

    private void DisposeAtlas()
    {
        if (m_atlasTexture != null)
        {
            foreach (Texture atlas in m_atlasTexture)
            {
                if (atlas != null && !HasTexShow(atlas.name))
                {
                    Resources.UnloadAsset(atlas);
                }
            }    
        }
        
    }

    // 所有继承自PanelBase并且被UiMgr创建的界面，退出都会经过该函数
    virtual public void Exit()
    {
        UIMgr.instance.DestroyPanel(panelName, gameObject);
    }

    //正在销毁中
    public bool IsDestroying
    {
        set;get;
    }

    //是否正在隐藏过程中
    public bool IsHiding
    {
        set;get;
    }

    //是否正在显示中
    public bool IsShowing
    {
        set;get;
    }

    /// <summary>
    /// 隐藏panel
    /// </summary>
    public void Hide()
    {
        IsShowing = false;
        IsHiding = true;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示panel
    /// </summary>
    public void Show()
    {
        Show(true);
    }

    public void Show(bool effect)
    {
        IsHiding = false;
        if(!effect)
        {
            gameObject.SetActive(true);
        }
        else
        {
            UITweenAnimation tweenAnimation = gameObject.GetComponent<UITweenAnimation>();
            gameObject.SetActive(false);
            if (tweenAnimation != null && !IsShowing && isSuportEffect)
            {
                Scheduler.Create(this, (sche, t, s) =>
                {
                    PanelBase panelBase = sche.owner as PanelBase; 
                    if (panelBase != null && !panelBase.IsHiding)
                    {
                        if (panelBase.gameObject != null)
                        {
                            panelBase.gameObject.SetActive(true);

                            UITweenAnimation tween = panelBase.gameObject.GetComponent<UITweenAnimation>();
                            if (tween != null)
                            {
                                IsShowing = true;
                                tween.OnShow(true,()=>
                                {
                                    IsShowing = false;
                                });
                            }
                        }
                    }
                }, 0, 0, 0.01f);

            }
            else
            {
                gameObject.SetActive(true);
            }
        }
        
    }
}
