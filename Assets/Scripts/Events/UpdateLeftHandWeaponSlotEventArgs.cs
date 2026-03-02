using UnityEngine;
using UnityEngine.UI;

namespace EventCenter.Events
{
    public class UpdateLeftHandWeaponSlotEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateLeftHandWeaponSlotEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public Sprite Image { get; private set; }

        public static UpdateLeftHandWeaponSlotEventArgs Create(Sprite image, object userData = null)
        {
            var args = ReferencePool.Acquire<UpdateLeftHandWeaponSlotEventArgs>();
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