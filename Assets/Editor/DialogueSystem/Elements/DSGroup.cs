using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 对话系统编辑器组类
/// 继承自GraphView.Group，表示对话图编辑器中的一个可折叠组容器
/// 用于在编辑器中组织和管理多个对话节点，支持错误样式显示、
/// </summary>
public class DSGroup : Group
{
    /// <summary>
    /// 组唯一标识符
    /// 用于在对话图中唯一标识一个组，通常为GUID
    /// </summary>
    public string ID { get; set; }
    
    /// <summary>
    /// 组旧标题
    /// 保存组之前的标题，用于撤销操作或标题冲突检测
    /// </summary>
    public string oldTitle {  get; set; }
    
    // 默认边框颜色和宽度，用于样式重置
    private Color defaultBorderColor;
    private float defaultBorderWidth;

    /// <summary>
    /// 构造函数
    /// 初始化对话组，生成唯一ID，设置标题和位置
    /// </summary>
    /// <param name="groupTitle">组显示标题</param>
    /// <param name="position">组在对话图中的初始位置</param>
    public DSGroup(string groupTitle, Vector2 position)
    {
        // 生成唯一标识符
        ID = Guid.NewGuid().ToString();
        
        // 设置标题
        title = groupTitle;
        oldTitle = groupTitle;
        
        // 设置组在对话图中的位置
        SetPosition(new Rect(position, Vector2.zero));
        
        // 保存默认样式，用于后续重置
        defaultBorderColor = contentContainer.style.borderBottomColor.value;
        defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
    }

    /// <summary>
    /// 设置错误样式
    /// 更改组容器的边框颜色和宽度，用于在编辑器中高亮显示错误状态的组
    /// </summary>
    /// <param name="color">错误高亮颜色</param>
    public void SetErrorSytle(Color color)
    {
        contentContainer.style.borderBottomColor = color;
        contentContainer.style.borderBottomWidth = 2f;
    }

    /// <summary>
    /// 重置样式
    /// 将组容器的边框样式恢复为默认值，移除错误高亮
    /// </summary>
    public void ResetStyle()
    {
        contentContainer.style.borderBottomColor = defaultBorderColor;
        contentContainer.style.borderBottomWidth = defaultBorderWidth;
    }
}
