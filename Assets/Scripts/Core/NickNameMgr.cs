using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 昵称管理器
/// </summary>
public class NickNameMgr : Singleton<NickNameMgr>
{
	// 姓
	private List<string> lastnameList = new List<string>();

	// 男名
	private List<string> firstnameListMan = new List<string>();

	// 女名
	private List<string> firstnameListWoman = new List<string>();

	public void Init()
	{
		var res = GameDataMgr.instance.resNickName;
		if (res == null)
			return;
		int count = res.GetCount();
		for (int i=0; i<count; i++) {
			var record = res.GetRecord(i);
			string surname = GlobalFunctions.TrimByteToString(record.szSurname);
			if(GlobalFunctions.GetStringLength(surname)>0)
				lastnameList.Add(surname);
			string maleName = GlobalFunctions.TrimByteToString(record.szMaleName);
			if(GlobalFunctions.GetStringLength(maleName)>0)
				firstnameListMan.Add(maleName);
			string femaleName = GlobalFunctions.TrimByteToString(record.szFemaleName);
			if(GlobalFunctions.GetStringLength(femaleName)>0)
				firstnameListWoman.Add(femaleName);
		}
	}

	public string GetRandomName()
	{
		int gender = Random.Range(0f, 1f) < 0.5f ? 1 : 2;
		int lastIndex = 0;
		int firstIndex = 0;
		string ret = "";
		if (gender == 1)
		{
			lastIndex = Random.Range(0, lastnameList.Count);
			firstIndex = Random.Range(0, firstnameListMan.Count);
			ret = lastnameList[lastIndex] + firstnameListMan[firstIndex];
		}
		else
		{
			lastIndex = Random.Range(0, lastnameList.Count);
			firstIndex = Random.Range(0, firstnameListWoman.Count);
			ret = lastnameList[lastIndex] + firstnameListWoman[firstIndex];
		}
		return ret;
	}
}

public class GlobalFunctions
{
	public static string TrimByteToString(byte[] originByte)
	{
		if (originByte == null) return null;
		var str = GlobalFunctions.Trim(originByte);
		return System.Text.Encoding.UTF8.GetString(str == null ? originByte : str);
	}
	
	public static byte[] Trim(byte[] originByte)
	{
		byte[] resultByte = null;
		for (int i = 0; i < originByte.Length; ++i)
		{
			if (originByte[i] == 0)
			{
				resultByte = new byte[i];
				for (int j = 0; j < i; ++j)
				{
					resultByte[j] = originByte[j];
				}
				break;
			}
		}
		
		return resultByte;
	}

	public static int GetStringLength(string text)
	{
		if (text == null)
			return 0;
		int length = 0;
		int len = text.Length;
		for (int i = 0; i < len; i++)
		{
			if (text[i] != 0)
				++length;
			else
				break;
		}
		return length;
	}

	public static string GetFormatTimeString(int sec)
	{
		int restSec = sec % 60;
		int restMin = sec / 60;
		return string.Format("{0:00}:{1:00}", restMin, restSec);
	}
}
