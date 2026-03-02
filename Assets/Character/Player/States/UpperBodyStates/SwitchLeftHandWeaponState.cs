using System.Collections;
using System.Collections.Generic;
using Character.Player;
using EventCenter.Events;
using UnityEngine;

/// <summary>
/// 切换左手武器状态
/// 处理玩家切换左手武器的状态逻辑，包括武器索引更新、模型加载和UI事件触发
/// </summary>
public class SwitchLeftHandWeaponState : PlayerState
{
    private float timer = 0;
    public SwitchLeftHandWeaponState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // 重置状态计时器
        timer = 0;
        
        // 执行左手武器切换逻辑
        SwitchLeftWeapon();
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
    
    private void SwitchLeftWeapon()
    {
        // 增加左手武器索引
        agent.leftHandWeaponIndex += 1;
        
        // 限制索引在0-2范围内循环
        if (agent.leftHandWeaponIndex < 0 || agent.leftHandWeaponIndex > 2)
            agent.leftHandWeaponIndex = 0;

        // 根据索引获取对应的左手武器数据
        agent.leftHandWeaponItem = agent.weaponsInLeftHandSlots[agent.leftHandWeaponIndex];
        
        // 加载左手武器模型
        agent.LoadLeftHandWeapon();
        
        // 触发事件更新左手武器槽UI图标
        EventCenter.EventCenter.Fire(this, UpdateLeftHandWeaponSlotEventArgs.Create(agent.leftHandWeaponItem.itemIcon));
    }
}
