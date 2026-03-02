using UnityEngine;

namespace Character.Player.States.UpperBodyStates
{
    /// <summary>
    /// 喝药结束状态
    /// 处理玩家完成喝药动作后的动画播放和模型切换
    /// </summary>
    public class DrinkDownState : PlayerState
    {
        private float timer = 0;
        public DrinkDownState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();

            // 重置状态计时器
            timer = 0;
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
            
            // 1秒后结束喝药结束状态
            if (timer >= 1.0f)
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