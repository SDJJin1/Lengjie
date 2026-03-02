using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourTree.BehaviourTree;
using BehaviourTree.ExTools;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// OpenViewAttribute属性绘制器
    /// 用于在Odin Inspector中绘制带有OpenViewAttribute的字段，提供一个按钮来打开行为树视图窗口
    /// </summary>
    public class OpenViewAttributeDrawer : OdinAttributeDrawer<OpenViewAttribute, BehaviourTreeData>
    {
        /// <summary>
        /// 初始化绘制器
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// 绘制属性布局
        /// 在Odin Inspector中绘制属性，并添加一个按钮来打开行为树视图窗口
        /// </summary>
        /// <param name="label">属性标签</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 让此label传递下去，便于其他的特性进行绘制
            this.CallNextDrawer(label);
            if (GUILayout.Button(Attribute.ButtonName))
            {
                BehaviourTreeView.TreeData = ValueEntry.SmartValue;
                BehaviourTreeView.OpenView();
            }
        }
    }
    
    /// <summary>
    /// 播放模式状态变化监听器
    /// 在进入播放模式时刷新行为树视图
    /// 使用InitializeOnLoad特性确保在Unity编辑器启动时注册事件
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStateChangedExample
    {
        /// <summary>
        /// 静态构造函数
        /// 注册播放模式状态变化事件
        /// </summary>
        static PlayModeStateChangedExample()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        /// <summary>
        /// 播放模式状态变化事件处理
        /// 在退出编辑模式和进入播放模式时刷新行为树视图
        /// </summary>
        /// <param name="state">播放模式状态变化</param>
        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (!BehaviourTreeView.TreeWindow) return;
            if (state != PlayModeStateChange.ExitingEditMode && state != PlayModeStateChange.EnteredPlayMode) return;
            BehaviourTreeView.TreeWindow.Refresh();
        }
    }
    
    /// <summary>
    /// 行为树编辑器主窗口
    /// 继承自EditorWindow，提供可视化编辑行为树的功能
    /// 包含树状图视图、节点创建、连接编辑、数据保存等核心功能
    /// </summary>
    public class BehaviourTreeView : EditorWindow
    {
        // 单例实例
        public static BehaviourTreeView TreeWindow;
        // 当前编辑的行为树数据
        public static BehaviourTreeData TreeData;
        
        #region UI元素
        // 窗口根元素，包含树视图和检查器视图
        public SplitView WindowRoot;
        #endregion

        /// <summary>
        /// 打开行为树视图窗口的菜单项
        /// 通过Tools/BehaviourTree/BehaviourTreeView菜单或快捷键Shift+Alt+I打开
        /// </summary>
        [MenuItem("Tools/BehaviourTree/BehaviourTreeView _#&i")]
        public static void OpenView()
        {
            BehaviourTreeView wnd = GetWindow<BehaviourTreeView>();
            wnd.titleContent = new GUIContent("行为树");
            TreeWindow = wnd;
        }
        
        /// <summary>
        /// 创建GUI
        /// 加载和初始化编辑器窗口的视觉元素
        /// </summary>
        public void CreateGUI()
        {
            BehaviourTreeDataInit(TreeData);
        }

        /// <summary>
        /// 编辑器更新
        /// 每帧调用，用于更新树视图的GUI
        /// </summary>
        private void OnInspectorUpdate()
        {
            WindowRoot?.TreeView?.OnGUI();
        }

        /// <summary>
        /// 窗口启用时调用
        /// 注册撤销/重做事件
        /// </summary>
        private void OnEnable()
        {
            Undo.undoRedoPerformed = Refresh;
        }

        /// <summary>
        /// 窗口销毁时调用
        /// 保存数据并清理引用
        /// </summary>
        private void OnDestroy()
        {
            Save();
            TreeData = null;
        }

        /// <summary>
        /// 选择变化时调用
        /// 当编辑器中的选择对象变化时触发
        /// </summary>
        private void OnSelectionChange()
        {
            // 当前实现为空，可根据需要添加选择变化处理逻辑
        }

        /// <summary>
        /// 刷新窗口
        /// 清空当前UI并重新初始化行为树数据
        /// </summary>
        public void Refresh()
        {
            rootVisualElement.Clear();
            BehaviourTreeDataInit(TreeData);
        }

        /// <summary>
        /// 初始化行为树数据
        /// 根据提供的行为树数据创建和配置编辑器界面
        /// </summary>
        /// <param name="treeData">行为树数据</param>
        public void BehaviourTreeDataInit(BehaviourTreeData treeData)
        {
            TreeWindow = this;
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("BehaviourTreeView");
            visualTree.CloneTree(root);
            WindowRoot = root.Q<SplitView>("SplitView");
            WindowRoot.TreeView = WindowRoot.Q<TreeView>();
            WindowRoot.InspectorView = WindowRoot.Q<InspectorView>();
            WindowRoot.InspectorTitle = WindowRoot.Q<Label>("InspectorTitle");
            
            // 获取并设置窗口大小与位置
            if (treeData == null) return;
            var tr = treeData.ViewTransform;
            if (tr != null)
            {
                WindowRoot.TreeView.viewTransform.position = tr.position;
                WindowRoot.TreeView.viewTransform.scale = tr.scale;
            }
            
            // 不再需要从根节点生成整个树
            if (treeData.NodeData == null) return;

            // 为每个节点数据创建节点视图
            treeData.NodeData.ForEach(n => CreateNodes(n, treeData.Root));
            
            // 为所有节点视图添加连接边
            WindowRoot.TreeView.nodes.OfType<NodeView>().ForEach(n => n.AddEdge());
        }
        
        /// <summary>
        /// 创建节点
        /// 根据节点数据在树视图中创建对应的节点视图
        /// </summary>
        /// <param name="nodeData">节点数据</param>
        /// <param name="rootNode">根节点数据（用于标识根节点）</param>
        public void CreateNodes(BtNodeBase nodeData, BtNodeBase rootNode = null)
        {
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(nodeData);
            
            // 如果是根节点，设置树视图的根节点引用
            if (nodeData == rootNode)
            {
                view.RootNode = nodeView;
            }
            
            // 设置节点位置
            nodeView.SetPosition(new Rect(nodeData.Position, Vector2.one));
            view.AddElement(nodeView);
        }

        /// <summary>
        /// 通过创建根节点创建树
        /// 从根节点开始递归创建整个行为树的节点视图
        /// </summary>
        /// <param name="rootNode">根节点数据</param>
        public void CreateRoot(BtNodeBase rootNode)
        {
            if (rootNode == null) return;
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(rootNode);
            nodeView.SetPosition(new Rect(rootNode.Position, Vector2.one));
            view.AddElement(nodeView);
            view.RootNode = nodeView;
            
            // 根据节点类型递归创建子节点
            switch (rootNode)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(CreateNodeView);
                    break;
                case BtPrecondition precondition:
                    CreateNodeView(precondition.ChildNode);
                    break;
            }
        }
        
        /// <summary>
        /// 通过根节点去创建整颗树
        /// 递归创建节点及其所有子节点的视图
        /// </summary>
        /// <param name="rootNode">当前节点数据</param>
        public void CreateNodeView(BtNodeBase rootNode)
        {
            if (rootNode == null) return;
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(rootNode);
            nodeView.SetPosition(new Rect(rootNode.Position, Vector2.one));
            view.AddElement(nodeView);
            
            // 根据节点类型递归创建子节点
            switch (rootNode)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(CreateNodeView);
                    break;
                case BtPrecondition precondition:
                    CreateNodeView(precondition.ChildNode);
                    break;
            }
        }

        /// <summary>
        /// 保存数据
        /// 将当前的视图变换状态保存到行为树数据中，并标记场景为脏
        /// </summary>
        public void Save()
        {
            if (Application.isPlaying) return;
            if (TreeData == null) return;
            
            // 保存视图变换状态
            TreeData.ViewTransform = new SaveTransform();
            TreeData.ViewTransform.position = WindowRoot.TreeView.viewTransform.position;
            TreeData.ViewTransform.scale = WindowRoot.TreeView.viewTransform.scale;
            
            // 标记场景为脏并保存
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
    
    /// <summary>
    /// 右键菜单
    /// 实现ISearchWindowProvider接口，提供节点创建的右键菜单
    /// 用于在树视图空白处右键点击时显示可创建的节点类型
    /// </summary>
    public class RightClickMenu : ScriptableObject, ISearchWindowProvider
    {
        /// <summary>
        /// 选择条目委托
        /// 当用户在搜索窗口中选择一个条目时调用
        /// </summary>
        /// <param name="searchTreeEntry">用户选择的搜索树条目</param>
        /// <param name="context">搜索窗口上下文</param>
        /// <returns>是否成功处理选择</returns>
        public delegate bool SelectEntryDelegate(SearchTreeEntry searchTreeEntry, SearchWindowContext context);

        /// <summary>
        /// 选择条目事件处理器
        /// </summary>
        public SelectEntryDelegate OnSelectEntryHandler;
        
        /// <summary>
        /// 创建搜索树
        /// 实现ISearchWindowProvider接口，定义右键菜单的层级结构
        /// </summary>
        /// <param name="context">搜索窗口上下文</param>
        /// <returns>搜索树条目列表</returns>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            entries = AddNodeType<BtComposite>(entries, "组合节点");
            entries = AddNodeType<BtPrecondition>(entries, "条件节点");
            entries = AddNodeType<BtActionNode>(entries, "行为节点");
            return entries;
        }
        
        /// <summary>
        /// 添加节点类型到菜单
        /// 通过反射获取指定基类的所有派生类，并将它们添加到菜单中
        /// </summary>
        /// <typeparam name="T">节点基类类型</typeparam>
        /// <param name="entries">搜索树条目列表</param>
        /// <param name="pathName">菜单分组名称</param>
        /// <returns>更新后的搜索树条目列表</returns>
        public List<SearchTreeEntry> AddNodeType<T>(List<SearchTreeEntry> entries, string pathName)
        {
            entries.Add(new SearchTreeGroupEntry(new GUIContent(pathName)) { level = 1 });
            
            // 获取T的所有派生类
            List<System.Type> rootNodeTypes = typeof(T).GetDerivedClasses();
            foreach (var rootType in rootNodeTypes)
            {
                string menuName = rootType.Name;
                
                // 检查是否有NodeLabelAttribute特性，用于自定义菜单名称
                if (rootType.GetCustomAttribute(typeof(NodeLabelAttribute)) is NodeLabelAttribute nodeLabel)
                {
                    menuName = nodeLabel.MenuName;
                    if (nodeLabel.MenuName == "")
                    {
                        menuName = rootType.Name;
                    }
                }
                
                entries.Add(new SearchTreeEntry(new GUIContent(menuName)) { level = 2, userData = rootType });
            }
            return entries;
        }

        /// <summary>
        /// 选择条目回调
        /// 当用户在搜索窗口中选择一个条目时调用
        /// </summary>
        /// <param name="SearchTreeEntry">用户选择的搜索树条目</param>
        /// <param name="context">搜索窗口上下文</param>
        /// <returns>是否成功处理选择</returns>
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(SearchTreeEntry, context);
        }
    }
}