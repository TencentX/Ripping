using UnityEngine;
using System.Collections;

public class RankItem : MonoBehaviour
{
	public UILabel rank;

	public UILabel nameLabel;

	public UILabel score;

	private RankInfo _rankInfo;
	public RankInfo rankInfo
	{
		get
		{
			return _rankInfo;
		}
	}

	public void SetData(RankInfo rankInfo)
	{
		_rankInfo = rankInfo;
		if (rankInfo.rank == -1)
		{
			gameObject.SetActive(false);
			return;
		}
		gameObject.SetActive(true);
		this.rank.text = (rankInfo.rank + 1).ToString();
		this.nameLabel.text = rankInfo.name;
		this.score.text = rankInfo.score.ToString();
	}
}
