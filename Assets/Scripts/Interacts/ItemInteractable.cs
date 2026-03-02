using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UIForm;
using UIManager;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物品可交互组件
/// 实现Interactable接口，允许玩家拾取物品
/// 当玩家靠近物品时显示交互UI，与物品交互时将物品添加到玩家库存
/// 通过触发器检测玩家接近，处理UI的显示和隐藏
/// </summary>
public class ItemInteractable : MonoBehaviour, Interactable
{
    /// <summary>
    /// 物品数据
    /// 序列化字段，允许在Inspector中设置要拾取的物品
    /// 包含物品名称、图标、属性等
    /// </summary>
    [SerializeField] 
    private Item.Item item;
    
    /// <summary>
    /// 交互方法实现
    /// 当玩家与物品交互时调用，将物品添加到玩家库存
    /// 关闭UI并销毁物品对象
    /// </summary>
    /// <param name="player">执行交互的玩家对象</param>
    public void Interact(Player player)
    { 
        // 将物品添加到玩家库存
        player.AddItemToInventory(item);
        
        // 关闭UI
        Close();
        // 销毁物品对象
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 触发器进入事件
    /// 当玩家进入触发器范围时，打开交互UI
    /// 显示物品名称和图标，提示玩家可以拾取
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的对象是否为玩家
        if (other.GetComponent<Player>())
        {
            // 打开交互UI，传递物品名称和图标
            UIManager.UIManager.Instance.OpenUIForm<InteractForm>(
                EnumUIForm.InteractForm, 
                this,           // 交互对象引用
                item.itemName,  // 物品名称
                item.itemIcon   // 物品图标
            );
        }
    }

    /// <summary>
    /// 触发器离开事件
    /// 当玩家离开触发器范围时，关闭交互UI
    /// 防止玩家在不可交互位置看到UI
    /// </summary>
    /// <param name="other">离开触发器的碰撞体</param>
    private void OnTriggerExit(Collider other)
    {
        Close();
    }

    /// <summary>
    /// 关闭交互UI
    /// 通过UIManager关闭指定的UI表单
    /// 在多种情况下调用：交互后、玩家离开、物品销毁
    /// </summary>
    private void Close()
    {
        UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.InteractForm);
    }
}