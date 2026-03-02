using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIManager
{
    /// <summary>
    /// UI表单基类
    /// 所有UI界面的抽象基类，定义了UI表单的基本生命周期和行为
    /// 提供初始化、打开、关闭、显示顺序管理等核心功能
    /// 通过继承此类创建具体的UI界面实现
    /// </summary>
    public abstract class UIForm : MonoBehaviour
    {
        /// <summary>
        /// UI显示顺序
        /// 数值越大显示在越上层
        /// 用于控制UI的叠加顺序，通常由UIManager自动管理
        /// 默认值为0，表示使用默认顺序
        /// </summary>
        [NonSerialized] 
        public int Order = 0;
        
        /// <summary>
        /// 初始化方法
        /// 在UI表单创建时调用，用于执行一次性的初始化操作
        /// 通常用于获取组件引用、初始化变量、注册事件等
        /// 此方法在Awake之后、OnEnable之前调用
        /// </summary>
        public virtual void OnInit() {}

        /// <summary>
        /// 打开UI表单
        /// 每次UI表单被显示时调用，用于设置UI内容和状态
        /// 可以接收用户数据和可变参数，用于动态配置UI
        /// 默认实现是激活GameObject
        /// </summary>
        public virtual void OnOpen(object userData = null, params object[] args)
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 关闭UI表单
        /// 每次UI表单被隐藏时调用，用于执行清理操作
        /// 通常用于取消事件订阅、重置状态、释放资源等
        /// 不销毁GameObject，UI表单可以重复使用
        /// </summary>
        public virtual void OnClose() {}

        /// <summary>
        /// 关闭UI表单
        /// 外部调用的关闭方法，触发OnClose()并停用GameObject
        /// 可以安全地从外部（如UIManager）调用此方法来关闭UI
        /// </summary>
        public virtual void Close()
        {
            gameObject.SetActive(false);
            OnClose();
        }
    }
}

