using DialogueSystem.Enumerations;
using DialogueSystem.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Elements
{
    /// <summary>
    /// 对话系统单选节点
    /// 继承自DSNode，表示一个只有一个输出选项的对话节点
    /// 用于创建线性对话流程，玩家只能选择一个选项继续对话
    /// 注意：仅在Unity编辑器中可用
    /// </summary>
    public class DSSingleChoiceNode : DSNode
    {
        /// <summary>
        /// 初始化单选节点
        /// 设置节点名称、图形视图引用、位置和对话类型，并创建一个默认选项
        /// </summary>
        /// <param name="nodeName">节点名称</param>
        /// <param name="dsGraphView">所属的对话图视图</param>
        /// <param name="position">节点初始位置</param>
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            // 调用基类初始化方法
            base.Initialize(nodeName, dsGraphView, position);

            // 设置对话类型为单选
            DialogueType = DSDialogueType.SingleChoice;

            // 创建一个默认选项数据，文本为"Next Dialogue"
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };
            
            // 添加到选项列表（单选节点通常只有一个选项）
            Choices.Add(choiceData);
        }

        /// <summary>
        /// 绘制节点
        /// 创建节点的视觉元素，包括一个输出端口用于连接下一个对话节点
        /// </summary>
        public override void Draw()
        {
            // 调用基类绘制方法
            base.Draw();

            // 为每个选项创建输出端口
            foreach (DSChoiceSaveData choice in Choices)
            {
                // 创建一个输出端口，使用选项文本作为端口标签
                Port choicePort = this.CreatePort(choice.Text);
                
                // 将选项数据存储在端口的userData中
                choicePort.userData = choice;
                
                // 将端口添加到输出容器
                outputContainer.Add(choicePort);
                
                // 刷新节点的展开状态
                RefreshExpandedState();
            }
        }
    }
}
