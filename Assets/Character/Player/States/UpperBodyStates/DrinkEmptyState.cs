using UnityEngine;

namespace Character.Player.States.UpperBodyStates
{
    /// <summary>
    /// 药水空状态
    /// 处理玩家药水耗尽时尝试使用药水的状态逻辑，播放空药水动画
    /// </summary>
    public class DrinkEmptyState : PlayerState
    {
        private float timer = 0;
        public DrinkEmptyState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            timer = 0;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 更新状态计时器
            timer += Time.deltaTime;

            // 2秒后结束空药水状态
            if (timer >= 2.0f)
            {
                agent.PlayerUpperBody.ChangeState(typeof(UpperBodyEmptyState));
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            // 隐藏药水模型
            agent.flaskModel.SetActive(false);
            // 重新显示右手武器模型
            agent.rightHandWeaponModel.SetActive(true);
        }
    }
}