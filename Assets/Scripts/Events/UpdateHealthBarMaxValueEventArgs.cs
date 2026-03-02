namespace EventCenter.Events
{
    public class UpdateHealthBarMaxValueEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateHealthBarMaxValueEventArgs).GetHashCode();

        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public float NewHealthMaxValue { get; private set; }

        public static UpdateHealthBarMaxValueEventArgs Create(object userData, float newHealthMaxValue)
        {
            var args = ReferencePool.Acquire<UpdateHealthBarMaxValueEventArgs>();
            args.UserData = userData;
            args.NewHealthMaxValue = newHealthMaxValue;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            
            UserData = null;
            NewHealthMaxValue = 0;
        }
    }
}