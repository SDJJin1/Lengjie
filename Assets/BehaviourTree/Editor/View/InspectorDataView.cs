using System.Collections.Generic;
using BehaviourTree.BehaviourTree;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace BehaviourTree.Editor.View
{
    /// <summary>
    /// 检查器数据视图
    /// 继承自SerializedScriptableObject，用于在编辑器检查器面板中序列化显示行为树节点数据
    /// 主要用于在多选节点时，集中显示和管理被选中的节点
    /// 通过Odin序列化器实现复杂数据的序列化和Inspector显示
    /// </summary>
    public class InspectorDataView : SerializedScriptableObject
    {
        /// <summary>
        /// 选择的节点数据集合
        /// 存储当前在编辑器中被选中的节点数据，使用HashSet确保唯一性
        /// 通过Odin属性控制Inspector中的显示方式：
        ///   - OdinSerialize：使用Odin序列化器序列化此字段
        ///   - LabelText：在Inspector中显示为"选择的点"
        ///   - HideReferenceObjectPicker：隐藏引用对象选择器，防止直接编辑引用
        ///   - ListDrawerSettings(IsReadOnly = true)：设置为只读模式，防止在Inspector中直接修改
        /// </summary>
        [OdinSerialize, LabelText("选择的点"), HideReferenceObjectPicker]
        [ListDrawerSettings(IsReadOnly = true)]
        public HashSet<BtNodeBase> selectDatas;
 
        /// <summary>
        /// 构造函数
        /// 初始化选择节点数据集合
        /// </summary>
        public InspectorDataView()
        {
            selectDatas = new HashSet<BtNodeBase>();
        }
    }

    /* 注释掉的ExposureNode类，可能是之前用于暴露节点数据的结构
    /// <summary>
    /// 暴露节点类
    /// 用于在Inspector中显示节点的详细信息，包括节点视图引用、节点数据和字段信息
    /// 此结构被暂时注释掉，可能因为功能未完成或已废弃
    /// </summary>
    public class ExposureNode
    {
        /// <summary>
        /// 节点视图引用
        /// 隐藏条件：始终隐藏，可能在代码中使用但不在Inspector中显示
        /// </summary>
        [HideIf("@true")]
        public NodeView NodeView;
        
        /// <summary>
        /// 节点数据引用
        /// 隐藏条件：始终隐藏，可能在代码中使用但不在Inspector中显示
        /// </summary>
        [HideIf("@true")]
        public BtNodeBase NodeData;
        
        /// <summary>
        /// 字段名称
        /// 在水平组"nodeData"中显示，但隐藏条件为true，所以实际不显示
        /// 可能用于标识节点中特定的字段
        /// </summary>
        [HideLabel, HorizontalGroup("nodeData"), HideIf("@true")]
        public string FiledName;

        /// <summary>
        /// 字段对象
        /// 存储实际的字段值，可以是任何类型的对象
        /// </summary>
        public object FiledObject;
    }*/
}