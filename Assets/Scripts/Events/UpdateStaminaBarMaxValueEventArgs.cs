namespace EventCenter.Events
{
    public class UpdateStaminaBarMaxValueEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateStaminaBarMaxValueEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public float NewStaminaMaxValue { get; private set; }

        public static UpdateStaminaBarMaxValueEventArgs Create(object userData, float newStaminaMaxValue)
        {
            var args = ReferencePool.Acquire<UpdateStaminaBarMaxValueEventArgs>();
            args.UserData = userData;
            args.NewStaminaMaxValue = newStaminaMaxValue;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            
            this.UserData = null;
            this.NewStaminaMaxValue = 0;
        }
    }
}