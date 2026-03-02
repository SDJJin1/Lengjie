using DialogueSystem.Enumerations;
using DialogueSystem.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    /// <summary>
    /// 对话系统多选节点
    /// 继承自DSNode，表示一个支持多个选项的对话节点，玩家可以同时选择多个选项
    /// 用于创建分支丰富的对话，每个选项都连接到一个独立的输出端口
    /// </summary>
    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            // 调用基类初始化方法
            base.Initialize(nodeName, dsGraphView, position);

            // 设置对话类型为多选
            DialogueType = DSDialogueType.MultipleChoice;

            // 创建一个默认选项数据
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "New Choice",
            };
            
            // 添加到选项列表
            Choices.Add(choiceData); 
        }

        /// <summary>
        /// 绘制节点
        /// 创建节点的视觉元素，包括添加选项按钮和所有选项端口
        /// </summary>
        public override void Draw()
        {
            // 调用基类绘制方法
            base.Draw();

            // 创建"添加选项"按钮
            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                // 创建新的选项数据
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice",
                };
                
                // 为选项创建端口
                Port choicePort = CreateChoicePort(choiceData);
            
                // 添加到选项列表
                Choices.Add(choiceData); 
                
                // 将端口添加到输出容器
                outputContainer.Add(choicePort);
            });
            
            // 添加CSS类
            addChoiceButton.AddToClassList("ds-node__button");
            
            // 将按钮插入到主容器的第二个位置（在标题之后）
            mainContainer.Insert(1, addChoiceButton);
            
            // 为每个选项创建端口
            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                
                outputContainer.Add(choicePort);
            }
            
            // 刷新节点的展开状态
            RefreshExpandedState();
        }

        /// <summary>
        /// 创建选项端口
        /// 为每个选项创建一个包含文本输入框和删除按钮的端口
        /// </summary>
        /// <param name="userData">选项数据（DSChoiceSaveData）</param>
        /// <returns>创建的端口</returns>
        private Port CreateChoicePort(object userData)
        {
            // 创建一个输出端口
            Port choicePort = this.CreatePort();
            
            // 将选项数据存储在端口的userData中
            choicePort.userData = userData;

            // 获取选项数据
            DSChoiceSaveData choiceData = (DSChoiceSaveData)userData;

            // 创建删除按钮
            Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
            {
                // 如果只有一个选项，不允许删除
                if (Choices.Count == 1)
                {
                    return;
                }

                // 如果端口已连接，删除连接
                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }
                
                // 从选项列表中移除数据
                Choices.Remove(choiceData);
                
                // 从图中移除端口
                graphView.RemoveElement(choicePort);
            });
            deleteChoiceButton.AddToClassList("ds-node__button");

            // 创建文本输入框用于编辑选项文本
            TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });
            
            // 添加CSS类
            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden"
            );
                
            // 将文本输入框和删除按钮添加到端口
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            
            return choicePort;
        }
    }
}
