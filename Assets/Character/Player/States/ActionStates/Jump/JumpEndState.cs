using UnityEngine;

namespace Character.Player.States.ActionStates.Jump
{
    /// <summary>
    /// 跳跃结束状态
    /// 处理玩家跳跃落地后的状态逻辑，包括动画播放、状态转换和物理参数重置
    /// </summary>
    public class JumpEndState : PlayerState
    {
        private float timer = 0;
    
        public JumpEndState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 重置状态计时器
            timer = 0;

            // 重置空中相关参数
            agent.inAirTimer = 0;
            agent.fallingVelocityHasBeenSet = false;
            // 设置垂直速度为地面速度，防止玩家在落地后继续下落
            agent.yVelocity.y = agent.groundedYVelocity;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
        
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(PlayerDeadState));
            }
        
            // 更新状态计时器
            timer += Time.deltaTime;

            // 0.16秒后结束跳跃结束状态，切换到待机状态
            if (timer > 0.16f)
            {
                agent.PlayerFsmBase.ChangeState(typeof(PlayerIdleState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
