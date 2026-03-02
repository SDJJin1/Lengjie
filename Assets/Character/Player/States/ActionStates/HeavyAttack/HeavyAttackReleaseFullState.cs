using UnityEngine;

namespace Character.Player.States.ActionStates.HeavyAttack
{
    /// <summary>
    /// 重攻击完全蓄力释放状态
    /// 处理玩家完成最大蓄力后释放重攻击的动画播放和状态管理
    /// </summary>
    public class HeavyAttackReleaseFullState : PlayerState
    {
        private float timer = 0.0f;
        public HeavyAttackReleaseFullState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            timer = 0.0f;
            
            agent.CostStamina(20);

            agent.finalDamage = agent.rightHandWeaponItem.physicalDamage * agent.rightHandWeaponItem
                .heavyAttackCombos[agent.currentHeavyAttackCombo].damageModifier;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 检查是否达到重攻击动画结束时间
            if (timer > agent.rightHandWeaponItem.heavyAttackCombos[agent.currentHeavyAttackCombo].endTimer)
            {
                // 增加重攻击连击计数
                agent.currentHeavyAttackCombo++;
                
                // 如果连击计数超过连击列表长度，重置为0
                if (agent.currentHeavyAttackCombo >= agent.rightHandWeaponItem.heavyAttackCombos.Count)
                {
                    agent.currentHeavyAttackCombo = 0;
                }
                
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