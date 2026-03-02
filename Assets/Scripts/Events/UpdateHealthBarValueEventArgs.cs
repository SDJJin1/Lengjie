namespace EventCenter.Events
{
    public class UpdateHealthBarValueEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateHealthBarValueEventArgs).GetHashCode();

        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public float NewHealthValue { get; private set; }

        public static UpdateHealthBarValueEventArgs Create(object userData, float newHealthMaxValue)
        {
            var args = ReferencePool.Acquire<UpdateHealthBarValueEventArgs>();
            args.UserData = userData;
            args.NewHealthValue = newHealthMaxValue;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            
            UserData = null;
            NewHealthValue = 0;
        }
    }
}