using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 对话系统检查器工具类
/// 提供一系列静态方法用于简化Inspector面板的UI绘制
/// 仅在Unity编辑器中使用，通常放在Editor文件夹中
/// </summary>
public static class DSInspectorUtility
{
    /// <summary>
    /// 绘制禁用字段组
    /// 在一个禁用的GUI组中执行指定的绘制操作，使字段显示为不可编辑状态
    /// 常用于显示只读信息
    /// </summary>
    /// <param name="action">要在禁用组中执行的绘制操作</param>
    public static void DrawDisabledFields(Action action)
    {
        // 开始禁用组
        EditorGUI.BeginDisabledGroup(true);
        
        // 执行传入的绘制操作
        action.Invoke();
        
        // 结束禁用组
        EditorGUI.EndDisabledGroup();
    }
    
    /// <summary>
    /// 绘制标题
    /// 使用加粗样式绘制一个标签，通常用于分组或节标题
    /// </summary>
    /// <param name="label">标题文本</param>
    public static void DrawHeader(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
    }

    /// <summary>
    /// 绘制帮助框
    /// 显示一个带有信息、警告或错误图标的帮助框，用于向用户提供提示或错误信息
    /// </summary>
    /// <param name="message">帮助框显示的文本信息</param>
    /// <param name="messageType">消息类型（信息、警告、错误等），默认为Info</param>
    /// <param name="wide">是否使用宽模式，默认为true</param>
    public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info, bool wide = true)
    {
        EditorGUILayout.HelpBox(message, messageType, wide);
    }

    /// <summary>
    /// 绘制属性字段（扩展方法）
    /// 为SerializedProperty提供便捷的绘制方法，自动使用PropertyField绘制
    /// </summary>
    /// <param name="serializedProperty">要绘制的序列化属性</param>
    public static void DrawPropertyField(this SerializedProperty serializedProperty)
    {
        EditorGUILayout.PropertyField(serializedProperty);
    }

    /// <summary>
    /// 绘制下拉菜单（重载1：通过序列化属性）
    /// 绘制一个下拉菜单，并将选中的索引保存到指定的序列化属性中
    /// </summary>
    /// <param name="label">下拉菜单的标签文本</param>
    /// <param name="selectedIndexProperty">存储选中索引的序列化属性</param>
    /// <param name="options">下拉菜单的选项数组</param>
    /// <returns>用户选择的索引（与selectedIndexProperty.intValue相同）</returns>
    public static int DrawPopup(string label, SerializedProperty selectedIndexProperty, string[] options)
    {
        return EditorGUILayout.Popup(label, selectedIndexProperty.intValue, options);
    }
    
    /// <summary>
    /// 绘制下拉菜单（重载2：通过整型索引）
    /// 绘制一个下拉菜单，并返回用户选择的索引
    /// </summary>
    /// <param name="label">下拉菜单的标签文本</param>
    /// <param name="selectedIndex">当前选中的索引</param>
    /// <param name="options">下拉菜单的选项数组</param>
    /// <returns>用户选择的新索引</returns>
    public static int DrawPopup(string label, int selectedIndex, string[] options)
    {
        return EditorGUILayout.Popup(label, selectedIndex, options);
    }

    /// <summary>
    /// 绘制间距
    /// 在GUI布局中添加垂直间距，用于分隔UI元素
    /// </summary>
    /// <param name="amount">间距大小（以像素为单位），默认为4</param>
    public static void DrawSpace(int amount = 4)
    {
        EditorGUILayout.Space(amount);
    }
}