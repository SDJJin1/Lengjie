using UnityEngine;

namespace Character.Player.States.ActionStates.HeavyAttack
{
    /// <summary>
    /// 重攻击普通释放状态
    /// 处理玩家在未达到完全蓄力时释放重攻击的动画播放和状态管理
    /// </summary>
    public class HeavyAttackReleaseState : PlayerState
    {
        private float timer = 0.0f;
        public HeavyAttackReleaseState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            timer = 0.0f;
            
            agent.CostStamina(20);
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 检查是否达到重攻击动画结束时间
            if (timer > agent.rightHandWeaponItem.heavyAttackCombos[agent.currentHeavyAttackCombo].endTimer)
            {
                // 直接切换到空动作状态，不增加连击计数
                agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}