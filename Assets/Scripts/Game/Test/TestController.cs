using UnityEngine;
using System.Collections;

public class TestController : MonoBehaviour
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
		EventMgr.instance.TriggerEvent("beginProcessInput");
		EventMgr.instance.AddListener<Vector3>("joystickMove", OnMove);
		EventMgr.instance.AddListener("joystickStop", OnStop);
		EventMgr.instance.AddListener("jumpPress", OnJumpPress);
	}

	void FixedUpdate()
	{
		thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
	}

	private void OnMove(string gameEvent, Vector3 dir)
	{
		if (true)
		{
			if (stateMachine.playerState != PlayerStateMachine.PlayerState.Catch)
				stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
			FaceToDir(dir);
			controller.Move(dir * moveSpeed * Time.deltaTime);
		}
//		else
//		{
//			stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
//			FaceToDir(dir);
//			controller.Move(dir * runSpeed * Time.deltaTime);
//		}
	}

	private void FaceToDir(Vector3 dir)
	{
		dir.y = 0.0f;
		dir.Normalize();
		targetRotation = Quaternion.LookRotation(dir);
	}

	private void OnStop(string gameEvent)
	{
		stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
	}

	private void OnJumpPress(string gameEvent)
	{
		stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
	}

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
			stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
		}, 0f, 0f, 1f);
	}
	#endregion 玩家状态
}
