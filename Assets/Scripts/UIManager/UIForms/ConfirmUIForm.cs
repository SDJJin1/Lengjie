using EventCenter.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 确认对话框UI表单
    /// 继承自UIManager.UIForm，提供确认/取消操作的对话框界面
    /// 通过事件系统与外部交互，支持自定义提示文本
    /// 用于需要用户确认的重要操作，如删除、退出、保存等
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 打开确认对话框
    /// UIManager.Instance.OpenUIForm<ConfirmUIForm>(
    ///     EnumUIForm.ConfirmForm, 
    ///     "确定要删除此物品吗？"  // 用户数据：提示文本
    /// );
    /// 
    /// // 在外部订阅事件
    /// EventCenter.EventCenter.Subscribe(
    ///     ConfirmedEventArgs.EventId, 
    ///     OnConfirmed
    /// );
    /// 
    /// // 事件处理方法
    /// private void OnConfirmed(object sender, GameEventArgs e)
    /// {
    ///     if (e is ConfirmedEventArgs confirmedArgs)
    ///     {
    ///         Debug.Log("用户点击了确认");
    ///         // 执行确认操作
    ///     }
    /// }
    /// </code>
    /// </remarks>
    public class ConfirmUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 提示文本组件
        /// 显示对话框的主要内容，如"确定要退出游戏吗？"
        /// 文本内容在打开表单时通过userData参数传入
        /// </summary>
        [SerializeField] 
        private TextMeshProUGUI TipText;
        
        /// <summary>
        /// 确认按钮
        /// 用户点击确认时触发ConfirmedEventArgs事件
        /// </summary>
        [SerializeField] 
        private Button ConfirmButton;
        
        /// <summary>
        /// 取消按钮
        /// 用户点击取消时触发CanceledEventArgs事件
        /// </summary>
        [SerializeField] 
        private Button CancelButton;
        
        /// <summary>
        /// 表单打开时调用
        /// 初始化对话框内容，设置提示文本，绑定按钮事件
        /// </summary>
        /// <param name="userData">用户数据，应包含提示文本字符串</param>
        /// <param name="args">附加参数数组</param>
        /// <remarks>
        /// 注意：此方法会在每次打开表单时调用
        /// 如果表单被重复使用，需要确保不会重复绑定事件
        /// 建议在OnClose中解绑事件，或在OnOpen中先解绑再绑定
        /// </remarks>
        public override void OnOpen(object userData = null, params object[] args)
        {
            base.OnOpen(userData);
            
            // 设置提示文本
            if (userData is string message)
            {
                TipText.text = message;
            }
            
            // 绑定按钮事件
            ConfirmButton.onClick.AddListener(Confirm);
            CancelButton.onClick.AddListener(CloseForm);
        }

        /// <summary>
        /// 表单关闭时调用
        /// 清理资源，解绑按钮事件
        /// </summary>
        /// <remarks>
        /// 重要：必须在此方法中解绑按钮事件
        /// 防止表单重复使用时事件被多次绑定
        /// 也防止表单被销毁时引用无效导致内存泄漏
        /// </remarks>
        public override void OnClose()
        {
            // 解绑按钮事件
            ConfirmButton.onClick.RemoveListener(Confirm);
            CancelButton.onClick.RemoveListener(CloseForm);
            
            base.OnClose();
        }

        /// <summary>
        /// 确认按钮点击处理
        /// 触发ConfirmedEventArgs事件，通知外部用户点击了确认
        /// 事件参数中传递当前对话框实例作为发送者
        /// </summary>
        private void Confirm()
        {
            // 触发确认事件
            EventCenter.EventCenter.Fire(this, ConfirmedEventArgs.Create(this));
            
            // 可选：播放按钮音效
            // AudioManager.Instance.PlaySound("ButtonClick");
            
            // 可选：关闭表单
            // UIManager.Instance.CloseUIForm(this);
        }

        /// <summary>
        /// 取消按钮点击处理
        /// 触发CanceledEventArgs事件，通知外部用户点击了取消
        /// 事件参数中传递当前对话框实例作为发送者
        /// </summary>
        private void CloseForm()
        {
            // 触发取消事件
            EventCenter.EventCenter.Fire(this, CanceledEventArgs.Create(this));
            
            // 可选：播放按钮音效
            // AudioManager.Instance.PlaySound("ButtonClick");
            
            // 可选：关闭表单
            // UIManager.Instance.CloseUIForm(this);
        }
    }
}