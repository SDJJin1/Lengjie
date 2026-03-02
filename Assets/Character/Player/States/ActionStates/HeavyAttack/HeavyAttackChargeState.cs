using UnityEngine;

namespace Character.Player.States.ActionStates.HeavyAttack
{
    /// <summary>
    /// 重攻击蓄力状态
    /// 处理玩家开始蓄力重攻击时的初始逻辑
    /// </summary>
    public class HeavyAttackChargeState : PlayerState
    {
        private float timer = 0.0f;
        public HeavyAttackChargeState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        /// <summary>
        /// 进入重攻击蓄力状态
        /// 初始化动画控制器和攻击连击计数器
        /// </summary>
        public override void Enter()
        {
            // 设置重攻击对应的动画覆盖控制器
            agent.anim.runtimeAnimatorController =
                agent.rightHandWeaponItem.heavyAttackCombos[agent.currentHeavyAttackCombo].AnimatorOverrideController;

            // 重置轻攻击连击计数器
            agent.currentLightAttackCombo = 0;
            
            // 重置计时器
            timer = 0.0f;
            
            base.Enter();
        }

        /// <summary>
        /// 逻辑更新
        /// 每帧更新蓄力计时器，并在达到时间阈值后转换到蓄力保持状态
        /// </summary>
        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新计时器
            timer += Time.deltaTime;

            // 达到0.2秒后切换到重攻击蓄力保持状态
            if (timer >= 0.2f)
            {
                agent.PlayerActions.ChangeState(typeof(HeavyAttackHoldState));
            }
        }

        /// <summary>
        /// 退出重攻击蓄力状态
        /// </summary>
        public override void Exit()
        {
            base.Exit();
        }
    }
}
