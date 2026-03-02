using UnityEngine;

namespace Character.Player.States.ActionStates.HeavyAttack
{
    /// <summary>
    /// 重攻击蓄力保持状态
    /// 处理玩家保持蓄力重攻击时的持续逻辑，监测蓄力时间和输入释放
    /// </summary>
    public class HeavyAttackHoldState : PlayerState
    {
        private float timer = 0.0f;
        public HeavyAttackHoldState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            timer = 0.0f;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新蓄力计时器
            timer += Time.deltaTime;

            // 如果检测到松开重攻击输入，转换为释放状态
            if (!agent.inputManager.heavyAttackInput)
            {
                agent.PlayerActions.ChangeState(typeof(HeavyAttackReleaseState));
            }
            
            // 如果蓄力时间达到武器连击计时器定义的时间阈值，且仍保持输入，转换为完全蓄力释放状态
            if (timer > agent.rightHandWeaponItem.heavyAttackCombos[agent.currentHeavyAttackCombo].comboTimer && agent.inputManager.heavyAttackInput)
            {
                agent.PlayerActions.ChangeState(typeof(HeavyAttackReleaseFullState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}