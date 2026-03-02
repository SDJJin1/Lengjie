using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

/// <summary>
/// 伤害碰撞检测器
/// 用于玩家武器检测对可受击目标（实现IHitAble接口）的伤害
/// 通常附加在玩家武器模型上，通过触发检测对敌人造成伤害
/// </summary>
public class DamageCollider : MonoBehaviour
{
    public Collider damageCollider;

    private void Start()
    {
        damageCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 尝试获取碰撞体上的可受击组件
        IHitAble IhitAble = other.GetComponent<IHitAble>();

        // 如果碰撞体具有可受击组件，则调用其受击方法
        if (IhitAble != null)
        {
            // 传递玩家组件作为伤害来源
            IhitAble.TakeDamage(GetComponentInParent<Player>());
        }
    }
}
