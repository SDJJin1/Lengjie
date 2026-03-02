using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIManager
{
    /// <summary>
    /// UI管理器
    /// 单例模式，负责管理游戏中所有UI表单的创建、打开、关闭和层级管理
    /// 通过UI数据配置（UIManagerData）动态管理UI资源
    /// 提供扩展方法支持枚举方式直接操作UI
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例
        /// 确保全局只有一个UI管理器实例
        /// 通过DontDestroyOnLoad在场景切换时保持存活
        /// </summary>
        public static UIManager Instance;
        
        /// <summary>
        /// UI数据配置
        /// 包含所有UI表单的预制体引用和默认层级设置
        /// 在Inspector中配置，通过InitData方法初始化
        /// </summary>
        public UIManagerData uiData;
        
        /// <summary>
        /// UI表单字典
        /// 存储已打开的UI表单实例
        /// 键：UI表单枚举类型
        /// 值：UI表单实例
        /// 确保每种UI类型只有一个实例
        /// </summary>
        private Dictionary<EnumUIForm, UIForm> _uiForms = new Dictionary<EnumUIForm, UIForm>();

        /// <summary>
        /// UI画布
        /// 所有UI表单的父级容器
        /// 在Inspector中分配，确保UI在正确的画布中渲染
        /// </summary>
        public RectTransform canvas;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                uiData.InitData();  // 初始化UI数据
                DontDestroyOnLoad(gameObject);  // 场景切换时不销毁
            }
            else
            {
                // 如果已存在实例，销毁当前对象
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            startOpenUIForm();
        }

        /// <summary>
        /// 打开UI表单
        /// 通用方法，不指定具体UI类型
        /// 如果UI已存在，则重新打开；否则创建新实例
        /// </summary>
        /// <param name="uiFormEnum">要打开的UI表单枚举类型</param>
        /// <param name="userData">用户自定义数据，传递给UI表单的OnOpen方法</param>
        /// <returns>打开的UI表单实例，如果打开失败返回null</returns>
        public UIForm OpenUIForm(EnumUIForm uiFormEnum, object userData = null)
        {
            // 检查UI是否已打开
            if (_uiForms.ContainsKey(uiFormEnum))
            {
                _uiForms[uiFormEnum].OnOpen(userData);
                return _uiForms[uiFormEnum];
            }

            // 检查UI预制体是否存在
            if (!uiData.UIFormsDict.ContainsKey(uiFormEnum))
            {
                Debug.Log("UI Forms not found");
                return null;
            }
            
            // 创建UI实例
            _uiForms[uiFormEnum] = Instantiate(uiData.UIFormsDict[uiFormEnum].uiForm, canvas).GetComponent<UIForm>();
            _uiForms[uiFormEnum].OnInit();  // 初始化UI
            _uiForms[uiFormEnum].Order = uiData.UIFormsDict[uiFormEnum].order;  // 设置默认层级
            _uiForms[uiFormEnum].OnOpen(userData);  // 打开UI
            RefreshUIFormsOrder();  // 刷新UI层级顺序
            return _uiForms[uiFormEnum];
        }

        /// <summary>
        /// 打开UI表单（泛型版本）
        /// 指定具体UI类型，方便获取特定类型的引用
        /// 支持传递多个参数
        /// </summary>
        /// <typeparam name="T">UI表单类型，必须是UIForm的派生类</typeparam>
        /// <param name="uiFormEnum">要打开的UI表单枚举类型</param>
        /// <param name="userData">用户自定义数据</param>
        /// <param name="args">可变参数数组，传递给UI表单的OnOpen方法</param>
        /// <returns>打开的UI表单实例，如果打开失败返回null</returns>
        public T OpenUIForm<T>(EnumUIForm uiFormEnum, object userData = null, params object[] args) where T : UIForm
        {
            // 检查UI是否已打开
            if (_uiForms.ContainsKey(uiFormEnum))
            {
                _uiForms[uiFormEnum].OnOpen(userData, args);
                return _uiForms[uiFormEnum] as T;
            }

            // 检查UI预制体是否存在
            if (!uiData.UIFormsDict.ContainsKey(uiFormEnum))
            {
                Debug.Log("UI Forms not found");
                return null;
            }
            
            // 创建UI实例
            var uiObj = Instantiate(uiData.UIFormsDict[uiFormEnum].uiForm, canvas);
            var uiForm = uiObj.GetComponent<T>();
            
            // 双重检查组件，确保获取到正确的UI组件
            if (uiForm == null)
            {
                uiForm = uiObj.GetComponent<T>();
            }
            
            _uiForms[uiFormEnum] = uiForm;
            _uiForms[uiFormEnum].OnInit();
            _uiForms[uiFormEnum].Order = uiData.UIFormsDict[uiFormEnum].order;
            _uiForms[uiFormEnum].OnOpen(userData, args);
            RefreshUIFormsOrder();
            return uiForm;
        }

        /// <summary>
        /// 关闭UI表单
        /// 关闭指定枚举类型的UI表单
        /// 不销毁实例，但会调用Close方法隐藏UI
        /// </summary>
        /// <param name="uiFormEnum">要关闭的UI表单枚举类型</param>
        public void CloseUIForm(EnumUIForm uiFormEnum)
        {
            if (!_uiForms.ContainsKey(uiFormEnum)) return;
            _uiForms[uiFormEnum].Close();
        }

        /// <summary>
        /// 关闭所有UI表单
        /// 关闭当前已打开的所有UI表单
        /// 通常用于切换场景或返回主菜单
        /// </summary>
        public void CloseAllUIForms()
        {
            foreach (var uiForm in _uiForms)
            {
                uiForm.Value.Close();
            }
        }
        
        /// <summary>
        /// 检查UI表单是否存在
        /// 检查指定枚举类型的UI表单是否已打开
        /// </summary>
        /// <param name="uiFormEnum">要检查的UI表单枚举类型</param>
        /// <returns>如果UI表单已打开返回true，否则返回false</returns>
        public bool HasUIForm(EnumUIForm uiFormEnum) => _uiForms.ContainsKey(uiFormEnum);

        /// <summary>
        /// 获取UI表单实例
        /// 获取指定枚举类型和特定类型的UI表单实例
        /// 如果UI表单未打开，返回null
        /// </summary>
        /// <typeparam name="T">UI表单类型，必须是UIForm的派生类</typeparam>
        /// <param name="uiFormEnum">要获取的UI表单枚举类型</param>
        /// <returns>UI表单实例，如果未找到返回null</returns>
        public T GetUIForm<T>(EnumUIForm uiFormEnum) where T : UIForm
        {
            return _uiForms.GetValueOrDefault(uiFormEnum) as T;
        }

        /// <summary>
        /// 刷新UI层级顺序
        /// 根据UI表单的Order属性重新排序UI的渲染顺序
        /// Order值越大，显示在越上层
        /// 通过设置Transform的SiblingIndex实现
        /// </summary>
        private void RefreshUIFormsOrder()
        {
            var orderByUIForm = _uiForms.Values.OrderBy(uiForm => uiForm.Order).ToList();
            for (int i = 0; i < orderByUIForm.Count; i++)
            {
                orderByUIForm[i].transform.SetSiblingIndex(i);
            }
        }

        /// <summary>
        /// 启动时打开的UI表单枚举
        /// 在Inspector中配置，游戏启动时自动打开
        /// </summary>
        public EnumUIForm startEnumUIForm;

        /// <summary>
        /// 启动时打开UI表单
        /// 在Start方法中调用，打开配置的初始UI
        /// 通过扩展方法实现
        /// </summary>
        private void startOpenUIForm()
        {
            startEnumUIForm.OpenUIForm("Start Game");
        }
    }

    /// <summary>
    /// UI表单枚举扩展方法
    /// 为EnumUIForm提供便捷的UI操作扩展方法
    /// 使UI操作更简洁，支持链式调用
    /// </summary>
    public static class EnumUIFormEx
    {
        /// <summary>
        /// 打开UI表单
        /// 扩展方法，通过枚举值直接打开UI
        /// 不指定具体UI类型
        /// </summary>
        /// <param name="uiFormEnum">UI表单枚举类型</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>打开的UI表单实例</returns>
        public static UIForm OpenUIForm(this EnumUIForm uiFormEnum, object userData = null) =>
            UIManager.Instance.OpenUIForm<UIForm>(uiFormEnum, userData);
        
        /// <summary>
        /// 打开UI表单（泛型版本）
        /// 扩展方法，通过枚举值直接打开指定类型的UI
        /// </summary>
        /// <typeparam name="T">UI表单类型，必须是UIForm的派生类</typeparam>
        /// <param name="uiFormEnum">UI表单枚举类型</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>打开的UI表单实例</returns>
        public static UIForm OpenUIForm<T>(this EnumUIForm uiFormEnum, object userData = null) where T : UIForm =>
            UIManager.Instance.OpenUIForm<T>(uiFormEnum, userData);
        
        /// <summary>
        /// 关闭UI表单
        /// 扩展方法，通过枚举值直接关闭UI
        /// </summary>
        /// <param name="uiFormEnum">要关闭的UI表单枚举类型</param>
        public static void CloseUIForm(this EnumUIForm uiFormEnum) => 
            UIManager.Instance.CloseUIForm(uiFormEnum);
        
        /// <summary>
        /// 检查UI表单是否存在
        /// 扩展方法，检查指定枚举类型的UI是否已打开
        /// </summary>
        /// <param name="uiFormEnum">要检查的UI表单枚举类型</param>
        /// <returns>如果UI表单已打开返回true，否则返回false</returns>
        public static bool HasUIForm(this EnumUIForm uiFormEnum) =>
            UIManager.Instance.HasUIForm(uiFormEnum);
        
        /// <summary>
        /// 获取UI表单实例
        /// 扩展方法，获取指定枚举类型和特定类型的UI实例
        /// </summary>
        /// <typeparam name="T">UI表单类型，必须是UIForm的派生类</typeparam>
        /// <param name="uiFormEnum">要获取的UI表单枚举类型</param>
        /// <returns>UI表单实例，如果未找到返回null</returns>
        public static T GetUIForm<T>(this EnumUIForm uiFormEnum) where T : UIForm =>
            UIManager.Instance.GetUIForm<T>(uiFormEnum);
    }
}
