using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviourTree.BehaviourTree
{
    /// <summary>
    /// 序列节点
    /// 按顺序执行子节点，直到所有子节点成功或遇到失败节点
    /// 顺序执行模式：成功则继续下一个，失败则停止并返回失败
    /// </summary>
    public class Sequence : BtComposite
    {
        /// <summary>
        /// 当前执行索引
        /// 记录当前正在执行的子节点在列表中的位置
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("执行index"),FoldoutGroup("@NodeName")]
        public int currnetNode;
        
        /// <summary>
        /// 序列节点执行逻辑
        /// 按顺序执行子节点，当前子节点成功后执行下一个，遇到失败则停止
        /// 当所有子节点成功时，返回成功并重置索引
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 如果没有子节点，直接返回失败
            if (ChildNodes.Count == 0)
            {
                NodeState = BehaviourState.失败;
                return BehaviourState.失败;
            }
            
            // 执行当前索引指向的子节点
            var state = ChildNodes[currnetNode].Tick();
            
            switch (state)
            {
                case BehaviourState.成功:
                    // 子节点成功，移动到下一个节点
                    currnetNode++;
                    
                    // 如果已执行完所有子节点，重置索引并返回成功
                    if (currnetNode >= ChildNodes.Count)
                    {
                        currnetNode = 0;
                        NodeState = BehaviourState.成功;
                        return BehaviourState.成功;
                    }
                    // 还有子节点未执行，继续执行当前节点
                    NodeState = BehaviourState.成功;
                    return BehaviourState.成功;
                    
                default:
                    // 子节点失败或执行中，保持当前状态
                    NodeState = state;
                    return state;
            }
        }
    }
    
    /// <summary>
    /// 选择器节点
    /// 从子节点中选择一个执行，直到找到成功的节点
    /// 顺序选择模式：尝试每个子节点，直到找到成功的，否则返回失败
    /// </summary>
    public class Selector : BtComposite
    {
        /// <summary>
        /// 选择的索引
        /// 记录当前选择的子节点在列表中的位置
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("选择的index"),FoldoutGroup("@NodeName")]
        public int selectIndex;
        
        /// <summary>
        /// 选择器节点执行逻辑
        /// 从当前选择的子节点开始执行，如果失败则尝试其他子节点
        /// 找到一个成功的子节点则返回成功，所有子节点都失败则返回失败
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 如果没有子节点，将状态设为失败并返回
            if (ChildNodes.Count == 0)
            {
                ChangeFailState();
                return NodeState;
            }
            
            // 先尝试当前选择的子节点
            var selectState = ChildNodes[selectIndex].Tick();
            
            switch (selectState)
            {
                case BehaviourState.失败:
                    // 当前选择的子节点失败，重置所有子节点状态
                    ChangeFailState();
                    break;
                default:
                    // 当前选择的子节点成功或执行中，重置选择索引并返回状态
                    selectIndex = 0;
                    NodeState = selectState;
                    return selectState;
            }
            
            // 遍历所有子节点，尝试找到可以成功的节点
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                var state = ChildNodes[i].Tick();
                
                // 跳过失败的节点和已经尝试过的节点
                if (state == BehaviourState.失败 || selectIndex == i) continue;
                
                // 找到成功或执行中的节点，更新选择索引并返回状态
                selectIndex = i;
                NodeState = state;
                return state;
            }
            
            // 所有子节点都失败，返回失败状态
            ChangeFailState();
            return BehaviourState.失败;
        }
    }

    /// <summary>
    /// 并行节点
    /// 同时执行所有子节点，根据子节点的执行结果决定自身状态
    /// 执行策略：任意子节点失败则自身失败，所有子节点成功则自身成功
    /// </summary>
    public class Parallel : BtComposite
    {
        /// <summary>
        /// 并行节点执行逻辑
        /// 同时执行所有子节点，收集执行结果
        /// 如果任一子节点失败，则返回失败；如果所有子节点都成功，则返回成功
        /// 如果有子节点还在执行中，则返回执行中
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 存储所有子节点的执行状态
            List<BehaviourState> starts = new List<BehaviourState>();
            
            // 第一轮：执行所有子节点
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                var start = ChildNodes[i].Tick();
                
                switch (start)
                {
                    case BehaviourState.失败:
                        // 如果有子节点失败，将所有子节点状态设为失败并返回失败
                        ChangeFailState();
                        return NodeState;
                    default:
                        // 记录子节点的状态
                        starts.Add(start);
                        break;
                }
            }
            
            // 第二轮：检查所有子节点的状态
            for (int i = 0; i < starts.Count; i++)
            {
                // 如果有子节点还在执行中，返回执行中状态
                if (starts[i] == BehaviourState.执行中)
                {
                    NodeState = BehaviourState.执行中;
                    return BehaviourState.执行中;
                }
            }
            
            // 所有子节点都成功，返回成功状态
            NodeState = BehaviourState.成功;
            return BehaviourState.成功;
        }
    }
    
    /// <summary>
    /// 重复节点
    /// 重复执行子节点指定的次数，达到停止条件后返回成功
    /// 用于创建循环行为模式
    /// </summary>
    public class Repeat : BtPrecondition
    {
        /// <summary>
        /// 当前循环次数
        /// 记录子节点已经被执行的次数
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("循环次数"),FoldoutGroup("@NodeName")]
        public int LoopNumber;
        
        /// <summary>
        /// 循环停止次数
        /// 子节点需要执行的次数，达到此值后停止循环
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("循环停止数"),FoldoutGroup("@NodeName")]
        public int LoopStop;
        
        /// <summary>
        /// 重复节点执行逻辑
        /// 重复执行子节点，直到达到指定的停止次数
        /// 每次执行子节点，如果子节点失败则返回失败
        /// 达到停止次数后重置循环计数并返回成功
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 执行子节点
            var start = ChildNode.Tick();
            
            // 检查是否达到停止次数
            if (LoopStop <= LoopNumber)
            {
                // 达到停止次数，重置循环计数并返回成功
                LoopNumber = 0;
                NodeState = BehaviourState.成功;
                return BehaviourState.成功;
            }
            
            // 增加循环计数
            LoopNumber++;
            
            // 根据子节点的执行结果设置自身状态
            if (start == BehaviourState.失败)
            {
                // 子节点失败，将所有子节点状态设为失败
                ChangeFailState();
            }
            else
            {
                // 子节点成功或执行中，设置自身为执行中状态
                NodeState = BehaviourState.执行中;
            }
            
            return NodeState;
        }
    }
    
    /// <summary>
    /// 条件节点（So）
    /// 检查条件是否为真，条件满足时执行子节点
    /// 名称"So"可能表示"如果满足条件则执行"的逻辑
    /// </summary>
    public class So : BtPrecondition
    {
        /// <summary>
        /// 执行条件
        /// 一个返回布尔值的委托，用于判断是否应该执行子节点
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("执行条件"),FoldoutGroup("@NodeName")]
        public Func<bool> Condition;
        
        /// <summary>
        /// 条件节点执行逻辑
        /// 检查条件是否为真，如果条件为真则执行子节点
        /// 条件为假或未设置条件则返回失败
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 检查条件是否设置
            if (Condition == null)
            {
                ChangeFailState();
                return BehaviourState.失败;
            }
            
            // 检查条件是否为真
            if (Condition.Invoke())
            {
                // 条件为真，执行子节点并返回子节点的状态
                NodeState = ChildNode.Tick();
                return NodeState;
            }
            
            // 条件为假，返回失败状态
            ChangeFailState();
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// 非条件节点（Not）
    /// 检查条件是否为假，条件不满足时执行子节点
    /// 名称"Not"表示"如果条件不满足则执行"的逻辑
    /// </summary>
    public class Not : BtPrecondition
    {
        /// <summary>
        /// 执行条件
        /// 一个返回布尔值的委托，用于判断是否应该执行子节点
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("执行条件"),FoldoutGroup("@NodeName")]
        public Func<bool> Condition;
        
        /// <summary>
        /// 非条件节点执行逻辑
        /// 检查条件是否为假，如果条件为假则执行子节点
        /// 条件为真或未设置条件则返回失败
        /// </summary>
        /// <returns>节点执行状态：成功、失败或执行中</returns>
        public override BehaviourState Tick()
        {
            // 检查条件是否设置
            if (Condition == null)
            {
                ChangeFailState();
                return BehaviourState.失败;
            }
            
            // 检查条件是否为假
            if (!Condition.Invoke())
            {
                // 条件为假，执行子节点并返回子节点的状态
                NodeState = ChildNode.Tick();
                return NodeState;
            }
            
            // 条件为真，返回失败状态
            ChangeFailState();
            return BehaviourState.失败;
        }
    }

    /// <summary>
    /// 运行节点（Run）
    /// 简单行为节点，立即执行并返回成功
    /// 用于执行不需要多帧完成的任务
    /// </summary>
    public class Run : BtActionNode
    {
        /// <summary>
        /// 运行节点执行逻辑
        /// 调用Running方法执行具体行为，然后返回成功状态
        /// </summary>
        /// <returns>总是返回成功状态</returns>
        public override BehaviourState Tick()
        {
            // 执行具体行为
            Running();
            
            // 设置状态为成功并返回
            NodeState = BehaviourState.成功;
            return BehaviourState.成功;
        }

        /// <summary>
        /// 运行节点的具体行为
        /// 当前实现为输出调试日志
        /// 可以在子类中重写此方法实现具体逻辑
        /// </summary>
        public void Running()
        {
            Debug.Log(NodeName + " 节点执行了！");
        }
    }
    
    /// <summary>
    /// 运行中节点（Running）
    /// 需要多帧完成的行为节点，模拟长时间执行的任务
    /// 通过进度变量控制任务的完成状态
    /// </summary>
    public class Running : BtActionNode
    {
        /// <summary>
        /// 执行进度
        /// 记录任务的完成进度，取值范围0-1
        /// 使用Odin Inspector在折叠组中显示，分组名为节点名称
        /// </summary>
        [LabelText("执行进度"),FoldoutGroup("@NodeName")]
        public float Schedule;
        
        /// <summary>
        /// 运行中节点执行逻辑
        /// 每帧增加进度，直到进度达到0.6，然后重置进度并返回成功
        /// 在进度达到阈值前返回执行中状态
        /// </summary>
        /// <returns>节点执行状态：成功或执行中</returns>
        public override BehaviourState Tick()
        {
            // 检查进度是否达到阈值
            if (Schedule >= 0.6f)
            {
                // 进度达到阈值，重置进度并返回成功
                Schedule = 0;
                Debug.Log(NodeName + " 节点任务完成");
                NodeState = BehaviourState.成功;
                return BehaviourState.成功;
            }
            
            // 增加进度
            Schedule += Time.deltaTime;
            // 调试日志：输出当前进度
            // Debug.Log(NodeName + " 节点进度： " + Schedule * 20 + "%");
            
            // 设置状态为执行中并返回
            NodeState = BehaviourState.执行中;
            return BehaviourState.执行中;
        }
    }
}