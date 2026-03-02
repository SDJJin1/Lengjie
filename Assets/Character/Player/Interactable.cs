using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

/// <summary>
/// 可交互接口
/// 定义游戏中可交互对象（如NPC、道具、机关等）的交互行为规范
/// 实现此接口的类必须提供与玩家交互的具体逻辑
/// </summary>
public interface Interactable
{
    /// <summary>
    /// 交互方法
    /// 当玩家与实现此接口的对象交互时调用
    /// </summary>
    /// <param name="player">发起交互的玩家实例</param>
    void Interact(Player player);
}
