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
	GameObject model;
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
	private bool inputHide = false;
	[SyncVar]
	private bool outputCaught = false;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		stateMachine = GetComponent<PlayerStateMachine>();
		animate = GetComponentInChildren<Animation>();
		sight = GetComponentInChildren<Light>();
		model = transform.Find("Actormodel").gameObject;
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
			EventMgr.instance.AddListener<GameObject>("OnSignPress", OnSingPress);
			EventMgr.instance.AddListener("outPress", OnOutPress);
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
		if (inputHide)
		{
			// 躲藏
			if (hasAuthority)
			{
				thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
			}
		}
		else if (outputCaught)
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
			if (!inputRun)
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
				if (hasAuthority)
				{
					// 有数据权限才设置位置和朝向
					thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
					controller.Move(inputDir * moveSpeed * Time.deltaTime);
				}
			}
			else
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
				if (hasAuthority)
				{
					thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
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

	[Command]
	void CmdHide(bool hide, Vector3 hidePos)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController controller = go.GetComponent<TestController>();
			controller.InjectHide(hide, hidePos);
		}
	}
	#endregion

	#region input
	void InjectMove(Vector3 dir)
	{
		inputStop = false;
		inputDir = dir;
		FaceToDir(inputDir);
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

	void InjectHide(bool value, Vector3 hidePos)
	{
		if (inputHide == value)
			return;
		inputHide = value;
		if (inputHide)
		{
			gameObject.transform.position = hidePos;
		}
		else
		{
			gameObject.transform.position += gameObject.transform.forward * 1;
		}
		model.SetActive(!inputHide);
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

	private void OnSingPress(string gameEvent, GameObject go)
	{
		if (hasAuthority)
			InjectHide(true, go.transform.position);
		else
			CmdHide(true, go.transform.position);
	}

	private void OnOutPress(string gameEvent)
	{
		if (hasAuthority)
			InjectHide(false, transform.position);
		else
			CmdHide(false, transform.position);
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
