using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventCenter
{
    /// <summary>
    /// 事件中心
    /// 单例模式实现的事件系统，用于在游戏内实现松耦合的通信机制
    /// 通过整数事件ID注册、取消和触发事件，支持事件处理器（EventHandler）
    /// 使用字典存储事件ID和对应的委托，实现高效的事件分发
    /// </summary>
    public class EventCenter
    {
        // 单例实例
        private static EventCenter _eventInstance;

        /// <summary>
        /// 单例实例属性
        /// 懒加载模式创建实例
        /// </summary>
        private static EventCenter Instance
        {
            get
            {
                _eventInstance = _eventInstance ?? new EventCenter();  // 如果为空则创建新实例
                return _eventInstance;
            }
        }
        
        /// <summary>
        /// 事件处理器字典
        /// 键：事件ID（整数）
        /// 值：事件处理器委托（EventHandler）
        /// 存储所有已注册的事件和对应的处理器
        /// </summary>
        private Dictionary<int, EventHandler> _handlerDic = new Dictionary<int, EventHandler>();
        
        // ==================== 公共静态方法 ====================
        
        /// <summary>
        /// 订阅事件
        /// 为指定事件ID注册事件处理器，允许多个处理器订阅同一事件
        /// </summary>
        /// <param name="eventId">事件ID，用于标识特定事件类型</param>
        /// <param name="handler">事件处理器委托，事件触发时执行</param>
        public static void Subscribe(int eventId, EventHandler handler) => Instance.m_Subscribe(eventId, handler);
        
        /// <summary>
        /// 不重复订阅事件
        /// 确保同一处理器不会重复订阅同一事件，先移除再添加
        /// 适用于需要确保处理器只被添加一次的场景
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器委托</param>
        public static void SubscribeUnique(int eventId, EventHandler handler) => Instance.m_SubscribeUnique(eventId, handler);
        
        /// <summary>
        /// 取消订阅事件
        /// 从指定事件ID中移除事件处理器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">要移除的事件处理器委托</param>
        public static void Unsubscribe(int eventId, EventHandler handler) => Instance.m_Unsubscribe(eventId, handler);
    
        /// <summary>
        /// 判断是否订阅过事件
        /// 检查指定事件ID是否有任何处理器注册
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>如果有处理器订阅返回true，否则返回false</returns>
        public static bool HasSubscribe(int eventId) => Instance.m_HasSubscribe(eventId);
        
        /// <summary>
        /// 触发事件
        /// 调用指定事件ID的所有注册处理器
        /// </summary>
        /// <param name="sender">事件发送者对象</param>
        /// <param name="e">事件参数，必须包含事件ID</param>
        public static void Fire(object sender, GameEventArgs e) => Instance.m_Fire(sender, e);
        
        // ==================== 私有实例方法 ====================
        
        /// <summary>
        /// 订阅事件（实例方法）
        /// 将处理器添加到指定事件ID的委托链中
        /// 如果事件ID不存在，创建新条目
        /// </summary>
        private void m_Subscribe(int eventId, EventHandler handler)
        {
            // 调试日志：$"订阅事件 id：{eventId} handler：{handler.Method.Name}".InfoColor(Color.yellow);
            if (_handlerDic.ContainsKey(eventId))
            {
                _handlerDic[eventId] += handler;  // 添加到现有委托
            }
            else
            {
                _handlerDic.Add(eventId, handler);  // 创建新委托
            }
        }

        /// <summary>
        /// 不重复订阅事件（实例方法）
        /// 先移除处理器，再添加，确保处理器唯一
        /// 防止同一处理器被多次添加到同一事件
        /// </summary>
        private void m_SubscribeUnique(int eventId, EventHandler handler)
        {
            if (_handlerDic.ContainsKey(eventId))
            {
                _handlerDic[eventId] -= handler;  // 先移除
                _handlerDic[eventId] += handler;  // 再添加
            }
            else
            {
                _handlerDic.Add(eventId, handler);  // 创建新委托
            }
        }
        
        /// <summary>
        /// 取消订阅事件（实例方法）
        /// 从指定事件ID的委托链中移除处理器
        /// 如果事件ID不存在，抛出异常
        /// </summary>
        /// <exception cref="ArgumentException">当事件ID未找到时抛出</exception>
        private void m_Unsubscribe(int eventId, EventHandler handler)
        {
            // 调试日志：$"取消订阅事件 id：{eventId} handler：{handler.Method.Name}".InfoColor(Color.yellow);
            if (_handlerDic.ContainsKey(eventId))
            {
                _handlerDic[eventId] -= handler;  // 从委托链中移除
            }
        }

        /// <summary>
        /// 判断是否订阅过事件（实例方法）
        /// 检查字典中是否存在指定事件ID
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>如果事件ID存在返回true，否则返回false</returns>
        public bool m_HasSubscribe(int eventId)
        {
            return _handlerDic.ContainsKey(eventId);
        }
    
        /// <summary>
        /// 触发事件（实例方法）
        /// 调用与事件ID关联的所有处理器
        /// 如果事件ID不存在或处理器为空，则不执行任何操作
        /// </summary>
        private void m_Fire(object sender, GameEventArgs e)
        {
            // 调试日志：$"触发事件 id：{e.Id} args：{e.GetType().Name}".InfoColor(Color.yellow);
            if (_handlerDic.ContainsKey(e.Id) && _handlerDic[e.Id] != null)
            {
                _handlerDic[e.Id](sender, e);  // 调用委托链
            }
        }
    }
}