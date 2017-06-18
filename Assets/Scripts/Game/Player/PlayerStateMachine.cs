using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家状态机
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
	// 玩家状态
	public enum PlayerState
	{
		None = 0,
		Idle,
		Move,
		Run,
		Catch,
		StateNumber,
	}

	// 状态函数代理
	public delegate void StateFunction();

	// 玩家当前状态
	PlayerState _playerState;
	public PlayerState playerState
	{
		get
		{
			return _playerState;
		}
	}

	StateFunction[] stateEnterFunctions = new StateFunction[(int)PlayerState.StateNumber];
	StateFunction[] stateLeaveFunctions = new StateFunction[(int)PlayerState.StateNumber];

	/// <summary>
	/// 设置状态
	/// </summary>
	public void SetState(PlayerState newState)
	{
		if (_playerState == newState)
			return;
		
		PlayerState oldState = _playerState;
		_playerState = newState;
		
		if (stateLeaveFunctions[(int)oldState] != null)
			stateLeaveFunctions[(int)oldState]();
		
		if (stateEnterFunctions[(int)newState] != null)
			stateEnterFunctions[(int)newState]();
	}

	/// <summary>
	/// 设置状态函数
	/// </summary>
	public void SetStateFunction(PlayerState state, StateFunction enterFunction = null, StateFunction leaveFunction = null)
	{
		stateEnterFunctions[(int)state] = enterFunction;
		stateLeaveFunctions[(int)state] = leaveFunction;
	}
}
