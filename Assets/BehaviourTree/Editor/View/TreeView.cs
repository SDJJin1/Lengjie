using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourTree.BehaviourTree;
using BehaviourTree.BTAutoLayout;
using BehaviourTree.Editor.EditorExTools;
using BehaviourTree.ExTools;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 树视图
    /// 继承自GraphView，是行为树编辑器的主要图形视图组件
    /// 负责节点和边的创建、删除、复制、粘贴、布局等操作
    /// 通过UxmlFactory支持在UXML模板中实例化
    /// </summary>
    public class TreeView : GraphView
    {
        /// <summary>
        /// UXML工厂类
        /// 使TreeView可以通过UXML模板创建，继承自UxmlFactory
        /// 使用默认的UxmlTraits，无需自定义特性
        /// </summary>
        public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits>{}

        /// <summary>
        /// 根节点视图
        /// 当前行为树的根节点引用
        /// </summary>
        public NodeView RootNode;
        
        /// <summary>
        /// 节点视图字典
        /// 键：节点GUID，值：节点视图实例
        /// 用于快速通过GUID查找节点
        /// </summary>
        public Dictionary<string, NodeView> NodeViews;

        /// <summary>
        /// 构造函数
        /// 初始化树视图，添加背景、操作器、样式和事件回调
        /// </summary>
        public TreeView()
        {
            // 添加网格背景
            Insert(0, new GridBackground());
            
            // 添加视图操作器
            this.AddManipulator(new ContentZoomer());    // 内容缩放
            this.AddManipulator(new ContentDragger());   // 内容拖拽
            this.AddManipulator(new SelectionDragger()); // 选择拖拽
            this.AddManipulator(new RectangleSelector());// 矩形选择

            // 添加样式表
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/BehaviourTree/Editor/Resources/BehaviourTreeView.uss"));
            
            // 增加右键菜单
            GraphViewMenu();
            
            // 注册键盘和鼠标事件
            RegisterCallback<KeyDownEvent>(KeyDownControl);
            RegisterCallback<MouseEnterEvent>(MouseEnterControl);
            
            // 设置图形视图变化回调
            this.graphViewChanged = OnGraphViewChanged;
            
            // 初始化节点视图字典
            NodeViews = new Dictionary<string, NodeView>();
        }

        /// <summary>
        /// 图形视图更新
        /// 每帧调用，更新所有节点视图的状态
        /// </summary>
        public void OnGUI()
        {
            // 更新所有节点视图
            nodes.ForEach(node => (node as NodeView)?.UpdateNodeView());
        }
        
        /// <summary>
        /// 启动自动布局
        /// 使用RNG_LayoutNodeConvertor对树进行自动布局
        /// </summary>
        public void OnStartMove()
        {
            NodeAutoLayouter.Layout(new RNG_LayoutNodeConvertor().Init(RootNode));
        }
        
        /// <summary>
        /// 重写添加元素方法
        /// 当元素是NodeView时，将其添加到NodeViews字典中
        /// </summary>
        /// <param name="graphElement">要添加的图形元素</param>
        public new void AddElement(GraphElement graphElement)
        {
            base.AddElement(graphElement);
            
            // 如果是节点视图，添加到字典
            if (graphElement is NodeView nodeView)
            {
                NodeViews.Add(nodeView.NodeViewData.Guid, nodeView);
            }
        }
        
        /// <summary>
        /// 重写从选择中移除方法
        /// 处理边和节点的移除逻辑
        /// </summary>
        /// <param name="selectable">要移除的可选择项</param>
        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            
            switch (selectable)
            {
                case Edge edge:
                    edge.RemoveLink();  // 移除边的连接
                    break;
                case NodeView view:
                    NodeViews.Remove(view.NodeViewData.Guid);  // 从字典中移除节点
                    BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);  // 从数据中移除节点
                    break;
            }
        }

        /// <summary>
        /// 重写移除元素方法
        /// 处理节点视图的移除逻辑
        /// </summary>
        /// <param name="graphElement">要移除的图形元素</param>
        public new void RemoveElement(GraphElement graphElement)
        {
            base.RemoveElement(graphElement);
            
            if (graphElement is NodeView view)
            {
                NodeViews.Remove(view.NodeViewData.Guid);
                BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);
            }
        }
    
        #region 按键回调
        /// <summary>
        /// 鼠标进入控制
        /// 更新检查器显示当前选中的节点
        /// </summary>
        private void MouseEnterControl(MouseEnterEvent evt)
        {
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
        }

        /// <summary>
        /// 键盘按下控制
        /// 处理各种快捷键操作
        /// </summary>
        private void KeyDownControl(KeyDownEvent evt)
        {
            // 更新检查器
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
            
            // 阻止Tab键的默认行为
            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopPropagation();
            }

            // 只处理Ctrl组合键
            if (!evt.ctrlKey) return;
            
            switch (evt.keyCode)
            {
                case KeyCode.S:  // Ctrl+S: 保存
                    BehaviourTreeView.TreeWindow.Save();
                    evt.StopPropagation();
                    break;
                case KeyCode.E:  // Ctrl+E: 启动自动布局
                    OnStartMove();
                    evt.StopPropagation();
                    break;
                case KeyCode.X:  // Ctrl+X: 剪切
                    Cut(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.C:  // Ctrl+C: 复制
                    Copy(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.V:  // Ctrl+V: 粘贴
                    Paste(null);
                    evt.StopPropagation();
                    break;
            }
        }

        /// <summary>
        /// 剪切选择
        /// 复制选择的内容，然后删除
        /// </summary>
        public void Cut(DropdownMenuAction da)
        {
            Copy(null);  // 先复制
            base.CutSelectionCallback();  // 然后删除
        }

        /// <summary>
        /// 复制选择
        /// 将选中的节点数据复制到剪贴板
        /// </summary>
        private void Copy(DropdownMenuAction da)
        {
            // 获取选中的节点数据
            var ns = selection.OfType<NodeView>()
                .Select(n => n as NodeView)
                .Select(n => n.NodeViewData).ToList();
            
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            
            // 深度克隆节点数据并存储到设置中
            setting.CopyNode = ns.CloneData();
        }

        /// <summary>
        /// 粘贴剪贴板内容
        /// 创建剪贴板中节点数据的副本
        /// </summary>
        private void Paste(DropdownMenuAction da)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            if (setting.CopyNode == null) return;
            if (setting.CopyNode.Count == 0) return;
            
            // 清空当前选择
            ClearSelection();
            
            List<NodeView> pasteNode = new List<NodeView>();
            
            // 生成节点并选择，重新序列化克隆的节点
            for (int i = 0; i < setting.CopyNode.Count; i++)
            {
                NodeView node = new NodeView(setting.CopyNode[i]);
                this.AddElement(node);
                node.SetPosition(new Rect(setting.CopyNode[i].Position, Vector2.one));
                AddToSelection(node);
                pasteNode.Add(node);
                BehaviourTreeView.TreeData.NodeData.Add(setting.CopyNode[i]);
            }
            
            // 为粘贴的节点添加连接
            pasteNode.ForEach(n => n.AddEdge());
            
            // 重新克隆剪贴板数据，避免引用问题
            setting.CopyNode = setting.CopyNode.CloneData();
        }

        /// <summary>
        /// 图形视图变化回调
        /// 处理边和节点的创建与删除
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange viewChange)
        {
            // 处理新创建的边
            viewChange.edgesToCreate?.ForEach(edge =>
            {
                edge.AddNodeData();
            });
            
            // 处理要移除的元素
            viewChange.elementsToRemove?.ForEach(element =>
            {
                switch (element)
                {
                    case Edge edge:
                        edge.RemoveLink();
                        break;
                    case NodeView view:
                        NodeViews.Remove(view.NodeViewData.Guid);
                        BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);
                        break;
                }
            });
            
            return viewChange;
        }
        #endregion

        #region 右键菜单
        /// <summary>
        /// 右键菜单提供者
        /// </summary>
        private RightClickMenu menuWindowProvider;
        
        /// <summary>
        /// 初始化图形视图菜单
        /// 创建右键菜单提供者并注册节点创建请求
        /// </summary>
        public void GraphViewMenu()
        {
            menuWindowProvider = ScriptableObject.CreateInstance<RightClickMenu>();
            menuWindowProvider.OnSelectEntryHandler = OnMenuSelectEntry;
        
            // 注册节点创建请求
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };
        }
        
        /// <summary>
        /// 构建上下文菜单
        /// 在图形视图空白处右键时显示的菜单
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 添加"创建节点"菜单项
            evt.menu.AppendAction("Create Node", _ =>
            {
                var windowRoot = BehaviourTreeView.TreeWindow.rootVisualElement;
                var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, 
                    _.eventInfo.mousePosition + BehaviourTreeView.TreeWindow.position.position);
                SearchWindow.Open(new SearchWindowContext(windowMousePosition), menuWindowProvider);
            });
            
            // 添加编辑菜单项
            evt.menu.AppendAction("Cut", Cut);
            evt.menu.AppendAction("Copy", Copy);
            evt.menu.AppendAction("Paste", Paste);
            
            // 调用基类方法
            base.BuildContextualMenu(evt);
        }

        /// <summary>
        /// 右键菜单选择回调
        /// 在菜单中选择节点类型时创建新节点
        /// </summary>
        private bool OnMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // 坐标转换：屏幕坐标 -> 窗口坐标 -> 图形视图坐标
            var windowRoot = BehaviourTreeView.TreeWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, 
                context.screenMousePosition - BehaviourTreeView.TreeWindow.position.position);
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            
            // 通过反射创建节点实例
            var nodeBase = System.Activator.CreateInstance((System.Type)searchTreeEntry.userData) as BtNodeBase;
            var nodeLabel = nodeBase.GetType().GetCustomAttribute(typeof(NodeLabelAttribute)) as NodeLabelAttribute;
            
            // 设置节点名称
            nodeBase.NodeName = nodeBase.GetType().Name;
            if (nodeLabel != null)
            {
                if (nodeLabel.Label != "")
                {
                    nodeBase.NodeName = nodeLabel.Label;
                }
            }
            
            // 设置节点类型
            nodeBase.NodeType = nodeBase.GetType().GetNodeType();
            nodeBase.Position = graphMousePosition;
            nodeBase.Guid = System.Guid.NewGuid().ToString();
            
            // 创建节点视图
            NodeView group = new NodeView(nodeBase);
            group.SetPosition(new Rect(graphMousePosition, Vector2.one));
            this.AddElement(group);
            
            // 添加到行为树数据
            BehaviourTreeView.TreeData.NodeData.Add(nodeBase);
            AddToSelection(group);
            
            return true;
        }

        /// <summary>
        /// 获取兼容的端口
        /// 确定哪些端口可以连接到给定的起始端口
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPorts => 
                endPorts.direction != startAnchor.direction &&  // 方向相反（输入->输出或输出->输入）
                endPorts.node != startAnchor.node)              // 不是同一个节点
                .ToList();
        }
        #endregion
    }
}