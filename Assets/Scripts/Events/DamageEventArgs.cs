using UnityEngine;

namespace EventCenter.Events
{
    public class DamageEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(DamageEventArgs).GetHashCode();

        public override int Id => EventId;
        
        public object UserData { get; private set; }
        public int Damage { get; private set; }
        public Vector3 ContactPoint { get; private set; }
        public Vector3 DamageTargetForward {  get; private set; }

        public static DamageEventArgs Create(object userData, int damage, Vector3 contactPoint,  Vector3 damageTargetForward)
        {
            var args = ReferencePool.Acquire<DamageEventArgs>();
            args.UserData = userData;
            args.Damage = damage;
            args.ContactPoint = contactPoint;
            args.DamageTargetForward = damageTargetForward;
            return args;
        }

        public override void Clear()
        {
            UserData = null;
            Damage = 0;
        }
    }
}