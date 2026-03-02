using System;
using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Enumerations;
using UnityEngine;

/// <summary>
/// 对话节点保存数据类
/// 用于序列化和保存对话系统编辑器中的节点（Node）数据，包含节点所有必要信息
/// 序列化支持，可在Unity编辑器中配置并用于存档系统
/// </summary>
[Serializable]
public class DSNodeSaveData
{
    /// <summary>
    /// 节点唯一标识符
    /// 用于在对话图中唯一标识一个节点，通常为GUID
    /// </summary>
    [field: SerializeField] public string ID { get; set; }
    
    /// <summary>
    /// 节点名称
    /// 在对话图编辑器中显示的节点名称，用于标识节点功能
    /// </summary>
    [field: SerializeField] public string Name { get; set; }
    
    /// <summary>
    /// 节点文本内容
    /// 该节点显示的对话文本内容
    /// </summary>
    [field: SerializeField] public string Text { get; set; }
    
    /// <summary>
    /// 节点选项列表
    /// 该节点包含的所有对话选项的保存数据
    /// </summary>
    [field: SerializeField] public List<DSChoiceSaveData> Choices { get; set; }
    
    /// <summary>
    /// 所属组ID
    /// 该节点所属的组的唯一标识符，如果为空则表示不属于任何组
    /// </summary>
    [field: SerializeField] public string GroupId { get; set; }
    
    /// <summary>
    /// 对话类型
    /// 定义该节点的对话交互类型（如单选、多选等）
    /// </summary>
    [field: SerializeField] public DSDialogueType DialogueType { get; set; }
    
    /// <summary>
    /// 节点位置
    /// 节点在对话图编辑器中的二维坐标位置
    /// </summary>
    [field: SerializeField] public Vector2 Position { get; set; }
}