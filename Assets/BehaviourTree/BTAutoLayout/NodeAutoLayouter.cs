/* 
 * 作者：烟雨迷离半世殇
 * 参考链接：https://www.lfzxb.top/non-layered-tidy-trees-practise/
 * 实现非分层紧凑树布局算法，用于行为树节点自动布局
 */

using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree.BTAutoLayout
{
    /// <summary>
    /// 布局节点转换器接口
    /// 定义原始节点与布局节点之间双向转换的规范
    /// 用于将任意类型的树节点转换为布局算法所需的统一格式
    /// </summary>
    public interface INodeForLayoutConvertor
    {
        /// <summary>
        /// 兄弟节点间的距离
        /// 控制同一层级中相邻节点之间的间距
        /// </summary>
        float SiblingDistance { get; }

        /// <summary>
        /// 原始根节点
        /// 待布局树的原始根节点引用
        /// </summary>
        object PrimRootNode { get; }
        
        /// <summary>
        /// 布局根节点
        /// 转换后的布局算法使用的根节点
        /// </summary>
        NodeAutoLayouter.TreeNode LayoutRootNode { get; }

        /// <summary>
        /// 初始化转换器
        /// 设置原始根节点并准备转换
        /// </summary>
        /// <param name="primRootNode">原始根节点</param>
        /// <returns>初始化后的转换器实例</returns>
        INodeForLayoutConvertor Init(object primRootNode);
        
        /// <summary>
        /// 原始节点转换为布局节点
        /// 将原始树结构转换为布局算法内部使用的树结构
        /// </summary>
        /// <returns>转换后的布局根节点</returns>
        NodeAutoLayouter.TreeNode PrimNode2LayoutNode();
        
        /// <summary>
        /// 布局节点转换回原始节点
        /// 将计算好的布局位置应用到原始节点
        /// </summary>
        void LayoutNode2PrimNode();
    }
    
    /// <summary>
    /// 节点自动布局器
    /// 实现非分层紧凑树布局算法，自动计算树形结构中每个节点的位置
    /// 支持水平和垂直两种布局方向
    /// </summary>
    public class NodeAutoLayouter
    {
        /// <summary>
        /// 计算模式枚举
        /// 定义布局算法的方向和坐标系方向
        /// 使用Flags特性支持多种模式组合
        /// </summary>
        [System.Flags]
        public enum CalculateMode
        {
            // 水平计算模式：节点在水平方向上排列
            Horizontal = 1,

            // 垂直计算模式：节点在垂直方向上排列
            Vertical = Horizontal << 1,

            // 正向计算模式：从上到下，从左到右
            Positive = Horizontal << 2,

            // 反向计算模式：从下到上，从右到左
            Negative = Horizontal << 3
        }

        /// <summary>
        /// 树节点类
        /// 布局算法内部使用的节点数据结构
        /// 包含位置、尺寸、子节点列表以及布局算法所需的各种临时变量
        /// </summary>
        public class TreeNode
        {
            // 尺寸和位置属性
            public float w, h;      // 节点的宽度和高度
            public float x, y;      // 节点的坐标
            public float prelim;    // 初步x坐标（相对于父节点）
            public float mod;       // 修正值，用于调整子树位置
            public float shift;     // 移动值
            public float change;    // 变化值
            
            // 线程引用（用于连接不同子树）
            public TreeNode tl, tr; // 左线程和右线程
            
            // 极端节点引用
            public TreeNode el, er; // 最左节点和最右节点
            
            // 极端节点的修正值总和
            public float msel, mser; // 最左节点和最右节点的修正值总和
            
            // 子节点列表
            public List<TreeNode> children = new List<TreeNode>();
            public int childrenCount => children.Count; // 子节点数量
            
            // 计算模式
            public CalculateMode CalculateMode;

            /// <summary>
            /// 构造函数
            /// 初始化树节点，设置尺寸、位置和计算模式
            /// </summary>
            /// <param name="w">节点宽度</param>
            /// <param name="h">节点高度</param>
            /// <param name="y">节点y坐标</param>
            /// <param name="calculateMode">计算模式，默认为垂直正向</param>
            public TreeNode(float w, float h, float y,
                CalculateMode calculateMode = CalculateMode.Vertical | CalculateMode.Positive)
            {
                this.w = w;
                this.h = h;
                this.y = y;
                this.CalculateMode = calculateMode;
            }

            /// <summary>
            /// 添加子节点
            /// 将节点添加到子节点列表中
            /// </summary>
            /// <param name="child">要添加的子节点</param>
            public void AddChild(TreeNode child)
            {
                this.children.Add(child);
            }

            /// <summary>
            /// 获取实际位置
            /// 根据计算模式转换坐标，返回节点在最终布局中的位置
            /// </summary>
            /// <returns>转换后的位置向量</returns>
            public Vector2 GetPos()
            {
                var calculateResult = new Vector2(x, y);

                // 根据计算模式进行坐标变换
                // 水平反向模式：旋转90度并翻转
                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Horizontal | NodeAutoLayouter.CalculateMode.Negative))
                {
                    Vector2 temp = calculateResult;
                    temp.x = -calculateResult.y - h;  // 交换x和y，考虑高度偏移
                    temp.y = calculateResult.x;
                    calculateResult = temp;
                }

                // 水平正向模式：旋转-90度
                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Horizontal | NodeAutoLayouter.CalculateMode.Positive))
                {
                    Vector2 temp = calculateResult;
                    temp.x = calculateResult.y;
                    temp.y = -calculateResult.x - w;  // 交换x和y，考虑宽度偏移
                    calculateResult = temp;
                }

                // 垂直反向模式：垂直翻转
                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Vertical | NodeAutoLayouter.CalculateMode.Negative))
                {
                    calculateResult.y = -calculateResult.y - h;
                }

                return calculateResult;
            }
        }

        /// <summary>
        /// 执行布局计算
        /// 主布局方法，协调整个布局过程
        /// </summary>
        /// <param name="nodeForLayoutConvertor">节点转换器</param>
        public static void Layout(INodeForLayoutConvertor nodeForLayoutConvertor)
        {
            // 转换为布局节点
            if (nodeForLayoutConvertor.PrimNode2LayoutNode() == null)
            {
                return;
            }

            // 执行布局算法的两次遍历
            firstWalk(nodeForLayoutConvertor.LayoutRootNode);  // 第一次遍历：计算初步位置
            secondWalk(nodeForLayoutConvertor.LayoutRootNode, 0);  // 第二次遍历：计算最终位置
            nodeForLayoutConvertor.LayoutNode2PrimNode();  // 转换回原始节点
        }

        /// <summary>
        /// 第一次遍历（自下而上）
        /// 计算每个节点的prelim（初步x坐标）和mod（修正值）
        /// 同时计算极端节点和线程
        /// </summary>
        /// <param name="t">当前遍历的节点</param>
        static void firstWalk(TreeNode t)
        {
            // 如果是叶子节点，设置极端节点并返回
            if (t.childrenCount == 0)
            {
                setExtremes(t);
                return;
            }

            // 递归遍历第一个子节点
            firstWalk(t.children[0]);
            
            // 创建IYL（索引-最低Y坐标链表），用于跟踪子树的底部边界
            IYL ih = updateIYL(bottom(t.children[0].el), 0, null);
            
            // 遍历剩余子节点
            for (int i = 1; i < t.childrenCount; i++)
            {
                firstWalk(t.children[i]);
                
                // 获取当前子树的最低Y坐标
                float minY = bottom(t.children[i].er);
                
                // 分离当前子树和左侧兄弟子树
                seperate(t, i, ih);
                
                // 更新IYL
                ih = updateIYL(minY, i, ih);
            }

            // 定位根节点在子节点之间的位置
            positionRoot(t);
            
            // 设置极端节点
            setExtremes(t);
        }

        /// <summary>
        /// 设置极端节点
        /// 确定子树的最左和最右节点
        /// </summary>
        /// <param name="t">当前节点</param>
        static void setExtremes(TreeNode t)
        {
            if (t.childrenCount == 0)
            {
                // 叶子节点：自身就是极端节点
                t.el = t;
                t.er = t;
                t.msel = t.mser = 0;
            }
            else
            {
                // 非叶子节点：继承第一个子节点的最左节点，最后一个子节点的最右节点
                t.el = t.children[0].el;
                t.msel = t.children[0].msel;
                t.er = t.children[t.childrenCount - 1].er;
                t.mser = t.children[t.childrenCount - 1].mser;
            }
        }

        /// <summary>
        /// 分离子树
        /// 计算并调整当前子树与左侧兄弟子树之间的间距
        /// 防止子树间重叠
        /// </summary>
        /// <param name="t">父节点</param>
        /// <param name="i">当前子节点的索引</param>
        /// <param name="ih">IYL链表</param>
        static void seperate(TreeNode t, int i, IYL ih)
        {
            // 左侧兄弟子树的最右节点及其修正值总和
            TreeNode sr = t.children[i - 1];
            float mssr = sr.mod;
            
            // 当前子树的最左节点及其修正值总和
            TreeNode cl = t.children[i];
            float mscl = cl.mod;
            
            // 遍历左右轮廓，计算所需间距
            while (sr != null && cl != null)
            {
                // 如果左侧兄弟的底部低于当前IYL的最低Y，则移动到下一个IYL
                if (bottom(sr) > ih.lowY) ih = ih.nxt;

                // 计算重叠距离：左侧兄弟右边缘 - 当前子树左边缘
                float dist = (mssr + sr.prelim + sr.w) - (mscl + cl.prelim);
                
                // 如果重叠，将当前子树向右移动
                if (dist > 0)
                {
                    mscl += dist;
                    moveSubtree(t, i, ih.index, dist);
                }

                float sy = bottom(sr), cy = bottom(cl);
                
                // 沿着轮廓移动：选择较低的轮廓前进
                if (sy <= cy)
                {
                    sr = nextRightContour(sr);
                    if (sr != null) mssr += sr.mod;
                }

                if (sy >= cy)
                {
                    cl = nextLeftContour(cl);
                    if (cl != null) mscl += cl.mod;
                }
            }

            // 设置线程并更新极端节点
            // 情况1：当前子树比左侧兄弟高
            if (sr == null && cl != null) setLeftThread(t, i, cl, mscl);
            // 情况2：左侧兄弟比当前子树高
            else if (sr != null && cl == null) setRightThread(t, i, sr, mssr);
        }

        /// <summary>
        /// 移动子树
        /// 通过修改mod值来移动整个子树
        /// </summary>
        /// <param name="t">父节点</param>
        /// <param name="i">当前子节点索引</param>
        /// <param name="si">起始索引</param>
        /// <param name="dist">移动距离</param>
        static void moveSubtree(TreeNode t, int i, int si, float dist)
        {
            // 通过mod值移动子树
            t.children[i].mod += dist;
            t.children[i].msel += dist;
            t.children[i].mser += dist;
            
            // 分配额外的间距给中间的子节点
            distributeExtra(t, i, si, dist);
        }

        /// <summary>
        /// 获取左轮廓的下一个节点
        /// 沿着左轮廓向下遍历
        /// </summary>
        static TreeNode nextLeftContour(TreeNode t) 
        { 
            return t.childrenCount == 0 ? t.tl : t.children[0]; 
        }
        
        /// <summary>
        /// 获取右轮廓的下一个节点
        /// 沿着右轮廓向下遍历
        /// </summary>
        static TreeNode nextRightContour(TreeNode t) 
        { 
            return t.childrenCount == 0 ? t.tr : t.children[t.childrenCount - 1]; 
        }
        
        /// <summary>
        /// 获取节点的底部Y坐标
        /// 节点底部 = 节点Y坐标 + 节点高度
        /// </summary>
        static float bottom(TreeNode t) 
        { 
            return t.y + t.h; 
        }

        /// <summary>
        /// 设置左线程
        /// 当左侧兄弟子树比当前子树矮时，连接两棵子树
        /// </summary>
        static void setLeftThread(TreeNode t, int i, TreeNode cl, float modsumcl)
        {
            TreeNode li = t.children[0].el;
            li.tl = cl;
            
            // 调整修正值，使线程后的修正值总和正确
            float diff = (modsumcl - cl.mod) - t.children[0].msel;
            li.mod += diff;
            
            // 调整初步x坐标，使节点不移动
            li.prelim -= diff;
            
            // 更新极端节点及其修正值总和
            t.children[0].el = t.children[i].el;
            t.children[0].msel = t.children[i].msel;
        }

        /// <summary>
        /// 设置右线程
        /// 与设置左线程对称，当当前子树比左侧兄弟矮时
        /// </summary>
        static void setRightThread(TreeNode t, int i, TreeNode sr, float modsumsr)
        {
            TreeNode ri = t.children[i].er;
            ri.tr = sr;
            float diff = (modsumsr - sr.mod) - t.children[i].mser;
            ri.mod += diff;
            ri.prelim -= diff;
            t.children[i].er = t.children[i - 1].er;
            t.children[i].mser = t.children[i - 1].mser;
        }

        /// <summary>
        /// 定位根节点
        /// 计算父节点在子节点之间的位置
        /// </summary>
        static void positionRoot(TreeNode t)
        {
            // 根节点位置 = (第一个子节点左边缘 + 最后一个子节点右边缘) / 2 - 根节点宽度/2
            t.prelim = (t.children[0].prelim + t.children[0].mod + 
                        t.children[t.childrenCount - 1].mod +
                        t.children[t.childrenCount - 1].prelim + 
                        t.children[t.childrenCount - 1].w) / 2 - t.w / 2;
        }

        /// <summary>
        /// 第二次遍历（自上而下）
        /// 计算每个节点的最终x坐标
        /// </summary>
        /// <param name="t">当前节点</param>
        /// <param name="modsum">累积修正值</param>
        static void secondWalk(TreeNode t, float modsum)
        {
            modsum += t.mod;
            
            // 计算绝对x坐标
            t.x = t.prelim + modsum;
            
            // 添加子节点间距
            addChildSpacing(t);
            
            // 递归处理子节点
            for (int i = 0; i < t.childrenCount; i++) 
                secondWalk(t.children[i], modsum);
        }

        /// <summary>
        /// 分配额外间距
        /// 在移动子树时，将额外间距分配给中间的子节点
        /// </summary>
        static void distributeExtra(TreeNode t, int i, int si, float dist)
        {
            // 如果有中间子节点
            if (si != i - 1)
            {
                float nr = i - si;
                t.children[si + 1].shift += dist / nr;
                t.children[i].shift -= dist / nr;
                t.children[i].change -= dist - dist / nr;
            }
        }

        /// <summary>
        /// 添加子节点间距
        /// 处理shift和change，将中间间距添加到mod中
        /// </summary>
        static void addChildSpacing(TreeNode t)
        {
            float d = 0, modsumdelta = 0;
            for (int i = 0; i < t.childrenCount; i++)
            {
                d += t.children[i].shift;
                modsumdelta += d + t.children[i].change;
                t.children[i].mod += modsumdelta;
            }
        }

        /// <summary>
        /// 索引-最低Y坐标链表
        /// 用于跟踪每个左兄弟子树的最低Y坐标
        /// </summary>
        class IYL
        {
            public float lowY;    // 当前子树的最低Y坐标
            public int index;     // 子节点索引
            public IYL nxt;       // 下一个IYL节点

            public IYL(float lowY, int index, IYL nxt)
            {
                this.lowY = lowY;
                this.index = index;
                this.nxt = nxt;
            }
        }

        /// <summary>
        /// 更新IYL链表
        /// 移除被新子树遮挡的兄弟节点，并添加新子树
        /// </summary>
        static IYL updateIYL(float minY, int i, IYL ih)
        {
            // 移除被新子树遮挡的兄弟节点
            while (ih != null && minY >= ih.lowY) 
                ih = ih.nxt;
            
            // 在链表头部添加新子树
            return new IYL(minY, i, ih);
        }
    }
}