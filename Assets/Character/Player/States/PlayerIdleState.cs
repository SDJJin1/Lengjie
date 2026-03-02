using Character.Player.States.ActionStates;
using Character.Player.States.ActionStates.BlockState;
using UnityEngine;

namespace Character.Player.States
{
    /// <summary>
    /// 玩家待机状态
    /// 处理玩家在地面上静止时的状态逻辑，根据输入转换到移动或格挡状态
    /// </summary>
    public class PlayerIdleState : PlayerState
    {
        public PlayerIdleState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 检测是否有移动输入
            if (agent.inputManager.moveAmount != 0)
            {
                // 如果有移动输入，检查是否同时按下了格挡键
                if (agent.inputManager.blockInput)
                {
                    // 切换到格挡移动状态
                    agent.PlayerFsmBase.ChangeState(typeof(BlockingMoveState));
                }
                else
                {
                    // 切换到地面移动状态
                    agent.PlayerFsmBase.ChangeState(typeof(PlayerGroundMoveState));
                }
            }
            // 没有移动输入时，检查格挡输入
            else
            {
                if (agent.inputManager.blockInput)
                {
                    // 切换到格挡待机状态
                    agent.PlayerFsmBase.ChangeState(typeof(BlockingIdleState));
                }
                // 如果没有输入，则保持待机状态
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
