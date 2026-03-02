using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Character.Player.States.UpperBodyStates;
using UnityEngine;

/// <summary>
/// 上半身空闲状态
/// 玩家上半身状态机中的默认空闲状态，负责检测上半身相关输入（使用物品、切换武器）并转换到对应状态
/// 这是玩家上半身没有执行其他动作时的核心响应状态
/// </summary>
public class UpperBodyEmptyState : PlayerState
{
    public UpperBodyEmptyState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        // 检测使用物品输入（喝药水）
        if (agent.inputManager.useItemInput)
        {
            agent.inputManager.useItemInput = false;
            
            // 切换到喝药起始状态
            agent.PlayerUpperBody.ChangeState(typeof(DrinkUpState));
        }

        // 检测切换左手武器输入
        if (agent.inputManager.switchLeftWeaponInput)
        {
            agent.inputManager.switchLeftWeaponInput = false;
            
            // 切换到切换左手武器状态
            agent.PlayerUpperBody.ChangeState(typeof(SwitchLeftHandWeaponState));
        }
        
        // 检测切换右手武器输入
        if (agent.inputManager.switchRightWeaponInput)
        {
            agent.inputManager.switchRightWeaponInput = false;
            
            // 切换到切换右手武器状态
            agent.PlayerUpperBody.ChangeState(typeof(SwitchRightHandWeaponState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
