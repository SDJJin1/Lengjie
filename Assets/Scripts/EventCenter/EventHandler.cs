using System;

namespace EventCenter
{
    /// <summary>
    /// 事件处理器委托
    /// 定义事件系统中所有事件处理器的标准签名
    /// 遵循 .NET 事件模式，与 System.EventHandler 相似但使用自定义的事件参数
    /// 用于处理由 EventCenter 触发的事件
    /// </summary>
    /// <param name="sender">事件发送者对象，通常是触发事件的对象</param>
    /// <param name="e">事件参数，包含事件相关的数据和标识</param>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 定义事件处理方法
    /// private void OnPlayerDied(object sender, GameEventArgs e)
    /// {
    ///     Debug.Log("玩家死亡事件被触发");
    /// }
    /// 
    /// // 注册事件处理器
    /// EventCenter.Subscribe(1001, OnPlayerDied);
    /// 
    /// // 触发事件
    /// EventCenter.Fire(this, new GameEventArgs(1001));
    /// </code>
    /// </remarks>
    public delegate void EventHandler(object sender, GameEventArgs e);
}