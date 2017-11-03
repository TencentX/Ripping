using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TestController : NetworkBehaviour
{
	[Tooltip("移动速度")]
	[SyncVar]
	public float moveSpeed = 5.0f;

	[Tooltip("奔跑速度")]
	[SyncVar]
	public float runSpeed = 11.0f;

	[Tooltip("可奔跑能量")]
	[SyncVar]
	public float runEnergy = 3f;
	const float CONSUME_SPEED = 1f;
	const float RECOVER_SPEED = 0.4f;

	[Tooltip("身体半径，撕扯检测用")]
	[SyncVar]
	public float bodyRadius = 0.5f;

	[Tooltip("旋转速度，每帧旋转的度数")]
	[SyncVar]
	public float rotateSpeed = 2f;

	[Tooltip("奔跑旋转速度，每帧旋转的度数")]
	[SyncVar]
	public float runRotateSpeed = 4f;

	[Tooltip("被冲撞时初始速度")]
	public float offendStartSpeed = 16f;

	[Tooltip("被冲撞时旋转速度，每秒旋转的度数")]
	public float offendRotateSpeed = 120f;

	[Tooltip("被冲撞时的减速度")]
	public float offendSpeed = 5f;

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
	public SightController sightController;
	GameObject quan;
	GameObject ripArea;

	// 目标朝向
	private Quaternion targetRotation;

	// 蹦跑开始、结束时间
	private float runStartTime;
	private float runEndTime;
	private float leftRunEnergy;
	private float showRunEnergy;

	// 是否正在打开箱子
	private bool openingBox = false;

	// 是否正在防御
	private bool defending = false;
	private bool pushing = false;

	// 输入
	[SyncVar]
	private Vector3 inputDir = Vector3.zero;
	[SyncVar]
	private bool inputStop = true;
	[SyncVar]
	// 0-没有抓人，1-抓到人，2-没有抓到人
	private int inputCatch = 0;
	[SyncVar(hook = "OnRun")]
	private bool inputRun = false;
	public struct HideInfo
	{
		public bool hide;
		public int id;
	}
	[SyncVar(hook = "OnHide")]
	public HideInfo hideInfo;
	[SyncVar(hook = "OnCaught")]
	private bool outputCaught = false;
	public struct OffendInfo
	{
		public bool offend;
		public float speed;
		public Vector3 direction;
		public int clockwise;
		public float rotateSpeed;
	}
	[SyncVar(hook = "OnOffend")]
	private OffendInfo offendInfo;
	[SyncVar(hook = "OnScore")]
	public int score = 0;

	// 警戒范围
	const float WARN_DISTANCE = 14f;

	// 冲撞增加的速度的增量值的上限
	const float MAX_OFFEND_SPEED = 0f;

	// 冲撞增加的旋转速度的增量值的上限
	const float MAX_OFFEND_RATATE_SPEED = 0f;

	public static TestController mySelf;

	void Awake()
	{
		thisTransform = transform;
		controller = GetComponent<CharacterController>();
		stateMachine = GetComponent<PlayerStateMachine>();
		animate = GetComponentInChildren<Animation>();
		sight = GetComponentInChildren<Light>();
		model = transform.Find("Actormodel").gameObject;
		quan = model.transform.Find("Quan").gameObject;
		ripArea = model.transform.Find("Area").gameObject;
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Idle, EnterIdle);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Move, EnterMove);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Run, EnterRun);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Catch, EnterCatch);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Caught, EnterCaught);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Offend, EnterOffend);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Jump, EnterJump);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Defend, EnterDefend);
		stateMachine.SetStateFunction(PlayerStateMachine.PlayerState.Push, EnterPush);
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
			hudControl.CreateHudScore(this.score);
		}
		if (isLocalPlayer)
		{
			sight.enabled = true;
			sight.range = sightRange;
			sight.spotAngle = sightAngle;
			quan.SetActive(true);
			ripArea.SetActive(false);
			EventMgr.instance.TriggerEvent("beginProcessInput");
			EventMgr.instance.AddListener<Vector3>("joystickMove", OnMove);
			EventMgr.instance.AddListener("joystickStop", OnStop);
			EventMgr.instance.AddListener<bool>("catchPress", OnCatchPress);
			EventMgr.instance.AddListener<bool>("runPress", OnRunPress);
			EventMgr.instance.AddListener<GameObject>("OnSignPress", OnSignPress);
			EventMgr.instance.AddListener("boxPress", OnBoxPress);
		}
		else
		{
			sight.enabled = false;
			quan.SetActive(false);
			ripArea.SetActive(false);
		}
		leftRunEnergy = runEnergy;
		if (hasAuthority)
			RipMgr.instance.AddTarget(gameObject, bodyRadius);
		if (isClient)
			sightController = gameObject.AddMissingComponent<SightController>();
		// 有可能是中途进入游戏
		model.SetActive(!hideInfo.hide);
		if (!hideInfo.hide && sightController.InSight)
			if (hudControl != null)
				hudControl.Show();
		else
			if (hudControl != null)
				hudControl.Hide();
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
		else if (inputCatch != 0)
		{
			// 抓人
			stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
		}
		else if (offendInfo.offend)
		{
			// 被冲撞
			stateMachine.SetState(PlayerStateMachine.PlayerState.Offend);
			if (hasAuthority)
			{
				float speed = offendInfo.speed - offendSpeed * Time.deltaTime;
				if (speed < 0f)
				{
					OffendInfo newInfo = new OffendInfo();
					newInfo.offend = false;
					offendInfo = newInfo;
					return;
				}
				thisTransform.Rotate(Vector3.up * offendInfo.clockwise * Time.deltaTime * offendInfo.rotateSpeed);
				offendInfo.speed = speed;
				controller.Move(offendInfo.direction * offendInfo.speed * Time.deltaTime);
			}
		}
		else if (inputRun)
		{
			// 正在跑动
			float nowTime = Time.realtimeSinceStartup;
			if ((nowTime - runStartTime) / CONSUME_SPEED < leftRunEnergy)
			{
				stateMachine.SetState(PlayerStateMachine.PlayerState.Run);
				if (hasAuthority)
				{
					thisTransform.rotation = Quaternion.Lerp(thisTransform.rotation, targetRotation, Time.deltaTime * runRotateSpeed);
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
		else if (pushing)
		{
			// 推
			stateMachine.SetState(PlayerStateMachine.PlayerState.Push);
		}
		else if (defending)
		{
			// 防御
			stateMachine.SetState(PlayerStateMachine.PlayerState.Defend);
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
			// 计算奔跑时间
			float lastShowRunEnergy = showRunEnergy;
			float now = Time.realtimeSinceStartup;
			if (inputRun)
				showRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy - (now - runStartTime) * CONSUME_SPEED, runEnergy), 0);
			else
				showRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy + (now - runEndTime) * RECOVER_SPEED, runEnergy), 0);
			if (!lastShowRunEnergy.Equals(showRunEnergy))
			{
				EventMgr.instance.TriggerEvent<float, float>("RefreshRunEnergy", showRunEnergy, runEnergy);
			}
			if (hudControl != null)
				hudControl.ShowSliderEnergy(showRunEnergy, runEnergy);
			// 计算视野
			bool warn = false;
			SightMgr.instance.Check(sightController, sightRange, sightAngle, bodyRadius * 6, ref targetsInSight, ref targetsOutSight);
			for (int i = 0; i < targetsInSight.Count; i++)
			{
				targetsInSight[i].BecameVisible();
			}
			for (int i = 0; i < targetsOutSight.Count; i++)
			{
				SightController target = targetsOutSight[i];
				target.BecameInvisible();
				if (!warn)
				{
					TestController player = target.GetComponent<TestController>();
					if (player != null && player.inputRun && Vector3.Distance(sightController.transform.position, player.thisTransform.position) < WARN_DISTANCE)
						warn = true;
				}
			}
			if (hudControl != null)
			{
				if (warn && !hudControl.IsWarnShow())
				{
					// 显示警戒标识和提示
					hudControl.ShowWarnIcon();
					hudControl.ShoweHudTip("听到脚步声！");
				}
				else if (!warn && hudControl.IsWarnShow())
				{
					// 隐藏警戒标识
					hudControl.HideWarnIcon();
				}
			}
		}
	}

	#region command
	[Command]
	void CmdMove(Vector3 dir)
	{
		if (hasAuthority)
		{
			InjectMove(dir);
		}
	}

	[Command]
	void CmdStop()
	{
		if (hasAuthority)
		{
			InjectStop();
		}
	}

	[Command]
	void CmdCatch()
	{
		if (hasAuthority)
		{
			InjectCatch();
		}
	}

	[Command]
	void CmdRun(bool pressed)
	{
		if (hasAuthority)
		{
			InjectRun(pressed);
		}
	}

	[Command]
	void CmdOpenBox(int boxId, bool value)
	{
		if (hasAuthority)
		{
			Box box = BoxMgr.instance.GetBox(boxId);
			if (box == null)
				return;
			box.Open(value);
		}
	}

	[Command]
	void CmdHide(HideInfo hideInfo)
	{
		if (hasAuthority)
		{
			InjectHide(hideInfo);
		}
	}

	[Command]
	void CmdLook(int id)
	{
		if (hasAuthority)
		{
			InjectLook(id);
		}
	}

	[Command]
	void CmdPlayerName(string name)
	{
		playerName = name;
		if (hasAuthority)
			EventMgr.instance.TriggerEvent<NetworkInstanceId>("RefreshScore", netId);
	}
	#endregion

	#region input
	void InjectMove(Vector3 dir)
	{
		if (offendInfo.offend)
			return;
		if (inputCatch == 1 && animate.IsPlaying("attack"))
		{
			// 如果抓到人，则在抓人动作中途可以切为移动
			AnimationState state = animate["attack"];
			if (state.time >= state.length * 10f / 30f)
				inputCatch = 0;
		}
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
		if (offendInfo.offend)
			return;
		if (outputCaught)
			return;
		if (hideInfo.hide)
			return;
		stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
		GameObject ripTarget = null;
		if (RipMgr.instance.Check(gameObject, catchDistance, catchDegree, ref ripTarget))
		{
			// 抓到了某个人
			inputCatch = 1;
			TestController player = ripTarget.GetComponent<TestController>();
			BeCaught(player);
		}
		else if (RipMgr.instance.CheckHit(gameObject, catchDistance, catchDegree, ref ripTarget))
		{
			// 碰到了某个人
			inputCatch = 2;
			TestController player = ripTarget.GetComponent<TestController>();
			player.RpcBeHit();
		}
		else
		{
			// 没有抓到人
			inputCatch = 2;
		}
	}

	void InjectRun(bool value)
	{
		if (outputCaught)
			return;
		if (offendInfo.offend)
			return;
		if (inputRun == value)
			return;
		inputRun = value;
		if (inputRun)
			inputStop = false;
		else
			inputStop = true;
		float now = Time.realtimeSinceStartup;
		if (inputRun)
		{
			leftRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy + (now - runEndTime) * RECOVER_SPEED, runEnergy), 0);
			runStartTime = now;
		}
		else
		{
			leftRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy - (now - runStartTime) * CONSUME_SPEED, runEnergy), 0);
			runEndTime = now;
		}
	}

	void InjectHide(HideInfo hideInfo)
	{
		if (this.hideInfo.hide == hideInfo.hide && this.hideInfo.id == hideInfo.id)
			return;
		Box box = BoxMgr.instance.GetBox(hideInfo.id);
		if (box == null)
			return;
		if (hideInfo.hide)
		{
			TestController player = box.GetHider();
			if (player == null)
			{
				// 箱子里没人，则躲进箱子
				thisTransform.position = box.transform.position;
				box.SetHider(this);
				this.hideInfo = hideInfo;
				RpcHide(netId, hideInfo);
			}
			else
			{
				// 箱子里有人，则抓人
				BeCaught(player);
				HideInfo newhideInfo;
				newhideInfo.hide = false;
				newhideInfo.id = hideInfo.id;
				player.InjectHide(newhideInfo);
				RpcHide(player.netId, newhideInfo);
			}
		}
		else
		{
			// 跳出箱子
			this.hideInfo = hideInfo;
			thisTransform.position = box.GetOutPos();
			thisTransform.rotation = box.GetOutRotation();
			box.SetHider(null);
			RpcHide(netId, hideInfo);
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
		if (player.netId == netId)
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
				box.Open();
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
		if (hideInfo.hide)
		{
			stateMachine.SetState(PlayerStateMachine.PlayerState.Jump);
			Scheduler.Create(this, (sche, t, s) => {
				if (this.hideInfo.hide)
				{
					stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
					model.SetActive(false);
				}
			}, 0f, 0f, 0.4f);
		}
		else
		{
			model.SetActive(true);
		}
		if (!hideInfo.hide && sightController.InSight)
			if (hudControl != null)
				hudControl.Show();
		else
			if (hudControl != null)
				hudControl.Hide();
		if (isLocalPlayer)
			EventMgr.instance.TriggerEvent<bool>("SwitchHide", hideInfo.hide);
	}
	
	private void OnRun(bool inputRun)
	{
		if (!hasAuthority)
		{
			this.inputRun = inputRun;
			float now = Time.realtimeSinceStartup;
			if (inputRun)
			{
				leftRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy + (now - runEndTime) * RECOVER_SPEED, runEnergy), 0);
				runStartTime = now;
			}
			else
			{
				leftRunEnergy = Mathf.Max(Mathf.Min(leftRunEnergy - (now - runStartTime) * CONSUME_SPEED, runEnergy), 0);
				runEndTime = now;
			}
		}
	}

	private void OnCaught(bool outputCaught)
	{
		this.outputCaught = outputCaught;
		if (isLocalPlayer && outputCaught)
		{
			openingBox = false;
			defending = false;
			pushing = false;
			hudControl.HideSliderTime();
			UIMgr.instance.CreatePanel("p_ui_relive_panel");
		}
	}

	private void OnOffend(OffendInfo info)
	{
		if (!hasAuthority)
		{
			this.offendInfo = info;
		}
		if (this.offendInfo.offend)
			stateMachine.SetState(PlayerStateMachine.PlayerState.Offend);
		else
			stateMachine.SetState(PlayerStateMachine.PlayerState.Idle);
	}

	private void OnScore(int score)
	{
		int delta = score - this.score;
		this.score = score;
		if (isLocalPlayer && delta != 0)
		{
			if (hudControl != null)
				hudControl.ShoweHudTip("+" + delta);
			EventMgr.instance.TriggerEvent<int, int>("AddScore", score, delta);
		}
		if (hudControl != null)
			hudControl.CreateHudScore(this.score);
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
		if (outputCaught)
			return;
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

	private void OnCatchPress(string gameEvent, bool pressed)
	{
		if (outputCaught)
			return;
		if (openingBox)
			return;
		if (hideInfo.hide)
			return;
		if (!pressed)
		{
			// 撕
			ripArea.SetActive(false);
			if (hasAuthority)
				InjectCatch();
			else
				CmdCatch();
		}
		else
		{
			// 提示撕扯范围
			ripArea.SetActive(true);
		}
	}

	private void OnRunPress(string gameEvent, bool pressed)
	{
		if (outputCaught)
			return;
		if (openingBox)
			return;
		if (hideInfo.hide)
			return;
		if (hasAuthority)
			InjectRun(pressed);
		else
			CmdRun(pressed);
	}

	private void OnSignPress(string gameEvent, GameObject go)
	{
		if (outputCaught)
			return;
		if (openingBox)
			return;
		System.Action callback;
		Box box = go.GetComponent<Box>();
		if (box == null)
			return;
		CmdOpenBox(box.id, true);
		openingBox = true;
		OnStop("");
		EventMgr.instance.TriggerEvent<bool>("CloseToBox", false);
		callback = () =>
		{
			openingBox = false;
			CmdOpenBox(box.id, false);
			HideInfo hideInfo;
			hideInfo.hide = true;
			hideInfo.id = box.id;
			if (hasAuthority)
				InjectHide(hideInfo);
			else
				CmdHide(hideInfo);
		};
		if (hudControl != null)
			hudControl.ShowSliderTime(0f, 1f, 1f, callback);
	}

	private void OnBoxPress(string gameEvent)
	{
		if (outputCaught)
			return;
		Box box = BoxMgr.instance.GetBoxAround(thisTransform.position);
		if (box == null)
			return;
		System.Action callback;
		if (this.hideInfo.hide)
		{
			// 处于躲藏状态，跳出箱子
			if (box.isOpening)
			{
				// 如果箱子正在被打开，不让跳
				return;
			}
			CmdOpenBox(box.id, false);
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
			GameObject panel = UIMgr.instance.GetPanel("p_ui_sign_panel");
			if (panel != null)
				panel.SetActive(false);
			CmdOpenBox(box.id, true);
			openingBox = true;
			OnStop("");
			callback = () =>
			{
				CmdOpenBox(box.id, false);
				Scheduler.Create(this, (sche, t, s) => {
					if (panel != null)
						panel.SetActive(true);
				}, 0f, 0f, 1.0f);
				openingBox = false;
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
		if (!hasAuthority)
			return;
		if (outputCaught)
			return;
		TestController player = hit.gameObject.GetComponent<TestController>();
		if (player == null)
			return;
		if (player.outputCaught)
			return;
		if (RipMgr.instance.Check(gameObject, catchDistance, catchDegree, hit.gameObject, player.bodyRadius))
		{
			// 如果是抓到了某人
			inputCatch = 1;
			stateMachine.SetState(PlayerStateMachine.PlayerState.Catch);
			BeCaught(player);
		}
		else if (inputRun)
		{
			// 如果没有抓到，则且处于跑动状态，则进行冲撞
			OffendInfo info;
			info.offend = true;
			info.speed = offendStartSpeed + Mathf.Min(score * 0f, MAX_OFFEND_SPEED);
			info.direction = thisTransform.forward;
			info.clockwise = Vector3.Cross(player.thisTransform.position - thisTransform.position, thisTransform.forward).y < 0 ? 1 : -1;
			info.rotateSpeed = offendRotateSpeed + Mathf.Min(score * 0f, MAX_OFFEND_RATATE_SPEED);
			player.offendInfo = info;
			InjectRun(false);
			RpcPush();
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
		leftRunEnergy = Mathf.Min(leftRunEnergy + runEnergy * 0.5f, runEnergy);
		// 别人扣分
		player.AddScore(-half);
		player.outputCaught = true;
		Scheduler.Create(this, (sche, t, s) => {
			// 一段时间后复活玩家
			player.outputCaught = false;
			player.transform.position = NetManager.singleton.GetStartPosition().position;
			player.inputRun = false;
		}, 0f, 0f, RelivePanel.RELIVE_TIME);
		// 通知所有玩家
		RpcBeCaught(playerName, player.playerName, score);
	}

	public void AddScore(int score)
	{
		sightRange += score * 0.25f;
		runEnergy += 0.06f;
		this.score += score;
		if (hasAuthority)
			EventMgr.instance.TriggerEvent<NetworkInstanceId>("RefreshScore", netId);
	}

	[ClientRpc]
	public void RpcBeCaught(string casterName, string reciverName, int score)
	{
		string tip = string.Concat("[FFFF00]", casterName, "[-]撕掉了[FFFF00]", reciverName, "[-]，", "获得[FFFF00]", score, "分[-]");
		UIMgr.instance.ShowTipString(tip);
	}

	[ClientRpc]
	public void RpcBeHit()
	{
		defending = true;
		stateMachine.SetState(PlayerStateMachine.PlayerState.Defend);
	}

	[ClientRpc]
	public void RpcPush()
	{
		pushing = true;
		stateMachine.SetState(PlayerStateMachine.PlayerState.Push);
	}

	[ClientRpc]
	public void RpcHide(NetworkInstanceId netId, HideInfo hideInfo)
	{
		if (mySelf == null)
			return;
		Box box = BoxMgr.instance.GetBox(hideInfo.id);
		if (box == null)
			return;
		if (SightMgr.instance.Check(mySelf.sightController, mySelf.sightRange, mySelf.sightAngle, mySelf.bodyRadius * 4, box.gameObject))
			return;
		if (mySelf.hudControl != null && Vector3.Distance(box.transform.position, mySelf.thisTransform.position) < WARN_DISTANCE)
		{
			mySelf.hudControl.ShowWarnIcon(1.0f);
			mySelf.hudControl.ShoweHudTip("听到箱子的声音！");
		}
	}

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();
		mySelf = this;
		if (hasAuthority)
		{
			playerName = LoginPanel.inputName;
			EventMgr.instance.TriggerEvent<NetworkInstanceId>("RefreshScore", netId);
		}
		else
		{
			CmdPlayerName(LoginPanel.inputName);
		}
		BoxMgr.instance.Init();
	}

	public override void OnNetworkDestroy ()
	{
		base.OnNetworkDestroy ();
		if (hasAuthority)
		{
			EventMgr.instance.TriggerEvent<NetworkInstanceId>("NetworkDestroy", netId);
		}
	}

	public void GetInSight()
	{
		if (!hideInfo.hide)
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
		animate["walk"].speed = 1.5f;
		animate.CrossFade("walk");
	}

	private void EnterRun()
	{
		animate.CrossFade("run");
	}

	private void EnterCatch()
	{
		animate.Play("attack");
		Scheduler.Create(this, (sche, t, s) => {
			inputCatch = 0;
		}, 0f, 0f, 1f);
	}

	private void EnterCaught()
	{
		animate.Play("push&end");
		Scheduler.Create(this, (sche, t, s) => {
			outputCaught = false;
		}, 0f, 0f, RelivePanel.RELIVE_TIME);
	}

	private void EnterOffend()
	{
		animate.Play("hurt");
	}

	private void EnterJump()
	{
		animate.Play("jump");
	}

	private void EnterDefend()
	{
		animate.Play("defend");
		Scheduler.Create(this, (sche, t, s) => {
			defending = false;
		}, 0f, 0f, 0.5f);
	}

	private void EnterPush()
	{
		animate.Play("bunt");
		Scheduler.Create(this, (sche, t, s) => {
			pushing = false;
		}, 0f, 0f, 0.5f);
	}
	#endregion 玩家状态

	void OnDestroy()
	{
		if (isLocalPlayer)
			mySelf = null;
		EventMgr.instance.RemoveListener(this);
		RipMgr.instance.RemoveTarget(gameObject);
		if (hudControl != null)
			hudControl.Release();
		hudControl = null;
	}
}
