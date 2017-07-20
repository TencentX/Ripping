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

	CharacterController controller;
	PlayerStateMachine stateMachine;
	Animation animate;
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

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		stateMachine = GetComponent<PlayerStateMachine>();
		animate = GetComponentInChildren<Animation>();
		thisTransform = transform;
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Idle, EnterIdle);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Move, EnterMove);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Run, EnterRun);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Catch, EnterCatch);
		stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
		targetRotation = thisTransform.rotation;
	}

	void Start()
	{
		if (isLocalPlayer)
		{
			EventMgr.instance.TriggerEvent("beginProcessInput");
			EventMgr.instance.AddListener<Vector3>("joystickMove", OnMove);
			EventMgr.instance.AddListener("joystickStop", OnStop);
			EventMgr.instance.AddListener("jumpPress", OnJumpPress);
		}
	}

	void FixedUpdate()
	{
		if (!inputStop)
		{
			thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
			if (true)
			{
				if (stateMachine.playerState != PlayerStateMachine.PlayerState.Catch)
					stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
				if (hasAuthority)
				{
					// 有数据权限才设置位置和朝向
					FaceToDir(inputDir);
					controller.Move(inputDir * moveSpeed * Time.deltaTime);
				}
			}
//			else
//			{
//				stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
//				FaceToDir(dir);
//				controller.Move(dir * runSpeed * Time.deltaTime);
//			}
		}
		else
		{
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
		InjectCatch();
		if (hasAuthority)
			InjectCatch();
		else
			CmdCatch();
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
	#endregion 玩家状态
}
