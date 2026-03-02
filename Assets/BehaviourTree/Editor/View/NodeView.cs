using System;
using System.Linq;
using BehaviourTree.BehaviourTree;
using BehaviourTree.ExTools;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 节点视图
    /// 继承自GraphView.Node，用于在行为树编辑器中可视化和交互节点
    /// 将BtNodeBase数据模型转换为可视化元素，处理连接、样式和交互
    /// 通过Odin序列化支持节点数据的序列化
    /// </summary>
    public class NodeView : Node
    {
        /// <summary>
        /// 节点视图数据
        /// 通过Odin序列化，包含节点的行为树数据模型
        /// HideReferenceObjectPicker：隐藏引用对象选择器，防止直接编辑
        /// </summary>
        [LabelText("节点视图数据"), OdinSerialize, HideReferenceObjectPicker]
        public BtNodeBase NodeViewData;

        #region 连接数据
        /// <summary>
        /// 输入端口
        /// 用于接收来自父节点的连接
        /// HideIf("@true")：始终隐藏，不显示在Inspector中
        /// </summary>
        [LabelText("输入端口"), HideIf("@true")] 
        public Port InputPort;
        
        /// <summary>
        /// 输出端口
        /// 用于连接到子节点
        /// HideIf("@true")：始终隐藏，不显示在Inspector中
        /// </summary>
        [LabelText("输出端口"), HideIf("@true")] 
        public Port OutputPort;
        #endregion

        #region 节点样式
        // 背景元素
        private VisualElement _nodeBorderBar;
        // 标题背景元素
        private VisualElement _nodeTitleBar;
        // 标题标签
        private Label _titleLabel;
        #endregion
        
        /// <summary>
        /// 构造函数
        /// 初始化节点视图，加载UXML模板，设置节点数据和样式
        /// </summary>
        /// <param name="NodeViewData">节点数据模型</param>
        public NodeView(BtNodeBase NodeViewData) : base(
            "Assets/BehaviourTree/Editor/Resources/NodeView.uxml")
        {
            this.NodeViewData = NodeViewData;
            InitNodeView();      // 初始化节点类型和端口
            InitNodeStyleData(); // 初始化节点样式
        }

        /// <summary>
        /// 初始化节点视图
        /// 根据节点类型创建相应的端口配置
        /// </summary>
        private void InitNodeView()
        {
            switch (NodeViewData.NodeType)
            {
                case NodeType.组合节点:
                    LoadCompositeNode();      // 组合节点：有输入和多个输出
                    break;
                case NodeType.条件节点:
                    LoadPreconditionNode();   // 条件节点：有输入和单个输出
                    break;
                case NodeType.行为节点:
                    LoadActionNode();         // 行为节点：只有输入
                    break;
            }
        }

        /// <summary>
        /// 初始化节点样式数据
        /// 异步等待50毫秒后获取UI元素引用，设置初始颜色
        /// 确保UI元素在查询前已完成布局
        /// </summary>
        private async void InitNodeStyleData()
        {
            await UniTask.Delay(50);
            _nodeBorderBar = this.Q<VisualElement>("node-border");
            _nodeTitleBar = this.Q<VisualElement>("title");
            _titleLabel = this.Q<Label>("title-label");
            ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
        }
        
        /// <summary>
        /// 查询子节点并连接线条
        /// 根据节点类型查找子节点，并在节点视图之间创建连接线
        /// 通常在加载行为树时调用，重建节点间的连接
        /// </summary>
        public async void AddEdge()
        {
            TreeView view = BehaviourTreeView.TreeWindow.WindowRoot.TreeView;
            switch (NodeViewData)
            {
                case BtComposite composite:
                    // 组合节点：连接所有子节点
                    foreach (var t in composite.ChildNodes)
                    {
                        LinkNodes(OutputPort, view.NodeViews[t.Guid].InputPort);
                    }
                    break;
                case BtPrecondition precondition:
                    // 条件节点：连接唯一子节点
                    if (precondition.ChildNode == null) break;
                    LinkNodes(OutputPort, view.NodeViews[precondition.ChildNode.Guid].InputPort);
                    break;
            }
        }

        /// <summary>
        /// 添加子对象
        /// 在节点数据模型中添加子节点引用
        /// </summary>
        /// <param name="node">要添加的子节点视图</param>
        public void AddChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Add(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    precondition.ChildNode = node.NodeViewData;
                    break;
            }
        }
        
        /// <summary>
        /// 移除子对象
        /// 从节点数据模型中移除子节点引用
        /// </summary>
        /// <param name="node">要移除的子节点视图</param>
        public void RemoveChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Remove(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    if (precondition.ChildNode == node.NodeViewData)
                    {
                        precondition.ChildNode = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// 连接两个节点
        /// 创建边视图并连接输入输出端口
        /// </summary>
        /// <param name="outputSocket">输出端口（父节点）</param>
        /// <param name="inputSocket">输入端口（子节点）</param>
        void LinkNodes(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new EdgeView()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.Add(tempEdge);
        }

        /// <summary>
        /// 节点被选中时调用
        /// 更新检查器面板显示当前选中的节点
        /// </summary>
        public override void OnSelected()
        {
            base.OnSelected();
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
        }

        /// <summary>
        /// 节点取消选中时调用
        /// 更新检查器面板显示当前选中的节点
        /// </summary>
        public override void OnUnselected()
        {
            base.OnUnselected();
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
        }
        
        /// <summary>
        /// 加载组合节点配置
        /// 组合节点有输入端口和多容量输出端口
        /// </summary>
        private void LoadCompositeNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Multi);
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }

        /// <summary>
        /// 加载行为节点配置
        /// 行为节点只有输入端口，没有输出端口
        /// </summary>
        private void LoadActionNode()
        {
            InputPort = PortView.Create<Edge>();
            inputContainer.Add(InputPort);
        }

        /// <summary>
        /// 加载条件节点配置
        /// 条件节点有输入端口和单容量输出端口
        /// </summary>
        private void LoadPreconditionNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Single);
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }
        
        /// <summary>
        /// 每帧更新
        /// 在编辑器和运行时更新节点外观和状态
        /// </summary>
        public void UpdateNodeView()
        {
            // 更新名称
            title = NodeViewData.NodeName;
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
            
            // 运行时状态更新
            if (Application.isPlaying && NodeViewData.NodeState == BehaviourState.执行中)
            {
                ChangeBgColor(Color.cyan);  // 执行中状态显示青色背景
                // 更新线运行
            }
            else
            {
                // 更新颜色
                ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            }
            
            // 运行时更新连接线状态
            if (Application.isPlaying && InputPort.connected)
            {
                UpdateEdgeState();
            }
            
            // 更新子节点顺序（按X坐标排序）
            if (NodeViewData is BtComposite composite)
            {
                composite.ChildNodes
                    .Sort((x, y) => x.Position.x.CompareTo(y.Position.x));
            }
        }

        /// <summary>
        /// 更改背景颜色
        /// 设置节点边框和标题的背景颜色
        /// </summary>
        /// <param name="color">目标颜色</param>
        void ChangeBgColor(Color color)
        {
            if (_nodeBorderBar == null || _nodeTitleBar == null) return;
            _nodeBorderBar.style.unityBackgroundImageTintColor = color;
            _nodeTitleBar.style.unityBackgroundImageTintColor = color;
        }
        
        /// <summary>
        /// 更改标题颜色
        /// 设置节点标题文本颜色
        /// </summary>
        /// <param name="color">目标颜色</param>
        void ChangeTitleColor(Color color)
        {
            if (_titleLabel == null) return;
            _titleLabel.style.color = color;
        }

        /// <summary>
        /// 根据对象运行状态改变线条
        /// 启动或停止连接线上的动画点
        /// </summary>
        void UpdateEdgeState()
        {
            if (NodeViewData.NodeState == BehaviourState.执行中)
            {
                (InputPort.connections.First() as EdgeView).OnStartMovePoints();
            }
            else
            {
                (InputPort.connections.First() as EdgeView).OnStopMovePoints();
            }
        }

        /// <summary>
        /// 设置节点位置
        /// 重写基类方法，同时更新节点数据模型中的位置
        /// </summary>
        /// <param name="newPos">新的位置矩形</param>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeViewData.Position = new Vector2(newPos.xMin, newPos.yMin);
        }
        
        /// <summary>
        /// 构建上下文菜单
        /// 在节点上右键点击时显示的菜单
        /// </summary>
        /// <param name="evt">上下文菜单事件</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 添加"设为根节点"菜单项
            evt.menu.AppendAction("设为根节点", SetRoot);
        }

        /// <summary>
        /// 设为根节点
        /// 将此节点设为行为树的根节点
        /// </summary>
        /// <param name="obj">下拉菜单操作</param>
        private void SetRoot(DropdownMenuAction obj)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.RootNode = this;
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.OnStartMove();
            BehaviourTreeView.TreeData.Root = NodeViewData;
        }

        /// <summary>
        /// 重载+运算符
        /// 简化两个节点之间的连接操作
        /// </summary>
        /// <param name="p1">父节点视图</param>
        /// <param name="p2">子节点视图</param>
        /// <returns>总是返回false，仅用于语法糖</returns>
        public static bool operator+ (NodeView p1, NodeView p2)
        {
            p1.LinkNodes(p1.OutputPort, p2.InputPort);
            return false;
        }
    }

    /// <summary>
    /// 端口视图工具类
    /// 提供创建端口视图的静态工厂方法
    /// </summary>
    public class PortView
    {
        /// <summary>
        /// 创建端口
        /// 工厂方法，创建指定方向、流向和容量的端口
        /// </summary>
        /// <typeparam name="TEdge">边类型</typeparam>
        /// <param name="ori">端口方向（水平/垂直）</param>
        /// <param name="dir">数据流向（输入/输出）</param>
        /// <param name="cap">端口容量（单连接/多连接）</param>
        /// <param name="type">数据类型</param>
        /// <returns>创建的端口实例</returns>
        public static Port Create<TEdge>(Orientation ori = Orientation.Vertical, Direction dir = Direction.Input,
            Port.Capacity cap = Port.Capacity.Single, Type type = null) where TEdge : Edge, new()
        {
            Port port = Port.Create<TEdge>(ori, dir, cap, type);
            port.portName = "";  // 清空端口名称
            port.style.flexDirection = dir == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
            return port;
        }
    }
}