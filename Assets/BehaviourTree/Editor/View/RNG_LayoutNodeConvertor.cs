using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BehaviourTree.BTAutoLayout;
using BehaviourTree.Editor.View;
using UnityEngine;

/// <summary>
/// 行为树自动布局节点转换器
/// 实现INodeForLayoutConvertor接口，用于在行为树编辑器中进行节点自动布局
/// 将节点视图（NodeView）转换为布局算法使用的TreeNode结构，并将计算结果应用回原节点
/// 主要用于自动排列行为树节点，使其整洁、层次分明
/// </summary>
public class RNG_LayoutNodeConvertor : INodeForLayoutConvertor
{
    /// <summary>
    /// 兄弟节点间的水平间距
    /// 控制同一层级中相邻节点之间的水平距离
    /// </summary>
    public float SiblingDistance => 50;
    
    /// <summary>
    /// 树层级间的垂直间距
    /// 控制不同层级（父子节点之间）的垂直距离
    /// </summary>
    public float TreeDistance => 80;
    
    /// <summary>
    /// 原始根节点引用
    /// 要进行自动布局的根节点视图
    /// </summary>
    public object PrimRootNode => m_PrimRootNode;
    private object m_PrimRootNode;
    
    /// <summary>
    /// 布局根节点
    /// 转换后的布局算法使用的根节点
    /// </summary>
    private NodeAutoLayouter.TreeNode m_LayoutRootNode;
    public NodeAutoLayouter.TreeNode LayoutRootNode => m_LayoutRootNode;
    
    /// <summary>
    /// 初始化转换器
    /// 设置要进行自动布局的原始根节点
    /// </summary>
    /// <param name="primRootNode">原始根节点视图</param>
    /// <returns>初始化后的转换器实例</returns>
    public INodeForLayoutConvertor Init(object primRootNode)
    {
        this.m_PrimRootNode = primRootNode;
        return this;
    }
    
    /// <summary>
    /// 原始节点转换为布局节点
    /// 将NodeView树结构转换为NodeAutoLayouter.TreeNode树结构
    /// 递归处理所有子节点，构建完整的布局树
    /// </summary>
    /// <returns>转换后的布局根节点</returns>
    public NodeAutoLayouter.TreeNode PrimNode2LayoutNode()
    {
        NodeView graphNodeViewBase = m_PrimRootNode as NodeView;
        
        // 创建根布局节点
        m_LayoutRootNode =
            new NodeAutoLayouter.TreeNode(
                150 + SiblingDistance,  // 节点宽度 = 固定宽度150 + 兄弟间距
                120,                   // 节点高度
                graphNodeViewBase.NodeViewData.Position.y,  // 保持原始Y坐标
                NodeAutoLayouter.CalculateMode.Vertical |  // 垂直布局
                NodeAutoLayouter.CalculateMode.Positive    // 正向（从上到下）
            );
        
        // 递归转换子节点
        Convert2LayoutNode(
            graphNodeViewBase,
            m_LayoutRootNode, 
            graphNodeViewBase.NodeViewData.Position.y + 120,  // 下一个节点的Y坐标起始位置
            NodeAutoLayouter.CalculateMode.Vertical | 
            NodeAutoLayouter.CalculateMode.Positive
        );
        
        return m_LayoutRootNode;
    }
    
    /// <summary>
    /// 转换原始节点到布局节点（递归方法）
    /// 遍历节点的所有输出连接，为每个子节点创建布局节点
    /// 并建立父子关系
    /// </summary>
    /// <param name="rootPrimNode">当前原始节点</param>
    /// <param name="rootLayoutNode">当前布局节点</param>
    /// <param name="lastHeightPoint">上个节点的左下角坐标点.y（下一个节点的Y坐标起始位置）</param>
    /// <param name="calculateMode">计算模式</param>
    private void Convert2LayoutNode(
        NodeView rootPrimNode,
        NodeAutoLayouter.TreeNode rootLayoutNode, 
        float lastHeightPoint,
        NodeAutoLayouter.CalculateMode calculateMode)
    {
        // 如果节点没有输出端口（没有子节点），递归终止
        if (rootPrimNode.OutputPort == null) return;
        
        // 如果有子节点连接
        if (rootPrimNode.OutputPort.connected)
        {
            // 遍历所有输出连接
            foreach (var edge in rootPrimNode.OutputPort.connections)
            {
                // 获取子节点
                NodeView childNode = edge.input.node as NodeView;
                
                // 创建子布局节点
                NodeAutoLayouter.TreeNode childLayoutNode =
                    new NodeAutoLayouter.TreeNode(
                        150 + SiblingDistance,  // 节点宽度
                        120,                   // 节点高度
                        lastHeightPoint + SiblingDistance,  // 计算Y坐标
                        calculateMode
                    );
                
                // 添加到父节点
                rootLayoutNode.AddChild(childLayoutNode);
                
                // 递归处理子节点
                Convert2LayoutNode(
                    childNode, 
                    childLayoutNode,
                    lastHeightPoint + SiblingDistance + 120,  // 计算下一个节点的Y坐标
                    calculateMode
                );
            }
        }
    }
    
    /// <summary>
    /// 布局节点转换回原始节点
    /// 将自动布局计算后的位置应用到原始节点
    /// 更新节点的位置和数据结构
    /// </summary>
    public void LayoutNode2PrimNode()
    {
        // 计算根节点的最终位置
        Vector2 calculateRootResult = m_LayoutRootNode.GetPos();
        NodeView root = m_PrimRootNode as NodeView;
        
        // 更新根节点位置
        root.SetPosition(new Rect(calculateRootResult, Vector2.one));
        root.NodeViewData.Position = calculateRootResult;
        
        // 递归更新所有子节点位置
        Convert2PrimNode(m_PrimRootNode as NodeView, m_LayoutRootNode, root.NodeViewData.Position);
    }
    
    /// <summary>
    /// 转换布局节点到原始节点（递归方法）
    /// 遍历所有子节点，将布局计算的位置应用到原始节点
    /// </summary>
    /// <param name="rootPrimNode">当前原始节点</param>
    /// <param name="rootLayoutNode">当前布局节点</param>
    /// <param name="offset">偏移量（用于计算相对位置）</param>
    private void Convert2PrimNode(
        NodeView rootPrimNode,
        NodeAutoLayouter.TreeNode rootLayoutNode, 
        Vector2 offset)
    {
        // 如果节点没有输出端口（没有子节点），递归终止
        if (rootPrimNode.OutputPort == null) return;
        
        // 如果有子节点连接
        if (rootPrimNode.OutputPort.connected)
        {
            // 获取所有子节点视图
            List<NodeView> children = rootPrimNode.OutputPort.connections
                .Select(edge => edge.input.node as NodeView)
                .ToList();
            
            // 遍历所有子布局节点
            for (int i = 0; i < rootLayoutNode.children.Count; i++)
            {
                // 获取子节点的计算位置
                Vector2 calculateResult = rootLayoutNode.children[i].GetPos();
                
                // 更新子节点位置
                children[i].NodeViewData.Position = calculateResult;
                children[i].SetPosition(new Rect(calculateResult, Vector2.one));
                
                // 递归处理子节点
                Convert2PrimNode(children[i], rootLayoutNode.children[i], offset);
            }
        }
    }
}