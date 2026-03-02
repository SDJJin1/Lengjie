using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UIManager
{
    /// <summary>
    /// UI管理器数据
    /// 继承自ScriptableObject，用于在编辑器中配置UI表单的预制体、顺序和枚举映射
    /// 将UI表单的枚举类型与对应的预制体和显示顺序关联起来
    /// 通过字典实现快速查找，提高运行时性能
    /// </summary>
    public class UIManagerData : ScriptableObject
    {
        /// <summary>
        /// UI表单数据列表
        /// 在Inspector中配置的UI表单数据列表
        /// 包含所有UI类型的预制体引用和显示顺序
        /// 序列化字段，支持编辑器配置
        /// </summary>
        [SerializeField]
        private List<UIFormData> uiForms = new List<UIFormData>();

        /// <summary>
        /// UI表单字典
        /// 运行时使用的字典，将枚举映射到UI表单数据
        /// 通过InitData方法从列表初始化
        /// 非序列化字段，仅在运行时存在
        /// </summary>
        public Dictionary<EnumUIForm, UIFormData> UIFormsDict;

        /// <summary>
        /// 初始化数据
        /// 将UI表单列表转换为字典，提高运行时查找效率
        /// 在UIManager的Awake方法中调用
        /// 确保在游戏运行时只执行一次
        /// </summary>
        public void InitData()
        {
            UIFormsDict = new Dictionary<EnumUIForm, UIFormData>();
            foreach (var uiFormData in uiForms)
            {
                UIFormsDict[uiFormData.uiFormEnum] = uiFormData;
            }
        }
    }

    /// <summary>
    /// UI表单枚举
    /// 定义游戏中所有UI界面的类型
    /// 通过枚举值标识不同的UI界面，用于查找和打开UI
    /// 使用InspectorName属性在编辑器中显示中文名称
    /// </summary>
    public enum EnumUIForm
    {
        /// <summary>
        /// 无类型
        /// 作为默认值，不应配置为实际的UI
        /// </summary>
        None,
        
        /// <summary>
        /// 开始界面
        /// 游戏的主菜单界面，通常包含开始游戏、设置、退出等选项
        /// </summary>
        [InspectorName("开始界面")]
        StartForm,
        
        /// <summary>
        /// 设置界面
        /// 游戏设置界面，包含图形、音频、控制等设置选项
        /// </summary>
        [InspectorName("设置界面")]
        SettingForm,
        
        /// <summary>
        /// 加载界面
        /// 场景加载时显示的界面，通常包含进度条和加载提示
        /// </summary>
        [InspectorName("加载界面")]
        LoadingForm,
        
        /// <summary>
        /// 平视显示器
        /// 游戏内实时状态显示，如生命条、耐力条、快捷栏等
        /// </summary>
        [InspectorName("HUD")]
        HUDForm,
        
        /// <summary>
        /// 确认弹窗
        /// 用于用户确认操作的对话框，如删除、退出等确认
        /// </summary>
        [InspectorName("确认弹窗")]
        ConfirmForm,
        
        /// <summary>
        /// 死亡弹窗
        /// 玩家死亡时显示的界面，包含游戏结果和选项
        /// </summary>
        [InspectorName("死亡弹窗")]
        DeadForm,
        
        /// <summary>
        /// 交互弹窗
        /// 玩家与物体交互时显示的界面，显示交互提示
        /// </summary>
        [InspectorName("交互弹窗")]
        InteractForm,
        
        /// <summary>
        /// 提示弹窗
        /// 显示游戏内提示信息的界面，如教程、说明等
        /// </summary>
        [InspectorName("提示弹窗")]
        TipForm,
        
        /// <summary>
        /// 暂停界面
        /// 游戏暂停时显示的界面，包含继续、设置、退出等选项
        /// </summary>
        [InspectorName("暂停界面")]
        PauseForm,
        
        /// <summary>
        /// 装备界面
        /// 玩家装备管理界面，包含装备槽和背包
        /// </summary>
        [InspectorName("装备界面")]
        EquipmentForm,
    }

    /// <summary>
    /// UI表单数据
    /// 可序列化的UI配置数据，用于在UIManagerData中存储UI配置
    /// 包含UI枚举类型、预制体引用和显示顺序
    /// 通过Serializable属性支持在Inspector中显示
    /// </summary>
    [Serializable]
    public class UIFormData
    {
        /// <summary>
        /// UI表单枚举类型
        /// 对应EnumUIForm中的枚举值
        /// 用于标识UI的类型，在代码中引用
        /// </summary>
        public EnumUIForm uiFormEnum;
        
        /// <summary>
        /// UI预制体
        /// UI界面的预制体引用，包含完整的UI实现
        /// 在运行时实例化此预制体创建UI实例
        /// 必须包含继承自UIForm的组件
        /// </summary>
        public GameObject uiForm;
        
        /// <summary>
        /// 显示顺序
        /// UI的显示层级顺序，数值越大显示在越上层
        /// 通常范围在0-1000之间，建议按功能模块分配范围
        /// 例如：0-100: 系统UI，100-200: 游戏UI，200-300: 弹出窗口
        /// </summary>
        public int order;
    }
}
