using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 输入管理器
/// 处理Unity新输入系统的按键重绑定功能
/// 支持启动、取消、完成重绑定流程，并保存/加载按键设置到PlayerPrefs
/// 通过事件系统通知外部重绑定状态变化
/// 支持复合绑定、排除鼠标输入、重置绑定等功能
/// </summary>
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// 玩家输入动作实例
    /// 静态单例，通过Unity的Input Action Assets生成
    /// 包含所有游戏输入动作的定义
    /// </summary>
    public static PlayerInputActions inputActions;

    // 重绑定事件
    public static event Action rebindComplete;          // 重绑定完成事件
    public static event Action rebindCanceled;         // 重绑定取消事件
    public static event Action<InputAction, int> rebindStarted;  // 重绑定开始事件，传递动作和绑定索引

    /// <summary>
    /// 初始化
    /// 确保PlayerInputActions只被创建一次
    /// 使用单例模式避免重复创建
    /// </summary>
    private void Awake()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();
    }

    /// <summary>
    /// 开始重绑定
    /// 启动指定输入动作的按键重绑定流程
    /// 支持复合绑定和排除鼠标输入
    /// </summary>
    /// <param name="actionName">输入动作名称，对应Input Action Asset中的动作名</param>
    /// <param name="bindingIndex">绑定索引，指定要重绑定的具体绑定位置</param>
    /// <param name="statusText">状态文本，用于显示提示信息</param>
    /// <param name="excludeMouse">是否排除鼠标输入，为true时不接受鼠标按键</param>
    public static void StartRebind(string actionName, int bindingIndex, TextMeshProUGUI statusText, bool excludeMouse)
    {
        // 查找输入动作
        InputAction action = inputActions.asset.FindAction(actionName);
        
        // 检查动作和绑定索引是否有效
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("找不到动作或绑定索引");
            return;
        }

        // 处理复合绑定
        if (action.bindings[bindingIndex].isComposite)
        {
            // 复合绑定：如2D向量（WASD）或按键组合
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isComposite)
                DoRebind(action, bindingIndex, statusText, true, excludeMouse);
        }
        else
        {
            // 普通绑定
            DoRebind(action, bindingIndex, statusText, false, excludeMouse);
        }
    }

    /// <summary>
    /// 执行重绑定
    /// 实际执行交互式重绑定操作
    /// 通过Unity Input System的PerformInteractiveRebinding实现
    /// </summary>
    /// <param name="actionToRebind">要重绑定的输入动作</param>
    /// <param name="bindingIndex">绑定索引</param>
    /// <param name="statusText">状态文本</param>
    /// <param name="allCompositeParts">是否为复合绑定的所有部分</param>
    /// <param name="excludeMouse">是否排除鼠标输入</param>
    private static void DoRebind(InputAction actionToRebind, int bindingIndex, TextMeshProUGUI statusText, bool allCompositeParts, bool excludeMouse)
    {
        if (actionToRebind == null || bindingIndex < 0)
            return;

        // 设置状态文本提示
        statusText.text = $"请按下一个{actionToRebind.expectedControlType}按键";

        // 禁用动作以便重绑定
        actionToRebind.Disable();

        // 创建交互式重绑定操作
        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        // 设置重绑定完成回调
        rebind.OnComplete(operation =>
        {
            // 重新启用动作
            actionToRebind.Enable();
            // 释放操作资源
            operation.Dispose();

            // 如果是复合绑定，递归处理下一个部分
            if (allCompositeParts)
            {
                var nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isComposite)
                    DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
            }

            // 保存绑定覆盖
            SaveBindingOverride(actionToRebind);
            // 触发重绑定完成事件
            rebindComplete?.Invoke();
        });

        // 设置重绑定取消回调
        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();
            rebindCanceled?.Invoke();
        });

        // 设置取消键为Escape
        rebind.WithCancelingThrough("<Keyboard>/escape");

        // 如果需要排除鼠标输入
        if (excludeMouse)
            rebind.WithControlsExcluding("Mouse");

        // 触发重绑定开始事件
        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        // 开始重绑定流程
        rebind.Start();
    }

    /// <summary>
    /// 获取绑定显示名称
    /// 返回指定输入动作和绑定索引的显示名称
    /// 用于在UI中显示当前按键设置
    /// </summary>
    /// <param name="actionName">输入动作名称</param>
    /// <param name="bindingIndex">绑定索引</param>
    /// <returns>绑定的显示名称字符串</returns>
    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        InputAction action = inputActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    /// <summary>
    /// 保存绑定覆盖
    /// 将输入动作的所有绑定覆盖保存到PlayerPrefs
    /// 键名格式：动作映射名+动作名+绑定索引
    /// 值：绑定的覆盖路径
    /// </summary>
    /// <param name="action">要保存的输入动作</param>
    private static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    /// <summary>
    /// 加载绑定覆盖
    /// 从PlayerPrefs加载并应用保存的绑定覆盖
    /// 如果没有保存的覆盖，则不应用
    /// </summary>
    /// <param name="actionName">要加载的输入动作名称</param>
    public static void LoadBindingOverride(string actionName)
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        InputAction action = inputActions.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            string savedBinding = PlayerPrefs.GetString(action.actionMap + action.name + i);
            if (!string.IsNullOrEmpty(savedBinding))
                action.ApplyBindingOverride(i, savedBinding);
        }
    }

    /// <summary>
    /// 重置绑定
    /// 将指定输入动作的绑定重置为默认值
    /// 支持复合绑定和普通绑定
    /// 重置后保存到PlayerPrefs
    /// </summary>
    /// <param name="actionName">输入动作名称</param>
    /// <param name="bindingIndex">绑定索引</param>
    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = inputActions.asset.FindAction(actionName);

        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("找不到动作或绑定索引");
            return;
        }

        // 处理复合绑定
        if (action.bindings[bindingIndex].isComposite)
        {
            // 移除复合绑定的所有部分的覆盖
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
                action.RemoveBindingOverride(i);
        }
        else
        {
            // 移除普通绑定的覆盖
            action.RemoveBindingOverride(bindingIndex);
        }

        // 保存重置后的绑定
        SaveBindingOverride(action);
    }
}