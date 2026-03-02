using TMPro;
using UnityEngine;

namespace UIForm
{
    /// <summary>
    /// 提示界面UI表单
    /// 继承自UIManager.UIForm，用于显示游戏中的文本提示信息
    /// 通过参数传递提示文本，支持基本的字符串显示功能
    /// 适用于游戏内教程、操作说明、任务提示等场景
    /// </summary>
    public class TipUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 提示文本组件
        /// 用于显示传递给界面的提示信息
        /// 文本内容在打开表单时通过args[0]参数传入
        /// 如果参数为空或非字符串，文本将保持原有内容
        /// </summary>
        [SerializeField] 
        private TextMeshProUGUI TipText;

        /// <summary>
        /// 表单打开时调用
        /// 从参数中获取提示文本并显示
        /// 支持简单的字符串参数
        /// </summary>
        /// <param name="userData">用户数据，当前未使用</param>
        /// <param name="args">参数数组
        /// args[0]: 提示文本（string），必须传入字符串
        /// 如果未传入参数或参数不是字符串，文本将保持不变
        /// </param>
        public override void OnOpen(object userData = null, params object[] args)
        {
            // 检查参数并设置提示文本
            if (args.Length > 0 && args[0] is string tip)
            {
                TipText.text = tip;
            }
            
            // 调用基类方法
            base.OnOpen(userData);
        }
    }
}
