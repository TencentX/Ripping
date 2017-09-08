using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TestController : NetworkBehaviour
{
	[Tooltip("移动速度")]
	[SyncVar]
	public float moveSpeed = 1.0f;

	[Tooltip("奔跑速度")]
	[SyncVar]
	public float runSpeed = 2.0f;

	[Tooltip("可蹦跑时间")]
	[SyncVar]
	public float runTime = 3.0f;

	[Tooltip("身体半径，撕扯检测用")]
	[SyncVar]
	public float bodyRadius = 0.5f;

	[Tooltip("旋转速度，每帧旋转的度数")]
	[SyncVar]
	public float rotateSpeed = 2f;

	[Tooltip("撕扯距离")]
	[SyncVar]
	public float catchDistance = 1.0f;

	[Tooltip("撕扯角度")]
	[SyncVar]
	public float catchDegree = 80.0f;

	[Tooltip("玩家名称")]
	[SyncVar(hook = "OnPlayerName")]
	public string playerName = "";

	[Tooltip("视野半径")]
	[SyncVar(hook = "OnSightRange")]
	public float sightRange = 10;

	[Tooltip("视野角度")]
	[SyncVar]
	public float sightAngle = 80;

	CharacterController controller;
	PlayerStateMachine stateMachine;
	Animation animate;
	Light sight;
	GameObject model;
	Transform thisTransform;
	HudControl hudControl;
	SightController sightController;

	// 目标朝向
	private Quaternion targetRotation;

	// 蹦跑开始、结束时间
	private float runStartTime;
	private float runEndTime;
	private float showRunTime;

	// 是否正在打开箱子
	private bool openingBox = false;

	// 输入
	[SyncVar]
	private Vector3 inputDir = Vector3.zero;
	[SyncVar]
	private bool inputStop = true;
	[SyncVar]
	private bool inputCatch = false;
	[SyncVar(hook = "OnRun")]
	private bool inputRun = false;
	[SyncVar]
	private float leftRunTime;
	struct HideInfo
	{
		public bool hide;
		public int id;
	}
	[SyncVar(hook = "OnHide")]
	private HideInfo hideInfo;
	[SyncVar(hook = "OnCaught")]
	private bool outputCaught = false;
	[SyncVar(hook = "OnScore")]
	private int score = 0;

	void Awake()
	{
		thisTransform = transform;
		controller = GetComponent<CharacterController>();
		stateMachine = GetComponent<PlayerStateMachine>();
		animate = GetComponentInChildren<Animation>();
		sight = GetComponentInChildren<Light>();
		model = transform.Find("Actormodel").gameObject;
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
		if (hudControl == null)
		{
			hudControl = new HudControl();
			hudControl.Init(thisTransform);
			hudControl.CreateHudName(playerName);
		}
		if (isLocalPlayer)
		{
			sight.enabled = true;
			sight.range = sightRange;
			sight.spotAngle = sightAngle - 10;
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
		if (isClient)
			sightController = gameObject.AddMissingComponent<SightController>();
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
		else if (inputRun)
		{
			// 正在跑动
			float nowTime = Time.realtimeSinceStartup;
			if (nowTime - runStartTime < leftRunTime)
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
				if (hasAuthority)
				{
					thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
					controller.Move(inputDir * runSpeed * Time.deltaTime);
				}
			}
			else
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
				if (hasAuthority)
				{
					thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
					controller.Move(inputDir * moveSpeed * Time.deltaTime);
					InjectRun(false);
				}
			}
		}
		else if (!inputStop)
		{
			// 正在移动
			stateMachine.SetState(PlayerStateMachine.PlayerState.Move);
			if (hasAuthority)
			{
				thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
				controller.Move(inputDir * moveSpeed * Time.deltaTime);
			}
		}
		else
		{
			// 停止移动
			stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
		}
	}

	private List<SightController> targetsInSight = new List<SightController>();
	private List<SightController> targetsOutSight = new List<SightController>();
	void Update()
	{
		if (isLocalPlayer)
		{
			// 计算蹦跑时间
			float lastShowRunTime = showRunTime;
			if (inputRun)
				showRunTime = Mathf.Max(Mathf.Min(leftRunTime - (Time.realtimeSinceStartup - runStartTime), runTime), 0);
			else
				showRunTime = Mathf.Max(Mathf.Min(runTime - (runEndTime - runStartTime) + Time.realtimeSinceStartup - runEndTime, runTime), 0);
			if (!lastShowRunTime.Equals(showRunTime))
				EventMgr.instance.TriggerEvent<float>("RefreshRunTime", showRunTime);
			// 计算视野
			SightMgr.instance.Check(sightController, sightRange, sightAngle, ref targetsInSight, ref targetsOutSight);
			for (int i = 0; i < targetsInSight.Count; i++)
			{
				targetsInSight[i].BecameVisible();
			}
			for (int i = 0; i < targetsOutSight.Count; i++)
			{
				targetsOutSight[i].BecameInvisible();
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

	[Command]
	void CmdPlayerName(string name)
	{
		playerName = name;
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
		if (inputRun == value)
			return;
		inputRun = value;
		if (inputRun)
			inputStop = false;
		else
			inputStop = true;
		if (inputRun)
		{
			float lastRunStartTime = runStartTime;
			runStartTime = Time.realtimeSinceStartup;
			leftRunTime = Mathf.Max(Mathf.Min(runTime - (runEndTime - lastRunStartTime) + runStartTime - runEndTime, runTime), 0.0f);
		}
		else
		{
			runEndTime = Time.realtimeSinceStartup;
			leftRunTime = Mathf.Max(Mathf.Min(leftRunTime - (runEndTime - runStartTime), runTime), 0.0f);
		}
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
			thisTransform.position = box.GetOutPos();
			thisTransform.rotation = box.GetOutRotation();
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
		if (!hasAuthority)
		{
			this.hideInfo = hideInfo;
			Box box = BoxMgr.instance.GetBox(hideInfo.id);
			if (box != null)
			{
				if (hideInfo.hide)
				{
					thisTransform.position = box.transform.position;
				}
				else
				{
					thisTransform.position = box.GetOutPos();
					thisTransform.rotation = box.GetOutRotation();
				}
			}
		}
		model.SetActive(!hideInfo.hide);
		if (isLocalPlayer)
			EventMgr.instance.TriggerEvent<bool>("SwitchHide", hideInfo.hide);
	}
	
	private void OnRun(bool inputRun)
	{
		if (!hasAuthority)
		{
			this.inputRun = inputRun;
			if (inputRun)
			{
				runStartTime = Time.realtimeSinceStartup;
			}
			else
			{
				runEndTime = Time.realtimeSinceStartup;
			}
		}
	}

	private void OnCaught(bool outputCaught)
	{
		this.outputCaught = outputCaught;
		if (isLocalPlayer && outputCaught)
		{
			hudControl.HideSliderTime();
		}
	}

	private void OnScore(int score)
	{
		int delta = score - this.score;
		this.score = score;
		if (isLocalPlayer && delta != 0)
		{
			hudControl.ShoweHudTip("+" + delta);
			EventMgr.instance.TriggerEvent<int, int>("AddScore", score, delta);
		}
	}

	private void OnSightRange(float range)
	{
		this.sightRange = range;
		sight.range = this.sightRange;
	}

	private void OnPlayerName(string name)
	{
		this.playerName = name;
		if (hudControl != null)
			hudControl.CreateHudName(this.playerName);
	}

	private void OnMove(string gameEvent, Vector3 dir)
	{
		// 正在开箱子，不处理
		if (openingBox)
			return;
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
		System.Action callback;
		openingBox = true;
		callback = () =>
		{
			openingBox = false;
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
		};
		if (hudControl != null)
			hudControl.ShowSliderTime(0f, 1f, 1f, callback);
	}

	private void OnBoxPress(string gameEvent)
	{
		System.Action callback;
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
			openingBox = true;
			callback = () =>
			{
				openingBox = false;
				Box box = BoxMgr.instance.GetBoxAround(thisTransform.position);
				if (box == null)
					return;
				if (hasAuthority)
					InjectLook(box.id);
				else
					CmdLook(box.id);
			};
			if (hudControl != null)
				hudControl.ShowSliderTime(0f, 1f, 1f, callback);
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
		// 自己加分
		int half = Mathf.CeilToInt(player.score / 2.0f);
		int score = Mathf.Max(half, 1);
		AddScore(score);
		// 自己加能量
		leftRunTime = Mathf.Min(leftRunTime + runTime * 0.3f, runTime);
		// 别人扣分
		player.AddScore(-half);
		player.outputCaught = true;
		Scheduler.Create(this, (sche, t, s) => {
			// 一段时间后复活玩家
			player.transform.position = NetManager.singleton.GetStartPosition().position;
			inputRun = false;
		}, 0f, 0f, 1f);
		// 通知所有玩家
		RpcBeCaught(playerName, player.playerName, half);
	}

	public void AddScore(int score)
	{
		sightRange += score * 0.1f;
		runTime += 0.1f;
		this.score += score;
	}

	[ClientRpc]
	public void RpcBeCaught(string casterName, string reciverName, int score)
	{
		string tip = string.Concat(casterName, "玩家撕掉了", reciverName, "玩家的名牌", "获得", score, "分");
		UIMgr.instance.ShowTipString(tip);
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		if (hasAuthority)
			playerName = Game.inputName;
		else
			CmdPlayerName(Game.inputName);
	}

	public void GetInSight()
	{
		hudControl.Show();
	}
	
	public void OutOfSight()
	{
		hudControl.Hide();
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

	void OnDestroy()
	{
		RipMgr.instance.RemoveTarget(gameObject);
		if (hudControl != null)
			hudControl.Release();
		hudControl = null;
	}
}
