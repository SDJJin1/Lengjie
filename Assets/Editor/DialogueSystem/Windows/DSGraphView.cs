using System;
using System.Collections.Generic;
using DialogueSystem.Elements;
using DialogueSystem.Enumerations;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    /// <summary>
    /// 对话系统图形视图
    /// 继承自GraphView，用于可视化编辑对话树，包括节点、组、连接线等
    /// 处理节点和组的创建、删除、连接以及重复名称检测等逻辑
    /// </summary>
    public class DSGraphView : GraphView
    {
        // 编辑器窗口引用
        private DSEditorWindow editorWindow;
        // 搜索窗口，用于快速创建节点
        private DSSearchWindow searchWindow;
        
        // 迷你地图，用于快速导航大型对话图
        private MiniMap miniMap;

        // 重复名称检测数据结构
        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;               // 未分组节点的错误数据
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes; // 分组节点的错误数据
        private SerializableDictionary<string, DSGroupErrorData> groups;                     // 组的错误数据

        private int repeatedNamesAmount; // 重复名称计数

        /// <summary>
        /// 重复名称数量属性
        /// 当重复名称数量为0时启用保存，为1时禁用保存
        /// </summary>
        public int RepeatedNamesAmount
        {
            get { return repeatedNamesAmount; }
            set
            {
                repeatedNamesAmount = value;
                if (repeatedNamesAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (repeatedNamesAmount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }
        
        /// <summary>
        /// 构造函数
        /// 初始化图形视图，设置各种回调事件和UI元素
        /// </summary>
        /// <param name="editorWindow">所属的编辑器窗口</param>
        public DSGraphView(DSEditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
            
            // 初始化错误数据字典
            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();
            groups = new SerializableDictionary<string, DSGroupErrorData>();
            
            // 添加各种UI元素和功能
            AddManipulators();
            AddSearchWindow();
            AddMiniMap();
            AddGridBackground();
            AddStyles();
            AddMiniMapStyles();
            
            // 注册各种回调事件
            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();
        }

        #region  重写方法

        /// <summary>
        /// 获取兼容的端口列表
        /// 确定哪些端口可以连接到给定的起始端口
        /// </summary>
        /// <param name="startPort">起始端口</param>
        /// <param name="nodeAdapter">节点适配器</param>
        /// <returns>兼容的端口列表</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                // 不能连接到自身
                if (startPort == port)
                {
                    return;
                }

                // 不能连接到同一节点上的端口
                if (startPort.node == port.node)
                {
                    return;
                }

                // 输入端口只能连接到输出端口，反之亦然
                if (startPort.direction == port.direction)
                {
                    return;
                }
                
                compatiblePorts.Add(port);
            });
            
            return compatiblePorts;
        }

        #endregion
        
        #region 操作器

        /// <summary>
        /// 添加操作器
        /// 设置图形视图的交互操作，如缩放、拖拽、选择等
        /// </summary>
        private void AddManipulators()
        {
            // 设置缩放范围
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            
            // 添加各种操作器
            this.AddManipulator(new ContentDragger());      // 内容拖拽
            this.AddManipulator(new SelectionDragger());    // 选择拖拽
            this.AddManipulator(new SelectionDropper());    // 选择放置
            this.AddManipulator(new RectangleSelector());   // 矩形选择
            
            // 添加上下文菜单操作器
            this.AddManipulator(CreateNodeContextualMenu("Add Node(Single Choice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node(Multiple Choice)", DSDialogueType.MultipleChoice));
            
            // 添加组上下文菜单操作器
            this.AddManipulator(CreateGroupContextualMenu());
        }
        
        /// <summary>
        /// 创建组上下文菜单
        /// 在图形视图空白处右键点击时显示"添加组"选项
        /// </summary>
        /// <returns>上下文菜单操作器</returns>
        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(menuEvent =>
                menuEvent.menu.AppendAction("Add Group",
                    actionEvent => CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            
            return contextualMenuManipulator;
        }

        /// <summary>
        /// 创建节点上下文菜单
        /// 在图形视图空白处右键点击时显示"添加节点"选项
        /// </summary>
        /// <param name="actionTitle">菜单项标题</param>
        /// <param name="dialogueType">对话类型</param>
        /// <returns>上下文菜单操作器</returns>
        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(menuEvent =>
                menuEvent.menu.AppendAction(actionTitle,
                    actionEvent => AddElement(CreateNode("DialogueName", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            
            return contextualMenuManipulator;
        }

        #endregion

        #region 元素创建

        /// <summary>
        /// 创建组
        /// 在指定位置创建一个新的对话组，并将当前选中的节点添加到组中
        /// </summary>
        /// <param name="title">组标题</param>
        /// <param name="localMousePosition">鼠标在图形视图中的本地位置</param>
        /// <returns>创建的组</returns>
        public DSGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            // 创建组实例
            DSGroup group = new DSGroup(title, localMousePosition);

            // 添加到组管理（重复名称检测）
            AddGroup(group);
            
            // 添加到图形视图
            AddElement(group);

            // 将当前选中的节点添加到组中
            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode)selectedElement;
                
                group.AddElement(node);
            }
            
            return group;
        }
        
        /// <summary>
        /// 创建节点
        /// 根据对话类型在指定位置创建一个新的对话节点
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="dialogueType">对话类型</param>
        /// <param name="position">节点位置</param>
        /// <param name="shouldDraw">是否立即绘制节点，默认为true</param>
        /// <returns>创建的节点</returns>
        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            // 通过反射创建对应类型的节点实例
            Type nodeType = Type.GetType($"DialogueSystem.Elements.DS{dialogueType}Node");
            DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            
            // 初始化节点
            node.Initialize(nodeName, this, position);

            // 如果需要，绘制节点UI
            if (shouldDraw)
            {
                node.Draw();
            }
            

            // 添加到未分组节点管理
            AddUngroupedNode(node);
            
            return node;
        }
        
        #endregion

        #region 回调事件

        /// <summary>
        /// 元素删除回调
        /// 处理删除操作，包括节点、边和组
        /// </summary>
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);
                
                // 收集要删除的元素
                List<DSGroup> groupsToDelete = new List<DSGroup>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<DSNode> nodesToDelete = new List<DSNode>();
                
                // 遍历选中的元素，分类存储
                foreach (GraphElement element in selection)
                {
                    if (element is DSNode node)
                    {
                        nodesToDelete.Add(node);
                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = (Edge)element;
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if (element.GetType() != groupType)
                    {
                        continue;
                    }
                    
                    DSGroup group = (DSGroup)element;
                    RemoveGroup(group); // 从组管理中移除
                    groupsToDelete.Add(group);
                }

                // 删除组及其包含的节点
                foreach (DSGroup group in groupsToDelete)
                {
                    List<DSNode> groupNodes = new List<DSNode>();

                    // 收集组内的节点
                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is DSNode))
                        {
                            continue;
                        }

                        DSNode groupNode = (DSNode) groupElement;
                        groupNodes.Add(groupNode);
                    }
                    
                    // 从组中移除节点
                    group.RemoveElements(groupNodes);
                    // 从图形视图中移除组
                    RemoveElement(group);
                }

                // 删除边
                DeleteElements(edgesToDelete);

                // 删除节点
                foreach (DSNode node in nodesToDelete)
                {
                    // 如果节点属于某个组，从组中移除
                    if (node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }
                    
                    // 从节点管理中移除
                    RemoveUngroupedNode(node);
                    // 断开所有端口连接
                    node.DisconnectAllPorts();
                    // 从图形视图中移除
                    RemoveElement(node);
                }
            };
        }

        /// <summary>
        /// 组元素添加回调
        /// 当元素被添加到组时触发，更新节点管理状态
        /// </summary>
        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup nodeGroup = (DSGroup)group;
                    DSNode node = (DSNode)element;
                    
                    // 从未分组节点中移除，添加到分组节点
                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        /// <summary>
        /// 组元素移除回调
        /// 当元素从组中移除时触发，更新节点管理状态
        /// </summary>
        private void OnGroupElementRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }
                    
                    DSNode node = (DSNode)element;
                    
                    // 从分组节点中移除，添加到未分组节点
                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }

        /// <summary>
        /// 组重命名回调
        /// 当组标题更改时触发，处理重复名称检测
        /// </summary>
        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup)group;

                // 清理新标题（移除空白和特殊字符）
                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();
                
                // 更新重复名称计数
                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.oldTitle))
                    {
                        RepeatedNamesAmount++;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.oldTitle))
                    {
                        RepeatedNamesAmount--;
                    }
                }
                
                // 从组管理中移除旧名称，添加新名称
                RemoveGroup(dsGroup);
                dsGroup.oldTitle = dsGroup.title;
                AddGroup(dsGroup);
            };
        }

        /// <summary>
        /// 图形视图更改回调
        /// 当图形视图发生变化时触发，如连接创建或删除
        /// </summary>
        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                // 处理新创建的边（连接）
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;
                        choiceData.NodeId = nextNode.ID; // 设置选项的目标节点ID
                    }
                }

                // 处理删除的边（连接）
                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }
                        
                        Edge edge = (Edge)element;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;
                        choiceData.NodeId = ""; // 清空选项的目标节点ID
                    }
                }
                
                return changes;
            };
        }

        #endregion

        #region 重复元素管理

        /// <summary>
        /// 添加组到管理
        /// 检测组名称重复并设置错误样式
        /// </summary>
        /// <param name="group">要添加的组</param>
        public void AddGroup(DSGroup group)
        {
            string groupName = group.title.ToLower();

            // 如果组名不存在，创建新的错误数据
            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();
                groupErrorData.Groups.Add(group);
                groups.Add(groupName, groupErrorData);
                return;
            }
            
            // 组名已存在，添加到现有组列表
            List<DSGroup> groupsList = groups[groupName].Groups;
            groupsList.Add(group);

            // 设置错误颜色
            Color errorColor = groups[groupName].ErrorData.Color;
            group.SetErrorSytle(errorColor);

            // 如果这是第二个重复名称，增加重复计数并设置第一个组的错误样式
            if (groupsList.Count == 2)
            {
                ++RepeatedNamesAmount;
                groupsList[0].SetErrorSytle(errorColor);
            }
        }

        /// <summary>
        /// 添加未分组节点到管理
        /// 检测节点名称重复并设置错误样式
        /// </summary>
        /// <param name="node">要添加的节点</param>
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            // 如果节点名不存在，创建新的错误数据
            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                ungroupedNodes.Add(nodeName, nodeErrorData);
                return;
            }

            // 节点名已存在，添加到现有节点列表
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            ungroupedNodesList.Add(node);

            // 设置错误颜色
            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // 如果这是第二个重复名称，增加重复计数并设置第一个节点的错误样式
            if (ungroupedNodesList.Count == 2)
            {
                ++RepeatedNamesAmount;
                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// 添加分组节点到管理
        /// 检测同一组内节点名称重复并设置错误样式
        /// </summary>
        /// <param name="node">要添加的节点</param>
        /// <param name="group">节点所属的组</param>
        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();
            node.Group = group;

            // 如果组不存在于分组节点字典中，先创建组条目
            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            // 如果组内不存在该节点名，创建新的错误数据
            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.Nodes.Add(node);
                groupedNodes[group].Add(nodeName, nodeErrorData);
                return;
            }
            
            // 节点名已存在，添加到现有节点列表
            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;
            groupedNodesList.Add(node);
            
            // 设置错误颜色
            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            // 如果这是第二个重复名称，增加重复计数并设置第一个节点的错误样式
            if (groupedNodesList.Count == 2)
            {
                ++RepeatedNamesAmount;
                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        /// <summary>
        /// 从组管理中移除组
        /// 更新重复名称计数和样式
        /// </summary>
        /// <param name="group">要移除的组</param>
        public void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.oldTitle.ToLower();
            List<DSGroup> groupsList = groups[oldGroupName].Groups;
            
            // 从组列表中移除
            groupsList.Remove(group);
            group.ResetStyle(); // 重置组样式

            // 如果只剩一个组，减少重复计数并重置其样式
            if (groupsList.Count == 1)
            {
                --RepeatedNamesAmount;
                groupsList[0].ResetStyle();
                return;
            }

            // 如果组列表为空，从字典中移除该组名
            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }

        /// <summary>
        /// 从未分组节点管理中移除节点
        /// 更新重复名称计数和样式
        /// </summary>
        /// <param name="node">要移除的节点</param>
        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            
            ungroupedNodesList.Remove(node);
            node.ResetStyle(); // 重置节点样式

            // 如果只剩一个节点，减少重复计数并重置其样式
            if (ungroupedNodesList.Count == 1)
            {
                --RepeatedNamesAmount;
                ungroupedNodesList[0].ResetStyle();
                return;
            }

            // 如果节点列表为空，从字典中移除该节点名
            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        /// <summary>
        /// 从分组节点管理中移除节点
        /// 更新重复名称计数和样式
        /// </summary>
        /// <param name="node">要移除的节点</param>
        /// <param name="group">节点所属的组</param>
        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();
            node.Group = null;
            
            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;
            
            groupedNodesList.Remove(node);
            node.ResetStyle(); // 重置节点样式

            // 如果只剩一个节点，减少重复计数并重置其样式
            if (groupedNodesList.Count == 1)
            {
                --RepeatedNamesAmount;
                groupedNodesList[0].ResetStyle();
                return;
            }

            // 如果节点列表为空，从组字典中移除该节点名
            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                // 如果组内没有其他节点，从分组节点字典中移除该组
                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        #endregion
        
        #region 元素添加

        /// <summary>
        /// 添加网格背景
        /// 为图形视图添加网格背景，提高可读性
        /// </summary>
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        /// <summary>
        /// 添加搜索窗口
        /// 用于通过搜索创建节点
        /// </summary>
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initialize(this);
            }

            // 设置节点创建请求的处理，打开搜索窗口
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// 添加迷你地图
        /// 显示图形视图的缩略图，便于导航
        /// </summary>
        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true, // 固定位置
            };
            
            miniMap.SetPosition(new Rect(15, 50, 200, 180)); // 设置位置和大小
            Add(miniMap);
            miniMap.visible = false; // 默认隐藏
        }

        /// <summary>
        /// 添加样式表
        /// 为图形视图添加自定义样式
        /// </summary>
        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueSystem/DSGraphViewStyles.uss",
                "DialogueSystem/DSNodeStyles.uss"
            );
        }

        /// <summary>
        /// 添加迷你地图样式
        /// 设置迷你地图的背景和边框颜色
        /// </summary>
        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));
            
            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取本地鼠标位置
        /// 将屏幕坐标转换为图形视图内容容器的本地坐标
        /// </summary>
        /// <param name="mousePosition">鼠标位置</param>
        /// <param name="isSearchWindow">是否从搜索窗口调用</param>
        /// <returns>转换后的本地位置</returns>
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            // 如果从搜索窗口调用，需要减去编辑器窗口的位置
            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }
            
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            
            return localMousePosition;
        }

        /// <summary>
        /// 清空图形视图
        /// 移除所有图形元素并重置数据
        /// </summary>
        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            
            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            RepeatedNamesAmount = 0;
        }

        /// <summary>
        /// 切换迷你地图显示
        /// 显示或隐藏迷你地图
        /// </summary>
        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }

        #endregion
    }
}