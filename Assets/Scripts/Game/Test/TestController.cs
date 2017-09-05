using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TestController : NetworkBehaviour
{
	[Tooltip("移动速度")]
	public float moveSpeed = 1.0f;

	[Tooltip("奔跑速度")]
	public float runSpeed = 2.0f;

	[Tooltip("身体半径，撕扯检测用")]
	public float bodyRadius = 0.5f;

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
	struct HideInfo
	{
		public bool hide;
		public int id;
	}
	[SyncVar(hook = "OnHide")]
	private HideInfo hideInfo;
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
			EventMgr.instance.AddListener<GameObject>("OnSignPress", OnSignPress);
			EventMgr.instance.AddListener("boxPress", OnBoxPress);
		}
		else
		{
			sight.enabled = false;
		}
		if (hasAuthority)
			RipMgr.instance.AddTarget(gameObject, bodyRadius);
	}

	void FixedUpdate()
	{
		if (hideInfo.hide)
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
					controller.Move(inputDir * moveSpeed * Time.deltaTime);
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
			TestController player = go.GetComponent<TestController>();
			player.InjectMove(dir);
		}
	}

	[Command]
	void CmdStop()
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController player = go.GetComponent<TestController>();
			player.InjectStop();
		}
	}

	[Command]
	void CmdCatch()
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController player = go.GetComponent<TestController>();
			player.InjectCatch();
		}
	}

	[Command]
	void CmdRun(bool pressed)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController player = go.GetComponent<TestController>();
			player.InjectRun(pressed);
		}
	}

	[Command]
	void CmdHide(HideInfo hideInfo)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController player = go.GetComponent<TestController>();
			player.InjectHide(hideInfo);
		}
	}

	[Command]
	void CmdLook(int id)
	{
		if (hasAuthority)
		{
			GameObject go = NetworkServer.FindLocalObject(netId);
			TestController player = go.GetComponent<TestController>();
			player.InjectLook(id);
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
			TestController player = ripTarget.GetComponent<TestController>();
			BeCaught(player);
		}
	}

	void InjectRun(bool value)
	{
		inputStop = !value;
		inputRun = value;
	}

	void InjectHide(HideInfo hideInfo)
	{
		if (this.hideInfo.hide == hideInfo.hide && this.hideInfo.id == hideInfo.id)
			return;
		Box box = BoxMgr.instance.GetBox(hideInfo.id);
		if (box == null)
			return;
		this.hideInfo = hideInfo;
		if (hideInfo.hide)
		{
			thisTransform.position = box.transform.position;
			box.SetHider(this);
		}
		else
		{
			thisTransform.position = box.transform.position + thisTransform.forward * 1;
			box.SetHider(null);
		}
	}

	void InjectLook(int id)
	{
		Box box = BoxMgr.instance.GetBox(id);
		if (box == null)
			return;
		TestController player = box.GetHider();
		if (player == null)
			return;
		HideInfo hideInfo;
		hideInfo.hide = false;
		hideInfo.id = id;
		player.InjectHide(hideInfo);
		BeCaught(player);
	}
	#endregion

	#region local method
	private void FaceToDir(Vector3 dir)
	{
		dir.y = 0.0f;
		dir.Normalize();
		targetRotation = Quaternion.LookRotation(dir);
	}

	private void OnHide(HideInfo hideInfo)
	{
		this.hideInfo = hideInfo;
		if (!hasAuthority)
		{
			Box box = BoxMgr.instance.GetBox(hideInfo.id);
			if (box != null)
			{
				if (hideInfo.hide)
					thisTransform.position = box.transform.position;
				else
					thisTransform.position = box.transform.position + thisTransform.forward * 1;
			}
		}
		model.SetActive(!hideInfo.hide);
		if (isLocalPlayer)
			EventMgr.instance.TriggerEvent<bool>("SwitchHide", this.hideInfo.hide);
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

	private void OnSignPress(string gameEvent, GameObject go)
	{
		Box box = go.GetComponent<Box>();
		if (box != null)
		{
			HideInfo hideInfo;
			hideInfo.hide = true;
			hideInfo.id = box.id;
			if (hasAuthority)
				InjectHide(hideInfo);
			else
				CmdHide(hideInfo);
		}
	}

	private void OnBoxPress(string gameEvent)
	{
		if (this.hideInfo.hide)
		{
			// 处于躲藏状态，跳出箱子
			HideInfo hideInfo;
			hideInfo.hide = false;
			hideInfo.id = this.hideInfo.id;
			if (hasAuthority)
				InjectHide(hideInfo);
			else
				CmdHide(hideInfo);
		}
		else
		{
			// 不处于躲藏状态，翻看箱子
			Box box = BoxMgr.instance.GetBoxAround(thisTransform.position);
			if (box == null)
				return;
			if (hasAuthority)
				InjectLook(box.id);
			else
				CmdLook(box.id);
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		TestController player = hit.gameObject.GetComponent<TestController>();
		if (player == null)
			return;
		if (RipMgr.instance.Check(gameObject, catchDistance, catchDegree, hit.gameObject, player.bodyRadius))
		{
			BeCaught(player);
		}
	}

	void BeCaught(TestController player)
	{
		if (player.outputCaught)
			return;
		player.outputCaught = true;
		Scheduler.Create(this, (sche, t, s) => {
			// 一段时间后复活玩家
			player.transform.position = NetManager.singleton.GetStartPosition().position;
		}, 0f, 0f, 1f);
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
