using UnityEngine;
using System.Collections;

/// <summary>
/// 主界面
/// </summary>
public class MainPanel : PanelBase
{
	/// <summary>
	/// 我的ip
	/// </summary>
	public UILabel myIp;

	/// <summary>
	/// 蹦跑按钮
	/// </summary>
	public UIButton runBtn;
	
	/// <summary>
	/// 撕扯按钮
	/// </summary>
	public UIButton catchBtn;
	
	/// <summary>
	/// 查看箱子
	/// </summary>
	public UIButton lookBoxBtn;

	/// <summary>
	/// 跳出箱子
	/// </summary>
	public UIButton outBoxBtn;

	/// <summary>
	/// 玩家奔跑信息
	/// </summary>
	public UILabel runText;

	/// <summary>
	/// 倒计时
	/// </summary>
	public UILabel countDownTime;

	protected override void Init (GameObject obj)
	{
		myIp.text = Network.player.ipAddress;
		lookBoxBtn.gameObject.SetActive(false);
		outBoxBtn.gameObject.SetActive(false);
		UIEventListener.Get(runBtn.gameObject).onPress = OnPressRun;
		UIEventListener.Get(catchBtn.gameObject).onPress = OnPressCatch;
		EventMgr.instance.AddListener<bool>("SwitchHide", OnSwitchHide);
		EventMgr.instance.AddListener("OnClientConnect", OnClientConnect);
		EventMgr.instance.AddListener<float, float>("RefreshRunEnergy", OnRunEnergyRefresh);
		EventMgr.instance.AddListener<bool>("CloseToBox", OnCloseToBox);
	}

	void Update()
	{
		countDownTime.text = GlobalFunctions.GetFormatTimeString((int)RoundMgr.instance.leftTime);
	}

	/// <summary>
	/// 奔跑
	/// </summary>
	public void OnPressRun(GameObject go, bool pressed)
	{
		EventMgr.instance.TriggerEvent<bool>("runPress", pressed);
	}
	
	/// <summary>
	/// 撕名牌
	/// </summary>
	public void OnPressCatch(GameObject go, bool pressed)
	{
		EventMgr.instance.TriggerEvent<bool>("catchPress", pressed);
	}

	/// <summary>
	/// 箱子按钮
	/// </summary>
	public void OnBox()
	{
		EventMgr.instance.TriggerEvent("boxPress");
	}

	/// <summary>
	/// 帮助
	/// </summary>
	public void OnHelp()
	{
		UIMgr.instance.CreatePanel("p_ui_help_panel");
	}
	
	/// <summary>
	/// 退出
	/// </summary>
	public void OnClose()
	{
		if (TestController.mySelf == null)
			return;
		if (TestController.mySelf.isServer)
		{
			NetManager.singleton.StopHost();
			return;
		}
		if (TestController.mySelf.isClient)
		{
			NetManager.singleton.StopClient();
			return;
		}
	}

	private void OnSwitchHide(string gameEvent, bool hide)
	{
		outBoxBtn.gameObject.SetActive(hide);
		if (hide)
			lookBoxBtn.gameObject.SetActive(false);
	}
	
	private void OnClientConnect(string gameEvent)
	{
		if (NetManager.isServer)
			myIp.text = Network.player.ipAddress;
		else
			myIp.text = NetManager.singleton.networkAddress;
	}
	
	private void OnRunEnergyRefresh(string gameEvent, float leftRunEnergy, float runEnergy)
	{
		runText.text = string.Concat("能量：", (leftRunEnergy * 100).ToString("F0"), "/", (runEnergy * 100).ToString("F0"));
	}
	
	private void OnCloseToBox(string gameEvent, bool close)
	{
		lookBoxBtn.gameObject.SetActive(close);
	}
}
