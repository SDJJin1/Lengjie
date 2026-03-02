using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 按键重绑定UI组件
/// 用于在设置界面中显示和修改单个输入动作的按键绑定
/// 支持重绑定、重置到默认、显示当前绑定等功能
/// 与InputManager配合工作，管理输入系统的按键重绑定
/// </summary>
public class RebindUI : MonoBehaviour
{
    /// <summary>
    /// 输入动作引用
    /// 关联到Input Action Asset中的具体输入动作
    /// 在Inspector中设置，指定要重绑定的输入动作
    /// </summary>
    [SerializeField]
    private InputActionReference inputActionReference;
    
    /// <summary>
    /// 是否排除鼠标输入
    /// 设置为true时，重绑定时不接受鼠标按键
    /// 设置为false时，可以绑定鼠标按键
    /// </summary>
    [SerializeField]
    private bool excludeMouse = true;
    
    /// <summary>
    /// 选中的绑定索引
    /// 一个输入动作可能有多个绑定（如正向、负向、备用绑定）
    /// 指定要修改的绑定索引
    /// </summary>
    [Range(0, 10)]
    [SerializeField]
    private int selectedBinding;
    
    /// <summary>
    /// 显示字符串选项
    /// 控制绑定名称的显示格式
    /// 如是否显示设备名、绑定名等
    /// </summary>
    [SerializeField]
    private InputBinding.DisplayStringOptions displayStringOptions;
    
    /// <summary>
    /// 输入绑定信息 - 请勿在Inspector中编辑
    /// 存储当前选择的输入绑定信息
    /// 在OnValidate和GetBindingInfo中自动填充
    /// </summary>
    [Header("绑定信息 - 请勿编辑")]
    [SerializeField]
    private InputBinding inputBinding;
    
    /// <summary>
    /// 绑定索引
    /// 内部使用的实际绑定索引
    /// 通常与selectedBinding相同
    /// </summary>
    private int bindingIndex;
    
    /// <summary>
    /// 动作名称
    /// 输入动作的名称，从inputActionReference中获取
    /// 用于标识和查找输入动作
    /// </summary>
    public string actionName;
    
    /// <summary>
    /// 动作文本
    /// 显示输入动作名称的UI文本
    /// 如"移动"、"跳跃"、"攻击"等
    /// </summary>
    [Header("UI元素")]
    [SerializeField]
    private TextMeshProUGUI actionText;
    
    /// <summary>
    /// 重绑定按钮
    /// 点击此按钮启动重绑定流程
    /// </summary>
    [SerializeField]
    private Button rebindButton;
    
    /// <summary>
    /// 重绑定文本
    /// 显示当前绑定按键的文本
    /// 如"W"、"鼠标左键"、"空格"等
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI rebindText;
    
    /// <summary>
    /// 重置按钮
    /// 点击此按钮将绑定重置为默认值
    /// </summary>
    [SerializeField]
    private Button resetButton;
    
    /// <summary>
    /// 启用时初始化
    /// 注册按钮事件，加载绑定设置，更新UI显示
    /// 注册重绑定完成和取消事件
    /// </summary>
    private void OnEnable()
    {
        // 注册重绑定按钮点击事件
        rebindButton.onClick.AddListener(() => DoRebind());
        // 注册重置按钮点击事件
        resetButton.onClick.AddListener(() => ResetBinding());
        
        // 如果输入动作引用存在，加载绑定并更新UI
        if(inputActionReference != null)
        {
            // 从PlayerPrefs加载保存的绑定覆盖
            InputManager.LoadBindingOverride(actionName);
            // 获取绑定信息
            GetBindingInfo();
            // 更新UI显示
            UpdateUI();
        }
        
        // 注册重绑定完成和取消事件
        InputManager.rebindComplete += UpdateUI;
        InputManager.rebindCanceled += UpdateUI;
    }
    
    /// <summary>
    /// 禁用时清理
    /// 移除注册的事件，防止内存泄漏
    /// </summary>
    private void OnDisable()
    {
        // 移除重绑定完成和取消事件
        InputManager.rebindComplete -= UpdateUI;
        InputManager.rebindCanceled -= UpdateUI;
    }
    
    /// <summary>
    /// 验证方法
    /// 在Inspector中更改值或脚本编译时自动调用
    /// 用于在编辑器中实时更新UI显示
    /// </summary>
    private void OnValidate()
    {
        if (inputActionReference == null)
            return; 
        
        // 获取绑定信息
        GetBindingInfo();
        // 更新UI显示
        UpdateUI();
    }
    
    /// <summary>
    /// 获取绑定信息
    /// 从输入动作引用中提取绑定信息
    /// 包括动作名称、绑定详情和绑定索引
    /// </summary>
    private void GetBindingInfo()
    {
        if (inputActionReference.action != null)
            actionName = inputActionReference.action.name;
        
        if(inputActionReference.action.bindings.Count > selectedBinding)
        {
            // 获取选中的绑定
            inputBinding = inputActionReference.action.bindings[selectedBinding];
            bindingIndex = selectedBinding;
        }
    }
    
    /// <summary>
    /// 更新UI显示
    /// 刷新动作文本和绑定文本
    /// 运行时从InputManager获取绑定名称，编辑时从输入动作获取
    /// </summary>
    private void UpdateUI()
    {
        // 更新动作名称文本
        if (actionText != null)
            actionText.text = actionName;
        
        // 更新绑定按键文本
        if(rebindText != null)
        {
            if (Application.isPlaying)
            {
                // 运行时：从InputManager获取当前绑定名称
                rebindText.text = InputManager.GetBindingName(actionName, bindingIndex);
            }
            else
            {
                // 编辑时：直接从输入动作获取绑定显示字符串
                rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
            }
        }
    }
    
    /// <summary>
    /// 执行重绑定
    /// 启动重绑定流程
    /// 调用InputManager的StartRebind方法
    /// 在重绑定过程中，状态文本会更新为"请按下一个按键"
    /// </summary>
    private void DoRebind()
    {
        InputManager.StartRebind(actionName, bindingIndex, rebindText, excludeMouse);
    }
    
    /// <summary>
    /// 重置绑定
    /// 将绑定重置为默认值
    /// 调用InputManager的ResetBinding方法
    /// 重置后更新UI显示
    /// </summary>
    private void ResetBinding()
    {
        InputManager.ResetBinding(actionName, bindingIndex);
        UpdateUI();
    }
}