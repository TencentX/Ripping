#define DEBUG

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using tsf4g_tdr_csharp;
using conf;

/// <summary>
/// 所有表格的基类
/// </summary>
public class RecordTable<T> where T : tsf4g_csharp_interface, new() 
{
    private List<T> lstItems = new List<T>();
    private string name;
	public string Name
	{
		get
		{
			return name;
		}
	}

    private const int _headsize = 140;
    
    public RecordTable(string name)
    {
        this.name = name;
    }

    public void LoadTdrBin(TextAsset asset) 
    {
        byte[] assetBuffer = asset.bytes;
        Int32 assetLength = assetBuffer.Length;

        if (asset.bytes.Length > _headsize)
        {
            Int32 slen = (Int32)BitConverter.ToUInt32(asset.bytes, 8);            
            int nBase = _headsize;

            TdrReadBuf srcBuf = new TdrReadBuf();
            srcBuf.set(ref assetBuffer, assetLength);

            while (nBase + slen <= assetLength)
            {
                srcBuf.setposition(nBase);
                T record = new T();
                TdrError.ErrorType ret = record.load(ref srcBuf, 0);
				if( ret != TdrError.ErrorType.TDR_NO_ERROR )
	            	Console.WriteLine(ret);
	            TdrError.getErrorString(ret);

                // yee 20160118 每行配置增加ver控制，用于处理后续运营过程中可能存在的高低版本支持字段不同问题，可以使较旧版本不读取较新的配置行
                System.Reflection.FieldInfo keyType = record.GetType().GetField("chVer");
                if (null == keyType)
                {
                    lstItems.Add(record);
                }
                else
                {
                    if (keyType.FieldType.Name == "SByte")
                    {
                        sbyte bVer = (sbyte)(record.GetType().GetField("chVer").GetValue(record));
                        if (bVer <= 1)
                        {
                            lstItems.Add(record);
                        }
                    }
                }
                nBase += slen;
            }
        }
        else
        {
            Debug.Log("RecordTable<T>.LoadTdrBin:"+"read record error! file length is zero. ");
        }
    }
      
    public int Count
    {
        get{ return lstItems.Count;}
    }

    public T GetRecord(int idx)
    {
        return (T)lstItems[idx];
    }
}
 