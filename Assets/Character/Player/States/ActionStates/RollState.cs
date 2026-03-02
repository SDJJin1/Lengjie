using EventCenter.Events;
using UnityEngine;

namespace Character.Player.States.ActionStates
{
    /// <summary>
    /// 翻滚状态
    /// 处理玩家执行翻滚动作的逻辑，包括耐力消耗、方向旋转和状态转换
    /// </summary>
    public class RollState : PlayerState
    {
        private float timer = 0f;
        public RollState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            // 重置计时器
            timer = 0f;
            
            // 清除翻滚输入标志，防止输入残留
            agent.inputManager.dodgeInput = false;
            
            // 重置耐力恢复计时器并消耗翻滚耐力
            agent.staminaRegenerationTimer = 0;
            agent.currentStamina -= agent.dodgeStaminaCost;
            
            // 触发事件更新耐力条UI
            EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, agent.currentStamina / agent.maxStamina));
            
            // 根据相机方向和输入计算翻滚方向
            Vector3 targetDirection = Vector3.zero;
            targetDirection = PlayerCamera.instance.cameraObject.transform.forward * agent.inputManager.verticalInput;
            targetDirection += PlayerCamera.instance.cameraObject.transform.right * agent.inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;
                
            // 如果没有输入方向，则使用角色当前朝向
            if(targetDirection == Vector3.zero)
                targetDirection = agent.transform.forward;

            // 平滑旋转角色到翻滚方向
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion finalRotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, 40 * Time.deltaTime);
            agent.transform.rotation = finalRotation;
            
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerActions.ChangeState(typeof(PlayerDeadState));
            }
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 在0.6秒后检测轻攻击输入，转换为翻滚攻击状态
            if (timer >= 0.6f && agent.inputManager.lightAttackInput)
            {
                agent.PlayerActions.ChangeState(typeof(RollAttackState));
            }
            
            // 0.8秒后自动结束翻滚状态，切换回空动作状态
            if (timer >= 0.8f)
            {
                agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
