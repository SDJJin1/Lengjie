using UIManager;

namespace Character.Player.States
{
    /// <summary>
    /// 玩家死亡状态
    /// 处理玩家生命值归零后的死亡逻辑，包括打开死亡界面
    /// </summary>
    public class PlayerDeadState : PlayerState
    {
        public PlayerDeadState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 打开死亡界面
            UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.DeadForm);
        }
    }
}
