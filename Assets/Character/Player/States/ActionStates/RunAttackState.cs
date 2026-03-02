using UnityEngine;

namespace Character.Player.States.ActionStates
{
    /// <summary>
    /// 奔跑攻击状态
    /// 处理玩家在奔跑过程中执行攻击动作的状态逻辑
    /// </summary>
    public class RunAttackState : PlayerState
    {
        private float timer = 0;
        public RunAttackState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            // 清除轻攻击输入标志，防止输入残留
            agent.inputManager.lightAttackInput = false;
            base.Enter();
            
            // 重置状态计时器
            timer = 0;
            
            agent.CostStamina(20);
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新状态计时器
            timer += Time.deltaTime;
            
            // 检查是否达到奔跑攻击动画的结束时间
            if (timer >= agent.rightHandWeaponItem.RunAttack.endTimer)
            {
                // 切换到空动作状态
                agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
