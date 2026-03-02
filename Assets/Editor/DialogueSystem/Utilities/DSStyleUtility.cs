using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 对话系统样式工具类
/// 提供用于VisualElement的扩展方法，简化CSS类和样式表的添加过程
/// 仅在Unity编辑器中使用
/// </summary>
public static class DSStyleUtility
{
    /// <summary>
    /// 添加CSS类（扩展方法）
    /// 为VisualElement添加一个或多个CSS类，支持链式调用
    /// </summary>
    /// <param name="element">目标VisualElement</param>
    /// <param name="classNames">要添加的一个或多个CSS类名称</param>
    /// <returns>处理后的VisualElement，支持链式调用</returns>
    public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
    {
        // 遍历所有传入的类名
        foreach (string className in classNames)
        {
            // 将每个类名添加到VisualElement的类列表中
            element.AddToClassList(className);
        }
        
        // 返回VisualElement以支持链式调用
        return element;
    }
    
    /// <summary>
    /// 添加样式表（扩展方法）
    /// 为VisualElement添加一个或多个样式表，支持链式调用
    /// </summary>
    /// <param name="element">目标VisualElement</param>
    /// <param name="styleSheetNames">要添加的一个或多个样式表资源路径</param>
    /// <returns>处理后的VisualElement，支持链式调用</returns>
    public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
    {
        // 遍历所有传入的样式表名称
        foreach (string styleSheetName in styleSheetNames)
        {
            // 使用EditorGUIUtility加载样式表资源
            StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetName);
            
            // 将样式表添加到VisualElement的样式表集合中
            element.styleSheets.Add(styleSheet);
        }
        
        // 返回VisualElement以支持链式调用
        return element;
    }
}