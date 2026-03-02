using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 对话系统组错误数据类
/// 用于在对话系统编辑器窗口中存储和显示与组（DSGroup）相关的错误信息
/// 将多个具有相同错误类型的组归类到一起，并共享相同的错误颜色标识
/// </summary>
public class DSGroupErrorData
{
    /// <summary>
    /// 错误数据
    /// 包含错误颜色和基础错误信息
    /// </summary>
    public DSErrorData ErrorData { get; set; }
    
    /// <summary>
    /// 错误组列表
    /// 存储所有具有相同错误的DSGroup引用
    /// </summary>
    public List<DSGroup> Groups { get; set; }

    /// <summary>
    /// 构造函数
    /// 初始化错误数据和组列表
    /// </summary>
    public DSGroupErrorData()
    {
        ErrorData = new DSErrorData();
        Groups = new List<DSGroup>();
    }
}
