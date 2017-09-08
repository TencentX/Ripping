using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// 藏人的箱子
/// </summary>
public class Box : MonoBehaviour
{
	/// <summary>
	/// 箱子id
	/// </summary>
	public int id;

	/// <summary>
	/// 跳出的位置
	/// </summary>
	public Transform outTransform;

	// 隐藏的玩家
	private TestController player;

	public void OnTriggerEnter(Collider collider)
	{
		if (!collider.tag.Equals("Player"))
			return;
		NetworkIdentity identity = collider.gameObject.GetComponent<NetworkIdentity>();
		if (identity == null || !identity.isLocalPlayer)
			return;
		SignPanel panel = UIMgr.instance.GetOrCreatePanel("p_ui_sign_panel") as SignPanel;
		panel.SetOwner(gameObject);
		panel.gameObject.SetActive(true);
		EventMgr.instance.TriggerEvent<bool>("CloseToBox", true);
	}

	public void OnTriggerExit(Collider collider)
	{
		if (!collider.tag.Equals("Player"))
			return;
		NetworkIdentity identity = collider.gameObject.GetComponent<NetworkIdentity>();
		if (identity == null || !identity.isLocalPlayer)
			return;
		GameObject panel = UIMgr.instance.GetPanel("p_ui_sign_panel");
		if (panel != null)
			panel.SetActive(false);
		EventMgr.instance.TriggerEvent<bool>("CloseToBox", false);
	}

	public void SetHider(TestController player)
	{
		this.player = player;
	}

	public TestController GetHider()
	{
		return player;
	}

	public Vector3 GetOutPos()
	{
		return outTransform.position;
	}

	public Quaternion GetOutRotation()
	{
		return outTransform.rotation;
	}
}
