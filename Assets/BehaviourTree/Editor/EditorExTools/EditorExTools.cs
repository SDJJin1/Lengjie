using BehaviourTree.Editor.View;
using UnityEditor.Experimental.GraphView;

namespace BehaviourTree.Editor.EditorExTools
{
    /// <summary>
    /// 编辑器扩展工具类
    /// 提供针对行为树编辑器图视图的扩展方法，简化节点连接、数据同步和视图操作
    /// 主要用于处理节点之间的连接关系和视图-模型数据同步
    /// </summary>
    public static class EditorExTools
    {
        /// <summary>
        /// 连接时添加数据（扩展方法）
        /// 当两个节点之间创建连接边时调用，更新节点视图的父子关系和数据模型
        /// </summary>
        /// <param name="edge">要处理的连接边，通常为EdgeView实例</param>
        public static void AddNodeData(this Edge edge)
        {
            // 获取连接边的输出节点和输入节点视图
            NodeView outNodeView = edge.output.node as NodeView;
            NodeView inNodeView = edge.input.node as NodeView;
            
            // 如果输出节点视图存在，将输入节点添加为其子节点
            outNodeView?.AddChild(inNodeView);
        }
    
        /// <summary>
        /// 删除连接时清除数据（扩展方法）
        /// 当两个节点之间的连接边被删除时调用，移除节点视图的父子关系
        /// </summary>
        /// <param name="edge">要处理的连接边，通常为EdgeView实例</param>
        public static void RemoveLink(this Edge edge)
        {
            // 获取连接边的输出节点和输入节点视图
            NodeView outNodeView = edge.output.node as NodeView;
            NodeView inNodeView = edge.input.node as NodeView;
            
            // 如果输出节点视图存在，从其中移除输入节点
            outNodeView?.RemoveChild(inNodeView);
        }
        
        /// <summary>
        /// 两点相连 output <==> input（扩展方法）
        /// 在两个端口之间创建连接，包括视图连接和数据同步
        /// 输出端口连接到输入端口，同时更新行为树数据结构
        /// </summary>
        /// <param name="outputSocket">输出端口，通常为父节点的输出端口</param>
        /// <param name="inputSocket">输入端口，通常为子节点的输入端口</param>
        public static void LinkPort(this Port outputSocket, Port inputSocket)
        {
            // 创建新的边视图，用于可视化连接
            var tempEdge = new EdgeView()
            {
                output = outputSocket,
                input = inputSocket
            };
            
            // 连接输入端口到边
            tempEdge?.input.Connect(tempEdge);
            // 连接输出端口到边
            tempEdge?.output.Connect(tempEdge);
            
            // 将边添加到行为树图视图中
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.Add(tempEdge);
        }
    }
}