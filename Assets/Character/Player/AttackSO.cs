using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击数据配置脚本
/// 用于创建和管理攻击相关的配置数据，包括动画、伤害、计时等参数
/// 这是一个ScriptableObject，可在Unity编辑器中创建和配置
/// </summary>
[CreateAssetMenu(menuName = "Attack")]
public class AttackSO : ScriptableObject
{
    /// <summary>
    /// 动画覆盖控制器
    /// 用于替换基础动画控制器，实现特定攻击动作的动画播放
    /// </summary>
    public AnimatorOverrideController AnimatorOverrideController;
    
    /// <summary>
    /// 伤害修正系数
    /// 用于计算最终伤害：武器基础伤害 × 伤害修正系数
    /// </summary>
    public float damageModifier;
    
    /// <summary>
    /// 连击时间窗口
    /// 玩家可以在此期间输入下一个攻击指令以执行连击的时间阈值
    /// </summary>
    public float comboTimer;
    
    /// <summary>
    /// 攻击结束时间
    /// 攻击动画完全结束的时间点，到达此时间后将切换回空闲状态
    /// </summary>
    public float endTimer;
    
    /// <summary>
    /// 伤害碰撞体开启时间
    /// 攻击动画中伤害碰撞体开始生效的时间点
    /// </summary>
    public float colliderOpenTimer;
    
    /// <summary>
    /// 伤害碰撞体关闭时间
    /// 攻击动画中伤害碰撞体停止生效的时间点
    /// </summary>
    public float colliderCloseTimer;
}
