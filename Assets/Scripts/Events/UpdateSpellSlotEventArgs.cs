using UnityEngine;
using UnityEngine.UI;

namespace EventCenter.Events
{
    public class UpdateSpellSlotEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateSpellSlotEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public Sprite Image { get; private set; }

        public static UpdateSpellSlotEventArgs Create(Sprite image, object userData = null)
        {
            var args = ReferencePool.Acquire<UpdateSpellSlotEventArgs>();
            args.Image = image;
            args.UserData = userData;
            return args;
        }

        public override void Clear()
        {
            base.Clear();
            
            Image = null;
            UserData = null;
        }
    }
}