namespace Character.Player.States.ActionStates.BlockState
{
    /// <summary>
    /// 玩家格挡待机状态
    /// 处理玩家在格挡状态下的待机行为，根据输入转换为其他状态
    /// </summary>
    public class BlockingIdleState : PlayerState
    {
        public BlockingIdleState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();

            // 检测是否松开格挡键
            if (!agent.inputManager.blockInput)
            {
                // 根据移动输入决定切换到待机或移动状态
                if (agent.inputManager.moveAmount == 0)
                {
                    agent.PlayerFsmBase.ChangeState(typeof(PlayerIdleState));      // 切换到待机状态
                }
                else
                {
                    agent.PlayerFsmBase.ChangeState(typeof(PlayerGroundMoveState)); // 切换到地面移动状态
                }
            }
            // 如果保持格挡并开始移动，则切换到格挡移动状态
            else if(agent.inputManager.moveAmount != 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(BlockingMoveState));        // 切换到格挡移动状态
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}