using EventCenter.Events;
using UnityEngine;

namespace Character.Player.States.ActionStates.Jump
{
    /// <summary>
    /// 跳跃开始状态
    /// 处理跳跃的初始准备阶段，包括耐力消耗、方向计算和状态转换
    /// </summary>
    public class JumpStartState : PlayerState
    {
        private float timer = 0;
        public JumpStartState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
        
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(PlayerDeadState));
            }
        
            // 重置状态计时器
            timer = 0;
        
            // 设置初始垂直速度为下落起始速度
            agent.yVelocity.y = agent.fallStartYVelocity;
        
            // 消耗跳跃耐力
            agent.currentStamina -= agent.jumpStaminaCost;
            // 触发事件更新耐力条UI
            EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, agent.currentStamina));
        
            // 根据相机方向和输入计算跳跃水平方向
            agent.jumpDirection = PlayerCamera.instance.cameraObject.transform.forward * agent.inputManager.verticalInput;
            agent.jumpDirection += PlayerCamera.instance.cameraObject.transform.right * agent.inputManager.horizontalInput;
            agent.jumpDirection.y = 0;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
        
            // 更新状态计时器
            timer += Time.deltaTime;
            
            // 0.03秒后切换到跳跃上升状态
            if (timer >= 0.03f)
            {
                agent.PlayerFsmBase.ChangeState(typeof(JumpLiftState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
