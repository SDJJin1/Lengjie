using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSystem.Enumerations;
using DialogueSystem.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    /// <summary>
    /// 对话系统节点基类
    /// 继承自Unity GraphView的Node类，表示对话图编辑器中的一个对话节点
    /// 提供了对话节点的基础功能，包括ID管理、文本编辑、端口连接和错误样式显示
    /// </summary>   
    public class DSNode : Node
    {
        /// <summary>
        /// 节点唯一标识符
        /// 用于在对话图中唯一标识一个节点，通常为GUID
        /// </summary>
        public string ID { get; set; }
        
        /// <summary>
        /// 对话节点名称
        /// 用于标识节点的功能或内容，在编辑器中显示为节点标题
        /// </summary>
        public string DialogueName { get; set; }
        
        /// <summary>
        /// 节点选项列表
        /// 存储该节点所有对话选项的保存数据，每个选项包含文本和目标节点信息
        /// </summary>
        public List<DSChoiceSaveData> Choices { get; set; }
        
        /// <summary>
        /// 节点对话文本
        /// 节点中显示的主要对话内容
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// 对话类型
        /// 定义节点的对话交互类型（如单选、多选等）
        /// </summary>
        public DSDialogueType DialogueType { get; set; }
        
        /// <summary>
        /// 所属组引用
        /// 节点所属的组（DSGroup），如果为null则表示节点不属于任何组
        /// </summary>
        public DSGroup Group { get; set; }

        /// <summary>
        /// 图形视图引用
        /// 节点所属的对话图视图，用于节点管理和图形操作
        /// </summary>
        protected DSGraphView graphView;
        
        /// <summary>
        /// 默认背景颜色
        /// 用于节点样式重置时恢复的颜色值
        /// </summary>
        private Color defaultBackgroundColor;

        /// <summary>
        /// 初始化节点
        /// 设置节点基础属性，包括ID、名称、文本、位置和样式
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="dsGraphView">所属的对话图视图</param>
        /// <param name="position">节点初始位置</param>
        public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            // 生成唯一标识符
            ID = Guid.NewGuid().ToString();
            
            // 设置节点名称和初始文本
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue text.";
            
            // 保存图形视图引用
            graphView = dsGraphView;

            // 设置默认背景颜色（深灰色）
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
             
            // 设置节点位置
            SetPosition(new Rect(position, Vector2.zero));
             
            // 添加CSS类
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        #region 重写方法

        /// <summary>
        /// 构建上下文菜单
        /// 重写方法，在右键点击节点时显示自定义上下文菜单
        /// </summary>
        /// <param name="evt">上下文菜单事件</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 添加自定义菜单项
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());
             
            // 调用基类方法
            base.BuildContextualMenu(evt);
        }

        #endregion
         
        /// <summary>
        /// 绘制节点
        /// 创建节点的视觉元素，包括名称输入框、端口和文本编辑区域
        /// </summary>
        public virtual void Draw()
        {
            // 创建对话名称输入框
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null,callback =>
            {
                TextField target = (TextField)callback.target;

                // 移除空白字符和特殊字符，确保名称规范
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                // 处理重复名称检测
                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        graphView.RepeatedNamesAmount++;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        graphView.RepeatedNamesAmount--;
                    }
                }
                 
                // 更新节点名称并重新分组
                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;
                 
                    graphView.AddUngroupedNode(this);

                    return;
                }
                 
                DSGroup currentGroup = Group;
                 
                graphView.RemoveGroupedNode(this, Group);
                 
                DialogueName = callback.newValue;
                 
                graphView.AddGroupedNode(this, currentGroup);
            });

            // 添加CSS类
            dialogueNameTextField.AddClasses(
                 "ds-node__textfield",
                 "ds-node__filename-textfield",
                 "ds-node__textfield__hidden"
             );
             
            // 将名称输入框插入标题容器
            titleContainer.Insert(0, dialogueNameTextField);

            // 创建输入端口
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input,
                 Port.Capacity.Multi);
             
            inputContainer.Add(inputPort);
             
            // 创建自定义数据容器
            VisualElement customDataContainer = new VisualElement();
             
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            // 创建折叠区域用于编辑对话文本
            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            // 创建文本输入框
            TextField textTextField = DSElementUtility.CreateTextField(Text, null, callback =>
            {
                Text = callback.newValue;    
            });
             
            textTextField.AddClasses(
                 "ds-node__textfield",
                 "ds-node__quote-textfield"
             );
             
            textFoldout.Add(textTextField);
             
            customDataContainer.Add(textFoldout);
             
            extensionContainer.Add(customDataContainer);
        }

        #region 工具方法

        /// <summary>
        /// 断开所有端口连接
        /// 移除节点所有输入和输出端口的连接
        /// </summary>
        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }

        /// <summary>
        /// 断开输入端口连接
        /// 移除节点所有输入端口的连接
        /// </summary>
        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        /// <summary>
        /// 断开输出端口连接
        /// 移除节点所有输出端口的连接
        /// </summary>
        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        /// <summary>
        /// 断开指定容器的端口连接
        /// 移除指定容器中所有端口的连接
        /// </summary>
        /// <param name="container">包含端口的容器（inputContainer或outputContainer）</param>
        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }
                 
                graphView.DeleteElements(port.connections);
            }
        }

        /// <summary>
        /// 检查是否为起始节点
        /// 通过检查输入端口是否有连接来判断节点是否为对话树的起始节点
        /// </summary>
        /// <returns>如果是起始节点返回true，否则返回false</returns>
        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        /// <summary>
        /// 设置错误样式
        /// 更改节点背景颜色以高亮显示错误状态
        /// </summary>
        /// <param name="color">错误高亮颜色</param>
        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        /// <summary>
        /// 重置样式
        /// 将节点背景颜色恢复为默认值
        /// </summary>
        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

        #endregion
    }
}
