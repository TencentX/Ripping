using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏逻辑
/// </summary>
public class Game : MonoBehaviour
{
	// 出生点
	private List<Transform> bornPoses = new List<Transform>();

	void Start()
	{
		NetManager.networkSceneName = SceneManager.GetSceneAt(0).name;
		GameDataMgr.instance.LoadAllData();
		GameObject bornPos = GameObject.Find("BornPos");
		for (int i = 0; i < bornPos.transform.childCount; i++)
		{
			Transform pos = bornPos.transform.GetChild(i);
			if (!pos.gameObject.activeSelf)
				continue;
			bornPoses.Add(pos);
			NetworkManager.RegisterStartPosition(pos);
		}
		NickNameMgr.instance.Init();
		UIMgr.instance.GetOrCreatePanel("p_ui_main_panel");
		PanelBase loginPanel = UIMgr.instance.GetOrCreatePanel("p_ui_login_panel");
		GameObject resultPanel = UIMgr.instance.GetPanel("p_ui_result_panel");
		if (resultPanel != null)
			loginPanel.gameObject.SetActive(false);
	}

	void OnDestroy()
	{
		EventMgr.instance.RemoveListener(this);
		foreach (Transform pos in bornPoses)
			NetManager.UnRegisterStartPosition(pos);
	}
}
