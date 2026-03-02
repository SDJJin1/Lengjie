using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Item;
using UIManager;
using UIManager.UIForms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 装备背包槽位
/// 用于在装备界面中显示背包内的物品，并处理装备物品的逻辑
/// 每个槽位可以显示一个物品图标，当被选中时高亮，点击装备按钮时可装备物品
/// 与玩家的装备槽位交互，实现装备更换功能
/// </summary>
public class EquipmentInventorySlot : MonoBehaviour
{
    /// <summary>
    /// 物品图标显示
    /// 用于显示物品的缩略图
    /// </summary>
    public Image itemIcon;
    
    /// <summary>
    /// 高亮图标
    /// 当槽位被选中时显示的边框或背景高亮效果
    /// </summary>
    public Image highLightedIcon;
    
    /// <summary>
    /// 装备按钮
    /// 点击此按钮可装备当前槽位中的物品
    /// 序列化字段，可在Inspector中分配
    /// </summary>
    [SerializeField] 
    private Button equipButton;
    
    /// <summary>
    /// 当前槽位中的物品
    /// 存储当前槽位显示的物品数据
    /// 公共字段，供外部访问
    /// </summary>
    [SerializeField] 
    public Item.Item currentItem;

    private void OnEnable()
    {
        equipButton.onClick.AddListener(EquipItem);
    }

    private void OnDisable()
    {
        equipButton.onClick.RemoveListener(EquipItem);
    }

    /// <summary>
    /// 添加物品到槽位
    /// 设置物品图标显示，保存物品数据
    /// 如果物品为null，则隐藏图标
    /// </summary>
    /// <param name="item">要添加的物品</param>
    public void AddItem(Item.Item item)
    {
        if (item == null)
        {
            itemIcon.enabled = false;
            return;
        }
        
        itemIcon.enabled = true;
        currentItem = item;
        itemIcon.sprite = item.itemIcon;
    }

    /// <summary>
    /// 选中槽位
    /// 显示高亮图标，表示此槽位被选中
    /// 通常在鼠标悬停或点击时调用
    /// </summary>
    public void SelectedSlot()
    {
        highLightedIcon.enabled = true;
    }

    /// <summary>
    /// 取消选中槽位
    /// 隐藏高亮图标，表示此槽位不再被选中
    /// 通常在鼠标离开或选择其他槽位时调用
    /// </summary>
    public void UnSelectedSlot()
    {
        highLightedIcon.enabled = false;
    }

    /// <summary>
    /// 装备物品
    /// 从背包装备当前物品到玩家身上
    /// 根据当前选中的装备槽位类型，将物品装备到对应的装备槽
    /// 处理装备替换逻辑：当前装备放入背包，新装备从背包移除
    /// 如果装备的是当前手持武器，触发武器切换状态
    /// 更新UI显示
    /// </summary>
    private void EquipItem()
    {
        // 获取玩家实例
        Player player = PlayerCamera.instance.player;
        
        // 获取当前选中的装备槽位类型
        EquipmentType currentSelectedSlot = UIManager.UIManager.Instance
            .GetUIForm<EquipmentUIForm>(EnumUIForm.EquipmentForm)
            .currentSelectedEquipmentSlot;
        
        // 根据选中的装备槽位类型处理装备逻辑
        switch (currentSelectedSlot)
        {
            case EquipmentType.LeftHandWeapon01:
                // 左手武器槽位1
                HandleLeftHandWeaponSlot(player, 0);
                break;
            case EquipmentType.LeftHandWeapon02:
                // 左手武器槽位2
                HandleLeftHandWeaponSlot(player, 1);
                break;
            case EquipmentType.LeftHandWeapon03:
                // 左手武器槽位3
                HandleLeftHandWeaponSlot(player, 2);
                break;
            case EquipmentType.RightHandWeapon01:
                // 右手武器槽位1
                HandleRightHandWeaponSlot(player, 0);
                break;
            case EquipmentType.RightHandWeapon02:
                // 右手武器槽位2
                HandleRightHandWeaponSlot(player, 1);
                break;
            case EquipmentType.RightHandWeapon03:
                // 右手武器槽位3
                HandleRightHandWeaponSlot(player, 2);
                break;
        }
    }
    
    /// <summary>
    /// 处理左手武器槽位装备
    /// 通用方法，处理指定索引的左手武器槽位
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <param name="slotIndex">左手武器槽位索引（0,1,2）</param>
    private void HandleLeftHandWeaponSlot(Player player, int slotIndex)
    {
        // 获取当前槽位中的武器
        WeaponItem currentWeapon = player.weaponsInLeftHandSlots[slotIndex];
        
        // 如果当前槽位有武器，放回背包
        if (currentWeapon != null)
        {
            player.AddItemToInventory(currentWeapon);
        }
        
        // 从背包中装备新武器
        player.weaponsInLeftHandSlots[slotIndex] = currentItem as WeaponItem;
        player.RemoveItemFromInventory(currentItem);
        
        // 如果当前正在使用这个左手武器槽位，触发切换状态
        if (player.leftHandWeaponIndex == slotIndex)
        {
            player.PlayerUpperBody.ChangeState(typeof(SwitchLeftHandWeaponState));
        }
        
        // 刷新UI
        RefreshEquipmentUI();
    }
    
    /// <summary>
    /// 处理右手武器槽位装备
    /// 通用方法，处理指定索引的右手武器槽位
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <param name="slotIndex">右手武器槽位索引（0,1,2）</param>
    private void HandleRightHandWeaponSlot(Player player, int slotIndex)
    {
        // 获取当前槽位中的武器
        WeaponItem currentWeapon = player.weaponsInRightHandSlots[slotIndex];
        
        // 如果当前槽位有武器，放回背包
        if (currentWeapon != null)
        {
            player.AddItemToInventory(currentWeapon);
        }
        
        // 从背包中装备新武器
        player.weaponsInRightHandSlots[slotIndex] = currentItem as WeaponItem;
        player.RemoveItemFromInventory(currentItem);
        
        // 如果当前正在使用这个右手武器槽位，触发切换状态
        if (player.rightHandWeaponIndex == slotIndex)
        {
            player.PlayerUpperBody.ChangeState(typeof(SwitchRightHandWeaponState));
        }
        
        // 刷新UI
        RefreshEquipmentUI();
    }
    
    /// <summary>
    /// 刷新装备UI
    /// 更新装备界面和背包界面
    /// </summary>
    private void RefreshEquipmentUI()
    {
        var equipmentForm = UIManager.UIManager.Instance.GetUIForm<EquipmentUIForm>(EnumUIForm.EquipmentForm);
        equipmentForm.RefreshWeaponSlotIcons();
        equipmentForm.LoadEquipmentInventory();
    }
}
