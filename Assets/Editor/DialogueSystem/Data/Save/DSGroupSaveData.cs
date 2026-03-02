using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话组保存数据类
/// 用于序列化和保存对话系统编辑器中的组（Group）数据，包括ID、名称和位置
/// 序列化支持，可在Unity编辑器中配置并用于存档系统
/// </summary>
[Serializable]
public class DSGroupSaveData
{
    /// <summary>
    /// 组唯一标识符
    /// 用于在对话图中唯一标识一个组，通常为GUID
    /// </summary>
    [field: SerializeField] public string ID { get; set; }
    
    /// <summary>
    /// 组显示名称
    /// 在对话图编辑器中显示的组名称
    /// </summary>
    [field: SerializeField] public string Name { get; set; }
    
    /// <summary>
    /// 组位置
    /// 组在对话图编辑器中的二维坐标位置
    /// </summary>
    [field: SerializeField] public Vector2 Position { get; set; }
}
