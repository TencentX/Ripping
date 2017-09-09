using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO: 暂停逻辑优化


public class Scheduler
{
	public enum Priority{
		Highest,
		High,
		Normal,
		Low,
		Lowest,
	}

	static int SchedulerId = 1;
	static bool Updating = false;
	static Dictionary<int, Dictionary<int, Scheduler>> scheMap = null;
	static List<System.Action> addOp = null;
	static List<System.Action> removeOp = null;

	public int actionId = -1;

	float intervalTime;
	float accuTimeInInterval;
	float delayTime;
	float continuedTime;
	float timeSinceStart = 0;
	System.Action<Scheduler, float, float> nfunc;

    public System.Object owner;
	// ignore logic using pause to implement
	bool pause = false;
    public bool ignoreTimeScale = false;

	static public void UpdateSche(){
		Updating = true;

        for (int i = 0; i < removeOp.Count; ++i)
        {
            removeOp[i]();
        }
        removeOp.Clear();
		
        for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
			Dictionary<int, Scheduler>.Enumerator enumerator = scheMap[(int)i].GetEnumerator();
			while (enumerator.MoveNext())
			{
				Scheduler sche = enumerator.Current.Value as Scheduler;
				// 默认移除
				bool stop = false;
				
				// 避免其中一个异常，导致整个游戏卡死
				try{
					stop = sche.Update();
				}catch (System.Exception ex){
					Debug.LogError("Scheduler Update:" + ex.Message + " exception:" + ex.StackTrace);
				}
				
				if (stop == false){
					sche.Stop();
				}
			}
		}

		for (int i=0; i<addOp.Count; ++i){
			addOp[i]();
		}
		addOp.Clear();
		for (int i=0; i<removeOp.Count; ++i){
			removeOp[i]();
		}
		removeOp.Clear();


		Updating = false;
	}

	static public void Init()
	{
		// init
		if (scheMap == null){
			scheMap = new Dictionary<int, Dictionary<int, Scheduler>>();
			addOp = new List<System.Action>();
			removeOp = new List<System.Action>();
			for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
				scheMap[(int)i] = new Dictionary<int, Scheduler>();
			}
		}
	}

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f)
    {
        return Create(target, f, 0, 0, 0, Priority.Normal, false);
    }

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f, float interval)
    {
        return Create(target, f, interval, 0, 0, Priority.Normal, false);
    }

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f, float interval, float continued)
    {
        return Create(target, f, interval, continued, 0, Priority.Normal, false);
    }

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f, float interval, float continued, float delay)
    {
        return Create(target, f, interval, continued, delay, Priority.Normal, false);
    }

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f, float interval, float continued, float delay, Priority pri)
    {
        return Create(target, f, interval, continued, delay, pri, false);
    }

    static public Scheduler Create(System.Object target, System.Action<Scheduler, float, float> f, float interval, float continued, float delay, Priority pri, bool ignore)
	{
		Scheduler scheduler = new Scheduler ();
		scheduler.delayTime = delay;
		scheduler.intervalTime = interval;
		scheduler.nfunc = f;
		scheduler.accuTimeInInterval = -1.0f;
		scheduler.continuedTime = continued;
		scheduler.actionId = ++SchedulerId;
		scheduler.owner = target;
        scheduler.ignoreTimeScale = ignore;

		System.Action func = ()=>{
			scheMap[(int)pri][scheduler.actionId] = scheduler;
		};

		if (Scheduler.Updating){
			addOp.Add(func);
		} else {
			func();
		}

		return scheduler;
	}

	static public int RemoveSchedule(int scheId)
	{
		if (scheId < 0) return scheId;
		for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
			var priMap = scheMap[(int)i];
			if (priMap.ContainsKey(scheId)){
				System.Action func = ()=>{
					if (priMap.ContainsKey(scheId)){
						priMap.Remove(scheId);
					}
				};
                removeOp.Add(func);
#if false
				if (Scheduler.Updating){
				    removeOp.Add(func);	
				} else {
					func();
				}
#endif
            }
		}

		return -1;
	}

    static public void RemoveSchedule(System.Object target)
    {
		for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
            if (scheMap[(int)i] != null)
            {
                foreach (var sche in scheMap[(int)i])
                {
                    if (sche.Value != null && sche.Value.owner == target)
                    {
                        Scheduler.RemoveSchedule(sche.Key);
                    }
                }
            }
		}
    }

	static public void PauseOwner(System.Object target)
    {
		for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
			foreach(var sche in scheMap[(int)i]){
				if (sche.Value.owner == target){
					sche.Value.Pause();
				}
			}
		}
    }
	static public void ResumeOwner(System.Object target)
    {
		for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
			foreach(var sche in scheMap[(int)i]){
				if (sche.Value.owner == target){
					sche.Value.Resume();
				}
			}
		}
    }

	static public Scheduler GetSchedule(int scheId)
	{
		for (Priority i=Priority.Highest; i<=Priority.Lowest; ++i){
			if (scheMap[(int)i].ContainsKey(scheId)){
				return scheMap[(int)i][scheId];
			}
		}
		return null;
	}

    // TODO: 捕捉异常
	bool Update()
	{
		if (actionId < 0) return false;
		if (pause) return true;

		float deltaTime = Time.unscaledDeltaTime;
        if (deltaTime == 0) return true;

        timeSinceStart += deltaTime;
		bool ret = true;
		float stopTime = delayTime + continuedTime;

		do
		{
			if (timeSinceStart < delayTime) break;
			// 确保最少调用一次
			if (accuTimeInInterval == -1.0f)
			{
				accuTimeInInterval = 0.0f;
				if (nfunc != null)
					nfunc(this, timeSinceStart, stopTime);
                if (continuedTime != -1 && timeSinceStart >= stopTime)
                    Stop();
				break;
			}

			if (continuedTime >= 0.0f && timeSinceStart - delayTime >= continuedTime)
			{
				// 避免调用两次
				if (continuedTime != 0.0f)
				{
					if (nfunc != null)
						nfunc(this, timeSinceStart, stopTime);
				}
				ret = false;
				break;
			}

			accuTimeInInterval += deltaTime;
			if (accuTimeInInterval >= intervalTime)
			{
				accuTimeInInterval = 0.0f;
				if (nfunc != null)
					nfunc(this, timeSinceStart, stopTime);
				break;
			}

		}while(false);

		return ret;
	}

	public void Stop()
	{
		if (actionId > 0){
			Scheduler.RemoveSchedule(actionId);
		}
		delayTime = 0.0f;
		continuedTime = 0.0f;
		actionId = -1;
	}

	public bool IsStop()
	{
		return actionId == -1;
	}

    public void Pause()
    {
        if (pause)
            return;

        pause = true;
    }

    public void Resume()
    {
        if (!pause)
            return;

        pause = false;
    }
}
