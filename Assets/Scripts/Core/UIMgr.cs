using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMgr : Singleton<UIMgr>
{
    static public string UI_PREFIX = "prefabs/uis/";

    private Camera m_uiCamera;
    public GameObject m_uiRoot;
    public int uiHight = 0;
    public Transform hudPanel;
    int fullScreenPanelNum = 0;
    bool ignoreHudChange = false;

    Dictionary<string, List<GameObject>> panelTable = new Dictionary<string, List<GameObject>>();
    int[] depthTypeValue = new int[PanelBase.DepthValue.Length];

    public UIMgr()
    {
        EventMgr.instance.AddListener("startLoading", OnStartLoading);
        EventMgr.instance.AddListener("endLoading", OnEndLoading);
    }

    public override void Dispose()
    {
        //清除所有面板
        ClearPanels();
    }

    void OnStartLoading(string gameEvent)
    {
        if (GetPanel(PanelNameDefine.INDICATOR_PANEL) == null)
            CreatePanel(PanelNameDefine.INDICATOR_PANEL);
    }

    void OnEndLoading(string gameEvent)
    {
        DestroyPanel(PanelNameDefine.INDICATOR_PANEL);
    }

    public Camera uiCamera
    {
        get
        {
            if (m_uiRoot == null)
            {
                FetchRootAndCamera();
            }

            return m_uiCamera;
        }
    }

    public GameObject uiRoot
    {
        get
        {
            if (m_uiRoot == null)
            {
                FetchRootAndCamera();
            }

            return m_uiRoot;
        }
    }

    public void FetchRootAndCamera()
    {
        if (CoreEntry.isApplicationQuit)
            return;

        m_uiRoot = GameObject.Find("UI Root");
        if (m_uiRoot == null)
        {
            m_uiRoot = UnityEngine.Object.Instantiate(Resources.Load(UI_PREFIX + PanelNameDefine.UIROOT)) as GameObject;
            m_uiRoot.name = "UI Root";
            var ur = m_uiRoot.GetComponent<UIRoot>();
            uiHight = ur.activeHeight;
        }

        if (uiHight == 0)
        {
            var ur = m_uiRoot.GetComponent<UIRoot>();
            uiHight = ur.activeHeight;
        }

        m_uiCamera = m_uiRoot.transform.GetChild(0).GetComponent<Camera>();

        hudPanel = m_uiRoot.transform.GetChild(2);
        UIPanel uiPanel = hudPanel.gameObject.AddComponent<UIPanel>();
        uiPanel.depth = 1;
        uiPanel.renderQueue = UIPanel.RenderQueue.StartAt;
        uiPanel.startingRenderQueue = 1;

        GameObject.DontDestroyOnLoad(m_uiRoot);
    }

    public PanelBase CreatePanel(string panelName)
    {
        return CreatePanel(panelName, false);
    }

    /// <summary>
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="preInit">是在是在加载资源之前就创建</param>
    /// <returns></returns>
    public PanelBase CreatePanel(string panelName, bool preInit)
    {
        // core mechanism:
        // 1. create obj from prefab with same name
        // 2. call PanelBase.init
        // 3. done, add to panellist        

        if (!panelTable.ContainsKey(panelName))
        {
            panelTable.Add(panelName, new List<GameObject>());
        }
        var panels = panelTable[panelName];

        var panelPath = UI_PREFIX + panelName.ToLower();

        GameObject obj = null;
        if (preInit)
        {
            GameObject prefab = Resources.Load<GameObject>(panelPath);
            if (prefab != null)
            {
                obj = UnityEngine.Object.Instantiate(prefab);
            }
            else
            {
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.UIMgr, "devindzhang 加载Resoruces下没有的资源  path:" + panelPath);
            }
        }
        else
        {
            obj = ResourceMgr.instance.InstantiateUIObject(panelPath);
        }

        if (obj == null)
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.UIMgr, string.Format("can't instantiate prefab, panelName: {0},panelPath: {1}", panelName, panelPath));
            return null;
        }

        var panelBase = obj.GetComponent<PanelBase>();

        if (panelBase == null)
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.UIMgr, string.Format("PanelBase script miss,panelName:{0},panelPath{1}", panelName, panelPath));
            return null;
        }
        panels.Add(obj);
        panelBase.panelName = panelName;

        try
        {
            panelBase.InitPanel(obj);
        }
        catch (System.Exception ex)
        {
            LogMgr.instance.Log(LogLevel.ERROR, LogTag.UIMgr, "InitPanel ex:" + panelName + " msg:" + ex.Message + " exception:" + ex.StackTrace);
        }

        // 自动维护depth，保证最新创建的panel在其他panel的上方
        // 因此prefab上的depth只需要保证相对顺序正确即可
        var uipanels = obj.GetComponentsInChildren<UIPanel>(true);
        if (panelBase.depthType != PanelBase.DepthType.None)
        {
            AdjustPanels(uipanels, panelBase.depthType);
        }

        // 在拥有front面板之后，新创建的都添加到topcamera上
        Vector3 localPosition = obj.transform.localPosition;
        // 处理面板在z轴0点时，相机拍的特效显示异常的问题
        localPosition.z += 10f;
		obj.transform.parent = uiCamera.transform;
		obj.transform.localScale = Vector3.one;

        // 检查是否是fullscreen
        if (panelBase.isFullScreen)
        {
            if (fullScreenPanelNum == 0)
            {
                // 将所有low级别panel关闭，减小overdraw
                SetPanelVisible(false);
            }

            ++fullScreenPanelNum;
            SetHudVisible(false, false);
        }

        obj.transform.localPosition = localPosition;

        panelBase.Show();

#if UNITY_EDITOR
        panelBase.name = panelBase.name + panelBase.panelId.ToString();
#endif

        LogMgr.instance.Log(LogLevel.INFO, LogTag.UIMgr, "CreatePanel:" + panelBase.name);

        return panelBase;
    }

    /// <summary>
    /// 立即销毁正在销毁中的面板，减少2个界面快速切换体验的卡顿现象
    /// </summary>
    private void DestroyImmediateInDestroyingPanels()
    {
        List<string> keys = new List<string>(panelTable.Keys);
        foreach (string key in keys)
        {
            List<GameObject> panels = panelTable[key];
            for (int i = panels.Count - 1; i > 0; i--)
            {
                GameObject panel = panels[i];
                if (null != panel)
                {
                    PanelBase panelBase = panel.GetComponent<PanelBase>();
                    if (panelBase != null && panelBase.IsDestroying)
                    {
                        panelBase.IsDestroying = false;
                        DoDestroyPanel(panelBase.gameObject, panelBase.panelName);
                    }
                }
            }
        }
    }

    public void DestroyPanel(string panelName, GameObject panel = null)
    {
        // 是否删除指定panel，否则自己根据名字查找
        panel = panel == null ? GetPanel(panelName) : panel;

        if (panel != null)
        {
            //立即销毁正在tween中的界面
            DestroyImmediateInDestroyingPanels();

            EventMgr.instance.TriggerEvent<string>("panelDestroy", panelName);

            var panelBase = panel.GetComponent<PanelBase>();
            if (panelBase != null)
            {
                if (panelBase.isMarkDestroy)
                    return;

                if (panelBase.isFullScreen)
                {
                    --fullScreenPanelNum;
                    if (fullScreenPanelNum <= 0)
                    {
                        fullScreenPanelNum = 0;
                        SetHudVisible(true, false);
                        SetPanelVisible(true);
                    }
                }

                LogMgr.instance.Log(LogLevel.INFO, LogTag.UIMgr, "DestroyPanel:" + panelBase.name);

                try
                {
                    panelBase.Release();
                }
                catch (System.Exception ex)
                {
                    LogMgr.instance.Log(LogLevel.ERROR, LogTag.UIMgr, "Release Panel ex:" + panelName + " msg:" + ex.Message + " exception:" + ex.StackTrace);
                }

                UITweenAnimation tweenAnimation = panelBase.GetComponent<UITweenAnimation>();
                if (tweenAnimation != null && panelBase.isSuportEffect)
                {
                    panelBase.IsDestroying = true;
                    bool onHide = tweenAnimation.OnHide(() =>
                    {
                        panelBase.IsDestroying = false;
                        DoDestroyPanel(panel, panelName);
                    });

                    if (!onHide)
                    {
                        DoDestroyPanel(panel, panelName);
                    }
                }
                else
                {
                    DoDestroyPanel(panel, panelName);
                }
            }
            else
            {
                DoDestroyPanel(panel, panelName);
            }
        }
        else
        {
            //Debug.LogError("DestroyPanel : return " + panelName);
        }
    }

    void DoDestroyPanel(GameObject panel, string panelName)
    {
        if (panel == null || panelName == null) return;
        if (panelTable.ContainsKey(panelName) == false) return;

        LogMgr.instance.Log(LogLevel.INFO, LogTag.UIMgr, "DoDestroyPanel:" + panel.name);

        var panels = panelTable[panelName];

        GameObject.Destroy(panel);

        panels.Remove(panel);

        if (0 == panels.Count)
            panelTable.Remove(panelName);

        // 完成了面板销毁后的事件
        EventMgr.instance.TriggerEvent<string>("panelDestroyed", panelName);

        Resources.UnloadUnusedAssets();
    }

    public float GetFadeTime(PanelBase.TransitionStyle style)
    {
        if (style == PanelBase.TransitionStyle.Fade)
            return 0.25f;
        else if (style == PanelBase.TransitionStyle.FadeQuickly)
            return 0.1f;
        return 1.0f;
    }

    public void AdjustPanels(UIPanel[] uiPanels, PanelBase.DepthType depthType)
    {
        if (uiPanels == null || uiPanels.Length == 0) return;
        // adjust depth by logic depth
        int baseDepth = PanelBase.DepthValue[(int)depthType];
        int depth = ++depthTypeValue[(int)depthType];
        int logicDepth = depth * PanelBase.PanelDepthInteval + baseDepth;

        for (int i = 0; i < uiPanels.Length; ++i)
        {
            for (int j = i; j < uiPanels.Length; ++j)
            {
                if (uiPanels[i].depth > uiPanels[j].depth)
                {
                    var tempPanel = uiPanels[i];
                    uiPanels[i] = uiPanels[j];
                    uiPanels[j] = tempPanel;
                }
            }
        }

        for (int i = 0; i < uiPanels.Length; ++i)
        {
            uiPanels[i].depth = logicDepth + i * PanelBase.PanelDepthPerInteval;
            uiPanels[i].renderQueue = UIPanel.RenderQueue.StartAt;
            uiPanels[i].startingRenderQueue = uiPanels[i].depth;
        }
    }

    public GameObject GetPanel(string panelName)
    {
        if (panelTable.ContainsKey(panelName))
        {
            var panels = panelTable[panelName];
            if (panels.Count == 0) return null;
            var panel = panels[panels.Count - 1];
            return panel;
        }
        return null;
    }

    public PanelBase GetPanelBase(string panelName)
    {
        var panel = GetPanel(panelName);
        if (panel != null)
        {
            var pb = panel.GetComponent<PanelBase>();
            if (pb != null && pb.isMarkDestroy == false)
            {
                return pb;
            }
        }
        return null;
    }

	public PanelBase GetOrCreatePanel(string panelName)
	{
		var panel = GetPanelBase(panelName);
		if (panel == null)
			panel = CreatePanel(panelName);
		return panel;
	}

    public void ClearPanels()
    {
        List<string> _allPanelName = new List<string>();
        //清除所有面板
        foreach (var panel in panelTable)
        {
            _allPanelName.Add(panel.Key);
        }
        int _len = _allPanelName.Count;
        for (int i = 0; i < _len; i++)
        {
            var _panel = GetPanel(_allPanelName[i]);
            DestroyPanel(_allPanelName[i], _panel);
        }

        // 不需要删除，因为切场景本来就会删

        // 归零
        for (int i = 0; i < depthTypeValue.Length; ++i)
        {
            depthTypeValue[i] = 0;
        }
    }

    // 切上下；宽度不足初始设定高度时，切左右
    public void AdjustSizeAndPosition(UIWidget rect)
    {
        if (rect == null) return;

        float xscale, yscale;
        AdjustSizeAndPosition(rect, out xscale, out yscale);

        var size = rect.localSize;
        float newW = size.x * xscale;
        float newH = size.y * yscale;
        if (newH < uiHight)
        {
            newH = uiHight;
            newW = size.x / size.y * newH;
        }

        rect.SetRect(-newW / 2.0f, -newH / 2.0f, newW, newH);
    }
    public void AdjustSizeAndPosition(UIWidget rect, out float xscale, out float yscale)
    {
        if (rect == null)
        {
            xscale = 1;
            yscale = 1;
            return;
        }
        float ratio = Screen.width / (float)Screen.height;

        float screenHeight = uiHight;
        float screenWidth = screenHeight * ratio;

        var size = rect.localSize;
        float originR = size.x / size.y;
        float newW = screenWidth;
        float newH = newW / originR;

        xscale = newW / size.x;
        yscale = newH / size.y;
    }

    public HudBase CreateHud(string path, Camera gameCamera, Transform followTarget, float offset = 0)
    {
        GameObject go = ResourceMgr.instance.InstantiateUIObject(path);

        if (go != null)
        {
            var hudBase = go.GetComponent<HudBase>();
            if (hudBase != null)
            {
                hudBase.gameCamera = gameCamera;
                hudBase.transform.parent = UIMgr.instance.hudPanel.transform;
                hudBase.followTarget = followTarget;
                hudBase.offset = offset;
                return hudBase;
            }
        }

        return null;
    }

    public void ClearHudPanel()
    {
        if (hudPanel == null)
            return;
        hudPanel.DestroyChildren();
    }

    // ！！！需成对调用！！！
    public void IgnoreHudChange(bool ignore)
    {
        ignoreHudChange = ignore;

        if (!ignoreHudChange)
            SetHudVisible(true, false);
    }

    public void SetHudVisible(bool visible, bool force = true)
    {
        if (null == hudPanel || (ignoreHudChange && !force))
            return;

        if (!force)
            visible = !BlockHud();

        hudPanel.gameObject.SetActive(visible);
    }

    // TODO: 只要有一个系统把场景做在UI层，HUD就无法统一使用
    public void SetHudLayer(int layer)
    {
        if (hudPanel == null)
            return;

        hudPanel.gameObject.layer = layer;
    }

    public bool MountToHud(Transform hud)
    {
        if (null != hud && null != hudPanel)
        {
            hud.parent = hudPanel;
            return true;
        }

        return false;
    }

    private bool BlockHud()
    {
        return fullScreenPanelNum > 0;
    }

    public bool HaveFullScreenUI() { return fullScreenPanelNum > 0; }

    // 优化low级别的界面，优化overdraw
    public void SetPanelVisible(bool vis, PanelBase.DepthType depthType = PanelBase.DepthType.Low)
    {
        foreach (var panellist in panelTable)
        {
            for (int i = 0; i < panellist.Value.Count; ++i)
            {
                if (panellist.Value[i] != null)
                {
                    var panelBase = panellist.Value[i].GetComponent<PanelBase>();
                    if (panelBase.depthType == depthType)
                        panellist.Value[i].SetActive(vis);
                }
            }
        }
    }

    public Vector3 ScreenPosToNGUIPos(Vector3 screenPos, bool aa = true)
    {
        // 左上角原点改为左下角原点
        screenPos.y = CoreEntry.screenHeight - screenPos.y;
        // 世界变换
        Vector3 uiPos = uiCamera.ScreenToWorldPoint(screenPos);
        uiPos = uiCamera.transform.InverseTransformPoint(uiPos);
        // 进行AA
        if (aa)
        {
            uiPos.x = Mathf.FloorToInt(uiPos.x);
            uiPos.y = Mathf.FloorToInt(uiPos.y);
            uiPos.z = 0;
        }

        return uiPos;
    }
}
