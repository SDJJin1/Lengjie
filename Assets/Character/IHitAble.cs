using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

/// <summary>
/// 可受击接口
/// 定义游戏中可以被玩家攻击伤害的对象（如敌人、可破坏物体等）的受击行为规范
/// 实现此接口的类必须提供承受玩家伤害的具体逻辑
/// </summary>
public interface IHitAble
{
    /// <summary>
    /// 承受伤害方法
    /// 当对象受到玩家攻击时调用
    /// </summary>
    /// <param name="player">造成伤害的玩家实例</param>
    void TakeDamage(Player player);
}
