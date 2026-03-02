using UnityEngine;

namespace Character.Player.States.UpperBodyStates
{
    /// <summary>
    /// 喝药起始状态
    /// 处理玩家开始喝药动作的初始阶段，包括模型切换和状态转换准备
    /// </summary>
    public class DrinkUpState : PlayerState
    {
        private float timer = 0;
        public DrinkUpState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            // 隐藏右手武器模型，显示药水模型
            agent.rightHandWeaponModel.SetActive(false);
            agent.flaskModel.SetActive(true);
            
            base.Enter();
            
            // 重置状态计时器
            timer = 0;

            // 清除使用物品输入标志，防止输入残留
            agent.inputManager.useItemInput = false;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();

            // 更新状态计时器
            timer += Time.deltaTime;
            
            // 0.2秒后检测药水数量并转换到相应状态
            if (timer >= 0.2f)
            {
                // 如果药水数量为0，切换到空药水状态
                if (agent.remainingFocusPointsFlasks == 0)
                {
                    agent.PlayerUpperBody.ChangeState(typeof(DrinkEmptyState));
                }
                // 否则切换到喝药状态01（开始实际喝药动画）
                else
                {
                    agent.PlayerUpperBody.ChangeState(typeof(Drink01State));
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}