using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 集合工具类
/// 提供对可序列化字典等集合类型的扩展方法
/// </summary>
public static class CollectionUtility
{
    /// <summary>
    /// 向可序列化字典中添加元素（扩展方法）
    /// 如果键已存在，将值添加到对应的列表中；如果键不存在，创建新列表并添加值
    /// </summary>
    /// <typeparam name="K">字典键的类型</typeparam>
    /// <typeparam name="V">列表值的类型</typeparam>
    /// <param name="serializableDictionary">目标可序列化字典</param>
    /// <param name="key">要添加的键</param>
    /// <param name="value">要添加到列表的值</param>
    public static void AddItem<K,V>(this SerializableDictionary<K, List<V>> serializableDictionary, K key, V value)
    {
        // 检查字典中是否已包含该键
        if (serializableDictionary.ContainsKey(key))
        {
            // 键已存在：将值添加到对应的列表中
            serializableDictionary[key].Add(value);
            
            return;
        }
        
        // 键不存在：创建新列表并添加值，然后将键值对添加到字典中
        serializableDictionary.Add(key, new List<V>() { value });
    }
}
