using UnityEngine;

public class InputBase : MonoBehaviour
{
	// 是否处理输入操作
	protected bool processInput = true;

    protected Camera mainCamera;
    protected Transform cameraTransform;

    // 长按逻辑
    protected const float LongPressConfirmTime = 0.3f;
    // 手指点击屏幕第一帧的时间
    float longPressFirstTouchTime;    
    // 是否处于长按检测状态
    bool inlongPressRecogniz = false;
    // 是否处于长按状态中
    bool inLongPress = false;

    // 触摸逻辑
    protected int currentTouchCount = 0;
    protected int lastTouchCount = 0;
    protected Vector2 touchPos;
    protected Vector2 lastTouchPos = Vector2.zero;

	virtual public void Start()
	{
        PrepareProcessInput();
	}

    void PrepareProcessInput()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            cameraTransform = mainCamera.transform;
    }
	
	void Update()
	{
        bool shouldProcess = ShouldProcessInput();
        // 触屏逻辑
        UpdateTouch();
        UpdateLongPress(shouldProcess);
        if (shouldProcess)
        {
            ProcessTouch(touchPos);
        }            

        // 按键逻辑
        Vector2 movement = Vector2.zero;        
        if (shouldProcess)
        {
            ProcessKeyboard(out movement);
            ProcessMovement(movement);
        }   
        LateUpdateTouch();
	}

	virtual protected bool ShouldProcessInput()
	{
        return InputMgr.instance.processInput;
	}	

    public void ReadyLongPress()
    {
        longPressFirstTouchTime = Time.realtimeSinceStartup;
        inlongPressRecogniz = true;
    }

    public void BeginLongPress()
    {
        inLongPress = true;
        inlongPressRecogniz = false;
        EventMgr.instance.TriggerEvent("longPressBegin");
    }

    public void EndLongPress()
    {
        inlongPressRecogniz = false;
        if (inLongPress)
        {
            inLongPress = false;
            EventMgr.instance.TriggerEvent("longPressEnd");
        }
    }

    public bool InLongPress()
    {
        return inLongPress;
    }

    public bool InLongPressRecogniz()
    {
        return inlongPressRecogniz;
    }

    void UpdateLongPress(bool shouldProcess)
    {
        if (!shouldProcess)
        {
            EndLongPress();
        }
        else
        {
            if (currentTouchCount == 0)
                EndLongPress();
            else
            {
                if (lastTouchCount == 0)
                    ReadyLongPress();
                else
                {
                    if (InLongPressRecogniz())
                    {
                        if (Time.realtimeSinceStartup - longPressFirstTouchTime > LongPressConfirmTime)
                            BeginLongPress();
                    }
                }
                if (InLongPress())
                    EventMgr.instance.TriggerEvent<Vector3>("longPressMove", touchPos);
            }
            if (InLongPress())
                EventMgr.instance.TriggerEvent("longPressing");
        }
    }

    virtual protected bool ProcessMovement(Vector2 movement)
    {
        return true;
    }

    virtual protected void ProcessTouch(Vector2 pos)
    {

    }

    void UpdateTouch()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            currentTouchCount = 1;
            touchPos.x = Input.mousePosition.x;
            touchPos.y = Input.mousePosition.y;
        }
        else
        {
            currentTouchCount = 0;
        }
#else
		currentTouchCount = Input.touchCount;
		if (currentTouchCount > 0)
		{
			touchPos.x = Input.touches[0].position.x;
			touchPos.y = Input.touches[0].position.y;
		}
#endif
    }

    void LateUpdateTouch()
    {
        lastTouchCount = currentTouchCount;
        lastTouchPos = touchPos;
    }

    #region KeyBoardControl
#if true
    protected bool kkk = true;
    protected bool keyBoardMoving = false;
	protected Vector3 keyBoardMoveDir;

    protected void ProcessKeyboard(out Vector2 movement)
    {
        movement = Vector3.zero;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }

        if (cameraTransform == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            EventMgr.instance.TriggerEvent("jumpPress");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (kkk)
                Time.timeScale = 0.2f;
            else
                Time.timeScale = 1.0f;
            kkk = !kkk;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ReadyLongPress();
        }
        else if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            EndLongPress();
        }

        Vector3 dirZ = Vector3.zero;
        Vector3 dirX = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0)
        {
            dirZ = cameraTransform.forward;
            dirZ.y = 0.0f;
            dirZ.Normalize();
            movement.y -= 0.5f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0)
        {
            dirZ = -cameraTransform.forward;
            dirZ.y = 0.0f;
            dirZ.Normalize();
            movement.y += 0.5f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0)
        {
            dirX = -cameraTransform.right;
            dirX.y = 0.0f;
            dirX.Normalize();
            movement.x -= 0.5f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0)
        {
            dirX = cameraTransform.right;
            dirX.y = 0.0f;
            dirX.Normalize();
            movement.x += 0.5f;
        }

		keyBoardMoveDir = dirX + dirZ;
		keyBoardMoveDir.Normalize();
		if (keyBoardMoveDir.sqrMagnitude > 0.5f)
        {
			EventMgr.instance.TriggerEvent<Vector3>("joystickMove", keyBoardMoveDir);
            keyBoardMoving = true;
        }
        else
        {
            if (keyBoardMoving)
            {
                EventMgr.instance.TriggerEvent("joystickStop");
                keyBoardMoving = false;
            }
        }
    }
#endif
    #endregion
}