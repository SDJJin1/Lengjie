using UnityEngine;

namespace Character.Player.States.ActionStates.Jump
{
    /// <summary>
    /// 跳跃空中状态（跳跃待机状态）
    /// 处理玩家在空中时的移动控制、重力应用和落地检测
    /// </summary>
    public class JumpIdleState : PlayerState
    {
        public JumpIdleState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
        
            agent.inAirTimer = 0;
            agent.fallingVelocityHasBeenSet = false;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
        
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(PlayerDeadState));
            }
        
            Vector3 freeFallDirection;
            
            // 根据相机方向计算空中移动方向
            freeFallDirection = PlayerCamera.instance.cameraObject.transform.forward * agent.inputManager.verticalInput;
            freeFallDirection +=  PlayerCamera.instance.cameraObject.transform.right * agent.inputManager.horizontalInput;
            freeFallDirection.y = 0;
        
            // 应用空中水平移动
            agent.characterController.Move(freeFallDirection * agent.walkingSpeed * Time.deltaTime);
        
            // 更新空中计时器
            agent.inAirTimer += Time.deltaTime;
            // 应用重力
            agent.yVelocity.y += agent.gravityForce * Time.deltaTime;
        
            // 应用垂直速度（重力影响）
            agent.characterController.Move(agent.yVelocity * Time.deltaTime);

            // 检测是否落地，落地后切换到跳跃结束状态
            if (agent.isGrounded)
            {
                agent.PlayerFsmBase.ChangeState(typeof(JumpEndState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
