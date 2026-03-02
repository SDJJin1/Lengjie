using System;
using Character.Player;
using TMPro;
using UIForm;
using UIManager;
using UnityEngine;

namespace Interacts
{
    /// <summary>
    /// 提示交互组件
    /// 实现Interactable接口，用于显示游戏内提示信息
    /// 当玩家接近时显示交互提示，交互后显示详细的提示文本
    /// 通过触发器检测玩家位置，控制UI的显示和隐藏
    /// </summary>
    public class TipInteractable : MonoBehaviour, Interactable
    {
        /// <summary>
        /// 交互提示文本
        /// 在玩家接近时显示的简短提示，如"按E查看提示"
        /// 序列化字段，允许在Inspector中编辑
        /// </summary>
        [SerializeField] 
        private string interactText;
        
        /// <summary>
        /// 提示详细内容文本
        /// 玩家交互后显示的详细提示信息
        /// 序列化字段，允许在Inspector中编辑
        /// 可包含多行文本，支持格式和标记
        /// </summary>
        [SerializeField] 
        private string tipText;
        
        /// <summary>
        /// 交互方法实现
        /// 当玩家与提示交互时调用，显示详细提示信息
        /// 先关闭交互提示UI，然后打开详细提示UI
        /// </summary>
        /// <param name="player">执行交互的玩家对象</param>
        public void Interact(Player player)
        {
            // 关闭交互提示UI
            UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.InteractForm);
            // 打开详细提示UI，传递提示文本
            UIManager.UIManager.Instance.OpenUIForm<TipUIForm>(
                EnumUIForm.TipForm, 
                null,       // 上下文参数，此处为null
                tipText     // 要显示的提示文本
            );
        }

        /// <summary>
        /// 触发器进入事件
        /// 当玩家进入触发器范围时，显示交互提示UI
        /// 告知玩家可以与此对象交互
        /// </summary>
        /// <param name="other">进入触发器的碰撞体</param>
        private void OnTriggerEnter(Collider other)
        {
            // 检查进入的对象是否为玩家
            if (other.GetComponent<Player>())
            {
                // 打开交互提示UI，传递当前对象和交互文本
                UIManager.UIManager.Instance.OpenUIForm<InteractForm>(
                    EnumUIForm.InteractForm, 
                    this,           // 可交互对象引用
                    interactText    // 交互提示文本
                );
            }
        }

        /// <summary>
        /// 触发器离开事件
        /// 当玩家离开触发器范围时，关闭所有相关的UI
        /// 确保玩家离开后不再显示提示
        /// </summary>
        /// <param name="other">离开触发器的碰撞体</param>
        private void OnTriggerExit(Collider other)
        {
            // 关闭交互提示UI
            UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.InteractForm);
            // 关闭详细提示UI（如果已打开）
            UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.TipForm);
        }
    }
}