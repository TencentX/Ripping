using UnityEngine;
using System.Collections;
using System;

public class Singleton<T> : IDisposable where T : new()
{
	private static T inst;

	public static T instance
	{
		get 
		{
			if (inst == null) 
			{ 
				inst = new T(); 
			} 
			return inst; 
		}
	}

	public virtual void Dispose()
	{
		inst = default(T);
	}
}
