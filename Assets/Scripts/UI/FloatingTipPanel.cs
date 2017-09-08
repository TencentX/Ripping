﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 飘字提示界面
/// </summary>
public class FloatingTipPanel : PanelBase
{
    public UILabel text;
    [System.NonSerialized]
    public float existSec = 3.0f;
    int actionId = -1;

    void Start()
    {
        if (existSec <= 0) return;

        actionId = Scheduler.Create(this, (sche, t, s) =>
        {
            Exit();
        }, 0, 0, existSec).actionId;
    }
}
