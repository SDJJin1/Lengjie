using System.Collections.Generic;
using Character.Player;
using Item;
using UnityEngine;
using UnityEngine.UI;

namespace UIManager.UIForms
{
    /// <summary>
    /// 装备界面UI表单
    /// 继承自UIForm，用于管理玩家的装备系统界面
    /// 包括左右手武器槽位显示、装备背包管理、装备选择和更换功能
    /// 与Player组件交互，实时更新装备状态
    /// </summary>
    public class EquipmentUIForm : UIForm
    {
        /// <summary>
        /// 右手武器槽位列表
        /// 序列化字段，在Inspector中分配的右手武器图标Image组件
        /// 对应Player.weaponsInRightHandSlots数组
        /// </summary>
        [Header("武器槽位")] 
        [SerializeField] 
        private List<Image> rightHandSlots;
        
        /// <summary>
        /// 左手武器槽位列表
        /// 序列化字段，在Inspector中分配的左手武器图标Image组件
        /// 对应Player.weaponsInLeftHandSlots数组
        /// </summary>
        [SerializeField] 
        private List<Image> leftHandSlots;

        /// <summary>
        /// 装备背包窗口
        /// 显示背包中可装备物品的容器
        /// </summary>
        [Header("装备背包")]
        [SerializeField] 
        private GameObject equipmentInventoryWindow;
        
        /// <summary>
        /// 装备背包槽位预制体
        /// 用于在装备背包中实例化单个装备槽位
        /// 通常包含物品图标和装备按钮
        /// </summary>
        [SerializeField] 
        private GameObject equipmentInventorySlotPrefab;
        
        /// <summary>
        /// 装备背包内容窗口变换组件
        /// 装备背包槽位的父级容器，用于管理动态创建的槽位
        /// </summary>
        [SerializeField] 
        private Transform equipmentInventoryContentWindow;
        
        /// <summary>
        /// 当前选中的物品
        /// 在装备背包中高亮显示的物品
        /// 用于装备操作的目标物品
        /// </summary>
        [SerializeField] 
        private Item.Item currentSelectedItem;
        
        /// <summary>
        /// 当前选中的装备槽位类型
        /// 表示用户想要装备到的槽位（如左手武器槽位1、右手武器槽位2等）
        /// 在点击装备槽位时设置
        /// </summary>
        public EquipmentType currentSelectedEquipmentSlot;
        
        public override void OnOpen(object userData = null, params object[] args)
        {
            base.OnOpen(userData, args);
            
            RefreshWeaponSlotIcons();
        }

        public override void OnClose()
        {
            // 清空装备背包
            ClearEquipmentInventory();
            // 隐藏装备背包窗口
            equipmentInventoryWindow.SetActive(false);
            
            base.OnClose();
        }

        /// <summary>
        /// 刷新武器槽位图标
        /// 从玩家组件获取当前装备的武器，更新左右手武器槽位的图标显示
        /// 如果槽位中没有装备，则隐藏图标
        /// 注意：如果weaponItem为null，则隐藏图标
        /// </summary>
        public void RefreshWeaponSlotIcons()
        {
            // 获取玩家实例
            Player player = PlayerCamera.instance.player;

            // 刷新右手武器槽位
            for (int i = 0; i < rightHandSlots.Count; i++)
            {
                // 获取右手武器
                WeaponItem rightHandWeapon = player.weaponsInRightHandSlots[i]; 
                
                // 检查武器和图标是否存在
                if (rightHandWeapon != null && rightHandWeapon.itemIcon != null)
                {
                    rightHandSlots[i].enabled = true;
                    rightHandSlots[i].sprite = rightHandWeapon.itemIcon;
                }
                else
                {
                    // 没有武器或图标，隐藏图标
                    rightHandSlots[i].enabled = false;
                }
            }
            
            // 刷新左手武器槽位
            for (int i = 0; i < leftHandSlots.Count; i++)
            {
                // 获取左手武器
                WeaponItem leftHandWeapon = player.weaponsInLeftHandSlots[i];
                
                // 检查武器和图标是否存在
                if (leftHandWeapon != null && leftHandWeapon.itemIcon != null)
                {
                    leftHandSlots[i].enabled = true;
                    leftHandSlots[i].sprite = leftHandWeapon.itemIcon;
                }
                else
                {
                    // 没有武器或图标，隐藏图标
                    leftHandSlots[i].enabled = false;
                }
            }
        }

        public void LoadEquipmentInventory()
        {
            // 清空现有装备槽位
            foreach (Transform transform in equipmentInventoryContentWindow)
            {
                Destroy(transform.gameObject);
            }
            
            // 激活装备背包窗口
            equipmentInventoryWindow.SetActive(true);
            
            // 获取玩家实例
            Player player = PlayerCamera.instance.player;

            // 获取背包中的武器列表
            List<WeaponItem> weaponsInInventory = new List<WeaponItem>();
            
            for (int i = 0; i < player.itemsInInventory.Count; i++)
            {
                // 检查是否为武器
                WeaponItem weaponItem = player.itemsInInventory[i] as WeaponItem;
                if(weaponItem != null) 
                {
                    weaponsInInventory.Add(weaponItem);
                }
            }

            // 如果没有可装备的武器，隐藏窗口
            if (weaponsInInventory.Count <= 0)
            {
                equipmentInventoryWindow.SetActive(false);
                return;
            }
            
            // 创建装备槽位
            for (int i = 0; i < weaponsInInventory.Count; i++)
            {
                // 实例化装备槽位
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                EquipmentInventorySlot inventorySlot = inventorySlotGameObject.GetComponent<EquipmentInventorySlot>();
                
                // 添加物品到槽位
                inventorySlot.AddItem(weaponsInInventory[i]);
            }
        }

        private void ClearEquipmentInventory()
        {
            foreach (Transform item in equipmentInventoryContentWindow)
            {
                Destroy(item.gameObject);
            }
        }

        /// <summary>
        /// 设置当前选中的装备槽位
        /// 根据传入的索引设置对应的装备槽位类型
        /// 用于装备槽位点击事件，确定要将物品装备到哪个槽位
        /// </summary>
        /// <param name="slotIndex">槽位索引：
        /// 0: LeftHandWeapon01
        /// 1: LeftHandWeapon02
        /// 2: LeftHandWeapon03
        /// 3: RightHandWeapon01
        /// 4: RightHandWeapon02
        /// 5: RightHandWeapon03
        /// </param>
        /// <remarks>
        /// 此方法通常由装备槽位的按钮点击事件调用
        /// 设置后，当用户从装备背包中选择物品时，会知道要装备到哪个槽位
        /// </remarks>
        public void SetCurrentSelectedSlot(int slotIndex)
        {
            // 根据索引设置对应的装备槽位类型
            switch (slotIndex)
            {
                case 0:
                    currentSelectedEquipmentSlot = EquipmentType.LeftHandWeapon01;
                    break;
                case 1:
                    currentSelectedEquipmentSlot = EquipmentType.LeftHandWeapon02;
                    break;
                case 2:
                    currentSelectedEquipmentSlot = EquipmentType.LeftHandWeapon03;
                    break;
                case 3:
                    currentSelectedEquipmentSlot = EquipmentType.RightHandWeapon01;
                    break;
                case 4:
                    currentSelectedEquipmentSlot = EquipmentType.RightHandWeapon02;
                    break;
                case 5:
                    currentSelectedEquipmentSlot = EquipmentType.RightHandWeapon03;
                    break;
            }
        }
    }
}
