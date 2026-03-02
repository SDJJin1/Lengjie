using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话系统图形保存数据脚本化对象
/// 用于序列化和持久化对话图编辑器的状态，包括组、节点及其关联关系
/// 继承自ScriptableObject，可在Unity编辑器中创建和配置，用于存档系统
/// </summary>
public class DSGraphSaveDataSO : ScriptableObject
{
    /// <summary>
    /// 文件名/标识名
    /// 用于标识和加载特定对话图数据的唯一名称
    /// </summary>
    [field:SerializeField] public string FileName { get; set; }
    
    /// <summary>
    /// 组保存数据列表
    /// 存储对话图中所有组（DSGroup）的序列化数据
    /// </summary>
    [field:SerializeField] public List<DSGroupSaveData>  Groups { get; set; }
    
    /// <summary>
    /// 节点保存数据列表
    /// 存储对话图中所有节点（DSNode）的序列化数据
    /// </summary>
    [field:SerializeField] public List<DSNodeSaveData>  Nodes { get; set; }
    
    /// <summary>
    /// 旧组名称列表
    /// 用于版本控制或数据迁移，存储之前版本中的组名称
    /// </summary>
    [field:SerializeField] public List<string> OldGroupNames { get; set; }
    
    /// <summary>
    /// 旧未分组节点名称列表
    /// 用于版本控制或数据迁移，存储之前版本中未分组的节点名称
    /// </summary>
    [field:SerializeField] public List<string> OldUngroupedNodeNames { get; set; }
    
    /// <summary>
    /// 旧分组节点名称字典
    /// 用于版本控制或数据迁移，存储之前版本中分组节点的映射关系
    /// 键：组名称，值：该组下的节点名称列表
    /// </summary>
    [field:SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

    /// <summary>
    /// 初始化保存数据对象
    /// 设置文件名并初始化数据结构
    /// </summary>
    /// <param name="filename">对话图文件名/标识名</param>
    public void Initialize(string filename)
    {
        FileName = filename;
        
        Groups = new List<DSGroupSaveData>();
        Nodes = new List<DSNodeSaveData>();
    }
}