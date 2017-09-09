using conf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using tdir_cs;

// 序列化字段，标记之后，认为需要被序列化到XML
[System.AttributeUsage(System.AttributeTargets.Field)]
public class SerializedPropertyAttribute : System.Attribute { }

public class GameDataMgr : Singleton<GameDataMgr>
{          
	/// <summary>
	/// 是否解析完成
	/// </summary>
	public bool isDone = false;

	public RecordTableData<ResNickname> resNickName;

    void Init()
    {
        LoadAllData();
    }

    private int m_dataCount;
    public int GetLoadDataCount() { return m_dataCount;}
    private int m_currentDataLoad;

    public void LoadAllData()
    {
        isDone = false;
        m_dataCount = 0;
        m_currentDataLoad = 0;

        // 加载配置文件              
        ++m_dataCount;
		resNickName = new RecordTableData<conf.ResNickname>("configs/bin/nickname", "iID");
		resNickName.Init(ParserConfigData);
        EventMgr.instance.TriggerEvent("LoadDataEnd");
    }
    
    /// <summary>
    /// 检测数据是否在加载完，如果加载完，则派发加载完的事件
    /// </summary>
    private void CheckDataEnd()
    {
        m_currentDataLoad++;
        if(m_dataCount== m_currentDataLoad)
        {
            isDone = true;
            EventMgr.instance.TriggerEvent("LoadDataEnd");
        }
    }

	void ParserConfigData(string str)
	{
		CheckDataEnd();
	}
}
