using EventCenter.Events;
using UnityEngine;

namespace Character.Player.States.ActionStates.BlockState
{
    /// <summary>
    /// 玩家格挡移动状态
    /// 处理玩家在格挡状态下的移动逻辑，包括移动控制、旋转动画和状态转换
    /// </summary>
    public class BlockingMoveState : PlayerState
    {
        public BlockingMoveState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 当不应用根运动时，手动处理移动和动画
            if (!agent.anim.applyRootMotion)
            {
                HandleAllMovement();

                // 根据锁定状态和冲刺输入设置动画参数
                if (!agent.isLockOn || agent.inputManager.sprintInput)
                {
                    agent.anim.SetFloat("Vertical", agent.inputManager.moveAmount, 0.1f, Time.deltaTime);
                    agent.anim.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
                }
                else
                {
                    agent.anim.SetFloat("Vertical", agent.inputManager.verticalInput, 0.1f, Time.deltaTime);
                    agent.anim.SetFloat("Horizontal", agent.inputManager.horizontalInput, 0.1f, Time.deltaTime);
                }
            }

            // 状态转换逻辑
            if (!agent.inputManager.blockInput)
            {
                // 松开格挡键时，根据移动输入切换到待机或移动状态
                if (agent.inputManager.moveAmount == 0)
                {
                    agent.PlayerFsmBase.ChangeState(typeof(PlayerIdleState));
                }
                else
                {
                    agent.PlayerFsmBase.ChangeState(typeof(PlayerGroundMoveState));
                }
            }
            // 保持格挡但停止移动时，切换到格挡待机状态
            else if (agent.inputManager.blockInput && agent.inputManager.moveAmount == 0)
            {
                agent.PlayerFsmBase.ChangeState(typeof(BlockingIdleState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
        
        private void HandleAllMovement()
        {
            HandleGroundedMovement();
            HandleRotation();
        }
        
        /// <summary>
        /// 处理地面移动
        /// 根据输入计算移动方向，并处理冲刺和行走/奔跑
        /// </summary>
        private void HandleGroundedMovement()
        {
            // 基于相机方向计算移动向量
            agent.moveDirection = PlayerCamera.instance.transform.forward * agent.inputManager.verticalInput;
            agent.moveDirection += PlayerCamera.instance.transform.right * agent.inputManager.horizontalInput;
            agent.moveDirection.Normalize();
            agent.moveDirection.y = 0;
            
            // 记录翻滚方向
            agent.rollDirection = agent.moveDirection;

            // 处理冲刺移动
            if (agent.inputManager.sprintInput && agent.currentStamina >= agent.sprintingStaminaCost)
            {
                agent.staminaRegenerationTimer = 0;
                agent.characterController.Move(agent.moveDirection * (agent.sprintingSpeed * Time.deltaTime));
                agent.currentStamina -= agent.sprintingStaminaCost * Time.deltaTime;
                // 触发事件更新耐力条UI
                EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, agent.currentStamina / agent.maxStamina));
            }
            // 处理行走/奔跑移动
            else
            {
                float moveSpeed = agent.inputManager.moveAmount > 0.5f ? agent.runningSpeed : agent.walkingSpeed;
                agent.characterController.Move(agent.moveDirection * (moveSpeed * Time.deltaTime));
            }
        }

        /// <summary>
        /// 处理角色旋转
        /// 根据锁定状态和输入计算目标旋转方向
        /// </summary>
        private void HandleRotation()
        {
            // 锁定目标时的旋转逻辑
            if (agent.isLockOn)
            {
                if (agent.inputManager.sprintInput)
                {
                    // 冲刺时使用相机方向进行移动旋转
                    Vector3 targetDirection = Vector3.zero;
                    targetDirection = PlayerCamera.instance.cameraObject.transform.forward * agent.inputManager.verticalInput;
                    targetDirection += PlayerCamera.instance.cameraObject.transform.right * agent.inputManager.horizontalInput;
                    targetDirection.Normalize();
                    targetDirection.y = 0;
                
                    if(targetDirection == Vector3.zero)
                        targetDirection = agent.transform.forward;

                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    Quaternion finalRotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, agent.rotationSpeed * Time.deltaTime);
                    agent.transform.rotation = finalRotation;
                }
                else
                {
                    // 非冲刺时面向锁定目标
                    if(agent.currentTarget == null)
                        return;
                
                    Vector3 targetDirection = Vector3.zero;
                    targetDirection = agent.currentTarget.transform.position - agent.transform.position;
                    targetDirection.y = 0;
                    targetDirection.Normalize();
                
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    Quaternion finalRotation = Quaternion.Slerp(agent.transform.rotation, targetRotation, agent.rotationSpeed * Time.deltaTime);
                    agent.transform.rotation = finalRotation;
                }
            }
            // 非锁定状态下的旋转逻辑
            else
            {
                agent.targetRotationDirection = Vector3.zero;
                agent.targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * agent.inputManager.verticalInput;
                agent.targetRotationDirection += PlayerCamera.instance.cameraObject.transform.right * agent.inputManager.horizontalInput;
                agent.targetRotationDirection.Normalize();
                agent.targetRotationDirection.y = 0;

                if (agent.targetRotationDirection == Vector3.zero)
                {
                    agent.targetRotationDirection = agent.transform.forward;
                }
        
                Quaternion newRotation = Quaternion.LookRotation(agent.targetRotationDirection);
                Quaternion targetRotation = Quaternion.Slerp(agent.transform.rotation, newRotation, agent.rotationSpeed * Time.deltaTime);
                agent.transform.rotation = targetRotation;
            }
        }
    }
}