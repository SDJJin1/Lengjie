using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话选项数据类
/// 存储对话系统中单个选择选项的文本内容和选择后的下一段对话引用
/// 序列化支持，可在Unity编辑器中配置
/// </summary>
[Serializable]
public class DSDialogueChoiceData
{
    /// <summary>
    /// 选项显示的文本内容
    /// 玩家将在对话界面中看到的可选选项文本
    /// </summary>
    [field:SerializeField] public string Text { get; set; }
    
    /// <summary>
    /// 选择此选项后进入的下一段对话
    /// 如果为null，则选择此选项将结束对话
    /// </summary>
    [field:SerializeField] public DSDialogueSO NextDialogue { get; set; }
}
