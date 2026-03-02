using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Elements;
using DialogueSystem.Enumerations;
using DialogueSystem.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 对话系统搜索窗口
/// 实现Unity的ISearchWindowProvider接口，用于在对话图编辑器中通过搜索窗口创建节点和组
/// 作为ScriptableObject，可在编辑器中创建和配置
/// </summary>
public class DSSearchWindow : ScriptableObject, ISearchWindowProvider
{
    // 图形视图引用，用于创建新元素
    private DSGraphView graphView;
    // 缩进图标，用于在搜索树中创建视觉缩进效果
    private Texture2D indentationIcon;
    
    /// <summary>
    /// 初始化搜索窗口
    /// 设置图形视图引用并创建透明缩进图标
    /// </summary>
    /// <param name="dsGraphView">目标对话图视图</param>
    public void Initialize(DSGraphView dsGraphView)
    {
        this.graphView = dsGraphView;
        
        // 创建1x1的透明纹理，用于在搜索树中作为缩进占位符
        indentationIcon = new Texture2D(1, 1);
        indentationIcon.SetPixel(0, 0, Color.clear);
        indentationIcon.Apply();
    }
    
    /// <summary>
    /// 创建搜索树
    /// 实现ISearchWindowProvider接口，定义搜索窗口的层级结构和选项
    /// </summary>
    /// <param name="context">搜索窗口上下文，包含鼠标位置等信息</param>
    /// <returns>搜索树条目列表，定义搜索窗口的层级结构</returns>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        // 创建搜索树条目列表
        List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
        {
            // 顶层分组："Create Element"
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            // 第一级分组："Dialogue Node"（对话节点）
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            // 第二级选项："Single Choice"（单选项节点）
            new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
            {
                level = 2,
                userData = DSDialogueType.SingleChoice
            },
            // 第二级选项："Multiple Choice"（多选项节点）
            new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
            {
                level = 2,
                userData = DSDialogueType.MultipleChoice
            },
            // 第一级分组："Dialogue Group"（对话组）
            new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
            // 第二级选项："Single Group"（单个组）
            new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
            {
                level = 2,
                userData = new Group()
            }
        };
        
        return searchTreeEntries;
    }

    /// <summary>
    /// 选择搜索条目回调
    /// 当用户在搜索窗口中选择一个条目时调用，创建对应的图形元素
    /// </summary>
    /// <param name="SearchTreeEntry">用户选择的搜索树条目</param>
    /// <param name="context">搜索窗口上下文</param>
    /// <returns>是否成功处理选择</returns>
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        // 将屏幕鼠标位置转换为图形视图的本地坐标
        Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);
        
        // 根据用户选择的条目类型创建不同的图形元素
        switch (SearchTreeEntry.userData)
        {
            // 创建单选项对话节点
            case DSDialogueType.SingleChoice:
            {
                // 创建单选项节点实例
                DSSingleChoiceNode singleChoiceNode =
                    (DSSingleChoiceNode)graphView.CreateNode("DialogueName", DSDialogueType.SingleChoice, localMousePosition);
                
                // 将节点添加到图形视图
                graphView.AddElement(singleChoiceNode);
                
                return true; // 成功处理
            }
            // 创建多选项对话节点
            case DSDialogueType.MultipleChoice:
            {
                // 创建多选项节点实例
                DSMultipleChoiceNode multipleChoiceNode =
                    (DSMultipleChoiceNode)graphView.CreateNode("DialogueName", DSDialogueType.MultipleChoice, localMousePosition);
                
                // 将节点添加到图形视图
                graphView.AddElement(multipleChoiceNode);
                
                return true; // 成功处理
            }
            // 创建对话组
            case Group _:
            {
                // 在鼠标位置创建对话组
                graphView.CreateGroup("Dialogue Group", localMousePosition);
                
                return true; // 成功处理
            }
            default:
                return false; // 未知类型，未处理
        }
    }
}