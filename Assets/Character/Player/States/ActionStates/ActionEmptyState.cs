using Character.Player.States.ActionStates.BlockState;
using Character.Player.States.ActionStates.HeavyAttack;
using Character.Player.States.ActionStates.Jump;
using UnityEngine;

namespace Character.Player.States.ActionStates
{
    /// <summary>
    /// 动作空闲状态
    /// 玩家动作状态机中的默认空闲状态，负责检测各种动作输入并转换到对应状态
    /// 这是玩家没有执行其他动作时的核心响应状态
    /// </summary>
    public class ActionEmptyState : PlayerState
    {
        public ActionEmptyState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            // 重置翻滚方向，防止上一次的翻滚方向影响当前状态
            agent.rollDirection = Vector3.zero;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();

            // 如果当前处于格挡状态，则不处理其他动作输入（保持格挡优先）
            if (agent.PlayerFsmBase.CurrentState.GetType() == typeof(BlockingIdleState) ||
                agent.PlayerFsmBase.CurrentState.GetType() == typeof(BlockingMoveState))
            {
                return;
            }

            // 奔跑攻击检测：在移动状态下按下轻攻击键
            if (agent.PlayerFsmBase.CurrentState.GetType() == typeof(PlayerGroundMoveState) && agent.inputManager.lightAttackInput)
            {
                agent.inputManager.lightAttackInput = false;
                
                // 耐力不足时无法攻击
                if(agent.currentStamina <= 0) return;
                
                agent.PlayerActions.ChangeState(typeof(RunAttackState));
            }

            // 跳跃输入检测
            if (agent.inputManager.jumpInput)
            {
                agent.inputManager.jumpInput = false;
                
                // 耐力不足时无法跳跃
                if(agent.currentStamina <= 0) return;
                
                agent.PlayerActions.ChangeState(typeof(JumpStartState));
            }

            // 更新翻滚方向为当前移动方向
            agent.rollDirection = agent.moveDirection;
            
            // 翻滚/闪避输入检测
            if (agent.inputManager.dodgeInput)
            {
                agent.inputManager.dodgeInput = false;

                // 耐力不足时无法翻滚
                if(agent.currentStamina <= 0) return;
                
                // 根据移动方向决定是后撤步还是翻滚
                agent.PlayerActions.ChangeState(agent.rollDirection == Vector3.zero 
                ? typeof(BackStepState) : typeof(RollState));
            }

            // 轻攻击输入检测
            if (agent.inputManager.lightAttackInput)
            {
                agent.inputManager.lightAttackInput = false;
                    
                // 耐力不足时无法攻击
                if(agent.currentStamina <= 0) return;
                
                agent.PlayerActions.ChangeState(typeof(LightAttackState));
            }

            // 重攻击输入检测
            if (agent.inputManager.heavyAttackInput)
            {
                // 耐力不足时无法攻击
                if(agent.currentStamina <= 0) return;
                
                agent.PlayerActions.ChangeState(typeof(HeavyAttackChargeState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}