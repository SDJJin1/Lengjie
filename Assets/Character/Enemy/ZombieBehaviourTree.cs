using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourTree.BehaviourTree;
using BehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class ZombieBehaviourTree : SerializedMonoBehaviour
{
    [OdinSerialize, HideReferenceObjectPicker, OpenView(ButtonName = "打开视图")]
    public BehaviourTreeData TreeData;

    public BehaviourTreeData GetBtData() => TreeData;
    
    private void FixedUpdate()
    {
        TreeData?.Root?.Tick();
    }
}
