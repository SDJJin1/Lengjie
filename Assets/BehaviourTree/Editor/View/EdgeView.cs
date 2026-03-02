using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 移动点状态枚举
    /// 定义连接线上动画点的三种状态
    /// </summary>
    public enum MovePointState
    {
        停止的,  // 动画点已停止且移除
        运行的,  // 动画点正在运行
        暂停的   // 动画点暂停在当前位置
    }

    /// <summary>
    /// 自定义边视图
    /// 继承自GraphView.Edge，扩展了动画点的功能，用于可视化数据流动
    /// 在行为树运行时显示流动的动画点，增强可视化效果
    /// </summary>
    public class EdgeView : Edge
    {
        // 移动点数组，存储动画点的VisualElement引用
        private VisualElement[] _movePoints;
        // 当前移动点状态
        private MovePointState _isMoveState;
        // 动画步进索引，控制点在路径上的位置
        private int _stepIndex;
        // 动画点数量
        private int _pointNumber = 4;
        
        /// <summary>
        /// 构造函数
        /// 初始化边视图，创建动画点数组
        /// </summary>
        public EdgeView() : base()
        {
            _movePoints = new VisualElement[_pointNumber];
        }

        /// <summary>
        /// 开始移动点动画
        /// 创建动画点并启动动画循环
        /// 如果已经是运行状态，则不重复启动
        /// </summary>
        public void OnStartMovePoints()
        {
            // 如果已经在运行，直接返回
            if (_isMoveState == MovePointState.运行的) return;
            
            // 如果是停止状态，创建新的动画点
            if (_isMoveState == MovePointState.停止的)
            {
                for (int i = 0; i < _pointNumber; i++)
                {
                    _movePoints[i] = new MovePoint();  // 创建移动点元素
                    Add(_movePoints[i]);               // 添加到边视图中
                }
                _stepIndex = 0;                         // 重置动画索引
            }
            
            _isMoveState = MovePointState.运行的;  // 设置为运行状态
            MovePoints();  // 启动动画循环
        }

        /// <summary>
        /// 移动点动画循环
        /// 异步更新每个动画点在连接线上的位置
        /// 通过插值计算点在贝塞尔曲线上的位置
        /// </summary>
        async void MovePoints()
        {
            while (_isMoveState == MovePointState.运行的)
            {
                _stepIndex = _stepIndex % 100;  // 确保索引在0-99范围内
                
                // 更新每个动画点的位置
                for (int i = 0; i < _pointNumber; i++)
                {
                    // 计算当前点的时间参数t（0-1之间）
                    // 通过i/_pointNumber使每个点有相位差
                    float t = (_stepIndex / 100f) + (float)i / _pointNumber;
                    t = t > 1 ? t - 1 : t;  // 如果超过1，回到起点
                    
                    // 在连接线的两个控制点之间插值
                    _movePoints[i].transform.position = Vector2.Lerp(
                        edgeControl.controlPoints[1],  // 贝塞尔曲线的起点
                        edgeControl.controlPoints[2],  // 贝塞尔曲线的终点
                        t
                    );
                }

                _stepIndex++;  // 增加步进索引
                await UniTask.Delay(10);  // 等待10毫秒
            }
        }

        /// <summary>
        /// 暂停移动点动画
        /// 暂停动画循环，保持当前点的位置
        /// </summary>
        public void OnSuspendMovePoints() => _isMoveState = MovePointState.暂停的;

        /// <summary>
        /// 停止移动点动画
        /// 停止动画循环并移除所有动画点
        /// </summary>
        public void OnStopMovePoints()
        {
            if (_isMoveState == MovePointState.停止的) return;
            
            _isMoveState = MovePointState.停止的;  // 设置为停止状态
            
            // 从后往前移除所有动画点
            for (int i = _movePoints.Length - 1; i >= 0; i--)
            {
                if (_movePoints[i] != null)
                {
                    this.Remove(_movePoints[i]);  // 从边视图中移除动画点
                }
            }
        }
    }
}