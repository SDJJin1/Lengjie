using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviourTree.BehaviourTree
{
    /// <summary>
    /// 行为节点状态枚举
    /// 定义行为树中节点可能处于的执行状态
    /// </summary>
    public enum BehaviourState
    {
        未执行,  // 节点尚未开始执行
        成功,    // 节点执行成功
        失败,    // 节点执行失败
        执行中   // 节点正在执行中（用于需要多帧完成的任务）
    }
    
    /// <summary>
    /// 节点类型枚举
    /// 定义行为树中不同类型的节点，用于节点分类和逻辑处理
    /// </summary>
    public enum NodeType
    {
        无,       // 未指定类型
        根节点,   // 行为树的根节点，整个行为树的入口
        组合节点, // 包含多个子节点的节点，用于控制执行流程
        条件节点, // 条件判断节点，用于检查条件并控制子节点执行
        行为节点  // 实际执行行为的节点，如移动、攻击等
    }

    #region 根数据
    
    /// <summary>
    /// 行为树节点基类
    /// 所有行为树节点的抽象基类，定义节点的公共属性和方法
    /// 使用Odin Inspector属性来在Unity编辑器中组织显示
    /// </summary>
    [BoxGroup]  // 在Inspector中显示为可折叠的组
    [HideLabel]  // 隐藏基类的标签
    [HideReferenceObjectPicker]  // 隐藏引用对象选择器
    public abstract class BtNodeBase
    {
        /// <summary>
        /// 节点唯一标识符
        /// 使用GUID确保每个节点的唯一性，用于节点间的引用和查找
        /// </summary>
        [ReadOnly, FoldoutGroup("基础数据"), LabelText("标识")]
        public string Guid;
        
        /// <summary>
        /// 节点在编辑器中的位置
        /// 用于在行为树编辑器中定位节点
        /// </summary>
        [ReadOnly, FoldoutGroup("基础数据"), LabelText("位置")]
        public Vector2 Position;
        
        /// <summary>
        /// 节点名称
        /// 便于在编辑器中识别节点的功能
        /// </summary>
        [FoldoutGroup("基础数据"), LabelText("名称")]
        public string NodeName;
        
        /// <summary>
        /// 节点类型
        /// 标识节点的具体类型，如根节点、组合节点等
        /// </summary>
        [FoldoutGroup("基础数据"), LabelText("类型")]
        public NodeType NodeType;
        
        /// <summary>
        /// 节点当前状态
        /// 记录节点的执行状态，如未执行、成功、失败、执行中
        /// </summary>
        [FoldoutGroup("基础数据"), LabelText("状态")]
        public BehaviourState NodeState;

        /// <summary>
        /// 节点执行方法（抽象方法）
        /// 每个具体节点必须实现此方法，定义节点的具体行为逻辑
        /// 返回值表示节点执行的结果状态
        /// </summary>
        /// <returns>节点的执行状态</returns>
        public abstract BehaviourState Tick();

        /// <summary>
        /// 将节点状态更改为失败
        /// 当节点执行失败时调用，并递归地将子节点状态也设置为失败
        /// 用于确保失败状态的正确传播
        /// </summary>
        protected void ChangeFailState()
        {
            NodeState = BehaviourState.失败;
            
            // 根据节点类型递归处理子节点
            switch (this)
            {
                case BtComposite composite:
                    // 如果是组合节点，将所有子节点状态设为失败
                    composite.ChildNodes.ForEach(node => node.ChangeFailState());
                    break;
                case BtPrecondition precondition:
                    // 如果是条件节点，将子节点状态设为失败
                    precondition.ChildNode?.ChangeFailState();
                    break;
            }
        }
    }

    /// <summary>
    /// 组合节点基类
    /// 继承自BtNodeBase，表示可以包含多个子节点的节点类型
    /// 用于构建行为树的分支结构，如序列节点、选择节点等
    /// </summary>
    public abstract class BtComposite : BtNodeBase
    {
        /// <summary>
        /// 子节点列表
        /// 包含该组合节点下的所有子节点
        /// 使用Odin Inspector属性动态分组显示，分组名为节点名称
        /// </summary>
        [FoldoutGroup("@NodeName"), LabelText("子节点")]
        public List<BtNodeBase> ChildNodes = new List<BtNodeBase>();
    }

    /// <summary>
    /// 条件节点基类
    /// 继承自BtNodeBase，表示包含一个子节点的节点类型
    /// 用于条件判断，根据条件决定是否执行子节点
    /// </summary>
    public abstract class BtPrecondition : BtNodeBase
    {
        /// <summary>
        /// 子节点引用
        /// 条件节点包含的唯一子节点
        /// 使用Odin Inspector属性动态分组显示，分组名为节点名称
        /// </summary>
        [FoldoutGroup("@NodeName"), LabelText("子节点")]
        public BtNodeBase ChildNode;
    }

    /// <summary>
    /// 行为节点基类
    /// 继承自BtNodeBase，表示叶子节点，执行具体的行为
    /// 如移动、攻击、等待等具体动作
    /// </summary>
    public abstract class BtActionNode : BtNodeBase { }
    
    #endregion
}