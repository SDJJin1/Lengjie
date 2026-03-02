using Character.Player;
using EventCenter;
using EventCenter.Events;
using Save_Load;
using TMPro;
using UIManager;
using UnityEngine;
using UnityEngine.UI;

namespace UIForm
{
    /// <summary>
    /// 开始界面UI表单
    /// 继承自UIManager.UIForm，作为游戏的主菜单界面
    /// 提供开始新游戏、加载游戏、设置和退出功能
    /// 通过事件系统处理存档存在时的确认流程
    /// </summary>
    public class StartUIForm : UIManager.UIForm
    {
        /// <summary>
        /// 游戏标题文本
        /// 显示游戏的名称或Logo
        /// </summary>
        [SerializeField] 
        private TextMeshProUGUI Title;
        
        /// <summary>
        /// 开始游戏按钮
        /// 点击此按钮开始新游戏
        /// 如果已有存档，会弹出确认对话框
        /// </summary>
        [SerializeField] 
        private Button StartGameButton;
        
        /// <summary>
        /// 加载游戏按钮
        /// 点击此按钮加载已有存档
        /// 如果没有存档，按钮应为不可用状态
        /// </summary>
        [SerializeField] 
        private Button LoadGameButton;
        
        /// <summary>
        /// 设置按钮
        /// 点击此按钮打开设置界面
        /// 关闭开始界面，打开设置界面
        /// </summary>
        [SerializeField] 
        private Button SettingButton;
        
        /// <summary>
        /// 退出按钮
        /// 点击此按钮退出游戏
        /// 当前未实现功能
        /// </summary>
        [SerializeField] 
        private Button QuitButton;
        

        public override void OnOpen(object userData = null,   params object[] args)
        {
            if (!SaveManager.hasSaveFile("PlayerSaveData"))
            {
                LoadGameButton.gameObject.SetActive(false);
            }
            
            base.OnOpen(userData);

            // 绑定按钮事件
            StartGameButton.onClick.AddListener(ClickNewGameButton);
            LoadGameButton.onClick.AddListener(LoadGame);
            SettingButton.onClick.AddListener(OpenSettingUIForm);
        }

        public override void OnClose()
        {
            // 解绑按钮事件
            StartGameButton.onClick.RemoveAllListeners();
            LoadGameButton.onClick.RemoveAllListeners();
            SettingButton.onClick.RemoveListener(OpenSettingUIForm);
            
            base.OnClose();
        }

        #region 新游戏

        /// <summary>
        /// 点击新游戏按钮处理
        /// 检查是否存在存档，如果存在则弹出确认对话框
        /// 如果不存在，直接开始新游戏
        /// </summary>
        private void ClickNewGameButton()
        {
            if (SaveManager.hasSaveFile("PlayerSaveData"))
            {
                // 已有存档，打开确认对话框
                UIManager.UIManager.Instance.OpenUIForm(
                    EnumUIForm.ConfirmForm,
                    "You already have a save file. Do you want to delete?"
                );
                
                // 订阅确认和取消事件
                EventCenter.EventCenter.Subscribe(ConfirmedEventArgs.EventId, ConfirmNewGame);
                EventCenter.EventCenter.Subscribe(CanceledEventArgs.EventId, CancelNewGame);
            }
            else
            {
                // 没有存档，直接开始新游戏
                StartNewGame();
            }
        }

        /// <summary>
        /// 开始新游戏
        /// 创建新的玩家存档，保存到文件系统
        /// 关闭开始界面，打开加载界面进入游戏
        /// </summary>
        private void StartNewGame()
        {
            // 创建新的玩家存档数据
            var playerSave = new PlayerSaveData
            { 
                position = Vector3.zero,  // 起始位置
                rotation = Quaternion.identity,  // 默认朝向
                endurance = 10,  // 初始耐力
                vitality = 10  // 初始生命值
            };
            
            // 创建存档配置
            var saveProfile = new SaveProfile<PlayerSaveData>("PlayerSaveData", playerSave);
            
            // 设置到玩家相机
            PlayerCamera.instance.playerSaveData = saveProfile;
            PlayerCamera.instance.isLoad = false;  // 标记为新游戏，非加载
            
            // 保存存档
            SaveManager.Save(saveProfile);
            
            // 关闭开始界面，打开加载界面进入游戏
            UIManager.UIManager.Instance.CloseAllUIForms();
            UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.LoadingForm);
        }

        /// <summary>
        /// 确认新游戏事件处理
        /// 当用户在确认对话框中点击确认时调用
        /// 开始新游戏，并取消事件订阅
        /// </summary>
        private void ConfirmNewGame(object sender, GameEventArgs e)
        {
            
            // 开始新游戏
            StartNewGame();
        }


        /// <summary>
        /// 取消新游戏事件处理
        /// 当用户在确认对话框中点击取消时调用
        /// 关闭确认对话框，取消事件订阅
        /// </summary>
        private void CancelNewGame(object sender, GameEventArgs e)
        {
            
            // 关闭确认对话框
            UIManager.UIManager.Instance.CloseUIForm(EnumUIForm.ConfirmForm);
        }

        #endregion

        /// <summary>
        /// 加载游戏
        /// 从存档文件加载玩家数据
        /// 关闭开始界面，打开加载界面进入游戏
        /// </summary>
        private void LoadGame()
        {
            try
            {
                // 加载存档
                var playerData = SaveManager.Load<PlayerSaveData>("PlayerSaveData");
                
                // 设置到玩家相机
                PlayerCamera.instance.isLoad = true;  // 标记为加载
                PlayerCamera.instance.playerSaveData = playerData;
                
                // 关闭开始界面，打开加载界面进入游戏
                UIManager.UIManager.Instance.CloseAllUIForms();
                UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.LoadingForm);
            }
            catch (System.Exception ex)
            {
                // 加载失败，显示错误提示
                Debug.LogError($"加载存档失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开设置界面
        /// 关闭开始界面，打开设置界面
        /// </summary>
        private void OpenSettingUIForm()
        {
            UIManager.UIManager.Instance.CloseAllUIForms();
            UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.SettingForm);
        }
        
        /// <summary>
        /// 取消订阅所有事件
        /// 在界面关闭时清理所有事件订阅
        /// 防止内存泄漏和重复事件处理
        /// </summary>
        private void UnsubscribeAllEvents()
        {
            // 取消订阅确认事件
            EventCenter.EventCenter.Unsubscribe(ConfirmedEventArgs.EventId, ConfirmNewGame);
            EventCenter.EventCenter.Unsubscribe(CanceledEventArgs.EventId, CancelNewGame);
        }
    }
}
