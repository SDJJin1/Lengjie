using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    /// <summary>
    /// 对话系统编辑器窗口
    /// 对话图编辑器的主窗口，提供图形化界面用于创建和编辑对话树
    /// 继承自EditorWindow，仅在Unity编辑器中使用
    /// </summary>
    public class DSEditorWindow : EditorWindow
    {
        // 图形视图，用于显示和编辑对话图
        private DSGraphView graphView;
        
        // 默认文件名和文件名输入框
        private readonly string defaultFileName = "DialoguesFileName";
        private static TextField fileNameTextField;
        
        // 工具栏按钮
        private Button saveButton;
        private Button miniMapButton;
        
        /// <summary>
        /// 显示编辑器窗口菜单项
        /// 在Unity菜单栏"DS"下添加"Dialogue Graph"菜单项
        /// </summary>
        [MenuItem("DS/Dialogue Graph")]
        public static void ShowExample()
        {
            // 创建并显示对话图编辑器窗口
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        /// <summary>
        /// 窗口启用时调用
        /// 初始化编辑器窗口的各个组成部分
        /// </summary>
        private void OnEnable()
        {
            // 添加图形视图
            AddGraphView();
            // 添加工具栏
            AddToolbar();
            // 添加样式
            AddStyles();
        }

        #region 元素添加

        /// <summary>
        /// 添加图形视图
        /// 创建对话图视图并使其填满整个窗口
        /// </summary>
        private void AddGraphView()
        {
            graphView = new DSGraphView(this);
            // 使图形视图填满父容器
            graphView.StretchToParentSize();
            
            // 将图形视图添加到根视觉元素
            rootVisualElement.Add(graphView);
        }

        /// <summary>
        /// 添加工具栏
        /// 创建包含文件名输入、保存、加载、清除、重置和迷你地图按钮的工具栏
        /// </summary>
        private void AddToolbar()
        {
            // 创建工具栏
            Toolbar toolbar = new Toolbar();
            
            // 创建文件名输入框
            fileNameTextField = DSElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                // 移除输入中的空白字符和特殊字符，确保文件名有效
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            // 创建保存按钮
            saveButton = DSElementUtility.CreateButton("Save", () => Save());
            // 创建加载按钮
            Button loadButton = DSElementUtility.CreateButton("Load", () => Load());
            // 创建清除按钮
            Button clearButton = DSElementUtility.CreateButton("Clear", () => Clear());
            // 创建重置按钮
            Button resetButton = DSElementUtility.CreateButton("Reset", () => ResetGraph());
            // 创建迷你地图按钮
            miniMapButton = DSElementUtility.CreateButton("MiniMap", () => ToggleMiniMap());
            
            // 将所有控件添加到工具栏
            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            // 为工具栏添加样式表
            toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");
            
            // 将工具栏添加到根视觉元素
            rootVisualElement.Add(toolbar);
        }
        
        /// <summary>
        /// 添加样式
        /// 为编辑器窗口添加对话系统的样式表
        /// </summary>
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }

        #endregion

        #region 工具栏操作

        /// <summary>
        /// 保存对话图
        /// 将当前对话图保存为ScriptableObject资产
        /// </summary>
        private void Save()
        {
            // 检查文件名是否为空
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a file name.", "Ok");
                return;
            }
            
            // 初始化IO工具并执行保存
            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }

        /// <summary>
        /// 加载对话图
        /// 打开文件选择对话框，加载选中的对话图文件
        /// </summary>
        private void Load()
        { 
            // 打开文件选择对话框
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs ", "asset");

            // 如果用户取消选择，则返回
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            
            // 清除当前图形
            Clear();
            
            // 初始化IO工具并执行加载
            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        /// <summary>
        /// 清除当前对话图
        /// 移除图形视图中的所有元素
        /// </summary>
        private void Clear()
        {
            graphView.ClearGraph();
        }

        /// <summary>
        /// 重置对话图
        /// 清除当前对话图并重置文件名为默认值
        /// </summary>
        private void ResetGraph()
        {
            // 清除图形
            Clear();
            // 重置文件名为默认值
            UpdateFileName(defaultFileName);
        }

        /// <summary>
        /// 切换迷你地图显示
        /// 显示或隐藏图形视图的迷你地图，并切换按钮的选中状态
        /// </summary>
        private void ToggleMiniMap()
        {
            // 切换图形视图的迷你地图
            graphView.ToggleMiniMap();
            // 切换迷你地图按钮的选中样式
            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        #endregion
        
        #region 工具方法

        /// <summary>
        /// 更新文件名
        /// 静态方法，用于在加载文件时更新文件名输入框的值
        /// </summary>
        /// <param name="newFileName">新的文件名</param>
        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        /// <summary>
        /// 启用保存按钮
        /// 当图形视图处于可保存状态时调用
        /// </summary>
        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        /// <summary>
        /// 禁用保存按钮
        /// 当图形视图处于不可保存状态时调用
        /// </summary>
        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        
        #endregion
    }
}