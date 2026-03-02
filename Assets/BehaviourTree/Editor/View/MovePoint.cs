using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 移动点元素
    /// 用于在行为树编辑器连接线上显示移动的动画点，增强数据流动的可视化效果
    /// 继承自VisualElement，通过UxmlFactory支持在UXML模板中实例化
    /// </summary>
    public class MovePoint : VisualElement
    {
        /// <summary>
        /// UXML工厂类
        /// 使MovePoint可以通过UXML模板创建，继承自UxmlFactory
        /// 使用默认的UxmlTraits，无需自定义特性
        /// </summary>
        public new class UxmlFactory : UxmlFactory<MovePoint, UxmlTraits>{}

        /// <summary>
        /// 边控制器引用
        /// 用于获取连接线的控制点位置，控制点在贝塞尔曲线上定义
        /// 控制点[1]是起点，控制点[2]是终点
        /// </summary>
        private EdgeControl edgeC;
        
        /// <summary>
        /// 构造函数
        /// 初始化移动点，创建图标并设置偏移位置
        /// 图标偏移-10像素，使其中心对齐路径
        /// </summary>
        public MovePoint()
        {
            // 创建图标图片元素
            Image icon = new Image(){name = "MovePoint" };
            this.Add(icon);
            // 调整图标位置，使其居中
            icon.transform.position -= new Vector3(10, 10, 0);
        }

        /// <summary>
        /// 开始移动
        /// 设置边控制器引用并启动移动动画
        /// </summary>
        /// <param name="edgeC">边控制器，包含连接线的控制点信息</param>
        public void ToMove(EdgeControl edgeC)
        {
            this.edgeC = edgeC;
            MoveStep();  // 启动移动动画
        }

        /// <summary>
        /// 移动步骤动画
        /// 异步方法，在100帧内从起点移动到终点
        /// 使用线性插值平滑移动，每帧等待10毫秒
        /// 动画完成后自动从父元素中移除
        /// </summary>
        async Task MoveStep()
        {
            for (int i = 0; i < 100; i++)
            {
                Debug.Log("开始");
                Debug.Log($"{edgeC.controlPoints[1]}  {edgeC.controlPoints[2]}");
                
                // 计算当前位置：在起点和终点之间线性插值
                this.transform.position = Vector2.Lerp(
                    edgeC.controlPoints[1],  // 起点控制点
                    edgeC.controlPoints[2],  // 终点控制点
                    i / 100f  // 动画进度（0到1之间）
                );
                
                // 等待10毫秒，控制动画速度
                await Task.Delay(10); 
            }
            Debug.Log("完成");
            
            // 动画完成，从父元素中移除
            this.parent.Remove(this);
        }
    }
}