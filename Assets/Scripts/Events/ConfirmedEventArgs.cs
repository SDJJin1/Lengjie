namespace EventCenter.Events
{
    public class ConfirmedEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ConfirmedEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }

        public static ConfirmedEventArgs Create(object userData = null)
        {
            var args = ReferencePool.Acquire<ConfirmedEventArgs>();
            args.UserData = userData;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            UserData = null;
        }
    }
}