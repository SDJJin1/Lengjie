using UnityEngine;
using UnityEngine.UI;

namespace EventCenter.Events
{
    public class UpdateRightHandWeaponSlotEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UpdateRightHandWeaponSlotEventArgs).GetHashCode();
        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public Sprite Image { get; private set; }

        public static UpdateRightHandWeaponSlotEventArgs Create(Sprite image, object userData = null)
        {
            var args = ReferencePool.Acquire<UpdateRightHandWeaponSlotEventArgs>();
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