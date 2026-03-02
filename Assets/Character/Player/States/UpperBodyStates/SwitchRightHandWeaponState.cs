using System.Collections;
using System.Collections.Generic;
using Character.Player;
using EventCenter.Events;
using Item;
using UnityEngine;

/// <summary>
/// 切换右手武器状态
/// 处理玩家切换右手武器的状态逻辑，包括武器索引更新、模型加载和UI事件触发
/// </summary>
public class SwitchRightHandWeaponState : PlayerState
{
    private float timer;
    
    public SwitchRightHandWeaponState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // 重置状态计时器
        timer = 0;

        // 执行右手武器切换逻辑
        SwitchRightWeapon();
    }
    
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        
        // 更新状态计时器
        timer += Time.deltaTime;

        // 0.25秒后结束切换武器状态
        if (timer >= 0.25f)
        {
            agent.PlayerUpperBody.ChangeState(typeof(UpperBodyEmptyState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private void SwitchRightWeapon()
    {
        // 增加右手武器索引
        agent.rightHandWeaponIndex += 1;
        
        // 限制索引在0-2范围内循环
        if (agent.rightHandWeaponIndex < 0 || agent.rightHandWeaponIndex > 2)
            agent.rightHandWeaponIndex = 0;

        // 根据索引获取对应的右手武器数据
        agent.rightHandWeaponItem = agent.weaponsInRightHandSlots[agent.rightHandWeaponIndex];
        
        // 加载右手武器模型
        agent.LoadRightHandWeapon();
        
        // 触发事件更新右手武器槽UI图标
        EventCenter.EventCenter.Fire(this, UpdateRightHandWeaponSlotEventArgs.Create(agent.rightHandWeaponItem.itemIcon));
    }
}
