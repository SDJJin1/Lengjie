using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// NOT测试播放行为
/// 继承自BasicPlayableBehaviour，用于在Unity Timeline中创建自定义播放行为
/// 可以附加到Playable资产上，控制Timeline中特定轨道的行为
/// 此示例类展示了PlayableBehaviour的生命周期方法，但当前为空实现
/// </summary>
// 一个附加到Playable资产上的行为
public class NOTTest : BasicPlayableBehaviour
{
    // 当所属的Playable图形开始播放时调用
    public override void OnGraphStart(Playable playable)
    {
        // 可以在这里进行初始化操作，如获取组件引用、重置状态等
        // 注意：此时Playable图形即将开始播放，但还未进入Play状态
    }

    // 当所属的Playable图形停止播放时调用
    public override void OnGraphStop(Playable playable)
    {
        // 可以在这里进行清理操作，如释放资源、重置状态等
        // 注意：此时Playable图形已经完全停止，不会再收到任何更新
    }

    // 当此播放行为的状态被设置为Play时调用
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // 当此行为开始播放时调用
        // 可以在这里执行播放开始时的逻辑，如播放音效、显示UI等
        // info参数包含当前帧的数据，如时间、权重等
    }

    // 当此播放行为的状态被设置为Paused时调用
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        // 当此行为暂停时调用
        // 可以在这里执行暂停时的逻辑，如停止粒子效果、保存状态等
        // 注意：当播放结束或Timeline被禁用时也会调用此方法
    }

    // 在状态设置为Play时，每帧调用，用于准备当前帧
    public override void PrepareFrame(Playable playable, FrameData info)
    {
        // 在每一帧渲染前调用，用于更新此播放行为的状态
        // 可以在这里更新动画、移动对象、计算逻辑等
        // info参数包含当前帧的时间、权重等信息
        // 这是实现自定义播放逻辑的主要方法
    }
}