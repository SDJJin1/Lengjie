using EventCenter;
using EventCenter.Events;
using UnityEngine;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 平视显示器(HUD) UI表单
    /// 继承自UIManager.UIForm，用于显示游戏中的实时状态信息
    /// 包括生命条、耐力条、快捷栏(法术、物品、左右手武器)等
    /// 通过事件系统接收游戏状态变化，实时更新UI显示
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 打开HUD界面
    /// UIManager.Instance.OpenUIForm<HUDUIForm>(EnumUIForm.HUDForm);
    /// 
    /// // 更新生命值事件
    /// EventCenter.Fire(this, UpdateHealthBarValueEventArgs.Create(75f));
    /// EventCenter.Fire(this, UpdateHealthBarMaxValueEventArgs.Create(100f));
    /// 
    /// // 更新快捷栏事件
    /// EventCenter.Fire(this, UpdateRightHandWeaponSlotEventArgs.Create(weaponIcon));
    /// </code>
    /// </remarks>
    public class HUDUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 耐力条
        /// 显示玩家的当前耐力值
        /// 通常用于奔跑、攻击、闪避等动作
        /// </summary>
        [Header("状态条")]
        [SerializeField] 
        private Slider StaminaBar;
        
        /// <summary>
        /// 生命条
        /// 显示玩家的当前生命值
        /// 当生命值为0时玩家死亡
        /// </summary>
        [SerializeField] 
        private Slider HealthBar;

        /// <summary>
        /// 法术快捷栏图标
        /// 显示当前装备的法术图标
        /// 通常对应玩家可使用的特殊技能
        /// </summary>
        [Header("快捷栏")] 
        [SerializeField] 
        private Image SpellSlot;
        
        /// <summary>
        /// 物品快捷栏图标
        /// 显示当前装备的物品图标
        /// 如药水、炸弹等消耗品
        /// </summary>
        [SerializeField] 
        private Image ItemSlot;
        
        /// <summary>
        /// 左手武器快捷栏图标
        /// 显示当前装备的左手武器图标
        /// 通常对应盾牌、副手武器等
        /// </summary>
        [SerializeField] 
        private Image LeftHandWeaponSlot;
        
        /// <summary>
        /// 右手武器快捷栏图标
        /// 显示当前装备的右手武器图标
        /// 通常对应主手武器
        /// </summary>
        [SerializeField] 
        private Image RightHandWeaponSlot;
        
        /// <summary>
        /// 表单打开时调用
        /// 订阅所有UI更新事件
        /// 确保HUD能够接收并响应游戏状态变化
        /// </summary>
        /// <param name="userData">用户数据，当前未使用</param>
        /// <param name="args">附加参数数组，当前未使用</param>
        public override void OnOpen(object userData = null, params object[] args)
        {
            base.OnOpen(userData);
            
            // 耐力相关事件订阅
            EventCenter.EventCenter.Subscribe(UpdateStaminaBarMaxValueEventArgs.EventId, UpdateMaxStamina);
            EventCenter.EventCenter.Subscribe(UpdateStaminaBarValueEventArgs.EventId, UpdateStamina);
            
            // 生命值相关事件订阅
            EventCenter.EventCenter.Subscribe(UpdateHealthBarValueEventArgs.EventId, UpdateHealth);
            EventCenter.EventCenter.Subscribe(UpdateHealthBarMaxValueEventArgs.EventId, UpdateMaxHealth);
            
            // 快捷栏相关事件订阅
            EventCenter.EventCenter.Subscribe(UpdateItemSlotEventArgs.EventId, UpdateItemSlot);
            EventCenter.EventCenter.Subscribe(UpdateSpellSlotEventArgs.EventId, UpdateSpellSlot);
            EventCenter.EventCenter.Subscribe(UpdateLeftHandWeaponSlotEventArgs.EventId, UpdateLeftHandWeaponSlot);
            EventCenter.EventCenter.Subscribe(UpdateRightHandWeaponSlotEventArgs.EventId, UpdateRightHandWeaponSlot);
        }

        /// <summary>
        /// 表单关闭时调用
        /// 取消订阅所有事件，防止内存泄漏
        /// 确保HUD不再接收事件更新
        /// </summary>
        public override void OnClose()
        {
            // 耐力相关事件取消订阅
            EventCenter.EventCenter.Unsubscribe(UpdateStaminaBarMaxValueEventArgs.EventId, UpdateMaxStamina);
            EventCenter.EventCenter.Unsubscribe(UpdateStaminaBarValueEventArgs.EventId, UpdateStamina);
            
            // 生命值相关事件取消订阅
            EventCenter.EventCenter.Unsubscribe(UpdateHealthBarValueEventArgs.EventId, UpdateHealth);
            EventCenter.EventCenter.Unsubscribe(UpdateHealthBarMaxValueEventArgs.EventId, UpdateMaxHealth);
            
            // 快捷栏相关事件取消订阅
            EventCenter.EventCenter.Unsubscribe(UpdateItemSlotEventArgs.EventId, UpdateItemSlot);
            EventCenter.EventCenter.Unsubscribe(UpdateSpellSlotEventArgs.EventId, UpdateSpellSlot);
            EventCenter.EventCenter.Unsubscribe(UpdateLeftHandWeaponSlotEventArgs.EventId, UpdateLeftHandWeaponSlot);
            EventCenter.EventCenter.Unsubscribe(UpdateRightHandWeaponSlotEventArgs.EventId, UpdateRightHandWeaponSlot);
            
            base.OnClose();
        }

        /// <summary>
        /// 更新物品快捷栏图标
        /// 当UpdateItemSlotEventArgs事件触发时调用
        /// 更新ItemSlot的图标显示
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的物品图标</param>
        private void UpdateItemSlot(object sender, GameEventArgs e)
        {
            UpdateItemSlotEventArgs updateItemSlotEventArgs = (UpdateItemSlotEventArgs)e;
            ItemSlot.sprite = updateItemSlotEventArgs.Image;
        }
        
        /// <summary>
        /// 更新法术快捷栏图标
        /// 当UpdateSpellSlotEventArgs事件触发时调用
        /// 更新SpellSlot的图标显示
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的法术图标</param>
        private void UpdateSpellSlot(object sender, GameEventArgs e)
        {
            UpdateSpellSlotEventArgs updateSpellSlotEventArgs = (UpdateSpellSlotEventArgs)e;
            SpellSlot.sprite = updateSpellSlotEventArgs.Image;
        }
        
        /// <summary>
        /// 更新左手武器快捷栏图标
        /// 当UpdateLeftHandWeaponSlotEventArgs事件触发时调用
        /// 更新LeftHandWeaponSlot的图标显示
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的左手武器图标</param>
        private void UpdateLeftHandWeaponSlot(object sender, GameEventArgs e)
        {
            UpdateLeftHandWeaponSlotEventArgs updateLeftHandWeaponSlotEvent = (UpdateLeftHandWeaponSlotEventArgs)e;
            LeftHandWeaponSlot.sprite = updateLeftHandWeaponSlotEvent.Image;
        }
        
        /// <summary>
        /// 更新右手武器快捷栏图标
        /// 当UpdateRightHandWeaponSlotEventArgs事件触发时调用
        /// 更新RightHandWeaponSlot的图标显示
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的右手武器图标</param>
        private void UpdateRightHandWeaponSlot(object sender, GameEventArgs e)
        {
            UpdateRightHandWeaponSlotEventArgs updateRightHandWeaponSlotEvent = (UpdateRightHandWeaponSlotEventArgs)e;
            RightHandWeaponSlot.sprite = updateRightHandWeaponSlotEvent.Image;
        }

        /// <summary>
        /// 更新耐力条当前值
        /// 当UpdateStaminaBarValueEventArgs事件触发时调用
        /// 更新StaminaBar的当前值
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的耐力值</param>
        private void UpdateStamina(object sender, GameEventArgs e)
        {
            UpdateStaminaBarValueEventArgs updateStaminaBarMaxValueEventArgs = (UpdateStaminaBarValueEventArgs)e;
            StaminaBar.value = updateStaminaBarMaxValueEventArgs.newStamina;
        }

        /// <summary>
        /// 更新耐力条最大值
        /// 当UpdateStaminaBarMaxValueEventArgs事件触发时调用
        /// 更新StaminaBar的最大值，并将当前值设为最大值
        /// 通常用于耐力上限提升时
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的耐力最大值</param>
        private void UpdateMaxStamina(object sender, GameEventArgs e)
        {
            UpdateStaminaBarMaxValueEventArgs updateStaminaBarMaxValueEventArgs = (UpdateStaminaBarMaxValueEventArgs)e;
            StaminaBar.maxValue = updateStaminaBarMaxValueEventArgs.NewStaminaMaxValue;
            StaminaBar.value = StaminaBar.maxValue;  // 将当前耐力设置为最大值
        }
        
        /// <summary>
        /// 更新生命条当前值
        /// 当UpdateHealthBarValueEventArgs事件触发时调用
        /// 更新HealthBar的当前值
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的生命值</param>
        private void UpdateHealth(object sender, GameEventArgs e)
        {
            UpdateHealthBarValueEventArgs updateHealthBarValueEventArgs = (UpdateHealthBarValueEventArgs)e;
            HealthBar.value = updateHealthBarValueEventArgs.NewHealthValue;
        }

        /// <summary>
        /// 更新生命条最大值
        /// 当UpdateHealthBarMaxValueEventArgs事件触发时调用
        /// 更新HealthBar的最大值，并将当前值设为最大值
        /// 通常用于生命上限提升时
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数，包含新的生命最大值</param>
        private void UpdateMaxHealth(object sender, GameEventArgs e)
        {
            UpdateHealthBarMaxValueEventArgs updateHealthBarMaxValueEventArgs = (UpdateHealthBarMaxValueEventArgs)e;
            HealthBar.maxValue = updateHealthBarMaxValueEventArgs.NewHealthMaxValue;
            HealthBar.value = HealthBar.maxValue;  // 将当前生命设置为最大值
        }
    }
}