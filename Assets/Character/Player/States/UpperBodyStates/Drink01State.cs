using UnityEngine;

namespace Character.Player.States.UpperBodyStates
{
    public class Drink01State : PlayerState
    {
        private float timer = 0;
        public Drink01State(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            
            // 恢复指定量的生命值
            agent.RestoreHealth(agent.flaskRestoration);
            // 减少剩余药水数量
            agent.remainingFocusPointsFlasks--;

            // 重置状态计时器
            timer = 0;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();

            // 更新状态计时器
            timer += Time.deltaTime;
            
            // 1秒后检测使用物品输入
            if (timer >= 1.0f && agent.inputManager.useItemInput)
            {
                // 清除使用物品输入标志
                agent.inputManager.useItemInput = false;
                
                // 如果药水数量为0，切换到空药水状态
                if (agent.remainingFocusPointsFlasks == 0)
                {
                    agent.PlayerUpperBody.ChangeState(typeof(DrinkEmptyState));
                }
                // 否则连续喝药
                else
                {
                    agent.PlayerUpperBody.ChangeState(typeof(Drink02State));
                }
            }
            // 1秒后如果没有继续喝药的输入，则切换到药水结束状态
            else if (timer >= 1.0f)
            {
                agent.PlayerUpperBody.ChangeState(typeof(DrinkDownState));
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}