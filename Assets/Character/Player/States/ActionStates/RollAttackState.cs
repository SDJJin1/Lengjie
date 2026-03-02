using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Character.Player.States.ActionStates;
using UnityEngine;

/// <summary>
/// 翻滚攻击状态
/// 处理玩家在翻滚后立即进行攻击的状态逻辑
/// </summary>
public class RollAttackState : PlayerState
{
    private float timer = 0;
    public RollAttackState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        // 清除轻攻击输入标志，防止输入残留
        agent.inputManager.lightAttackInput = false;
        base.Enter();
        
        // 重置状态计时器
        timer = 0;
        
        agent.CostStamina(20);
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        
        // 更新状态计时器
        timer += Time.deltaTime;
        
        // 检查是否达到翻滚攻击动画的结束时间
        if (timer >= agent.rightHandWeaponItem.RollAttack.endTimer)
        {
            // 切换到空动作状态
            agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
