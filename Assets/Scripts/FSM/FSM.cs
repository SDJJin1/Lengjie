using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泛型有限状态机
/// 管理不同状态之间的切换和执行逻辑
/// 通过泛型支持不同类型的状态实现
/// </summary>
/// <typeparam name="T">状态类型，必须实现IFSMState接口</typeparam>
/// <remarks>
/// 使用示例：
/// <code>
/// // 定义状态接口
/// public interface IFSMState
/// {
///     void Enter();
///     void LogicalUpdate();
///     void Exit();
/// }
/// 
/// // 实现具体状态
/// public class IdleState : IFSMState
/// {
///     public void Enter() { Debug.Log("进入空闲状态"); }
///     public void LogicalUpdate() { Debug.Log("空闲状态更新"); }
///     public void Exit() { Debug.Log("退出空闲状态"); }
/// }
/// 
/// // 使用状态机
/// var fsm = new FSM<IFSMState>();
/// fsm.AddState(new IdleState());
/// fsm.SwitchOn(typeof(IdleState));
/// fsm.OnUpdate(); // 调用当前状态的逻辑更新
/// fsm.ChangeState(typeof(MoveState)); // 切换到移动状态
/// </code>
/// </remarks>
public class FSM<T> where T : IFSMState
{
    /// <summary>
    /// 状态表
    /// 存储所有已注册的状态实例，以状态类型为键
    /// 通过System.Type作为键，确保每种状态类型只能有一个实例
    /// </summary>
    public Dictionary<System.Type, T> StateTable { get; private set; }
    
    /// <summary>
    /// 当前状态
    /// 表示状态机当前正在执行的状态
    /// 默认为default(T)，在启动前为null
    /// </summary>
    protected T currentState;
    
    /// <summary>
    /// 构造函数
    /// 初始化状态表和当前状态
    /// 状态机初始时为空，需要手动添加状态
    /// </summary>
    public FSM()
    {
        StateTable = new Dictionary<System.Type, T>();
        currentState = default;  // 当前状态初始化为默认值
    }

    /// <summary>
    /// 添加状态
    /// 将状态实例添加到状态表中
    /// 状态类型作为字典的键，确保每种状态只有一个实例
    /// </summary>
    /// <param name="state">要添加的状态实例</param>
    /// <exception cref="System.ArgumentException">当状态类型已存在时抛出</exception>
    public void AddState(T state)
    {
        StateTable.Add(state.GetType(), state);
    }

    /// <summary>
    /// 启动状态机
    /// 从指定状态开始运行状态机
    /// 调用起始状态的Enter方法
    /// </summary>
    /// <param name="startState">起始状态类型</param>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">当起始状态未注册时抛出</exception>
    public void SwitchOn(System.Type startState)
    {
        // 从状态表中获取起始状态
        currentState = StateTable[startState];
        // 调用起始状态的进入方法
        currentState.Enter();
    }

    /// <summary>
    /// 切换状态
    /// 退出当前状态，进入下一个状态
    /// 先调用当前状态的Exit方法，然后切换到新状态并调用其Enter方法
    /// </summary>
    /// <param name="nextState">下一个状态类型</param>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">当下一个状态未注册时抛出</exception>
    public void ChangeState(System.Type nextState)
    {
        // 退出当前状态
        currentState.Exit();
        // 切换到下一个状态
        currentState = StateTable[nextState];
        // 进入新状态
        currentState.Enter();
    }

    /// <summary>
    /// 状态机更新
    /// 调用当前状态的逻辑更新方法
    /// 应在游戏的每帧更新中调用（如MonoBehaviour的Update方法）
    /// </summary>
    public void OnUpdate()
    {
        // 调用当前状态的逻辑更新
        currentState.LogicalUpdate();
    }
}