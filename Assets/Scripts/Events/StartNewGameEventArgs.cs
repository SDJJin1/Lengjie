namespace EventCenter.Events
{
    public class StartNewGameEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(StartNewGameEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }

        public static StartNewGameEventArgs Create(object userData = null)
        {
            var args = ReferencePool.Acquire<StartNewGameEventArgs>();
            args.UserData = userData;
            return args;
        }

        public override void Clear()
        {
            this.UserData = null;
        }
    }
}
