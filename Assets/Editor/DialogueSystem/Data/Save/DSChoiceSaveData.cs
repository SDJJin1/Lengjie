using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话选项保存数据类
/// 用于序列化和保存对话系统中每个选项的数据，包括选项文本和目标节点ID
/// 序列化支持，可在Unity编辑器中配置并用于存档系统
/// </summary>
[Serializable]
public class DSChoiceSaveData
{
    /// <summary>
    /// 选项显示的文本内容
    /// 玩家在对话中看到的选项文本
    /// </summary>
    [field:SerializeField] public string Text { get; set; }
    
    /// <summary>
    /// 目标节点ID
    /// 选择此选项后将跳转到的对话节点ID，用于重建对话树结构
    /// </summary>
    [field:SerializeField] public string NodeId { get; set; }
}