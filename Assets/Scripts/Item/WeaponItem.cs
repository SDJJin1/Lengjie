using System.Collections.Generic;
using Character.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Item
{
    /// <summary>
    /// 武器物品类
    /// 继承自EquipmentItem，表示游戏中可装备的武器
    /// 包含武器模型、类型、需求属性、伤害值、耐力消耗和攻击动作等配置
    /// 通过ScriptableObject在编辑器中创建和配置武器数据
    /// </summary>
    public class WeaponItem : EquipmentItem
    {
        /// <summary>
        /// 武器模型
        /// 3D武器预制体，用于在游戏中实例化并显示
        /// 通常包含网格、碰撞体和动画组件
        /// 在装备武器时，将此模型实例化到角色的相应手部骨骼上
        /// </summary>
        [Header("武器模型")] 
        public GameObject weaponModel;

        /// <summary>
        /// 武器类型
        /// 定义武器的分类，用于确定武器动画、使用方式和属性加成
        /// 当前版本只定义了近战武器，未来可扩展为远程、魔法等类型
        /// </summary>
        [Header("武器类型")] 
        public WeaponType weaponType;
    
        /// <summary>
        /// 武器需求 - 力量
        /// 角色装备此武器所需的最低力量属性
        /// 如果角色力量不足，装备后可能无法发挥武器全部威力
        /// 通常重型武器需要更高的力量值
        /// </summary>
        [Header("武器需求")]
        [Tooltip("装备此武器所需的最低力量值")]
        public int strength;
        
        /// <summary>
        /// 武器需求 - 智力
        /// 角色装备此武器所需的最低智力属性
        /// 魔法武器或需要技巧的武器可能需要智力
        /// 如法杖、魔法剑等
        /// </summary>
        [Tooltip("装备此武器所需的最低智力值")]
        public int intelligence;
    
        /// <summary>
        /// 武器物理伤害
        /// 武器造成的物理伤害值
        /// 基础伤害值，实际伤害会受到力量、技能等因素影响
        /// 重型武器通常有更高的基础物理伤害
        /// </summary>
        [Header("武器伤害")]
        [Min(0), Tooltip("武器的物理伤害值")]
        public int physicalDamage = 20;
    
        /// <summary>
        /// 基础耐力消耗
        /// 使用武器进行攻击时消耗的耐力值
        /// 重型武器通常有更高的耐力消耗
        /// 影响战斗节奏和连击次数
        /// </summary>
        [Header("耐力消耗")]
        [Min(0), Tooltip("每次攻击的基础耐力消耗")]
        public int baseStaminaCost = 20;
        
        /// <summary>
        /// 轻攻击连击
        /// 武器支持的轻攻击连击动作列表
        /// 包含一系列AttackSO脚本对象，定义每个连击阶段的动作
        /// 通常包括1-3个连击动作
        /// </summary>
        [Header("连击动作")] 
        [Tooltip("轻攻击连击序列")]
        public List<AttackSO> lightAttackCombos;
        
        /// <summary>
        /// 重攻击连击
        /// 武器支持的重攻击连击动作列表
        /// 重攻击通常伤害更高、前摇更长
        /// 可以用于破防或造成特殊效果
        /// </summary>
        [Tooltip("重攻击连击序列")]
        public List<AttackSO> heavyAttackCombos;

        /// <summary>
        /// 翻滚攻击动作
        /// 在翻滚过程中可以使用的攻击动作
        /// 通常用于快速反击或闪避攻击
        /// </summary>
        [Header("特殊攻击")] 
        [Tooltip("翻滚攻击动作")]
        public AttackSO RollAttack;
        
        /// <summary>
        /// 奔跑攻击动作
        /// 在奔跑过程中可以使用的攻击动作
        /// 通常带有冲刺效果
        /// </summary>
        [Tooltip("奔跑攻击动作")]
        public AttackSO RunAttack;
        
        /// <summary>
        /// 后撤步攻击动作
        /// 在向后回避时可以使用的攻击动作
        /// 通常用于保持距离的同时反击
        /// </summary>
        [Tooltip("后撤步攻击动作")]
        public AttackSO BackStepAttack;
    }

    public enum WeaponType
    {
        MeleeWeapon,
        
    }
}
