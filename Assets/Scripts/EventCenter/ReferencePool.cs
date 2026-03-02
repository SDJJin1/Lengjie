namespace EventCenter
{
    public static class ReferencePool
    {
        /// <summary>
        /// 从池中获取事件参数实例
        /// 当前实现总是创建新实例，后续可扩展为真正的对象池
        /// </summary>
        /// <typeparam name="T">事件参数类型，必须是GameEventArgs的派生类</typeparam>
        /// <returns>事件参数实例，如果池中有可用实例则返回池中实例，否则创建新实例</returns>
        public static T Acquire<T>() where T : GameEventArgs, new()
        {
            return new T();
        }
    }
}