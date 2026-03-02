using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem.Elements;
using DialogueSystem.Windows;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 对话系统输入输出工具类
/// 负责对话图的保存和加载功能，将图形视图数据与ScriptableObject资产进行双向转换
/// 仅在Unity编辑器中使用
/// </summary>
public static class DSIOUtility
{
    // 静态字段，用于存储当前操作的图形视图和路径信息
    private static DSGraphView graphView;
    private static string graphFileName;
    private static string containerFolderPath;
    
    // 临时存储从图形视图中获取的元素
    private static List<DSGroup> groups;
    private static List<DSNode> nodes;

    // 用于保存时创建的ScriptableObject资产引用
    private static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
    private static Dictionary<string, DSDialogueSO>  createdDialogues;

    // 用于加载时恢复图形元素
    private static Dictionary<string, DSGroup> loadedGroups;
    private static Dictionary<string, DSNode> loadedNodes;
    
    /// <summary>
    /// 初始化IO工具类
    /// 设置当前操作的图形视图和文件名，初始化各种数据结构
    /// </summary>
    /// <param name="dsGraphView">要保存或加载的对话图视图</param>
    /// <param name="graphName">对话图的文件名（不含扩展名）</param>
    public static void Initialize(DSGraphView dsGraphView, string graphName)
    {
        graphView = dsGraphView;
        graphFileName = graphName;
        containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphFileName}";
        
        groups = new List<DSGroup>();
        nodes = new List<DSNode>();
        createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
        createdDialogues = new Dictionary<string, DSDialogueSO>();
        
        loadedGroups = new Dictionary<string, DSGroup>();
        loadedNodes = new Dictionary<string, DSNode>();
    }
    
    #region 保存方法

    /// <summary>
    /// 保存对话图
    /// 将图形视图中的所有元素保存为ScriptableObject资产和图形数据文件
    /// </summary>
    public static void Save()
    {
        // 创建必要的文件夹结构
        CreateStaticFolders();

        // 从图形视图中获取所有组和节点
        GetElementsFromGraphView();

        // 创建图形保存数据资产
        DSGraphSaveDataSO graphData =
            CreateAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");
        
        graphData.Initialize(graphFileName);

        // 创建对话容器资产
        DSDialogueContainerSO dialogueContainer =
            CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);
        dialogueContainer.Initialize(graphFileName);

        // 保存组和节点数据
        SaveGroups(graphData, dialogueContainer);
        SaveNodes(graphData, dialogueContainer);
        
        // 保存资产到磁盘
        SaveAsset(graphData);
        SaveAsset(dialogueContainer);
    }

    #endregion

    #region 加载方法

    /// <summary>
    /// 加载对话图
    /// 从保存的数据中重建图形视图中的组、节点和连接
    /// </summary>
    public static void Load()
    {
        // 加载图形保存数据资产
        DSGraphSaveDataSO graphData =
            LoadAsset<DSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

        // 如果资产不存在，显示错误对话框
        if (graphData == null)
        {
            EditorUtility.DisplayDialog(
                "Couldn't load the file!",
                "The file at the following path could not be found:\n\n" + 
                $"Assets/Editor/DialogueSystem/Graphs/{graphFileName}\n\n",
                "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                "Thanks!"
            );
            
            return;
        }
        
        // 更新编辑器窗口的文件名显示
        DSEditorWindow.UpdateFileName(graphData.FileName);

        // 依次加载组、节点和连接
        LoadGroups(graphData.Groups);
        LoadNodes(graphData.Nodes);
        LoadNodesConnections();
    }

    /// <summary>
    /// 加载组数据
    /// 根据保存的组数据在图形视图中创建组
    /// </summary>
    /// <param name="graphDataGroups">要加载的组数据列表</param>
    private static void LoadGroups(List<DSGroupSaveData> graphDataGroups)
    {
        foreach (DSGroupSaveData groupData in graphDataGroups)
        {
            // 使用图形视图创建组
            DSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);
            
            // 恢复组的ID
            group.ID = groupData.ID;
            
            // 添加到已加载组字典
            loadedGroups.Add(group.ID, group);
        }
    }

    /// <summary>
    /// 加载节点数据
    /// 根据保存的节点数据在图形视图中创建节点
    /// </summary>
    /// <param name="graphDataNodes">要加载的节点数据列表</param>
    private static void LoadNodes(List<DSNodeSaveData> graphDataNodes)
    {
        foreach (DSNodeSaveData nodeData in graphDataNodes)
        {
            // 克隆节点选项数据（避免引用问题）
            List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
            // 使用图形视图创建节点
            DSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
            
            // 恢复节点属性
            node.ID = nodeData.ID;
            node.Choices = choices;
            node.Text = nodeData.Text;
            
            // 绘制节点UI
            node.Draw();
            
            // 将节点添加到图形视图
            graphView.AddElement(node);
            
            // 添加到已加载节点字典
            loadedNodes.Add(node.ID, node);

            // 如果节点不属于任何组，跳过组分配
            if (string.IsNullOrEmpty(nodeData.GroupId))
            {
                continue;
            }
            
            // 将节点分配到对应的组
            DSGroup group = loadedGroups[nodeData.GroupId];
            node.Group = group;
            
            group.AddElement(node);
        }
    }
    
    /// <summary>
    /// 加载节点连接
    /// 根据保存的选项数据重建节点之间的连接线
    /// </summary>
    private static void LoadNodesConnections()
    {
        foreach (KeyValuePair<string, DSNode> loadedNode in loadedNodes)
        {
            // 遍历节点的每个输出端口
            foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
            {
                DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;

                // 如果选项没有连接的目标节点，跳过
                if (string.IsNullOrEmpty(choiceData.NodeId))
                {
                    continue;
                }
                
                // 获取目标节点
                DSNode nextNode = loadedNodes[choiceData.NodeId];

                // 获取目标节点的输入端口
                Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                
                // 创建连接线
                Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                
                // 将连接线添加到图形视图
                graphView.AddElement(edge);

                // 刷新端口显示
                loadedNode.Value.RefreshPorts();
            }
        }
    }

    #endregion
    
    #region 组处理

    /// <summary>
    /// 保存所有组
    /// 将图形视图中的组保存到图形数据和对话容器中
    /// </summary>
    /// <param name="graphData">图形保存数据对象</param>
    /// <param name="dialogueContainer">对话容器对象</param>
    public static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
    {
        List<string> groupNames = new List<string>();
        
        foreach (DSGroup group in groups)
        {
            // 保存组到图形数据和ScriptableObject
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, dialogueContainer);
            
            groupNames.Add(group.title);
        }

        // 更新旧组名列表，用于清理不再使用的组
        UpdateOldGroups(groupNames, graphData);
    }

    /// <summary>
    /// 更新旧组列表
    /// 比较新旧组列表，删除不再使用的组文件夹
    /// </summary>
    /// <param name="currentGroupNames">当前组名列表</param>
    /// <param name="dsGraphSaveDataSo">图形保存数据对象</param>
    private static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO dsGraphSaveDataSo)
    {
        // 如果有旧组名列表，找出需要删除的组
        if (dsGraphSaveDataSo.OldGroupNames != null && dsGraphSaveDataSo.OldGroupNames.Count != 0)
        {
            List<string> groupsToRemove = dsGraphSaveDataSo.OldGroupNames.Except(currentGroupNames).ToList();

            foreach (string groupToRemove in groupsToRemove)
            {
                RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
            }
        }

        // 更新旧组名列表为当前组名列表
        dsGraphSaveDataSo.OldGroupNames = new List<string>(currentGroupNames);
    }

    /// <summary>
    /// 保存组到ScriptableObject
    /// 创建对话组资产并设置到对话容器中
    /// </summary>
    /// <param name="group">要保存的组</param>
    /// <param name="dialogueContainer">对话容器对象</param>
    private static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
    {
        string groupName = group.title;
        
        // 创建组对应的文件夹结构
        CreateFolder($"{containerFolderPath}/Groups", groupName);
        CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

        // 创建对话组资产
        DSDialogueGroupSO dialogueGroup =
            CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);
        dialogueGroup.Initialize(groupName);
        
        // 添加到已创建组字典
        createdDialogueGroups.Add(group.ID, dialogueGroup);
        
        // 将组添加到对话容器
        dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

        // 保存资产
        SaveAsset(dialogueGroup);
    }

    /// <summary>
    /// 保存组到图形数据
    /// 将组的基本信息保存到图形保存数据中
    /// </summary>
    /// <param name="group">要保存的组</param>
    /// <param name="graphData">图形保存数据对象</param>
    private static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
    {
        DSGroupSaveData groupData = new DSGroupSaveData()
        {
            ID = group.ID,
            Name = group.title,
            Position = group.GetPosition().position,
        };
        
        graphData.Groups.Add(groupData);
    }

    #endregion

    #region 节点处理

    /// <summary>
    /// 保存所有节点
    /// 将图形视图中的节点保存到图形数据和对话容器中
    /// </summary>
    /// <param name="graphData">图形保存数据对象</param>
    /// <param name="dialogueContainer">对话容器对象</param>
    public static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
    {
        SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
        List<string> ungroupedNodeNames = new List<string>();
        
        foreach (DSNode node in nodes)
        {
            // 保存节点到图形数据和ScriptableObject
            SaveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, dialogueContainer);

            // 记录节点分组信息
            if (node.Group != null)
            {
                groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                continue;
            }
            
            ungroupedNodeNames.Add(node.DialogueName);
        }

        // 更新选项连接关系
        UpdateDialoguesChoicesConnections();

        // 更新旧分组节点列表，清理不再使用的节点
        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
    }

    /// <summary>
    /// 保存节点到图形数据
    /// 将节点的所有信息保存到图形保存数据中
    /// </summary>
    /// <param name="node">要保存的节点</param>
    /// <param name="graphData">图形保存数据对象</param>
    private static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
    {
        // 克隆选项数据（避免引用问题）
        List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);
        
        DSNodeSaveData nodeData = new DSNodeSaveData()
        {
            ID = node.ID,
            Name = node.DialogueName,
            Choices = choices,
            Text = node.Text,
            GroupId = node.Group?.ID,
            DialogueType = node.DialogueType,
            Position = node.GetPosition().position,
        };
        
        graphData.Nodes.Add(nodeData);
    }

    /// <summary>
    /// 保存节点到ScriptableObject
    /// 创建对话资产并设置到对话容器中
    /// </summary>
    /// <param name="node">要保存的节点</param>
    /// <param name="dialogueContainer">对话容器对象</param>
    private static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
    {
        DSDialogueSO dialogue;

        // 根据节点是否分组决定保存路径
        if (node.Group != null)
        {
            dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues",
                node.DialogueName);
            
            dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
        }
        else
        {
            dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
            
            dialogueContainer.UngroupedDialogues.Add(dialogue);
        }
        
        // 初始化对话资产
        dialogue.Initialize(
            node.DialogueName,
            node.Text,
            ConvertNodeChoicesToDialogueChoices(node.Choices),
            node.DialogueType,
            node.IsStartingNode()
            );
        
        createdDialogues.Add(node.ID, dialogue);
        
        SaveAsset(dialogue);
    }

    /// <summary>
    /// 将节点选项数据转换为对话选项数据
    /// 转换过程中丢失NodeId信息，因为ScriptableObject中不保存连接关系
    /// </summary>
    /// <param name="nodeChoices">节点选项数据列表</param>
    /// <returns>对话选项数据列表</returns>
    private static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
    {
        List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

        foreach (DSChoiceSaveData nodeChoice in nodeChoices)
        {
            DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
            {
                Text = nodeChoice.Text
            };
            
            dialogueChoices.Add(choiceData);
        }
        
        return dialogueChoices;
    }

    /// <summary>
    /// 更新对话选项连接
    /// 根据图形视图中的连接关系设置对话资产的NextDialogue引用
    /// </summary>
    private static void UpdateDialoguesChoicesConnections()
    {
        foreach (DSNode node in nodes)
        {
            DSDialogueSO dialogue = createdDialogues[node.ID];

            // 遍历节点的每个选项
            for (int choiceIndex = 0; choiceIndex < node.Choices.Count; choiceIndex++)
            {
                DSChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                // 如果选项没有连接，跳过
                if (string.IsNullOrEmpty(nodeChoice.NodeId))
                {
                    continue;
                }

                // 设置对话选项的下一个对话引用
                dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeId];
                
                SaveAsset(dialogue);
            }
        }
    }
    
    /// <summary>
    /// 更新旧分组节点列表
    /// 比较新旧分组节点列表，删除不再使用的节点资产
    /// </summary>
    /// <param name="groupedNodeNames">当前分组节点列表</param>
    /// <param name="graphData">图形保存数据对象</param>
    private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> groupedNodeNames, DSGraphSaveDataSO graphData)
    {
        if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
        {
            foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
            {
                List<string> nodesToRemove = new List<string>();

                // 如果组仍然存在，找出需要删除的节点
                if (groupedNodeNames.ContainsKey(oldGroupedNode.Key))
                {
                    nodesToRemove = oldGroupedNode.Value.Except(groupedNodeNames[oldGroupedNode.Key]).ToList();
                }

                // 删除不再使用的节点资产
                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                }
            }
        }

        // 更新旧分组节点列表
        graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(groupedNodeNames);
    }
    
    /// <summary>
    /// 更新旧未分组节点列表
    /// 比较新旧未分组节点列表，删除不再使用的节点资产
    /// </summary>
    /// <param name="ungroupedNodeNames">当前未分组节点列表</param>
    /// <param name="graphData">图形保存数据对象</param>
    private static void UpdateOldUngroupedNodes(List<string> ungroupedNodeNames, DSGraphSaveDataSO graphData)
    {
        if (graphData.OldUngroupedNodeNames != null && graphData.OldUngroupedNodeNames.Count != 0)
        {
            List<string> nodesToRemove = graphData.OldUngroupedNodeNames.Except(ungroupedNodeNames).ToList();

            foreach (string nodeToRemove in nodesToRemove)
            {
                RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
            }
        }
        
        // 更新旧未分组节点列表
        graphData.OldUngroupedNodeNames = new List<string>(ungroupedNodeNames);
    }
    
    #endregion

    #region 创建方法

    /// <summary>
    /// 创建静态文件夹结构
    /// 确保对话系统所需的所有文件夹都存在
    /// </summary>
    public static void CreateStaticFolders()
    {
        CreateFolder("Assets/Editor/DialogueSystem", "Graphs");
        
        CreateFolder("Assets", "DialogueSystem");
        CreateFolder("Assets/DialogueSystem", "Dialogues");
        CreateFolder("Assets/DialogueSystem/Dialogues", graphFileName);
        CreateFolder(containerFolderPath, "Global");
        CreateFolder(containerFolderPath, "Groups");
        CreateFolder($"{containerFolderPath}/Global", "Dialogues");
    }
    
    #endregion

    #region 获取元素方法

    /// <summary>
    /// 从图形视图中获取元素
    /// 遍历图形视图中的所有元素，将组和节点分别存储到对应的列表中
    /// </summary>
    private static void GetElementsFromGraphView()
    {
        Type groupType = typeof(DSGroup);
        graphView.graphElements.ForEach(graphElement =>
        {
            if (graphElement is DSNode node)
            {
                nodes.Add(node);
                return;
            }

            if (graphElement.GetType() == groupType)
            {
                DSGroup group = (DSGroup)graphElement;
                groups.Add(group);
                return;
            }
        });
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 创建文件夹
    /// 在指定路径下创建文件夹（如果不存在）
    /// </summary>
    /// <param name="path">父文件夹路径</param>
    /// <param name="folderName">要创建的文件夹名称</param>
    public static void CreateFolder(string path, string folderName)
    {
        if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
        {
            return;
        }
        
        AssetDatabase.CreateFolder(path, folderName);
    }

    /// <summary>
    /// 删除文件夹
    /// 删除指定路径的文件夹及其meta文件
    /// </summary>
    /// <param name="fullPath">要删除的文件夹完整路径</param>
    public static void RemoveFolder(string fullPath)
    {
        FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
        FileUtil.DeleteFileOrDirectory($"{fullPath}/");
    }

    /// <summary>
    /// 创建资产
    /// 在指定路径创建指定类型的ScriptableObject资产，如果已存在则加载
    /// </summary>
    /// <typeparam name="T">资产类型（必须继承自ScriptableObject）</typeparam>
    /// <param name="path">资产保存路径</param>
    /// <param name="assetName">资产名称（不含扩展名）</param>
    /// <returns>创建或加载的资产</returns>
    public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";

        T asset = LoadAsset<T>(path, assetName);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
        }
        
        return asset;
    }

    /// <summary>
    /// 加载资产
    /// 从指定路径加载指定类型的ScriptableObject资产
    /// </summary>
    /// <typeparam name="T">资产类型（必须继承自ScriptableObject）</typeparam>
    /// <param name="path">资产所在路径</param>
    /// <param name="assetName">资产名称（不含扩展名）</param>
    /// <returns>加载的资产，如果不存在则返回null</returns>
    public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
    {
        string fullPath = $"{path}/{assetName}.asset";
        
        return AssetDatabase.LoadAssetAtPath<T>(fullPath);
    }

    /// <summary>
    /// 删除资产
    /// 删除指定路径的资产文件
    /// </summary>
    /// <param name="path">资产所在路径</param>
    /// <param name="assetName">资产名称（不含扩展名）</param>
    public static void RemoveAsset(string path, string assetName)
    {
        AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
    }

    /// <summary>
    /// 保存资产
    /// 标记资产为脏并保存到磁盘
    /// </summary>
    /// <param name="asset">要保存的资产</param>
    public static void SaveAsset(UnityEngine.Object asset)
    {
        EditorUtility.SetDirty(asset);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    /// <summary>
    /// 克隆节点选项数据
    /// 创建节点选项数据的深拷贝，避免保存时引用问题
    /// </summary>
    /// <param name="nodeChoices">要克隆的节点选项数据列表</param>
    /// <returns>克隆后的节点选项数据列表</returns>
    public static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
    {
        List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();

        foreach (DSChoiceSaveData choice in nodeChoices)
        {
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = choice.Text,
                NodeId = choice.NodeId
            };
            
            choices.Add(choiceData);
        }
        
        return choices;
    }

    #endregion
}