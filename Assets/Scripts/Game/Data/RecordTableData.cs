//#define USING_STRING_KEY
#define USING_INT16_KEY

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
//using tsf4g_tdr_csharp;
using conf;
using System;



public class RecordTableData<T> where T : tsf4g_csharp_interface, new()
{
	private RecordTable<T> _recordTable;
	private Hashtable _dictionaryData;
	private Hashtable _dictionaryListData;
	private string _tableName;
	private string _keyName;
	private bool _isLoaded = false;
    private bool _isListData = false;

    private Action<string> loadedCallback;
	public T[] Records
	{
		get
		{
			if (null == _dictionaryData)
			{
				return null;
			}
			
			T[] records = new T[_dictionaryData.Values.Count];
			
			_dictionaryData.Values.CopyTo(records, 0);
			
			return records;
		}
	}
	
	public RecordTableData(string tableName, string keyName)
	{
		_tableName = tableName;
		
		_keyName = keyName;
	}
	
	private void LoadTable(UnityEngine.Object obj)
	{
		_isLoaded = true;
		TextAsset asset = obj as TextAsset;

		if (asset != null)
		{
			_recordTable = new RecordTable<T>(_tableName);
			_recordTable.LoadTdrBin(asset);

            if ( _isListData )
            {
                _InitList(_keyName);
            }
            else
            {
                _Init();
            }
            Resources.UnloadAsset(asset);
            //加载成功不打日志
            //Log.Debug("load table over: " + name);
        }
		else
		{
			//AiToyDebug.LogError("load table error: " + _tableName);
		}
        
        loadedCallback("");
	}
	
	public void Init(Action<string> callback)
	{
        loadedCallback = callback;
        _isListData = false;

        if ( !_isLoaded )
        {
            ResourceMgr.instance.LoadConfig(_tableName,false, LoadTable);
        }
        else
        {
            loadedCallback("");
            _Init();
        }		
	}

    public void InitList(Action<string> callback, string keyName = null)
    {
        loadedCallback = callback;
        _isListData = true;

        if ( null != keyName )
        {
            _keyName = keyName;
        }

        if ( !_isLoaded )
        {
            ResourceMgr.instance.LoadConfig(_tableName, false, LoadTable);
        }
        else
        {
            loadedCallback("");
            _InitList(_keyName);
        }
    }

    private void _Init()
	{
		_dictionaryData = new Hashtable();
		
		for (int i = 0; i < _recordTable.Count; ++i)
		{
			T record = _recordTable.GetRecord(i);
			if (null == record)
			{
				continue;
			}
#if USING_STRING_KEY
			string key = GetStringKey(record);
#else
			int key = GetIntKey(record);
#endif
			if (_dictionaryData.ContainsKey(key))
			{
				LogMgr.instance.Log(LogLevel.ERROR, LogTag.CoryEntry, "duplicate key: " + key + ", table: " + _recordTable.Name);
				continue;
			}
			_dictionaryData.Add(key, record);
		}
	}
#if USING_STRING_KEY
	string GetStringKey(T record)
	{
		string value = null;
		System.Reflection.FieldInfo keyType = record.GetType().GetField(_keyName);
		if (keyType.FieldType.Name == "Byte[]")
		{
			value = System.Text.Encoding.UTF8.GetString((byte[])keyType.GetValue(record));
		}
		else
		{
			value = (string)keyType.GetValue(record);
		}
		return value;
	}

#else
    int GetIntKey(T record)
    {
        int key = 0;
        System.Reflection.FieldInfo keyType = record.GetType().GetField(_keyName);
        if (keyType.FieldType.Name == "UInt16")
        {
            UInt16 uKey = (UInt16)(record.GetType().GetField(_keyName).GetValue(record));
            key = (int)uKey;
        }
#if USING_INT16_KEY
		else if (keyType.FieldType.Name == "Int16")
		{
			key = (Int16)(keyType.GetValue(record));
		}
#endif
        else
        {
			key = (int)(keyType.GetValue(record));
        }

        return key;
    }
#endif
	
// 	public IEnumerator InitList()
// 	{
// 		if (!_isLoaded)
// 			yield return CoreEntry.globalObject.GetComponent<CoreEntry>().StartCoroutine(CoreEntry.resourceMgr.Load(_tableName, LoadTable));
// 
// 		_InitList();
// 	}
// 
// 	public void InitListSync()
// 	{
// 		if (!_isLoaded)
// 			CoreEntry.resourceMgr.LoadSync(_tableName, LoadTable);
// 		
// 		_InitList();
// 	}

	private void _InitList(string keyName)
	{
		_dictionaryListData = new Hashtable();
		
		for (int i = 0; i < _recordTable.Count; i++)
		{
			T record = _recordTable.GetRecord(i);
			if(null == record)
			{
				continue;
			}
			System.Reflection.FieldInfo keyType = record.GetType().GetField(keyName);
			int key = 0;
			if (keyType.FieldType.Name == "UInt16")
			{
				UInt16 uKey = (UInt16)(record.GetType().GetField(keyName).GetValue(record));
				key = (int)uKey;
			}
#if USING_INT16_KEY
			else if (keyType.FieldType.Name == "Int16")
			{
				key = (Int16)(keyType.GetValue(record));
			}
#endif
			else if (keyType.FieldType.Name == "Byte")
			{
				Byte uKey = (Byte)(record.GetType().GetField(keyName).GetValue(record));
				key = (int)uKey;
			}
			else
			{
				key = (int)(record.GetType().GetField(keyName).GetValue(record));
			}
			
			List<T> recordList = (List<T>)_dictionaryListData[key];
			if (recordList == null)
			{
				recordList = new List<T>();
				
				recordList.Add(record);
				
				_dictionaryListData.Add(key, recordList);
			}
			else
			{
				recordList.Add(record);
			}
		}
	}

	public T GetRecordByIndex(int index)
	{
		if (index < 0 || index >= _recordTable.Count)
		{
			return default(T);
		}

		return _recordTable.GetRecord(index);
	}

#if USING_STRING_KEY
	public T GetRecord(string key)
	{
		if (null == _dictionaryData)
		{
			return default(T);
		}
		
		if (_dictionaryData[key] != null)
		{
			return (T)_dictionaryData[key];
		}
		
		return default(T);
	}
	
	public List<T> GetRecordList(string key)
	{
		if (null == _dictionaryListData)
		{
			return null;
		}
		
		if (_dictionaryListData[key] != null)
		{
			return (List<T>)_dictionaryListData[key];
		}
		
		return null;
	}

	public void PrintTable()
	{
		if (_dictionaryData != null)
		{
			foreach( System.Collections.DictionaryEntry entry in _dictionaryData )
			{
				AiToyDebug.Log(entry.Key);
			}
		}


		if (_dictionaryListData != null)
		{
			foreach( System.Collections.DictionaryEntry entry in _dictionaryListData )
			{
				AiToyDebug.Log(entry.Key);
			}
		}

	}
#else
	public T GetRecord(int key)
	{
		if (null == _dictionaryData)
		{
			return default(T);
		}
		
		if (_dictionaryData[key] != null)
		{
			return (T)_dictionaryData[key];
		}
		
		return default(T);
	}
	
	public List<T> GetRecordList(int key)
	{
		if (null == _dictionaryListData)
		{
			return null;
		}
		
		if (_dictionaryListData[key] != null)
		{
			return (List<T>)_dictionaryListData[key];
		}
		
		return null;
	}
#endif

    public Hashtable GetRecordList()
    {
        return _dictionaryListData;
    }

    public Hashtable GetAllRecord()
    {
        return _dictionaryData;
    }
	
	public int GetCount()
	{
		if (null == _dictionaryData)
		{
			return 0;
		}
		
		return _dictionaryData.Count;
	}
	
	public int GetListCount()
	{
		if (null == _dictionaryListData)
		{
			return 0;
		}
		
		return _dictionaryListData.Count;
	}

	public int GetTableCount()
	{
		return _recordTable == null ? 0 : _recordTable.Count;
	}
}