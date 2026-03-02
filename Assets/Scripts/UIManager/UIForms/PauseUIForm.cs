using UnityEngine;
using UnityEngine.UI;

namespace UIManager.UIForms
{
    /// <summary>
    /// 暂停界面UI表单
    /// 继承自UIForm，用于在游戏暂停时显示的游戏选项界面
    /// 包含打开装备窗口、角色信息、背包和退出等功能的按钮
    /// 通过按钮点击切换不同的游戏功能界面
    /// </summary>
    public class PauseUIForm : UIForm
    {
        /// <summary>
        /// 装备窗口按钮
        /// 点击此按钮打开装备界面
        /// 会关闭所有当前UI表单
        /// </summary>
        [SerializeField] 
        private Button EquipmentWindowButton;
        
        /// <summary>
        /// 角色信息按钮
        /// 点击此按钮打开角色信息界面
        /// 当前未实现功能
        /// </summary>
        [SerializeField] 
        private Button CharacterInfoButton;
        
        /// <summary>
        /// 背包窗口按钮
        /// 点击此按钮打开背包界面
        /// 当前未实现功能
        /// </summary>
        [SerializeField] 
        private Button InventoryWindowButton;
        
        /// <summary>
        /// 退出按钮
        /// 点击此按钮退出游戏
        /// 当前未实现功能
        /// </summary>
        [SerializeField] 
        private Button ExitButton;

        public override void OnOpen(object userData = null, params object[] args)
        {
            base.OnOpen(userData, args);
            
            // 绑定装备窗口按钮事件
            EquipmentWindowButton.onClick.AddListener(OpenEquipmentWindow);
        }

        public override void OnClose()
        {
            base.OnClose();
            
            // 解绑装备窗口按钮事件
            EquipmentWindowButton.onClick.RemoveListener(OpenEquipmentWindow);
        }

        private void OpenEquipmentWindow()
        {
            // 关闭所有UI表单
            UIManager.Instance.CloseAllUIForms();
            
            // 打开装备界面
            UIManager.Instance.OpenUIForm(EnumUIForm.EquipmentForm);
        }
    }
}
