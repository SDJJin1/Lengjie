using UnityEngine.UIElements;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 分隔视图
    /// 继承自Unity的TwoPaneSplitView，用于在编辑器中创建可调整大小的左右分隔视图
    /// 左侧显示行为树节点视图，右侧显示属性检查器
    /// 通过UxmlFactory支持在UXML模板中实例化
    /// </summary>
    public class SplitView : TwoPaneSplitView
    {
        #region VisualElement内容
        /// <summary>
        /// 检查器视图
        /// 右侧面板，用于显示和编辑选中节点的属性
        /// </summary>
        public InspectorView InspectorView;
        
        /// <summary>
        /// 检查器标题
        /// 检查器视图的标题标签
        /// </summary>
        public Label InspectorTitle;
        
        /// <summary>
        /// 树视图
        /// 左侧面板，用于显示和编辑行为树节点
        /// </summary>
        public TreeView TreeView;
        #endregion
        
        /// <summary>
        /// UXML工厂类
        /// 使SplitView可以通过UXML模板创建，继承自UxmlFactory
        /// 使用默认的UxmlTraits，无需自定义特性
        /// </summary>
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits>{}
        
        /// <summary>
        /// 构造函数
        /// 初始化分隔视图，目前初始化方法为空
        /// 具体的UI元素引用在外部通过查询方式设置
        /// </summary>
        public SplitView()
        {
            Init();
        }

        /// <summary>
        /// 初始化方法
        /// 当前为空实现，预留用于未来的初始化逻辑
        /// 实际的UI元素引用通常在外部通过VisualElement.Q()方法获取
        /// </summary>
        private void Init()
        {
            // 初始化逻辑（当前为空）
        }
    }
}