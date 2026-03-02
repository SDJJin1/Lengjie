using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

/// <summary>
/// 玩家状态机类
/// 继承自泛型有限状态机基类，专门管理玩家的状态转换和状态逻辑
/// 泛型参数限定为PlayerState，确保状态机只接受玩家状态类
/// </summary>
public class PlayerFSM : FSM<PlayerState>
{
    public PlayerState CurrentState => currentState;
}
