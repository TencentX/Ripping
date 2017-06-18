using UnityEngine;
using System.Collections;



public class Joystick : MonoBehaviour
{
    public delegate bool JoystickMove(Vector3 dir);
    public delegate bool JoystickMoveRelative(Vector3 dir);
    public delegate void JoystickStop();

    private Vector2 initPos;			// 初始位置坐标
    private Vector2 initPos2;			// 初始位置坐标(第一次按下后不再改变)
    private float staticSize;			// 摇杆未活动时的静态大小，半径
    private float workingSize;			// 摇杆移动中的大小，半径

    //public Texture2D backCircleImage;	// 标识摇杆静态范围和工作范围的圆
    //public Texture2D thumbImage;		// 用于显示拇指的贴图

    private bool working = false;		// 是否操作中
    private bool checkWorking = false;	// 当第一次触摸摇杆活动区域时并不直接进入working状态，而是先进入checkWorking状态，等拇指移动一定距离后再进入working状态

    private Vector2 lastTouchPos;		// 上次触摸位置
    private Vector2 thumbPos;			// 拇指位置位置

    private Vector3 dirCam;				// 相机方向
    private Vector3 dirRight;

    private float noWorkingTimer = 5.0f;		// 不工作时长
    private float workingMagnitude = 20.0f;		// 初始工作距离

    // 拇指和摇杆中心点的距离阈值
    private float moveDistanceThreshold = 20f;
    private bool inMiniCircle;

    public static Joystick m_instance;

    private bool processInput = true;

    private Vector3 joystickDir;
    private Vector3 joystickDir2;

    JoystickMove m_callbackMove;
    JoystickMoveRelative m_callbackMoveRelative;
    JoystickStop m_callbackStop;

    bool requireJoystick = false;
    bool drawConstant = false;

    float validFactor = 0.4f;

    JoystickPanel panel;

    //设置摇杆控制对象    
    public void SetCallbackMoveFun(JoystickMove func)
    {
        m_callbackMove = func;
    }

    public void SetCallbackMoveFunRelative(JoystickMoveRelative func)
    {
        m_callbackMoveRelative = func;
    }

    public void SetCallbackStopFun(JoystickStop func)
    {
        m_callbackStop = func;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        m_instance = this;
        EventMgr.instance.AddListener("joystickForceDraw", OnForceDraw);
    }

    void Start()
    {
        ResetInitPos();
        ResetInitPos2();
		
        staticSize = CoreEntry.screenHeight / 8;
        workingSize = staticSize * 1.2f;

		if (Camera.main != null)
		{
			dirCam = Camera.main.transform.forward;
		}

        dirCam.y = 0.0f;
        dirCam.Normalize();

        working = false;

        EventMgr.instance.AddListener("startProcessInput", OnStartProcessInput);
        EventMgr.instance.AddListener("stopProcessInput", OnStopProcessInput);
        EventMgr.instance.AddListener<float>("JoystickWorkingMagnitude", OnJoystickWorkingMagnitude);
        EventMgr.instance.AddListener<bool>("joystickDrawConstant", OnDrawConstant);
    }

    public bool IsRequire()
    {
        return requireJoystick;
    }

    // 请求摇杆功能，属于特殊情况
    public void RequireJoystick()
    {
        requireJoystick = true;
        OnStartProcessInput(null);
    }

    public void ReleaseJoystick()
    {
        requireJoystick = false;
        OnStopProcessInput(null);
    }

    public void ResetInitPos()
    {
        initPos.x = CoreEntry.screenHeight / 5;
        initPos.y = CoreEntry.screenHeight - CoreEntry.screenHeight / 5;
    }

    public void ResetInitPos2()
    {
        initPos2.x = CoreEntry.screenHeight / 5;
        initPos2.y = CoreEntry.screenHeight - CoreEntry.screenHeight / 5;
    }

    void OnDestroy()
    {
        EventMgr.instance.RemoveListener(this);
    }

    void OnEnable()
    {
        OnStartProcessInput(null);
    }

    void OnDisable()
    {
        OnStopProcessInput(null);
    }

    public void OnStartProcessInput(string gameEvent)
    {
        if (InputMgr.instance.inputModel != InputModel.JoystickModel)
            return;

        processInput = true;
        working = true;
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
        }
    }

    public void OnStopProcessInput(string gameEvent)
    {
        if (IsRequire()) return;

        working = false;
        processInput = false;
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    void OnJoystickWorkingMagnitude(string gameEvent, float mag)
    {
        workingMagnitude = mag;
    }

    public bool UpdateInput()
    {
        int count = 0;
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
            count = 1;
#else
		count = Input.touchCount;
#endif

        if (!processInput)
            return false;

        if (working && Camera.main != null)
        {
            dirCam = Camera.main.transform.forward;
            dirCam.y = 0.0f;
            dirCam.Normalize();
            noWorkingTimer = 0.0f;
            working = false;

            if (count > 0)
            {
                Vector2 touchPos = Vector2.zero;
                for (int i = 0; i < count; i++)
                {
#if UNITY_EDITOR
                    if (i == 0)
                    {
                        touchPos.x = Input.mousePosition.x;
                        touchPos.y = Input.mousePosition.y;
                    }
#else
					Touch touch = Input.GetTouch(i);
					touchPos = touch.position;
#endif
                    touchPos.y = CoreEntry.screenHeight - touchPos.y;

                    if ((lastTouchPos - touchPos).magnitude < 200.0f)
                    {
                        thumbPos = touchPos;
                        Vector2 vThumbDir = thumbPos - initPos;
                        vThumbDir.Normalize();
                        vThumbDir.y = -vThumbDir.y;

                        dirRight = Vector3.Cross(Vector3.up, dirCam);
                        Vector3 vMoveDir = dirCam * vThumbDir.y * Time.deltaTime * 4.0f;
                        vMoveDir += dirRight * vThumbDir.x * Time.deltaTime * 4.0f;
                        vMoveDir.Normalize();
                        joystickDir = vMoveDir;
                        joystickDir2 = touchPos - initPos2;

                        // 计算拇指位置距离摇杆中心点距离，如果小于阈值，改变计算摇杆方向的方式
                        Vector2 diff = touchPos - initPos;
                        float dist = Vector2.SqrMagnitude(diff);
                        if (dist <= moveDistanceThreshold * moveDistanceThreshold)
                        {
                            inMiniCircle = true;
                            //panel.thumb.GetComponent<UISprite>().color = Color.red;
                        }
                        else
                        {
                            inMiniCircle = false;
                            //panel.thumb.GetComponent<UISprite>().color = Color.white;
                        }

                       //Debug.LogError("touchpos:" + touchPos + "initpos:" + initPos + "dist to initpos:" + Vector2.ClampMagnitude(diff,1000f));

                        if (m_callbackMove != null)
                            m_callbackMove(vMoveDir);
                        if (m_callbackMoveRelative != null)
                            m_callbackMoveRelative(joystickDir2);

                        lastTouchPos = touchPos;
                        working = true;
                        break;
                    }
                }
            }

            if (working == false)
            {
                ResetInitPos();
                ResetInitPos2();
                EventMgr.instance.TriggerEvent("joystickInitPosReset");

                thumbPos = initPos;

                if (m_callbackStop != null)
                    m_callbackStop();
            }
        }
        else
        {
            noWorkingTimer += Time.deltaTime;
            if (count == 0)
                checkWorking = false;
            else
            {
                Vector2 touchPos = Vector2.zero;

                for (int i = 0; i < count; i++)
                {
#if UNITY_EDITOR
                    if (i == 0)
                    {
                        touchPos.x = Input.mousePosition.x;
                        touchPos.y = Input.mousePosition.y;
                    }
#else
					Touch touch = Input.GetTouch(i);
					touchPos = touch.position;
#endif
                    touchPos.y = CoreEntry.screenHeight - touchPos.y;

                    if (InputMgr.instance.inputControlMode == InputControlMode.Normal || InputMgr.instance.inputControlMode == InputControlMode.InitStatic)
                    {
                        if (IsPointInStaticArea(touchPos))
                        {
                            if (checkWorking)
                            {
                                if ((touchPos - initPos).magnitude > workingMagnitude)
                                {
                                    working = true;
                                    checkWorking = false;
                                }
                            }
                            else
                            {
                                checkWorking = true;
                                initPos = touchPos;
                                initPos2 = touchPos;
                                EventMgr.instance.TriggerEvent("joystickInitPosReset");
                            }

                            lastTouchPos = touchPos;
                            thumbPos = touchPos;
                            break;
                        }
                    }
                    else if (InputMgr.instance.inputControlMode == InputControlMode.Static)
                    {
                        if (IsPointInWorkingArea(touchPos))
                        {
                            if (checkWorking)
                            {
                                working = true;
                                checkWorking = false;
                            }
                            else
                            {
                                checkWorking = true;
                                EventMgr.instance.TriggerEvent("joystickInitPosReset");
                            }

                            lastTouchPos = touchPos;
                            break;
                        }
                    }
                }
            }
        }
        OnDraw();
        return working;
    }

    public Vector3 GetCamDir() { return dirCam; }

    public Vector3 GetCamDirRight() { return dirRight; }

    void OnDraw()
    {
        if (panel == null)
        {
            panel = UIMgr.instance.CreatePanel(PanelNameDefine.JOYSTICK_PANEL) as JoystickPanel;
        }
        else
        {
            InitJoystick();

        }
		if (panel == null || panel.uiPanel == null)
		{
			return;
		}

        if (!processInput)
            return;

        if (InputMgr.instance.inputControlMode == InputControlMode.Normal)
        {
            Vector2 vOffset = initPos - thumbPos;
            if (vOffset.magnitude > workingSize * validFactor)
                initPos = thumbPos + vOffset.normalized * workingSize * validFactor;
        }
        else if (InputMgr.instance.inputControlMode == InputControlMode.InitStatic || InputMgr.instance.inputControlMode == InputControlMode.Static)
        {
            Vector2 vOffset = thumbPos - initPos;
            if (vOffset.magnitude > workingSize * validFactor)
                thumbPos = initPos + vOffset.normalized * workingSize * validFactor;

            //Debug.LogError(workingSize * validFactor);
        }

        if (working || drawConstant)
        {
            panel.uiPanel.alpha = 1;
            panel.bg.localPosition = UIMgr.instance.ScreenPosToNGUIPos(initPos);
            panel.thumb.localPosition = UIMgr.instance.ScreenPosToNGUIPos(thumbPos);
        }
        else
        {
            if (noWorkingTimer > 0.1f)
            {
                // 不工作状态时摇杆显示在左下角
                InitJoystick();

                panel.uiPanel.alpha = noWorkingTimer - 0.1f;
                if (panel.uiPanel.alpha > 1.0f)
                    panel.uiPanel.alpha = 1.0f;
            }
        }
    }

    void InitJoystick()
    {
        Vector3 pos;
        pos.x = CoreEntry.screenHeight / 5;
        pos.y = CoreEntry.screenHeight - CoreEntry.screenHeight / 5;
        pos.z = 0;
		if (panel != null && panel.bg != null)
		{
			panel.bg.localPosition = UIMgr.instance.ScreenPosToNGUIPos(pos);
			panel.thumb.localPosition = UIMgr.instance.ScreenPosToNGUIPos(pos);
		}
    }

    /// <summary>
    /// 强制绘制
    /// 通过修改非工作时间
    /// </summary>
    void OnForceDraw(string gameEvent)
    {
        working = false;
        noWorkingTimer = 5.1f;
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
        }
    }

    void OnDrawConstant(string gameEvent, bool c)
    {
        working = true;
        drawConstant = c;
    }

    public void EndWorking()
    {
        working = false;
        // 归位
        if (panel != null)
        {
            ResetInitPos();
            ResetInitPos2();
            InitJoystick();
        }
    }

    // 检测点是否在工作区域内
    bool IsPointInWorkingArea(Vector2 pos)
    {
        return (pos - initPos).magnitude < workingSize;
    }

    bool IsPointInStaticArea(Vector2 pos)
    {
        return pos.x < CoreEntry.screenWidth / 2.0f;
    }

    // 触摸点是否处于小圈内
    public bool IsInMiniCircle()
    {
        if(!working)
        {
            return false;
        }

        return inMiniCircle;
    }

    public bool GetJoystickDir(out Vector3 dir)
    {
        if (!working)
        {
            dir = Vector3.zero;
            // 暂时改为return true
            return false;
        }

        dir = joystickDir;
        return true;
    }

    public bool GetJoystickThumbDir(out Vector3 dir)
    {
        // 原点在左上角，转换为左下角
        dir = (thumbPos - initPos);
        dir.y = -dir.y;
        dir.Normalize();
        return true;
    }

    public bool GetJoystickDirRelative(out Vector3 dir)
    {
        if (!working)
        {
            dir = Vector3.zero;
            return false;
        }

        dir = joystickDir2;
        return true;
    }

    public bool GetOriginPos(out Vector3 dir)
    {
        dir.x = initPos2.x;
        dir.y = initPos2.y;
        dir.z = 0;
        return true;
    }
}
