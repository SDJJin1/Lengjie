
    using Character.Player;

    public class BlockHeavyState : PlayerState
    {
        public BlockHeavyState(Player player, string animName, bool applyRootMotion) : base(player, animName, applyRootMotion)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
        }

        public override void LogicalUpdate()
        {
            base.LogicalUpdate();
        }

        public override void Exit()
        {
            base.Exit();
        }
    }