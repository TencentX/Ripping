
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 池的创建策略 
/// </summary>
interface GamePoolCreateTactice:IDisposable
{
    void Create(string belongtopath,UnityEngine.Object asset,int initNum ,int maxNum,GameObject root);

    GameObject Give();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns>这里的bool值代表是否销毁</returns>
    void Back(GameObject item);

    bool IsEmpty();
     
}