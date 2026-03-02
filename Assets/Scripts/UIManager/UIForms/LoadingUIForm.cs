using System;
using System.Collections;
using TMPro;
using UIManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 加载界面UI表单
    /// 继承自UIManager.UIForm，用于在场景加载时显示加载进度
    /// 通过协程异步加载场景，实时更新进度条和进度文本
    /// 支持加载下一场景和指定索引场景两种模式
    /// 加载完成后自动打开HUD界面
    /// </summary>
    public class LoadingUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 加载进度条
        /// 显示场景加载的进度，范围0-1
        /// 通过AsyncOperation.progress更新
        /// </summary>
        [SerializeField] 
        private Slider LoadingSlider;
        
        /// <summary>
        /// 加载进度文本
        /// 显示加载进度的百分比文本
        /// 格式："XX%"
        /// </summary>
        [SerializeField] 
        private TextMeshProUGUI LoadingText;

        public override void OnOpen(object userData = null,   params object[] args)
        {
            base.OnOpen(userData);

            // 根据参数数量决定加载方式
            if (args.Length == 1)
            {
                // 加载指定索引的场景
                StartCoroutine(LoadScene((int)args[0]));
            }
            else
            {
                // 加载下一个场景
                StartCoroutine(LoadScene());
            }
        }

        public override void OnClose()
        {
            base.OnClose();

            // 打开HUD界面
            UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.HUDForm);
        }

        IEnumerator LoadScene()
        {
            // 计算下一个场景的索引
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            // 确保索引有效
            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogWarning($"下一个场景索引{nextSceneIndex}超出范围，将返回第一个场景");
                nextSceneIndex = 0;
            }
            
            // 异步加载场景
            AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneIndex);
            
            // 注意：取消下面这行注释可以阻止场景自动激活
            // 这允许在加载完成后等待用户输入再显示新场景
            // operation.allowSceneActivation = false;
            
            // 更新加载进度
            while (!operation.isDone)
            {
                // 更新进度条
                LoadingSlider.value = operation.progress;
                
                // 更新进度文本
                LoadingText.text = (operation.progress * 100).ToString("F0") + "%";
                
                // 等待下一帧
                yield return null;
            }
            
            // 加载完成，关闭表单
            Close();
        }
        
        /// <summary>
        /// 加载指定索引场景协程
        /// 异步加载指定索引的场景
        /// 更新进度条和文本
        /// 加载完成后关闭当前表单
        /// </summary>
        /// <param name="sceneIndex">要加载的场景索引</param>
        /// <returns>协程迭代器</returns>
        IEnumerator LoadScene(int sceneIndex)
        {
            // 确保场景索引有效
            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError($"场景索引{sceneIndex}超出有效范围");
                yield break;
            }
            
            // 异步加载场景
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            
            // 注意：取消下面这行注释可以阻止场景自动激活
            // 这允许在加载完成后等待用户输入再显示新场景
            // operation.allowSceneActivation = false;
            
            // 更新加载进度
            while (!operation.isDone)
            {
                // 更新进度条
                LoadingSlider.value = operation.progress;
                
                // 更新进度文本
                LoadingText.text = (operation.progress * 100).ToString("F0") + "%";
                
                // 等待下一帧
                yield return null;
            }
            
            // 加载完成，关闭表单
            Close();
        }
    }
}
