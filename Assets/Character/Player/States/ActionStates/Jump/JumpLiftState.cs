using UnityEngine;

namespace Character.Player.States.ActionStates.Jump
{
    /// <summary>
    /// 跳跃上升状态
    /// 处理玩家跳跃初始阶段的上升运动，包括施加初始跳跃速度和重力影响
    /// </summary>
    public class JumpLiftState : PlayerState
    {
        public JumpLiftState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 根据跳跃高度和重力计算初始垂直速度
            agent.yVelocity.y = Mathf.Sqrt(agent.jumpHeight * -2 * agent.gravityForce);
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
        
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(PlayerDeadState));
            }
        
            // 应用重力
            agent.yVelocity.y += agent.gravityForce * Time.deltaTime;
        
            // 应用垂直速度（重力影响）
            agent.characterController.Move(agent.yVelocity * Time.deltaTime);

            // 应用水平方向的跳跃移动
            agent.characterController.Move(agent.jumpDirection * (agent.runningSpeed * Time.deltaTime));

            // 当垂直速度低于下落起始阈值时，切换到跳跃空中状态
            if (agent.yVelocity.y < agent.fallStartYVelocity)
            {
                agent.PlayerFsmBase.ChangeState(typeof(JumpIdleState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
