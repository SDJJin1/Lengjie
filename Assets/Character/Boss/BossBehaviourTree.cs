using System.Collections;
using System.Collections.Generic;
using BehaviourTree.BehaviourTree;
using BehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

/// <summary>
/// Boss 行为树控制器
/// 负责在 FixedUpdate 中驱动行为树的执行
/// </summary>
public class BossBehaviourTree : SerializedMonoBehaviour
{
    [OdinSerialize, HideReferenceObjectPicker, OpenView(ButtonName = "打开视图")]
    public BehaviourTreeData TreeData;

    public BehaviourTreeData GetBtData() => TreeData;
    
    private void FixedUpdate()
    {
        // 如果行为树数据与根节点存在，则执行 Tick
        TreeData?.Root?.Tick();
    }
}
