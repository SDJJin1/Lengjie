using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Enumerations;
using UnityEngine;

/// <summary>
/// 对话脚本化对象
/// 表示对话系统中的单个对话节点，包含对话文本、选项、类型等信息
/// 继承自ScriptableObject，可在Unity编辑器中创建和配置
/// </summary>
public class DSDialogueSO : ScriptableObject
{
    /// <summary>
    /// 对话名称
    /// 用于在编辑器或代码中标识此对话节点的唯一名称
    /// </summary>
    [field:SerializeField] public string DialogueName { get; set; }
    
    /// <summary>
    /// 对话文本内容
    /// 实际显示的对话文本，使用TextArea属性在Inspector中显示多行文本框
    /// </summary>
    [field:SerializeField] [field:TextArea()] public string Text { get; set; }
    
    /// <summary>
    /// 对话选项列表
    /// 玩家在对话中可选择的选项列表，每个选项包含文本和指向的下一个对话
    /// 如果列表为空，则对话无选项（单线程对话）
    /// </summary>
    [field:SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
    
    /// <summary>
    /// 对话类型
    /// 定义对话的交互类型（如单选、多选、回答等），影响对话系统的处理逻辑
    /// </summary>
    [field:SerializeField] public DSDialogueType DialogueType { get; set; }
    
    /// <summary>
    /// 是否为起始对话
    /// 标识此对话是否为对话树的起始节点，用于对话系统的入口判断
    /// </summary>
    [field:SerializeField] public bool IsStartingDialogue { get; set; }

    /// <summary>
    /// 初始化对话节点
    /// 设置对话的所有基本属性
    /// </summary>
    /// <param name="dialogueName">对话名称</param>
    /// <param name="text">对话文本内容</param>
    /// <param name="choices">对话选项列表</param>
    /// <param name="dialogueType">对话类型</param>
    /// <param name="isStartingDialogue">是否为起始对话</param>
    public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType, bool isStartingDialogue)
    {
        DialogueName = dialogueName;
        Text = text;
        Choices = choices;
        DialogueType = dialogueType;
        IsStartingDialogue = isStartingDialogue;
    }
}
