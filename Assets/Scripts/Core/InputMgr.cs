using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum InputModel : int
{
    DisableInput = 0,
    JoystickModel,       // 摇杆模式
}

public enum InputControlMode
{
    Normal = 0,
    InitStatic,
    Static,
    Touch,
}

public class InputMgr : Singleton<InputMgr>
{
    public InputModel inputModel = InputModel.JoystickModel;
    public bool processInput  =true;
    private InputJoystick inputJoystick;

    private HashSet<int> buttonPressList = new HashSet<int>();
    int jumpButtonIndex = 1;

    int lastTouchCount = 0;

    public InputControlMode inputControlMode = InputControlMode.InitStatic;

    public float gmMoveSpeed = 3.5f;
    public bool ignoreKeyBoardInput;

    public void Init()
    {
        //var gameStage = CoreEntry.GetGameStage();
        GameObject obj = new GameObject("InputRoot");
        inputJoystick = obj.AddComponent<InputJoystick>();
        SetInputModel(InputModel.JoystickModel);
        OnStopProcessInput("");
        InitEventListener();
    }
    
    public void Release()
    {
        EventMgr.instance.RemoveListener(this);
    }

    void InitEventListener()
    {
        EventMgr.instance.AddListener("jumpButtonPress", OnJumpButtonPressed, false, EventMgr.EventPriority.Highest);
        EventMgr.instance.AddListener("jumpButtonRelease", OnJumpButtonReleased, false, EventMgr.EventPriority.Highest);
        EventMgr.instance.AddListener("beginProcessInput", OnStartProcessInput, false);
        EventMgr.instance.AddListener("endProcessInput", OnStopProcessInput, false);
        EventMgr.instance.AddListener<int>("changeInputControlMode",OnChangeInputControlMode);
    }

    void OnChangeInputControlMode(string gameEvent,int mode)
    {
        var joystickPanel = UIMgr.instance.GetPanel(PanelNameDefine.JOYSTICK_PANEL);
        if(joystickPanel != null)
        {
            joystickPanel.gameObject.SetActive(true);
        }
        switch(mode)
        {
            case 0:
                inputControlMode = InputControlMode.Normal;
                break;
            case 1:
                inputControlMode = InputControlMode.InitStatic;
                break;
            case 2:
                inputControlMode = InputControlMode.Static;
                break;
            case 3:
                {
                    inputControlMode = InputControlMode.Touch;
                    joystickPanel.gameObject.SetActive(false);
                }
                break;
        }
    }

    void OnStartProcessInput(string gameEvent)
    {
        processInput = true;
        EventMgr.instance.TriggerEvent("startProcessInput");
    }

    void OnStopProcessInput(string gameEvent)
    {
        processInput = false;
        EventMgr.instance.TriggerEvent("stopProcessInput");
    }

    public void SetInputModel(InputModel inputModel)
    {
        this.inputModel = inputModel;
        if (inputModel == InputModel.JoystickModel)
        {
            inputJoystick.enabled = true;
            inputJoystick.joystick.enabled = true;
        }
    }

    public void UpdateTick()
    {
        int currentCount = 0;

#if UNITY_EDITOR
        currentCount = Input.GetMouseButton(0) ? 1 : 0;
        if (currentCount == 0)
        {
            if (lastTouchCount != 0)
                EventMgr.instance.TriggerEvent("noTouchInput");
        }
#else
        currentCount = Input.touchCount;
		if (currentCount == 0)
		{
			if (lastTouchCount != 0)
				EventMgr.instance.TriggerEvent("noTouchInput");
		}
#endif
        if (IsJumpButtonPressed(jumpButtonIndex) && IsProcessInput())
        {
            EventMgr.instance.TriggerEvent("jumpPress");
        }

        lastTouchCount = currentCount;
    }

    public InputBase GetInputBase(InputModel inputModel)
    {
        switch (inputModel)
        {
            case InputModel.JoystickModel:
                return inputJoystick;
        }

        return null;
    }

    public InputBase GetInputBase()
    {
        return GetInputBase(inputModel);
    }

    public bool IsProcessInput()
    {
        return processInput;
    }

    public bool IsJumpButtonPressed(int buttonIndex)
    {
        return buttonPressList.Contains(buttonIndex);
    }

    public bool GetJoystickDir(out Vector3 dir)
    {
        if (inputModel != InputModel.JoystickModel)
        {
            dir = Vector3.zero;
            return false;
        }

        return inputJoystick.GetJoystickDir(out dir);
    }

    public bool IsInMiniCircle()
    {
        if (inputModel != InputModel.JoystickModel)
        {
            return false;
        }

        return inputJoystick.joystick.IsInMiniCircle();
    }

    public Joystick GetJoystick()
    {
        return inputJoystick.joystick;
    }

    void ProcessButtonPress(int buttonIndex, bool press)
    {
        if (press)
        {
            if (!buttonPressList.Contains(buttonIndex))
                buttonPressList.Add(buttonIndex);
        }
        else
        {
            buttonPressList.Remove(buttonIndex);
        }
    }

    void OnJumpButtonPressed(string gameEvent)
    {
        ProcessButtonPress(1, true);
    }

    void OnJumpButtonReleased(string gameEvent)
    {
        ProcessButtonPress(1, false);
    }
}
