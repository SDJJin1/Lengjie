using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Save_Load
{
    /// <summary>
    /// 存档管理器
    /// 提供游戏存档的保存、加载、删除和检查功能
    /// 使用Json.NET序列化库，支持泛型存档数据
    /// 所有存档存储在Application.persistentDataPath下的GameData文件夹中
    /// </summary>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 定义存档数据类型
    /// public class GameSaveData : SaveProfileData
    /// {
    ///     public int playerLevel = 1;
    ///     public int playerHealth = 100;
    ///     public Vector3 playerPosition = Vector3.zero;
    /// }
    /// 
    /// // 保存存档
    /// var profile = new SaveProfile<GameSaveData>("SaveSlot1");
    /// profile.data.playerLevel = 10;
    /// SaveManager.Save(profile);
    /// 
    /// // 加载存档
    /// if (SaveManager.hasSaveFile("SaveSlot1"))
    /// {
    ///     var loadedProfile = SaveManager.Load<GameSaveData>("SaveSlot1");
    ///     Debug.Log($"玩家等级: {loadedProfile.data.playerLevel}");
    /// }
    /// 
    /// // 删除存档
    /// SaveManager.Delete("SaveSlot1");
    /// </code>
    /// </remarks>
    public static class SaveManager
    {
        /// <summary>
        /// 存档文件夹路径
        /// 使用Application.persistentDataPath确保跨平台兼容性
        /// Windows: C:\Users\[用户]\AppData\LocalLow\[公司名]\[游戏名]
        /// Mac: ~/Library/Application Support/[公司名]/[游戏名]
        /// iOS: /var/mobile/Containers/Data/Application/[应用GUID]/Documents
        /// Android: /data/data/[包名]/files
        /// </summary>
        private static readonly string saveFolder = Application.persistentDataPath + "/GameData";

        /// <summary>
        /// 删除指定的存档文件
        /// 永久删除存档文件，无法恢复
        /// </summary>
        /// <param name="profileName">存档文件名（不包含路径）</param>
        /// <exception cref="Exception">当指定的存档文件不存在时抛出</exception>
        /// <remarks>
        /// 使用示例：
        /// <code>
        /// try
        /// {
        ///     SaveManager.Delete("AutoSave");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Debug.LogError($"删除存档失败: {ex.Message}");
        /// }
        /// </code>
        /// </remarks>
        public static void Delete(string profileName)
        {
            // 构建完整的文件路径
            string filePath = $"{saveFolder}/{profileName}";
            
            // 检查文件是否存在
            if (!File.Exists(filePath))
                throw new Exception($"存档文件 {profileName} 不存在");
            
            // 删除文件
            File.Delete(filePath);
        }

        /// <summary>
        /// 加载指定的存档文件
        /// 从文件系统读取存档文件，反序列化为存档配置对象
        /// </summary>
        /// <typeparam name="T">存档数据类型，必须继承自SaveProfileData</typeparam>
        /// <param name="profileName">存档文件名（不包含路径）</param>
        /// <returns>反序列化的存档配置对象</returns>
        /// <exception cref="Exception">当指定的存档文件不存在时抛出</exception>
        /// <exception cref="JsonException">当JSON反序列化失败时抛出</exception>
        /// <remarks>
        /// 使用JsonConvert进行反序列化，支持复杂的对象图
        /// 返回的对象包含完整的存档信息
        /// </remarks>
        public static SaveProfile<T> Load<T>(string profileName) where T : SaveProfileData
        {
            // 构建完整的文件路径
            string filePath = $"{saveFolder}/{profileName}";
            
            // 检查文件是否存在
            if (!File.Exists(filePath))
                throw new Exception($"存档文件 {profileName} 不存在");

            // 读取文件内容
            var fileContents = File.ReadAllText(filePath);
            
            // 反序列化JSON为对象
            return JsonConvert.DeserializeObject<SaveProfile<T>>(fileContents);
        }

        /// <summary>
        /// 保存存档配置对象到文件
        /// 将存档配置对象序列化为JSON格式并保存到文件系统
        /// 如果存档文件已存在，会先删除再创建
        /// </summary>
        /// <typeparam name="T">存档数据类型，必须继承自SaveProfileData</typeparam>
        /// <param name="profile">要保存的存档配置对象</param>
        /// <remarks>
        /// 序列化设置：
        /// 1. Formatting.Indented: 使用缩进格式化，便于人工阅读
        /// 2. ReferenceLoopHandling.Ignore: 忽略循环引用，防止无限递归
        /// 
        /// 如果存档文件夹不存在，会自动创建
        /// 
        /// 使用示例：
        /// <code>
        /// var saveData = new GameSaveData
        /// {
        ///     playerLevel = 10,
        ///     playerHealth = 100
        /// };
        /// var profile = new SaveProfile<GameSaveData>("AutoSave", saveData);
        /// SaveManager.Save(profile);
        /// </code>
        /// </remarks>
        public static void Save<T>(SaveProfile<T> profile) where T : SaveProfileData
        {
            // 构建完整的文件路径
            string filePath = $"{saveFolder}/{profile.name}";
            
            // 如果文件已存在，先删除
            if (File.Exists(filePath))
                File.Delete(filePath);

            // 序列化对象为JSON字符串
            var jsonString = JsonConvert.SerializeObject(profile, Formatting.Indented,
                new JsonSerializerSettings 
                { 
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
                });
            
            // 确保存档文件夹存在
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);
            
            // 写入文件
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// 检查指定存档文件是否存在
        /// 不加载文件内容，仅检查文件是否存在
        /// </summary>
        /// <param name="profileName">存档文件名（不包含路径）</param>
        /// <returns>如果存档文件存在返回true，否则返回false</returns>
        /// <remarks>
        /// 使用示例：
        /// <code>
        /// if (SaveManager.hasSaveFile("AutoSave"))
        /// {
        ///     Debug.Log("找到自动存档");
        /// }
        /// else
        /// {
        ///     Debug.Log("没有找到自动存档");
        /// }
        /// </code>
        /// </remarks>
        public static bool hasSaveFile(string profileName)
        {
            return File.Exists($"{saveFolder}/{profileName}");
        }
    }
}