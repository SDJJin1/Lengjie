using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 交互界面表单
    /// 继承自UIManager.UIForm，用于显示临时的交互提示信息
    /// 当玩家靠近可交互物体时显示，通常包含交互说明和物品图标
    /// 通过参数动态设置文本和图标，支持各种交互场景
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 打开交互界面，显示文本和图标
    /// UIManager.Instance.OpenUIForm<InteractForm>(
    ///     EnumUIForm.InteractForm, 
    ///     null,                     // userData
    ///     "按E拾取",                // 交互文本
    ///     itemIcon                 // 物品图标
    /// );
    /// 
    /// // 关闭交互界面
    /// UIManager.Instance.CloseUIForm(EnumUIForm.InteractForm);
    /// </code>
    /// 
    /// 参数说明：
    /// 1. args[0]: 交互提示文本（string）
    /// 2. args[1]: 交互物品图标（Sprite）
    /// 3. 可以扩展更多参数
    /// </remarks>
    public class InteractForm : UIManager.UIForm
    {
        /// <summary>
        /// 交互文本组件
        /// 显示交互操作的提示文本，如"按E拾取"、"按F对话"等
        /// 文本内容在打开表单时通过args[0]参数传入
        /// </summary>
        [SerializeField] 
        private TextMeshProUGUI interactText;
        
        /// <summary>
        /// 交互图标组件
        /// 显示与交互相关的物品图标
        /// 图标在打开表单时通过args[1]参数传入
        /// 如果不需要图标，可以传入null
        /// </summary>
        [SerializeField] 
        private Image interactImage;
        
        /// <summary>
        /// 表单打开时调用
        /// 设置交互文本和图标
        /// 从参数数组中获取显示内容
        /// </summary>
        /// <param name="userData">用户数据，当前未使用</param>
        /// <param name="args">参数数组，必须包含交互文本和图标
        /// args[0]: 交互提示文本（string）
        /// args[1]: 交互物品图标（Sprite）
        /// </param>
        /// <remarks>
        /// 如果args参数不足，会跳过对应的设置
        /// 如果args[0]为null，文本将保持原有内容
        /// 如果args[1]为null，图标将隐藏
        /// </remarks>
        public override void OnOpen(object userData = null, params object[] args)
        {
            // 检查并设置交互文本
            if (args.Length > 0 && args[0] != null)
            {
                interactText.text = args[0].ToString();
            }
            else
            {
                // 如果没有传递文本，可以设置默认文本或保持为空
                // interactText.text = "可交互";
            }

            // 检查并设置交互图标
            if (args.Length > 1 && args[1] != null)
            {
                // 设置图标并显示
                interactImage.sprite = args[1] as Sprite;
                interactImage.gameObject.SetActive(true);
            }
            else
            {
                // 如果没有图标，隐藏图标组件
                interactImage.gameObject.SetActive(false);
            }
            
            // 调用基类方法
            base.OnOpen(userData);
        }

        /// <summary>
        /// 表单关闭时调用
        /// 清空文本和图标，准备下一次使用
        /// 确保表单可以重复使用
        /// </summary>
        public override void OnClose()
        {
            // 清空交互文本
            interactText.text = "";
            
            // 清空图标并隐藏
            interactImage.sprite = null;
            interactImage.gameObject.SetActive(false);
            
            // 调用基类方法
            base.OnClose();
        }
    }
}