using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器模型实例化槽位
/// 用于管理玩家角色身上特定槽位的武器模型实例，包括加载、卸载和碰撞体控制
/// </summary>
public class WeaponModelInstantiationSlot : MonoBehaviour
{
    /// <summary>
    /// 武器槽位类型（左手武器、左手盾牌、右手武器）
    /// 确定该槽位对应的装备位置
    /// </summary>
    public WeaponModelSlot weaponSlot;
    
    /// <summary>
    /// 当前槽位中加载的武器模型实例
    /// </summary>
    public GameObject currentWeaponModel;
    
    /// <summary>
    /// 武器模型的伤害碰撞体，用于攻击检测
    /// </summary>
    public Collider damageCollider;

    /// <summary>
    /// 卸载当前武器模型
    /// 销毁当前模型实例并清空引用
    /// </summary>
    public void UnLoadWeapon()
    {
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }
    }

    /// <summary>
    /// 加载武器模型到槽位
    /// 实例化武器模型并设置其父对象、位置、旋转和缩放
    /// 同时获取并禁用伤害碰撞体（通常由动画事件控制开关）
    /// </summary>
    /// <param name="weaponModel">要加载的武器模型预制体实例</param>
    public void LoadWeapon(GameObject weaponModel)
    {
        currentWeaponModel = weaponModel;
        weaponModel.transform.parent = transform;
        
        // 重置本地变换，确保模型正确对齐
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localRotation = Quaternion.identity;
        weaponModel.transform.localScale = Vector3.one;
        
        // 获取伤害碰撞体组件（通常在武器模型的子对象中）
        damageCollider = currentWeaponModel.GetComponentInChildren<Collider>();
        
        // 初始状态下禁用伤害碰撞体，仅在攻击动画中激活
        damageCollider.enabled = false;
    }
}

/// <summary>
/// 武器模型槽位枚举
/// 定义玩家角色身上可装备武器的不同位置
/// </summary>
public enum WeaponModelSlot
{
    /// <summary>
    /// 左手武器槽位：用于装备剑、斧等单手武器
    /// </summary>
    LeftHandWeapon,
    
    /// <summary>
    /// 左手盾牌槽位：专门用于装备盾牌
    /// </summary>
    LeftHandShield,
    
    /// <summary>
    /// 右手武器槽位：用于装备主手武器
    /// </summary>
    RightHand,
}