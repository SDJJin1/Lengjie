using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话组脚本化对象
/// 用于在对话编辑器中组织和管理相关的对话节点，作为对话容器中的分组单位
/// 继承自ScriptableObject，可在Unity编辑器中创建和配置
/// </summary>
public class DSDialogueGroupSO : ScriptableObject
{
    /// <summary>
    /// 对话组名称
    /// 用于标识和区分不同的对话分组
    /// </summary>
    [field:SerializeField] public string GroupName { get; set; }

    /// <summary>
    /// 初始化对话组
    /// 设置对话组的名称
    /// </summary>
    /// <param name="groupName">对话组名称</param>
    public void Initialize(string groupName)
    {
        GroupName = groupName;
    }
}
