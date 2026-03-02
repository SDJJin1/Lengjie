namespace EventCenter.Events
{
    public class UpdateStaminaBarValueEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateStaminaBarValueEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public float newStamina { get; private set; }

        public static UpdateStaminaBarValueEventArgs Create(object userData, float newStamina)
        {
            var args = ReferencePool.Acquire<UpdateStaminaBarValueEventArgs>();
            args.UserData = userData;
            args.newStamina = newStamina;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            
            this.UserData = null;
            this.newStamina = 0;
        }
    }
}