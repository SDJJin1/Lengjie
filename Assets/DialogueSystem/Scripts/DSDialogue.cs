using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话数据编辑器组件
/// 在场景中附加此组件，用于在Unity编辑器中选择和配置对话系统中的对话节点
/// 提供了可视化的对话选择界面，支持分组筛选和起始对话过滤
/// </summary>
public class DSDialogue : MonoBehaviour
{
    /// <summary>
    /// 对话容器引用
    /// 包含所有对话数据的根容器，通常引用一个DSDialogueContainerSO资产
    /// </summary>
    [SerializeField] private DSDialogueContainerSO dialogueContainer;
    
    /// <summary>
    /// 选中的对话组引用
    /// 在分组模式下，表示当前选中的对话组
    /// </summary>
    [SerializeField] private DSDialogueGroupSO dialogueGroup;
    
    /// <summary>
    /// 选中的对话节点引用
    /// 最终选择的对话数据，用于游戏中的实际对话显示
    /// </summary>
    [SerializeField] private DSDialogueSO dialogue;

    /// <summary>
    /// 是否显示分组对话
    /// 控制是否按对话组进行筛选，为true时显示分组对话，为false时显示未分组对话
    /// </summary>
    [SerializeField] private bool groupedDialogues;
    
    /// <summary>
    /// 是否仅显示起始对话
    /// 为true时只显示标记为IsStartingDialogue的对话，用于筛选对话树的起始节点
    /// </summary>
    [SerializeField] private bool startingDialoguesOnly;

    /// <summary>
    /// 选中的对话组索引
    /// 在分组模式下，当前选中的对话组在容器中的索引
    /// </summary>
    [SerializeField] private int selectedDialogueGroupIndex;
    
    /// <summary>
    /// 选中的对话索引
    /// 在当前对话组或未分组对话列表中选中的对话索引
    /// </summary>
    [SerializeField] private int selectedDialogueIndex;
}
