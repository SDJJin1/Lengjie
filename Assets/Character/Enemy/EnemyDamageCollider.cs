using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using EventCenter.Events;
using UnityEngine;

/// <summary>
/// 敌人伤害碰撞检测器
/// 检测敌人攻击命中玩家并施加伤害
/// </summary>
public class EnemyDamageCollider : MonoBehaviour
{
    // 父级敌人组件引用
    private Enemy enemy;
    
    // 记录最近一次受伤害的玩家，用于避免同一攻击多次伤害同一玩家
    public Player DamagedPlayer;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    /// <summary>
    /// 触发器进入检测
    /// 当与玩家发生碰撞时，对玩家造成伤害
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        
        // 检测到玩家时执行伤害逻辑
        if (player)
        {
            // 避免同一攻击对同一玩家造成多次伤害
            if(player == DamagedPlayer) return;
            
            // 记录受伤害玩家
            DamagedPlayer = player;

            // 对玩家造成伤害
            DamagedPlayer.TakeDamage(enemy);
        }
    }


    public void ClearPlayer()
    {
        DamagedPlayer = null;
    }
}
