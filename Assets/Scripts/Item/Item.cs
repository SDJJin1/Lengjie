using UnityEngine;

namespace Item
{
    /// <summary>
    /// 物品基类
    /// 继承自ScriptableObject，用于创建可配置的物品数据资产
    /// 包含所有物品共有的基本信息，如名称、图标和描述
    /// 作为游戏内所有物品类型的基类，支持在Unity编辑器中创建和配置
    /// </summary>
    public class Item : ScriptableObject
    {
        /// <summary>
        /// 物品名称
        /// 在游戏中显示的名称，应具有可读性和描述性
        /// 支持本地化键名，可在实际项目中扩展为本地化系统
        /// </summary>
        [Header("物品信息")] 
        public string itemName;
        
        /// <summary>
        /// 物品图标
        /// 在UI中显示的小图标，通常为64x64或128x128像素
        /// 支持透明背景，推荐使用PNG格式
        /// 用于背包、商店、合成等界面
        /// </summary>
        public Sprite itemIcon;
        
        /// <summary>
        /// 物品描述
        /// 物品的详细说明文本，可包含多行描述
        /// 使用TextArea属性在Inspector中显示多行文本框
        /// 可包含物品背景故事、使用效果、特殊说明等信息
        /// </summary>
        [TextArea] 
        public string itemDescription;
    }

    public enum EquipmentType
    {
        RightHandWeapon01 = 0,
        RightHandWeapon02 = 1,
        RightHandWeapon03 = 2,
        LeftHandWeapon01 = 3,
        LeftHandWeapon02 = 4,
        LeftHandWeapon03 = 5,
    }
}
