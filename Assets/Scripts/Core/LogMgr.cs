using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;

public enum LogLevel
{
    NONE = 100, //不打印日志
	INFO =1 ,
    DEBUG = 2,
    WARNING =3,
	ERROR =4,
}

public enum LogTag
{
    None,
    UIMgr,
    ResourceMgr,
    CoryEntry,
    AssetBundleMgr,
    LogMgr,
    GameServer,
    Login,
    Map,
    GamePool,
	Fight,
    Capture,
	UniGif,
	Action,
	Shop,
    Lua,
	Formation,
    Input,
    GameData,
    SignMgr,
	League,
    Voice,
    Town,
    DownLoad,
    Scan,
    Lockstep,
    StateSync,
    Room,
    Log,
    QQSugar,
}

/// <summary>
/// 日志系统：
///    使用方式
/// LogMgr.instance.Log(LogLevel.ERROR,LogTag.UIMgr ,"messge");  
/// LogMgr.instance.Log(LogLevel.ERROR, "messge", LogTag.UIMgr,false);
/// </summary>
public class LogMgr : Singleton<LogMgr>
{

	public StreamWriter writer = null;
    private string logFilePath;

	private List<string> writeTxt = new List<string>();
	public string outpath;

    //日志开启级别
#if UNITY_EDITOR
    public static int openLevel = (int)LogLevel.INFO;
#else
	public static int openLevel = (int)LogLevel.ERROR;
#endif

    private Thread wThread;


    public LogMgr()
    {

#if UNITY_EDITOR
        outpath = Application.dataPath + "/outLog.txt";
#else
        outpath = Application.persistentDataPath + "/outLog.txt";
#endif
        if (System.IO.File.Exists (outpath)) { 	//每次启动客户端删除之前保存的Log
			File.Delete (outpath);
		}
		

        //Unity5里允许多重监听了，所以真机模式没有必要用BuglyAgent.RegisterLogCallback了
        //unity5以下才用BuglyAgent.RegisterLogCallback
        //用真机模式下用logMessageReceivedThreaded是因为logMessageReceived在真机模式下没有堆栈信息
#if UNITY_EDITOR
        Application.logMessageReceived += OnLog;
#else
#if UNITY_5
        //
        Application.logMessageReceivedThreaded += OnLog;
#else
        BuglyAgent.RegisterLogCallback (OnLog);
#endif
#endif




	}

    /// <summary>
    /// 写日志的唯一入口
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    ///<param name="tag">日志类型 比如 NetSys ,UIMgr</param>
    /// <param name="strMessage">日志数据</param>
    /// <param name="flushIme">日志是否立即刷新，用于排查Crash出现的情况</param>
    /// 
    public void Log(LogLevel logLevel, LogTag tag ,string strMessage, bool flushIme = false)
    {
        Log(logLevel,tag.ToString(),strMessage, flushIme);
    }

	// LogLevel: log级别（info， warning， error）
	// strChannel: 频道
	// strMessage: log具体信息
	private void Log(LogLevel logLevel, string strChannel, string strMessage, bool flushIme = false)
	{
	    if (wThread == null)
	    {
            wThread = new Thread(new ThreadStart(TheradWriteLog));
            wThread.Start();
        }


        string strLog = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss:fff]") + "[" + strChannel + "]" + "[" + logLevel.ToString() + "] " + strMessage;
	    switch (logLevel)
	    {
	        case LogLevel.INFO:
            case LogLevel.DEBUG:
                if(openLevel<=(int)LogLevel.DEBUG)
                Debug.Log(strLog);
	            break;
            case LogLevel.WARNING:
                if (openLevel<=(int)LogLevel.WARNING)
                    Debug.LogWarning(strLog);
                break;
            case LogLevel.ERROR:
                if(openLevel <= (int)LogLevel.ERROR)
                    Debug.LogError(strLog);
	            break;
	    }

        if(flushIme) //立即刷新
        {
            WriteLog();
        }
	}



    /// <summary>
    /// 监听Unity的回调
    /// </summary>
    /// <param name="message">日志信息</param>
    /// <param name="stacktrace">日志堆栈</param>
    /// <param name="type">日志类型</param>
    void OnLog(string message, string stacktrace, LogType type)
    {
		writeTxt.Add(message);
		if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception || type == LogType.Warning)
        {			
			writeTxt.Add(stacktrace);
            Log(LogLevel.ERROR, LogTag.LogMgr, message + stacktrace);
        }
    }

    private void InitWriter()
	{
        

        writer = new StreamWriter(outpath, true);
    }


    public void WriteLog()
    {
    }

    public void WriteLog1()
	{
		
		try
		{
			
			if(writeTxt.Count > 0)
			{
				if ( writer == null )
					InitWriter();
				
				for( int i=0; i<writeTxt.Count; ++i )
				{
					writer.WriteLine(writeTxt[i]);
				}
				
				writeTxt.Clear();
				writer.Flush();
			}
		}
		catch (System.Exception)
		{
			if(writer!=null)writer.Close();
		}
	}

    private void TheradWriteLog()
    {
        while (true)
        {
            if (this == null)
            {
                break;
            }

            lock (writeTxt)
            {
                WriteLog1();
            }

            //Debug.LogError(this + "==" + "TheradWriteLog");

            Thread.Sleep(100);
        }
    }

}

