using UnityEngine;

namespace Character.Player.States.ActionStates
{
    /// <summary>
    /// 轻攻击状态
    /// 处理玩家执行轻攻击的逻辑，包括动画控制、连击管理和伤害计算
    /// </summary>
    public class LightAttackState : PlayerState
    {
        // 状态计时器，记录轻攻击状态的持续时间
        private float timer = 0;
        // 标志位：伤害碰撞体是否已开启（当前未使用）
        private bool isOpenedCollider = false;
        // 标志位：伤害碰撞体是否已关闭（当前未使用）
        private bool isClosedCollider = false;
        
        public LightAttackState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            // 清除轻攻击输入标志，防止输入残留
            agent.inputManager.lightAttackInput = false;
            
            // 设置当前连击对应的动画覆盖控制器
            agent.anim.runtimeAnimatorController =
                agent.rightHandWeaponItem.lightAttackCombos[agent.currentLightAttackCombo].AnimatorOverrideController;
            
            // 重置计时器
            timer = 0;
            
            // 启用根运动，让动画控制位移
            agent.anim.applyRootMotion = true;
            
            // 播放轻攻击动画（在动画层2，从头开始播放）
            agent.anim.Play("Light_Attack", 2, 0);
            
            agent.CostStamina(20);

            // 计算最终伤害：武器基础伤害 × 当前连击的伤害修正系数
            agent.finalDamage = agent.rightHandWeaponItem.physicalDamage *
                                agent.rightHandWeaponItem.lightAttackCombos[agent.currentLightAttackCombo].damageModifier;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 如果在连击时间窗口内检测到新的轻攻击输入，则进入下一个连击
            if (timer >= agent.rightHandWeaponItem.lightAttackCombos[agent.currentLightAttackCombo].comboTimer
                && agent.inputManager.lightAttackInput)
            {
                // 增加连击计数
                agent.currentLightAttackCombo++;
                
                // 如果连击计数超过连击列表长度，重置为0
                if(agent.currentLightAttackCombo > agent.rightHandWeaponItem.lightAttackCombos.Count - 1)
                    agent.currentLightAttackCombo = 0;
                
                // 切换回空动作状态，准备下一次攻击
                agent.PlayerActions.ChangeState(typeof(ActionEmptyState));
            }

            // 如果超过当前攻击的结束时间，重置连击计数并切换到空闲状态
            if (timer >= agent.rightHandWeaponItem.lightAttackCombos[agent.currentLightAttackCombo].endTimer)
            {
                // 重置连击计数
                agent.currentLightAttackCombo = 0;
                
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
