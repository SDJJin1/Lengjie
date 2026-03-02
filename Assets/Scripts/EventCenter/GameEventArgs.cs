using System;

namespace EventCenter
{
    /// <summary>
    /// 游戏事件参数基类
    /// 继承自System.EventArgs，为所有游戏事件提供统一的基类
    /// 通过Id属性标识事件类型，支持事件分发和参数传递
    /// 使用Clear方法支持事件参数的重用和对象池管理
    /// </summary>
    public abstract class GameEventArgs : EventArgs
    {
        /// <summary>
        /// 获取类型编号
        /// </summary>
        public abstract int Id
        {
            get;
        }

        public virtual void Clear() {}
    }
}