using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有限状态机状态接口
/// 定义状态机中每个状态必须实现的生命周期方法
/// 遵循状态模式设计模式，分离不同状态的行为
/// </summary>
public interface IFSMState
{
    void Enter();

    void LogicalUpdate();
    
    void Exit();
}
