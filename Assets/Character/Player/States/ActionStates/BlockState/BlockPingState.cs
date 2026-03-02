using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

public class BlockPingState : PlayerState
{
    public BlockPingState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
