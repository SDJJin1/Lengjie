using System;
using DialogueSystem.Elements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

/// <summary>
/// 对话系统元素工具类
/// 提供创建对话图编辑器UI元素的静态工具方法，简化节点和控件的创建过程
/// 仅在Unity编辑器中使用
/// </summary>
public static class DSElementUtility
{
    /// <summary>
    /// 创建按钮
    /// 创建一个带有指定文本和点击事件的按钮UI元素
    /// </summary>
    /// <param name="text">按钮显示的文本</param>
    /// <param name="onClick">按钮点击时调用的回调函数，可为null</param>
    /// <returns>创建好的Button实例</returns>
    public static Button CreateButton(string text, Action onClick = null)
    {
        Button button = new Button(onClick)
        {
            text = text,
        };
        
        return button;
    }
    
    /// <summary>
    /// 创建折叠区域
    /// 创建一个可折叠/展开的UI容器，用于组织内容
    /// </summary>
    /// <param name="title">折叠区域显示的标题</param>
    /// <param name="collapsed">初始状态是否为折叠，默认为false（展开）</param>
    /// <returns>创建好的Foldout实例</returns>
    public static Foldout CreateFoldout(string title, bool collapsed = false)
    {
        Foldout foldout = new Foldout()
        {
            text = title,
            value = !collapsed
        };
        
        return foldout;
    }

    /// <summary>
    /// 为对话节点创建端口（扩展方法）
    /// 创建一个用于节点连接的端口，可设置端口名称、方向、类型和容量
    /// </summary>
    /// <param name="node">目标对话节点，此方法为DSNode的扩展方法</param>
    /// <param name="portName">端口显示名称，默认为空字符串</param>
    /// <param name="orientation">端口方向（水平或垂直），默认为水平</param>
    /// <param name="direction">端口数据流向（输入或输出），默认为输出</param>
    /// <param name="capacity">端口连接容量（单连接或多连接），默认为单连接</param>
    /// <returns>创建好的Port实例</returns>
    public static Port CreatePort(this DSNode node, string portName = "", Orientation orientation = Orientation.Horizontal,
        Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single)
    {
        // 使用节点实例化端口，数据类型为bool（实际上对话系统使用自定义数据类型，这里使用bool作为占位符）
        Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
        port.portName = portName;
        
        return port;
    }
    
    /// <summary>
    /// 创建单行文本输入框
    /// 创建一个用于输入单行文本的UI控件
    /// </summary>
    /// <param name="value">文本框的初始值，可为null</param>
    /// <param name="label">文本框的标签文本，显示在输入框上方，可为null</param>
    /// <param name="onValueChanged">文本值变化时调用的回调函数，可为null</param>
    /// <returns>创建好的TextField实例</returns>
    public static TextField CreateTextField(string value = null,
        string label = null,
        EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textField = new TextField()
        {
            value = value,
            label = label,
        };

        // 如果提供了值变化回调，注册到文本框
        if (onValueChanged != null)
        {
            textField.RegisterValueChangedCallback(onValueChanged);
        }
        
        return textField;
    }

    /// <summary>
    /// 创建多行文本区域
    /// 创建一个用于输入多行文本的UI控件，继承自TextField但允许多行输入
    /// </summary>
    /// <param name="value">文本区域的初始值，可为null</param>
    /// <param name="label">文本区域的标签文本，可为null</param>
    /// <param name="onValueChanged">文本值变化时调用的回调函数，可为null</param>
    /// <returns>创建好的多行TextField实例</returns>
    public static TextField CreateTextArea(string value = null,
        string label = null,
        EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        // 先创建单行文本框
        TextField textArea = CreateTextField(value, label, onValueChanged);
        // 设置为允许多行输入
        textArea.multiline = true;
        
        return textArea;
    }
}