using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Elements;
using UnityEngine;

/// <summary>
/// 对话系统节点错误数据类
/// 用于在对话系统编辑器窗口中存储和显示与节点（DSNode）相关的错误信息
/// 将多个具有相同错误类型的节点归类到一起，并共享相同的错误颜色标识
/// </summary>
public class DSNodeErrorData
{
    /// <summary>
    /// 错误数据
    /// 包含错误颜色和基础错误信息
    /// </summary>
    public DSErrorData ErrorData { get; set; }
    
    /// <summary>
    /// 错误节点列表
    /// 存储所有具有相同错误的DSNode引用
    /// </summary>
    public List<DSNode> Nodes { get; set; }

    /// <summary>
    /// 构造函数
    /// 初始化错误数据和节点列表
    /// </summary>
    public DSNodeErrorData()
    {
        ErrorData = new DSErrorData();
        Nodes = new List<DSNode>();
    }
}
