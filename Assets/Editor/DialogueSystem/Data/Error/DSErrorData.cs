using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话系统错误数据类
/// 用于在对话系统编辑器窗口中存储和显示错误信息，通过随机颜色区分不同错误
/// </summary>
public class DSErrorData
{
    /// <summary>
    /// 错误显示颜色
    /// 用于在编辑器窗口中以特定颜色高亮显示错误
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// 构造函数
    /// 初始化时生成随机颜色
    /// </summary>
    public DSErrorData()
    {
        GenerateRandomColor();
    }

    /// <summary>
    /// 生成随机颜色
    /// 创建具有一定范围限制的随机颜色，确保颜色在可读性良好的范围内
    /// </summary>
    private void GenerateRandomColor()
    {
        // 生成RGB值在特定范围内的随机颜色：
        // R: 65-255（偏暖色调）
        // G: 50-175（限制绿色分量）
        // B: 50-175（限制蓝色分量）
        // A: 255（完全不透明）
        Color = new Color32(
            (byte)Random.Range(65, 256), 
            (byte)Random.Range(50, 176),
            (byte)Random.Range(50, 176),
            255
        );
    }
}
