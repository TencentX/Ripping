using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestController : NetworkBehaviour
{
	[Tooltip("移动速度")]
	public float moveSpeed = 1.0f;

	[Tooltip("奔跑速度")]
	public float runSpeed = 2.0f;

	[Tooltip("旋转速度，每帧旋转的度数")]
	public float rotateSpeed = 2f;

	[Tooltip("撕扯距离")]
	public float catchDistance = 1.0f;

	[Tooltip("撕扯角度")]
	public float catchDegree = 80.0f;

	CharacterController controller;
	PlayerStateMachine stateMachine;
	Animation animate;
	Light sight;
	Transform thisTransform;

	// 目标朝向
	private Quaternion targetRotation;

	// 输入
	[SyncVar]
	private Vector3 inputDir = Vector3.zero;
	[SyncVar]
	private bool inputStop = true;
	[SyncVar]
	private bool inputCatch = false;
	[SyncVar]
	private bool inputRun = false;
	[SyncVar]
	private bool outputCaught = false;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		stateMachine = GetComponent<PlayerStateMachine>();
		animate = GetComponentInChildren<Animation>();
		sight = GetComponentInChildren<Light>();
		thisTransform = transform;
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Idle, EnterIdle);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Move, EnterMove);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Run, EnterRun);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Catch, EnterCatch);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Caught, EnterCaught);
		stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
		targetRotation = thisTransform.rotation;
		inputDir = thisTransform.forward;
	}

	void Start()
	{
		if (isLocalPlayer)
		{
			sight.enabled = true;
			EventMgr.instance.TriggerEvent("beginProcessInput");
			EventMgr.instance.AddListener<Vector3>("joystickMove", OnMove);
			EventMgr.instance.AddListener("joystickStop", OnStop);
			EventMgr.instance.AddListener("jumpPress", OnJumpPress);
			EventMgr.instance.AddListener<bool>("runPress", OnRunPress);
		}
		else
		{
			sight.enabled = false;
		}
		if (hasAuthority)
			RipMgr.instance.AddTarget(gameObject, 0.5f);
	}

	void FixedUpdate()
	{
		if (outputCaught)
		{
			// 被抓
			stateMachine.SetState(PlayerStateMachine.PlayerState.Caught);
		}
		else if (inputCatch)
		{
			// 抓人
			stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
		}
		else if (!inputStop)
		{
			// 正在移动
			thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
			if (!inputRun)
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
				if (hasAuthority)
				{
					// 有数据权限才设置位置和朝向
					FaceToDir(inputDir);
					controller.Move(inputDir * moveSpeed * Time.deltaTime);
				}
			}
			else
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
				if (hasAuthority)
				{
					FaceToDir(inputDir);
					controller.Move(inputDir * runSpeed * Time.deltaTime);
				}
			}
		}
		else
		{
			// 停止移动
			if (inputCatch)
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
			}
			else
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
			}
		}
	}

	#region command
	[Command]
	void CmdMove(Vector3 dir)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController controller = go.GetComponent<TestController>();
			controller.InjectMove(dir);
		}
	}

	[Command]
	void CmdStop()
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController controller = go.GetComponent<TestController>();
			controller.InjectStop();
		}
	}

	[Command]
	void CmdCatch()
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController controller = go.GetComponent<TestController>();
			controller.InjectCatch();
		}
	}

	[Command]
	void CmdRun(bool pressed)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController controller = go.GetComponent<TestController>();
			controller.InjectRun(pressed);
		}
	}
	#endregion

	#region input
	void InjectMove(Vector3 dir)
	{
		inputStop = false;
		inputDir = dir;
	}

	void InjectStop()
	{
		inputStop = true;
	}

	void InjectCatch()
	{
		inputCatch = true;
		stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
		GameObject ripTarget = null;
		if (RipMgr.instance.Check(gameObject, catchDistance, catchDegree, ref ripTarget))
		{
			TestController controller = ripTarget.GetComponent<TestController>();
			controller.outputCaught = true;
		}
	}

	void InjectRun(bool value)
	{
		inputStop = !value;
		inputRun = value;
	}
	#endregion

	#region local method
	private void FaceToDir(Vector3 dir)
	{
		dir.y = 0.0f;
		dir.Normalize();
		targetRotation = Quaternion.LookRotation(dir);
	}

	private void OnMove(string gameEvent, Vector3 dir)
	{
		if (hasAuthority)
		{
			// 有数据权限，直接移动
			InjectMove(dir);
		}
		else
		{
			// 无数据权限，发送数据
			CmdMove(dir);
		}
	}

	private void OnStop(string gameEvent)
	{
		if (hasAuthority)
			InjectStop();
		else
			CmdStop();
	}

	private void OnJumpPress(string gameEvent)
	{
		if (hasAuthority)
			InjectCatch();
		else
			CmdCatch();
	}

	private void OnRunPress(string gameEvent, bool pressed)
	{
		if (hasAuthority)
			InjectRun(pressed);
		else
			CmdRun(pressed);
	}
	#endregion

	#region 玩家状态
	private void EnterIdle()
	{
		animate.CrossFade("idle");
	}

	private void EnterMove()
	{
		animate.CrossFade("run");
	}

	private void EnterRun()
	{
		animate.CrossFade("run");
	}

	private void EnterCatch()
	{
		animate.Play("out");
		Scheduler.Create(this, (sche, t, s) => {
			inputCatch = false;
		}, 0f, 0f, 1f);
	}

	private void EnterCaught()
	{
		animate.Play("cry");
		Scheduler.Create(this, (sche, t, s) => {
			outputCaught = false;
		}, 0f, 0f, 1f);
	}
	#endregion 玩家状态
}
