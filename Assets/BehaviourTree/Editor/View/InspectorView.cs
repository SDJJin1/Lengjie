using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 检查器视图
    /// 行为树编辑器中的属性检查器面板，用于显示和编辑选中的节点属性
    /// 继承自VisualElement，通过Odin Inspector提供高级属性编辑功能
    /// 通过UxmlFactory支持在UXML模板中实例化
    /// </summary>
    public class InspectorView : VisualElement
    {
        /// <summary>
        /// IMGUI容器
        /// 用于在UIElements中嵌入传统IMGUI内容，显示Odin Inspector界面
        /// </summary>
        public IMGUIContainer inspectorBar;

        /// <summary>
        /// 检查器数据视图
        /// 存储当前选中的节点数据，用于在检查器中显示和编辑
        /// </summary>
        public InspectorDataView InspectorDataView;
        
        /// <summary>
        /// UXML工厂类
        /// 使InspectorView可以通过UXML模板创建，继承自UxmlFactory
        /// 使用默认的UxmlTraits，无需自定义特性
        /// </summary>
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>{}
        
        /// <summary>
        /// 构造函数
        /// 初始化检查器视图，创建IMGUI容器和Odin Inspector编辑器
        /// </summary>
        public InspectorView()
        {
            Init();
        }

        /// <summary>
        /// 初始化方法
        /// 创建IMGUI容器，加载或创建检查器数据视图，设置GUI处理程序
        /// </summary>
        void Init()
        {
            inspectorBar = new IMGUIContainer() { name = "inspectorBar" };
            inspectorBar.style.flexGrow = 1;  // 使容器可伸缩填充可用空间
            CreateInspectorView();  // 创建检查器视图
            Add(inspectorBar);  // 将容器添加到视图
        }

        /// <summary>
        /// 更新选择节点面板显示
        /// 当树视图中的选择发生变化时调用，更新检查器中显示的数据
        /// 从树视图的当前选择中获取所有节点视图，将其数据添加到检查器数据视图中
        /// </summary>
        public void UpdateInspector()
        {
            // 清空当前选择的数据
            InspectorDataView.selectDatas.Clear();
            
            // 获取树视图中当前选中的节点，并转换为NodeView类型
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.selection
                .Select(node => node as NodeView)  // 转换为NodeView类型
                .ForEach(node =>
                {
                    if (node != null)
                    {
                        // 将节点数据添加到检查器数据视图中
                        InspectorDataView.selectDatas.Add(node.NodeViewData);
                    }
                });
        }

        /// <summary>
        /// 创建检查器视图
        /// 加载或创建InspectorDataView资产，创建Odin编辑器，设置GUI处理程序
        /// </summary>
        private void CreateInspectorView()
        {
            // 尝试加载现有的InspectorDataView资产
            InspectorDataView = AssetDatabase.LoadAssetAtPath<InspectorDataView>("Assets/BehaviourTree/Editor/Resources/InspectorDataView.asset");
            if (!InspectorDataView)
            {
                // 如果资产不存在，创建新的InspectorDataView
                InspectorDataView = ScriptableObject.CreateInstance<InspectorDataView>();
                // 创建资产文件
                AssetDatabase.CreateAsset(InspectorDataView, "Assets/BehaviourTree/Editor/Resources/InspectorDataView.asset");
            }
            
            // 为InspectorDataView创建Odin编辑器
            var odinEditor = UnityEditor.Editor.CreateEditor(InspectorDataView);
            
            // 设置IMGUIContainer的GUI处理程序，使用Odin编辑器绘制Inspector
            inspectorBar.onGUIHandler += () =>
            {
                odinEditor.OnInspectorGUI();
            };
        }
    }
}