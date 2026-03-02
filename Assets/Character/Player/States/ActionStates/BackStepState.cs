using EventCenter.Events;
using UnityEngine;

namespace Character.Player.States.ActionStates
{
    /// <summary>
    /// 后撤步状态
    /// 处理玩家执行后撤步动作的逻辑，包括耐力消耗、根运动控制和状态转换
    /// </summary>
    public class BackStepState : PlayerState
    {
        private float timer = 0f;
        public BackStepState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
            
        }

        public override void Enter()
        {
            // 重置计时器
            timer = 0f;
            // 重置耐力恢复计时器
            agent.staminaRegenerationTimer = 0;
            // 消耗后撤步所需耐力
            agent.currentStamina -= agent.dodgeStaminaCost;
            // 触发事件更新耐力条UI
            EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, agent.currentStamina / agent.maxStamina));
            
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            // 启用根运动，让动画控制位移
            agent.anim.applyRootMotion = true;
            
            base.LogicalUpdate();

            // 调试日志：当根运动被禁用时输出（当前未使用）
            if (!agent.anim.applyRootMotion)
            {
                Debug.Log(1);
            }
            
            // 如果玩家生命值小于等于0，切换到死亡状态
            if (agent.currentHealth <= 0)
            {
                agent.PlayerActions.ChangeState(typeof(PlayerDeadState));
            }
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 在0.6秒后检测轻攻击输入，转换为后撤步攻击状态
            if (timer >= 0.6f && agent.inputManager.lightAttackInput)
            {
                agent.PlayerActions.ChangeState(typeof(BackStepAttackState));
            }
            
            // 1.0秒后自动结束后撤步状态，切换回空动作状态
            if (timer >= 1.0f)
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
