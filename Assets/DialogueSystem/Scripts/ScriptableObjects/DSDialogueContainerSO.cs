using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话容器脚本化对象
/// 用于存储和管理整个对话系统中的所有对话数据，包括分组和未分组的对话
/// 继承自ScriptableObject，可在Unity编辑器中创建和配置
/// </summary>
public class DSDialogueContainerSO : ScriptableObject
{
    /// <summary>
    /// 文件名/标识名
    /// 用于标识此对话容器的名称
    /// </summary>
    [field:SerializeField] public string FileName { get; set; }
    
    /// <summary>
    /// 对话组字典
    /// 键：对话组（DSDialogueGroupSO），值：该组内的对话列表
    /// 使用可序列化字典存储分组对话，支持在Unity编辑器中编辑
    /// </summary>
    [field:SerializeField] public SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>> DialogueGroups { get; set; }
    
    /// <summary>
    /// 未分组的对话列表
    /// 存储不属于任何组的独立对话
    /// </summary>
    [field:SerializeField] public List<DSDialogueSO> UngroupedDialogues { get; set; }

    /// <summary>
    /// 初始化对话容器
    /// 设置文件名并初始化数据结构
    /// </summary>
    /// <param name="fileName">对话容器文件名/标识名</param>
    public void Initialize(string fileName)
    {
        FileName = fileName;
        
        DialogueGroups = new SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>>();
        UngroupedDialogues = new List<DSDialogueSO>();
    }

    /// <summary>
    /// 获取所有对话组的名称列表
    /// 返回容器中所有对话组的组名列表
    /// </summary>
    /// <returns>对话组名称列表</returns>
    public List<string> GetDialogueGroupNames()
    {
        List<string> dialogueGroupNames = new List<string>();

        foreach (DSDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
        {
            dialogueGroupNames.Add(dialogueGroup.GroupName);
        }
        
        return dialogueGroupNames;
    }

    /// <summary>
    /// 获取指定对话组内的对话名称列表
    /// 可以选择是否只返回起始对话（IsStartingDialogue为true的对话）
    /// </summary>
    /// <param name="dialogueGroup">目标对话组</param>
    /// <param name="startingDialoguesOnly">是否只返回起始对话</param>
    /// <returns>对话名称列表</returns>
    public List<string> GetGroupedDialogueNames(DSDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
    {
        List<DSDialogueSO> groupedDialogues = DialogueGroups[dialogueGroup];
        
        List<string> groupedDialogueNames = new List<string>();

        foreach (DSDialogueSO groupedDialogue in groupedDialogues)
        {
            // 如果要求只返回起始对话，但当前对话不是起始对话，则跳过
            if (startingDialoguesOnly && !groupedDialogue.IsStartingDialogue)
            {
                continue;
            }
            groupedDialogueNames.Add(groupedDialogue.DialogueName);
        }
        
        return groupedDialogueNames;
    }

    /// <summary>
    /// 获取未分组对话的名称列表
    /// 可以选择是否只返回起始对话（IsStartingDialogue为true的对话）
    /// </summary>
    /// <param name="startingDialoguesOnly">是否只返回起始对话</param>
    /// <returns>未分组对话名称列表</returns>
    public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
    {
        List<string> ungroupedDialogueNames = new List<string>();

        foreach (DSDialogueSO ungroupedDialogue in UngroupedDialogues)
        {
            // 如果要求只返回起始对话，但当前对话不是起始对话，则跳过
            if (startingDialoguesOnly && !ungroupedDialogue.IsStartingDialogue)
            {
                continue;
            }
            ungroupedDialogueNames.Add(ungroupedDialogue.DialogueName);
        }
        
        return ungroupedDialogueNames;
    }
    
    
}
