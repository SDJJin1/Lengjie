using UnityEngine;

namespace Character.Player
{
    /// <summary>
    /// 玩家状态基类
    /// 所有玩家具体状态的基类，实现了状态机状态接口，提供状态通用的进入、更新和退出逻辑
    /// </summary>
    public class PlayerState : IFSMState
    {
        protected readonly int animHash;        // 动画哈希值，用于高效播放动画
        protected Player agent;                 // 玩家实例引用，所有具体状态都可以访问
        private bool applyRootMotion;           // 是否应用根运动的标志

        public PlayerState(Player player, string animName, bool applyRootMotion)
        {
            this.agent = player;
            animHash = Animator.StringToHash(animName); // 将动画名称转换为哈希值，提高性能
            this.applyRootMotion = applyRootMotion;
        }
    
        /// <summary>
        /// 进入状态
        /// 设置根运动状态，并播放对应的动画
        /// 使用CrossFade实现动画平滑过渡（0.2秒过渡时间）
        /// </summary>
        public virtual void Enter()
        {
            agent.anim.applyRootMotion = applyRootMotion;
            agent.anim.CrossFade(animHash, 0.2f);
        }

        /// <summary>
        /// 逻辑更新
        /// 每帧调用的逻辑，具体状态可以重写此方法实现特定逻辑
        /// </summary>
        public virtual void LogicalUpdate()
        {
        
        }

        /// <summary>
        /// 退出状态
        /// 重置根运动状态为false，确保离开状态后由程序控制位移
        /// </summary>
        public virtual void Exit()
        {
            agent.anim.applyRootMotion = false;
        }
    }
}
