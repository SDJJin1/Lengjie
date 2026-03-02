using UnityEngine;

namespace Save_Load
{
    /// <summary>
    /// 存档配置类
    /// 泛型类，用于包装存档名称和具体存档数据
    /// 通过泛型约束确保数据类型必须是SaveProfileData的派生类
    /// 序列化后存储到文件系统
    /// </summary>
    /// /// <typeparam name="T">存档数据类型，必须继承自SaveProfileData</typeparam>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 创建存档数据
    /// var playerData = new PlayerSaveData
    /// {
    ///     position = new Vector3(10, 0, 5),
    ///     vitality = 100
    /// };
    /// 
    /// // 创建存档配置
    /// var saveProfile = new SaveProfile<PlayerSaveData>("AutoSave", playerData);
    /// 
    /// // 保存存档
    /// SaveManager.Save(saveProfile);
    /// 
    /// // 加载存档
    /// var loadedProfile = SaveManager.Load<PlayerSaveData>("AutoSave");
    /// var loadedData = loadedProfile.saveData;
    /// </code>
    /// </remarks>
    [System.Serializable]
    public sealed class SaveProfile<T> where T : SaveProfileData
    {
        /// <summary>
        /// 存档名称
        /// 唯一标识存档文件的名称，在存档文件夹中作为文件名
        /// 建议使用有意义的名称，如"AutoSave"、"SaveSlot1"等
        /// </summary>
        public string name;
        
        /// <summary>
        /// 存档数据
        /// 包含游戏状态的具体数据，类型为泛型T
        /// 在序列化时会被转换为JSON格式存储
        /// </summary>
        public T saveData;
        
        private SaveProfile() {}
        
        /// <summary>
        /// 构造函数
        /// 创建新的存档配置，需要指定存档名称和数据
        /// </summary>
        /// <param name="name">存档名称</param>
        /// <param name="saveData">存档数据</param>
        public SaveProfile(string name, T saveData)
        {
            this.name = name;
            this.saveData = saveData;
        }
    }

    public abstract record SaveProfileData { }

    /// <summary>
    /// 玩家存档数据记录
    /// 继承自SaveProfileData，包含玩家的游戏状态信息
    /// 使用记录类型，适用于存档数据这种通常不应在运行时修改的场景
    /// </summary>
    /// <remarks>
    /// 记录（record）的优势：
    /// 1. 不可变性：创建后不应修改，适合存档数据
    /// 2. 值语义：两个存档数据的比较基于内容而非引用
    /// 3. 简洁性：自动生成的方法减少样板代码
    /// </remarks>
    public record PlayerSaveData : SaveProfileData
    {
        /// <summary>
        /// 玩家位置
        /// 在游戏世界中的坐标
        /// 用于恢复玩家在场景中的位置
        /// </summary>
        public Vector3 position;
        
        /// <summary>
        /// 玩家旋转
        /// 玩家的朝向，使用四元数表示
        /// 用于恢复玩家的面向方向
        /// </summary>
        public Quaternion rotation;
        
        /// <summary>
        /// 耐力值
        /// 玩家的当前耐力，影响奔跑、攻击等动作
        /// 通常随时间恢复
        /// </summary>
        public int endurance;
        
        /// <summary>
        /// 生命值
        /// 玩家的当前生命值，为0时玩家死亡
        /// 可通过治疗或道具恢复
        /// </summary>
        public int vitality;
        
        /// <summary>
        /// Boss觉醒状态字典
        /// 记录每个Boss是否已被唤醒
        /// 键：Boss的唯一ID
        /// 值：是否已觉醒
        /// 用于恢复游戏世界中Boss的状态
        /// </summary>
        public SerializableDictionary<int, bool> bossesAwakened;
        
        /// <summary>
        /// Boss击败状态字典
        /// 记录每个Boss是否已被击败
        /// 键：Boss的唯一ID
        /// 值：是否已被击败
        /// 用于解锁成就、开启新区域等
        /// </summary>
        public SerializableDictionary<int, bool> bossesDefeated;
        
        /// <summary>
        /// 构造函数
        /// 初始化存档数据，创建Boss状态字典
        /// 使用无参构造函数，确保序列化系统能正确工作
        /// </summary>
        public PlayerSaveData()
        {
            // 初始化Boss状态字典
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
        }
    }
}