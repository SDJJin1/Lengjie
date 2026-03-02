using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 对话系统编辑器检查器
/// 为DSDialogue组件提供自定义的Inspector界面，实现对话选择功能的可视化编辑
/// 继承自UnityEditor.Editor，仅在Unity编辑器中使用
/// </summary>
[CustomEditor(typeof(DSDialogue))]
public class DSInspector : UnityEditor.Editor
{
    // 序列化属性引用
    private SerializedProperty dialogueContainerProperty;
    private SerializedProperty dialogueGroupProperty;
    private SerializedProperty dialogueProperty;
    
    private SerializedProperty groupedDialoguesProperty;
    private SerializedProperty startingDialoguesOnlyProperty;
    
    private SerializedProperty selectedDialogueGroupIndexProperty;
    private SerializedProperty selectedDialogueIndexProperty;

    /// <summary>
    /// 启用时获取序列化属性引用
    /// 在Inspector激活时调用，用于初始化所有需要序列化的属性引用
    /// </summary>
    private void OnEnable()
    {
        dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
        dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
        dialogueProperty = serializedObject.FindProperty("dialogue");
        
        groupedDialoguesProperty = serializedObject.FindProperty("groupedDialogues");
        startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");
        
        selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
        selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
    }

    /// <summary>
    /// 绘制自定义Inspector界面
    /// 覆盖默认的Inspector绘制，提供对话选择功能的可视化编辑
    /// </summary>
    public override void OnInspectorGUI()
    {
        // 开始序列化对象的更新
        serializedObject.Update();
        
        // 绘制对话容器选择区域
        DrawDialogueContainerArea();
        
        // 获取当前选择的对话容器
        DSDialogueContainerSO dialogueContainer = (DSDialogueContainerSO)dialogueContainerProperty.objectReferenceValue;

        // 如果未选择对话容器，显示提示并停止绘制
        if (dialogueContainer == null)
        {
            StopDrawing("Select a Dialogue Container to see the rest of the Inspector");
            
            return;
        }

        // 绘制过滤器区域（分组、起始对话过滤）
        DrawFiltersArea();
        
        // 获取当前起始对话过滤设置
        bool currentStartingDialoguesOnlyFilter = startingDialoguesOnlyProperty.boolValue;

        List<string> dialogueNames; // 将要显示的对话名称列表
        string dialogueFolderPath;  // 对话资源的文件夹路径
        string dialogueInfoMessage; // 无对话时的提示信息

        // 构建资源路径和信息提示
        string baseDialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}";

        // 根据是否分组进行处理
        if (groupedDialoguesProperty.boolValue)
        {
            // 获取所有对话组名称
            List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

            // 如果没有对话组，显示提示并停止绘制
            if (dialogueGroupNames.Count == 0)
            {
                StopDrawing("There are no Dialogue Groups in this Dialogue Container.");
                
                return;
            }
            
            // 绘制对话组选择区域
            DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);

            // 获取当前选择的对话组
            DSDialogueGroupSO dialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;

            // 获取该组内的对话名称列表
            dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnlyFilter);
            
            // 构建分组对话的文件夹路径
            dialogueFolderPath = $"{baseDialogueFolderPath}/Groups/{dialogueGroup.GroupName}/Dialogues";
            
            // 设置分组对话的提示信息
            dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Dialogues in this Dialogue Group.";
        }
        else
        {
            // 获取未分组对话名称列表
            dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);

            // 构建未分组对话的文件夹路径
            dialogueFolderPath = $"{baseDialogueFolderPath}/Global/Dialogues";
            
            // 设置未分组对话的提示信息
            dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Ungrouped Dialogues in this Dialogue Container.";
        }

        // 如果没有对话，显示提示并停止绘制
        if (dialogueNames.Count == 0)
        {
            StopDrawing(dialogueInfoMessage);
            
            return;
        }
        
        // 绘制对话选择区域
        DrawDialogueArea(dialogueNames, dialogueFolderPath);
        
        // 应用对序列化对象的修改
        serializedObject.ApplyModifiedProperties();
    }

    #region 绘制方法

    /// <summary>
    /// 绘制对话容器区域
    /// 显示对话容器选择字段
    /// </summary>
    private void DrawDialogueContainerArea()
    {
        DSInspectorUtility.DrawHeader("Dialogue Container");

        dialogueContainerProperty.DrawPropertyField();
        
       DSInspectorUtility.DrawSpace();
    }

    /// <summary>
    /// 绘制过滤器区域
    /// 显示分组和起始对话过滤选项
    /// </summary>
    private void DrawFiltersArea()
    {
        DSInspectorUtility.DrawHeader("Filters");
        
        groupedDialoguesProperty.DrawPropertyField();
        startingDialoguesOnlyProperty.DrawPropertyField();
        
        DSInspectorUtility.DrawSpace();
    }

    /// <summary>
    /// 绘制对话组选择区域
    /// 显示对话组下拉菜单，并根据选择更新对话组引用
    /// </summary>
    /// <param name="dialogueContainer">当前对话容器</param>
    /// <param name="dialogueGroupNames">对话组名称列表</param>
    private void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
    {
        DSInspectorUtility.DrawHeader("Dialogue Group");

        // 保存旧的选中索引和对话组
        int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;
        
        DSDialogueGroupSO oldDialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;

        bool isOldDialogueGroupNull = oldDialogueGroup == null;
        
        string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

        // 更新索引，确保在名称列表变化时保持正确选择
        UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

        // 绘制对话组下拉菜单
        selectedDialogueGroupIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group",
            selectedDialogueGroupIndexProperty, dialogueGroupNames.ToArray());
        
        // 获取选中的对话组名称
        string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];

        // 加载选中的对话组资源
        DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>(
            $"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}",
            selectedDialogueGroupName);
        
        // 更新对话组引用
        dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;
        
        // 以禁用形式显示对话组字段（只读）
        DSInspectorUtility.DrawDisabledFields(() => dialogueGroupProperty.DrawPropertyField());
        
        DSInspectorUtility.DrawSpace();
    }

    /// <summary>
    /// 绘制对话选择区域
    /// 显示对话下拉菜单，并根据选择更新对话引用
    /// </summary>
    /// <param name="dialogueNames">对话名称列表</param>
    /// <param name="dialogueFolderPath">对话资源文件夹路径</param>
    private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
    {
        DSInspectorUtility.DrawHeader("Dialogue");
        
        // 保存旧的选中索引和对话
        int oldSelectedDialogueIndex = selectedDialogueIndexProperty.intValue;
        
        DSDialogueSO oldDialogue = (DSDialogueSO)dialogueProperty.objectReferenceValue;
        
        bool isOldDialogueNull = oldDialogue == null;
        
        string  oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;
        
        // 更新索引，确保在名称列表变化时保持正确选择
        UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

        // 绘制对话下拉菜单
        selectedDialogueIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue",
            selectedDialogueIndexProperty, dialogueNames.ToArray());
        
        // 获取选中的对话名称
        string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];
        
        // 加载选中的对话资源
        DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);
        
        // 更新对话引用
        dialogueProperty.objectReferenceValue = selectedDialogue;
        
        // 以禁用形式显示对话字段（只读）
        DSInspectorUtility.DrawDisabledFields(() => dialogueProperty.DrawPropertyField());
    }

    /// <summary>
    /// 停止绘制并提供原因
    /// 在无法继续绘制时显示帮助框和提示信息
    /// </summary>
    /// <param name="reason">停止绘制的原因</param>
    /// <param name="messageType">消息类型（信息、警告、错误等）</param>
    private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
    {
        // 显示原因帮助框
        DSInspectorUtility.DrawHelpBox(reason, messageType);
        
        DSInspectorUtility.DrawSpace();

        // 显示运行时警告
        DSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly at Runtime!",
            MessageType.Warning);
            
        // 应用对序列化对象的修改
        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region 索引处理方法

    /// <summary>
    /// 在名称列表更新时更新索引
    /// 确保在列表变化时保持正确的选中项
    /// </summary>
    /// <param name="optionNames">选项名称列表</param>
    /// <param name="indexProperty">索引的序列化属性</param>
    /// <param name="oldSelectedPropertyIndex">旧的选中索引</param>
    /// <param name="oldPropertyName">旧的属性名称</param>
    /// <param name="isOldPropertyNull">旧的属性是否为null</param>
    private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
    {
        // 如果旧的属性为null，重置索引为0
        if (isOldPropertyNull)
        {
            indexProperty.intValue = 0;
            
            return;
        }
        
        // 检查旧索引是否超出列表范围
        bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
        // 检查旧名称是否与当前选中名称不同
        bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

        // 如果名称不同，尝试查找旧名称在新列表中的位置
        if (oldNameIsDifferentThanSelectedName)
        {
            if (optionNames.Contains(oldPropertyName))
            {
                // 如果旧名称仍在列表中，更新索引为其位置
                indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
            }
            else
            {
                // 如果旧名称不在列表中，重置索引为0
                indexProperty.intValue = 0;
            }
        }
    }

    #endregion
}