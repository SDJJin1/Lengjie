namespace EventCenter.Events
{
    public class CanceledEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CanceledEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }

        public static CanceledEventArgs Create(object userData = null)
        {
            var args = ReferencePool.Acquire<CanceledEventArgs>();
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