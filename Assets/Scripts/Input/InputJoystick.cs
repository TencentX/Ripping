using UnityEngine;
using System.Collections;

public class InputJoystick : InputBase
{
	public Joystick joystick;
	private Vector3 originPos = Vector3.zero;

	void Awake()
	{
        DontDestroyOnLoad(gameObject);
		joystick = gameObject.AddComponent<Joystick>();
		joystick.SetCallbackMoveFun(OnJoystickMove);
		joystick.SetCallbackMoveFunRelative(OnJoystickMoveRelative);
		joystick.SetCallbackStopFun(OnJoystickStop);
        joystick.ResetInitPos();
        joystick.ResetInitPos2();
		joystick.GetOriginPos(out originPos);
	}

    protected override bool ShouldProcessInput()
    {
        return base.ShouldProcessInput() || joystick.IsRequire();
    }

    override protected bool ProcessMovement(Vector2 movement)
    {
        originPos.x += movement.x;
        originPos.y += movement.y;

        return joystick.UpdateInput();
    }

    protected void StopProcessInput()
    {
        joystick.EndWorking();
    }

    bool OnJoystickMove(Vector3 dir)
    {
        EventMgr.instance.TriggerEvent<Vector3>("joystickMove", dir);
        return true;
    }

    bool OnJoystickMoveRelative(Vector3 dir)
    {
        EventMgr.instance.TriggerEvent<Vector3>("joystickMoveRelative", dir);
        return true;
    }

    void OnJoystickStop()
    {
        EventMgr.instance.TriggerEvent("joystickStop");
        return;
    }

	public bool GetJoystickDir(out Vector3 dir)
	{
        if (!InputMgr.instance.ignoreKeyBoardInput)
        {
            if (keyBoardMoving)
            {
                dir = keyBoardMoveDir;
                return true;
            }

        }
		return joystick.GetJoystickDir(out dir);
	}

    public bool GetJoystickThumbDir(out Vector3 dir)
    {
        return joystick.GetJoystickThumbDir(out dir);
    }

	public bool GetJoystickOriginPos(out Vector3 dir)
	{
		return joystick.GetOriginPos(out dir);
	}
}
