using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Character.Player.States.ActionStates;
using UnityEngine;

/// <summary>
/// 玩家前方受击状态
/// 处理玩家受到来自前方的攻击伤害时的状态逻辑
/// </summary>
public class HitForwardState : PlayerState
{
    private float timer = 0;
    public HitForwardState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        timer = 0;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        
        // 更新受击状态持续时间
        timer += Time.deltaTime;
        
        // 1秒后结束受击状态
        if (timer >= 1)
        {
            agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
