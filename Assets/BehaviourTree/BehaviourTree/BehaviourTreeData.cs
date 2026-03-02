using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree.BehaviourTree
{
    /// <summary>
    /// 行为树数据容器
    /// 用于存储和序列化整个行为树的结构数据，包括节点、变换状态和执行控制
    /// 通过Odin Inspector在Unity编辑器中提供可视化的界面
    /// </summary>
    [BoxGroup]  // 在Inspector中将整个类显示为可折叠的组
    [HideReferenceObjectPicker]  // 隐藏引用对象选择器，防止在Inspector中直接编辑引用
    [LabelText("行为树数据")]  // 在Inspector中显示的标签文本
    public class BehaviourTreeData
    {
        /// <summary>
        /// 是否在Inspector中显示数据
        /// 控制行为树数据的可见性，便于调试时查看
        /// </summary>
        [LabelText("是否显示数据")] 
        public bool IsShow = false;

        /// <summary>
        /// 行为树的根节点
        /// 行为树的入口点，所有行为从根节点开始执行
        /// 使用Odin序列化，支持复杂对象的序列化
        /// 仅在IsShow为true时显示
        /// </summary>
        [LabelText("根数据"), OdinSerialize, ShowIf("IsShow")]
        public BtNodeBase Root;

        /// <summary>
        /// 所有节点数据列表
        /// 包含行为树中的所有节点，用于在编辑器中管理节点
        /// 使用Odin序列化，支持复杂对象的序列化
        /// 仅在IsShow为true时显示
        /// </summary>
        [LabelText("根数据"), OdinSerialize, ShowIf("IsShow")]
        public List<BtNodeBase> NodeData = new List<BtNodeBase>();

        /// <summary>
        /// 视图变换保存数据
        /// 用于保存行为树编辑器中的视图变换状态，如位置、旋转、缩放
        /// 仅在IsShow为true时显示
        /// 隐藏引用对象选择器，防止直接编辑
        /// </summary>
        [OdinSerialize, ShowIf("IsShow"), HideReferenceObjectPicker]
        public SaveTransform ViewTransform;

        /// <summary>
        /// 行为树是否激活执行
        /// 控制行为树的执行状态，为true时行为树每帧更新
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// 开始执行行为树
        /// 激活行为树，并开始每帧更新执行
        /// 通过UniTask.Yield实现异步更新，避免阻塞主线程
        /// </summary>
        [Button("开始"), ButtonGroup("控制")]
        public void OnStart()
        {
            _isActive = true;
            OnUpdate();
        }

        /// <summary>
        /// 行为树更新循环
        /// 当行为树激活时，每帧执行根节点的Tick方法
        /// 使用UniTask异步等待下一帧，实现无阻塞的循环
        /// </summary>
        private async void OnUpdate()
        {
            while (_isActive)
            {
                // 执行行为树根节点
                Root?.Tick();
                await UniTask.Yield();  // 等待下一帧
            }
        }

        /// <summary>
        /// 停止执行行为树
        /// 将_isActive设为false，结束更新循环
        /// </summary>
        [Button("结束"), ButtonGroup("控制")]
        public void OnStop() => _isActive = false;
    }
}

/// <summary>
/// 变换保存数据类
/// 用于保存和恢复Unity变换组件的状态
/// 包含位置、旋转、缩放和矩阵表示
/// </summary>
public class SaveTransform
{
    /// <summary>
    /// 位置向量
    /// 保存GameObject在世界空间中的位置
    /// </summary>
    public Vector3 position;
    
    /// <summary>
    /// 旋转四元数
    /// 保存GameObject在世界空间中的旋转
    /// </summary>
    public Quaternion rotation;
    
    /// <summary>
    /// 缩放向量
    /// 保存GameObject在世界空间中的缩放
    /// </summary>
    public Vector3 scale;
    
    /// <summary>
    /// 变换矩阵
    /// 保存GameObject的完整变换矩阵
    /// 用于更复杂的变换操作和计算
    /// </summary>
    public Matrix4x4 matrix;
}