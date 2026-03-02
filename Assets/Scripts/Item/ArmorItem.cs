using UnityEngine;
using UnityEngine.Serialization;

namespace Item
{
    public class ArmorItem : EquipmentItem
    {
        [Header("Equipment Absorption Bonus")] 
        public float physicalDamageAbsorption;

        [Header("Poise")] 
        public float poise;
    }
}
