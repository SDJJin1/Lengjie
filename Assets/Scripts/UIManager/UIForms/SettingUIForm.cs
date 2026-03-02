using UIManager;
using UnityEngine;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 设置界面UI表单
    /// 继承自UIManager.UIForm，提供游戏设置功能
    /// </summary>
    public class SettingUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 返回主菜单按钮
        /// 点击此按钮关闭设置界面，打开开始界面
        /// 用于从设置界面返回游戏主菜单
        /// </summary>
        [SerializeField] 
        private Button BackToStartMenuButton;
    
        public override void OnOpen(object userData = null,  params object[] args)
        {
            base.OnOpen(userData);
        
            // 绑定返回主菜单按钮事件
            BackToStartMenuButton.onClick.AddListener(BackToStartMenu);
        }

        private void Update()
        {
        
        }

        public override void OnClose()
        {
            base.OnClose();
        
            // 解绑返回主菜单按钮事件
            BackToStartMenuButton.onClick.RemoveListener(BackToStartMenu);
        }

        private void BackToStartMenu()
        {
            // 关闭设置界面
            UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.SettingForm);
            
            // 打开开始界面
            UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.StartForm);
        }
    }
}
