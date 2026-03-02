using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 可序列化字典的标记基类
/// 用于标识这是一个可序列化的字典类型，不包含具体实现
/// 主要用于在Unity编辑器中识别和分类序列化字段
/// </summary>
public class SerializableDictionary
{
}

/// <summary>
/// 可序列化字典类
/// 允许在Unity中序列化字典结构，解决Unity默认不支持字典序列化的问题
/// 实现IDictionary接口以支持标准字典操作，同时实现ISerializationCallbackReceiver处理序列化回调
/// </summary>
/// <typeparam name="TKey">字典键的类型，必须可序列化</typeparam>
/// <typeparam name="TValue">字典值的类型，必须可序列化</typeparam>
[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    /// <summary>
    /// 序列化存储列表
    /// 在Unity编辑器中实际序列化的数据结构，存储键值对
    /// 通过将字典转换为列表来支持Unity的序列化系统
    /// </summary>
    [SerializeField]
    private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

    /// <summary>
    /// 可序列化键值对结构
    /// 包装键值对，使其可在Unity编辑器中序列化
    /// </summary>
    [Serializable]
    public struct SerializableKeyValuePair
    {
        public TKey Key;     // 键
        public TValue Value; // 值

        /// <summary>
        /// 构造函数
        /// 初始化键值对
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 设置值
        /// 修改当前键值对的值
        /// </summary>
        /// <param name="value">新值</param>
        public void SetValue(TValue value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// 键位置映射
    /// 延迟初始化的字典，用于快速查找键在列表中的位置
    /// 键：字典键，值：在list中的索引
    /// </summary>
    private Dictionary<TKey, uint> KeyPositions => _keyPositions.Value;
    private Lazy<Dictionary<TKey, uint>> _keyPositions;

    /// <summary>
    /// 默认构造函数
    /// 初始化延迟加载的键位置映射
    /// </summary>
    public SerializableDictionary()
    {
        _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
    }

    /// <summary>
    /// 从现有字典初始化的构造函数
    /// 将标准字典转换为可序列化字典
    /// </summary>
    /// <param name="dictionary">要转换的字典</param>
    public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);

        if (dictionary == null)
        {
            throw new ArgumentException("The passed dictionary is null.");
        }

        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// 创建键位置映射
    /// 遍历列表，为每个键创建到索引的映射
    /// 用于加速字典的查找操作
    /// </summary>
    /// <returns>键到索引的映射字典</returns>
    private Dictionary<TKey, uint> MakeKeyPositions()
    {
        int numEntries = list.Count;

        Dictionary<TKey, uint> result = new Dictionary<TKey, uint>(numEntries);

        for (int i = 0; i < numEntries; ++i)
        {
            result[list[i].Key] = (uint)i;
        }

        return result;
    }

    /// <summary>
    /// 序列化前回调
    /// 在Unity序列化此对象前调用，当前实现为空
    /// </summary>
    public void OnBeforeSerialize()
    {
        // 在序列化前不需要特殊处理
    }

    /// <summary>
    /// 反序列化后回调
    /// 在Unity反序列化此对象后调用，重新构建键位置映射
    /// 确保字典在反序列化后能正确工作
    /// </summary>
    public void OnAfterDeserialize()
    {
        // 反序列化后重新构建键位置映射
        _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
    }

    #region IDictionary接口实现

    /// <summary>
    /// 索引器
    /// 通过键获取或设置值
    /// </summary>
    /// <param name="key">要访问的键</param>
    /// <returns>对应的值</returns>
    public TValue this[TKey key]
    {
        get
        {
            // 通过键位置映射获取索引，然后从列表中返回值
            return list[(int)KeyPositions[key]].Value;
        }
        set
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                // 键存在：更新值
                list[(int)index].SetValue(value);
            }
            else
            {
                // 键不存在：添加新键值对
                KeyPositions[key] = (uint)list.Count;
                list.Add(new SerializableKeyValuePair(key, value));
            }
        }
    }

    /// <summary>
    /// 键集合
    /// 返回所有键的集合
    /// 通过LINQ从列表中选择所有键
    /// </summary>
    public ICollection<TKey> Keys => list.Select(tuple => tuple.Key).ToArray();

    /// <summary>
    /// 值集合
    /// 返回所有值的集合
    /// 通过LINQ从列表中选择所有值
    /// </summary>
    public ICollection<TValue> Values => list.Select(tuple => tuple.Value).ToArray();

    /// <summary>
    /// 添加键值对
    /// 向字典中添加新的键值对
    /// </summary>
    /// <param name="key">要添加的键</param>
    /// <param name="value">要添加的值</param>
    /// <exception cref="ArgumentException">当键已存在时抛出</exception>
    public void Add(TKey key, TValue value)
    {
        if (KeyPositions.ContainsKey(key))
        {
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        }
        else
        {
            // 添加新键值对，并更新键位置映射
            KeyPositions[key] = (uint)list.Count;
            list.Add(new SerializableKeyValuePair(key, value));
        }
    }

    /// <summary>
    /// 检查是否包含指定键
    /// </summary>
    /// <param name="key">要查找的键</param>
    /// <returns>如果字典包含该键返回true，否则返回false</returns>
    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

    /// <summary>
    /// 移除指定键及其值
    /// 从字典中移除键值对，并更新键位置映射
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <returns>如果成功移除返回true，否则返回false</returns>
    public bool Remove(TKey key)
    {
        if (KeyPositions.TryGetValue(key, out uint index))
        {
            Dictionary<TKey, uint> kp = KeyPositions;
            kp.Remove(key);

            // 从列表中移除元素
            list.RemoveAt((int)index);

            // 更新被移除元素之后所有元素的索引
            int numEntries = list.Count;
            for (uint i = index; i < numEntries; i++)
            {
                kp[list[(int)i].Key] = i;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 尝试获取指定键的值
    /// 安全地获取值，避免键不存在的异常
    /// </summary>
    /// <param name="key">要查找的键</param>
    /// <param name="value">如果找到键，返回对应的值</param>
    /// <returns>如果键存在返回true，否则返回false</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (KeyPositions.TryGetValue(key, out uint index))
        {
            value = list[(int)index].Value;
            return true;
        }

        value = default;
        return false;
    }

    #endregion

    #region ICollection接口实现

    /// <summary>
    /// 获取字典中键值对的数量
    /// </summary>
    public int Count => list.Count;

    /// <summary>
    /// 获取字典是否为只读
    /// 此实现总是返回false
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// 添加键值对
    /// 将KeyValuePair添加到字典中
    /// </summary>
    /// <param name="kvp">要添加的键值对</param>
    public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

    /// <summary>
    /// 清空字典
    /// 移除所有键值对
    /// </summary>
    public void Clear()
    {
        list.Clear();
        KeyPositions.Clear();
    }

    /// <summary>
    /// 检查字典是否包含指定的键值对
    /// 此实现只检查键，不检查值
    /// </summary>
    /// <param name="kvp">要查找的键值对</param>
    /// <returns>如果字典包含该键返回true，否则返回false</returns>
    public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

    /// <summary>
    /// 将字典元素复制到数组中
    /// 从指定索引开始复制
    /// </summary>
    /// <param name="array">目标数组</param>
    /// <param name="arrayIndex">目标数组的起始索引</param>
    /// <exception cref="ArgumentException">当目标数组空间不足时抛出</exception>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int numKeys = list.Count;

        if (array.Length - arrayIndex < numKeys)
        {
            throw new ArgumentException("arrayIndex");
        }

        for (int i = 0; i < numKeys; ++i, ++arrayIndex)
        {
            SerializableKeyValuePair entry = list[i];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// 移除指定的键值对
    /// 此实现只根据键移除
    /// </summary>
    /// <param name="kvp">要移除的键值对</param>
    /// <returns>如果成功移除返回true，否则返回false</returns>
    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

    #endregion

    #region IEnumerable接口实现

    /// <summary>
    /// 返回遍历字典的枚举器
    /// 用于foreach循环
    /// </summary>
    /// <returns>键值对枚举器</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return list.Select(ToKeyValuePair).GetEnumerator();

        // 局部函数：将SerializableKeyValuePair转换为KeyValuePair
        KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
        }
    }

    /// <summary>
    /// 返回遍历字典的非泛型枚举器
    /// </summary>
    /// <returns>非泛型枚举器</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}