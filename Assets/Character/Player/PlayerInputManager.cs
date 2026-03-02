using UIManager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character.Player
{
    /// <summary>
    /// 玩家输入管理器
    /// 负责处理玩家所有输入事件，包括移动、相机控制、动作输入和UI交互
    /// 使用Unity新的Input System处理输入
    /// </summary>
    public class PlayerInputManager : MonoBehaviour
    {
        public PlayerInputActions PlayerInputActions;    // 输入动作资产引用
        public InputAction movement;                     // 移动输入动作
        public InputAction cameraMovement;               // 相机移动输入动作
    
        // 移动输入
        [Header("MOVEMENT INPUT")]
        [SerializeField] private Vector2 movementInput;  // 原始移动输入值
        public float horizontalInput;                    // 水平输入（-1到1）
        public float verticalInput;                      // 垂直输入（-1到1）
        public float moveAmount;                         // 移动总量（0到1或2）
    
        // 相机移动输入
        [Header("CAMERA MOVEMENT INPUT")]
        [SerializeField] private Vector2 cameraInput;    // 原始相机输入值
        public float cameraHorizontalInput;              // 相机水平输入（-1到1）
        public float cameraVerticalInput;                // 相机垂直输入（-1到1）

        // 玩家动作输入标志
        [Header("PLAYER ACTION INPUT")] 
        public bool dodgeInput;          // 闪避/翻滚输入
        public bool sprintInput;         // 冲刺输入
        public bool jumpInput;           // 跳跃输入
        public bool lightAttackInput;    // 轻攻击输入
        public bool heavyAttackInput;    // 重攻击输入
        public bool switchLeftWeaponInput;   // 切换左手武器输入
        public bool switchRightWeaponInput;  // 切换右手武器输入
        public bool lockOnInput;         // 锁定目标输入
        public bool lockOnLeftInput;     // 向左切换锁定目标输入
        public bool lockOnRightInput;    // 向右切换锁定目标输入
        public bool interactInput;       // 交互输入
        public bool blockInput;          // 格挡输入
        public bool twoHandLeftWeaponInput;  // 双持左手武器输入
        public bool twoHandRightWeaponInput; // 双持右手武器输入
        public bool pauseInput;          // 暂停输入
        public bool useItemInput;        // 使用物品输入（喝药水）
        
        private void Awake()
        {
            PlayerInputActions = InputManager.inputActions;
        }

        private void OnEnable()
        {
            // 移动输入
            movement = PlayerInputActions.Player.Movement;
            movement.Enable();
        
            // 相机移动输入
            cameraMovement = PlayerInputActions.PlayerCamera.PlayerCameraMovement;
            cameraMovement.Enable();
            
            // 闪避输入（按下时触发）
            PlayerInputActions.Player.Dodge.performed += i => dodgeInput = true;
            PlayerInputActions.Player.Dodge.Enable();
            
            // 冲刺输入（按下/松开时触发）
            PlayerInputActions.Player.Sprint.performed += s => sprintInput = true;
            PlayerInputActions.Player.Sprint.canceled += s => sprintInput = false;
            PlayerInputActions.Player.Sprint.Enable();
            
            // 跳跃输入（按下时触发）
            PlayerInputActions.Player.Jump.performed += j => jumpInput = true;
            PlayerInputActions.Player.Jump.Enable();

            // 轻攻击输入（按下时触发）
            PlayerInputActions.Player.LightAttack.performed += i => lightAttackInput = true;
            PlayerInputActions.Player.LightAttack.Enable();

            // 重攻击输入（按下和松开时触发）
            PlayerInputActions.Player.HeavyAttack.performed += i => heavyAttackInput = true;
            PlayerInputActions.Player.HeavyAttack.canceled += i => heavyAttackInput = false;
            PlayerInputActions.Player.HeavyAttack.Enable();

            // 切换左手武器输入（按下时触发）
            PlayerInputActions.Player.SwapWeapon_L.performed += i => switchLeftWeaponInput = true;
            PlayerInputActions.Player.SwapWeapon_L.Enable();
            
            // 切换右手武器输入（按下时触发）
            PlayerInputActions.Player.SwapWeapon_R.performed += i => switchRightWeaponInput = true;
            PlayerInputActions.Player.SwapWeapon_R.Enable();
            
            // 锁定目标输入（按下时触发）
            PlayerInputActions.Player.LockOn.performed += l => lockOnInput = true;
            PlayerInputActions.Player.LockOn.Enable();
            
            // 交互输入（按下时触发）
            PlayerInputActions.Player.Interact.performed += i => interactInput = true;
            PlayerInputActions.Player.Interact.Enable();
            
            // 格挡输入（按下/松开时触发）
            PlayerInputActions.Player.Block.performed += i => blockInput = true;
            PlayerInputActions.Player.Block.canceled += i => blockInput = false;
            PlayerInputActions.Player.Block.Enable();
            
            // 双持左手武器输入（按下/松开时触发）
            PlayerInputActions.Player.TwoHandLeftWeapon.performed += i => twoHandLeftWeaponInput = true;
            PlayerInputActions.Player.TwoHandLeftWeapon.canceled += i => twoHandLeftWeaponInput = false;
            PlayerInputActions.Player.TwoHandLeftWeapon.Enable();
            
            // 双持右手武器输入（按下/松开时触发）
            PlayerInputActions.Player.TwoHandRightWeapon.performed += i => twoHandRightWeaponInput = true;
            PlayerInputActions.Player.TwoHandRightWeapon.canceled += i => twoHandRightWeaponInput = false;
            PlayerInputActions.Player.TwoHandRightWeapon.Enable();
            
            // 使用物品输入（按下时触发）
            PlayerInputActions.Player.UseItem.performed += i => useItemInput = true;
            PlayerInputActions.Player.UseItem.Enable();
            
            // 暂停输入（按下时触发，切换暂停状态）
            PlayerInputActions.UI.Pause.performed += i => HandlePauseInput();
            PlayerInputActions.UI.Pause.Enable();
        }

        private void Update()
        {
            HandleAllInputs();
        }

        private void HandleAllInputs()
        {
            HandlePlayerMovementInput();
            HandleCameraInput();
        }

        private void HandlePlayerMovementInput()
        {
            movementInput = movement.ReadValue<Vector2>();
        
            verticalInput = movementInput.y;
            horizontalInput = movementInput.x;
        
            // 计算移动总量（水平和垂直分量的绝对值之和）
            moveAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

            // 标准化移动量：小于等于0.5时为0.5，大于0.5时为1
            if (moveAmount <= 0.5 && moveAmount > 0)
            {
                moveAmount = 0.5f;
            }
            else if (moveAmount > 0.5 && moveAmount <= 1)
            {
                moveAmount = 1;
            }

            // 冲刺时移动量设为2
            if (sprintInput)
            {
                moveAmount = 2;
            }
        }

        /// <summary>
        /// 处理相机输入
        /// 读取相机控制输入并转换为水平和垂直分量
        /// </summary>
        private void HandleCameraInput()
        {
            cameraInput = cameraMovement.ReadValue<Vector2>();
        
            cameraVerticalInput = cameraInput.y;
            cameraHorizontalInput = cameraInput.x;
        }

        /// <summary>
        /// 处理暂停输入
        /// 切换暂停状态，打开或关闭暂停界面
        /// </summary>
        private void HandlePauseInput()
        {
            pauseInput = !pauseInput;
            if (pauseInput)
            {
                UIManager.UIManager.Instance.CloseAllUIForms();
                UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.PauseForm);
                
                PlayerInputActions.PlayerCamera.Disable();
                PlayerInputActions.Player.Disable();
            }
            else
            {
                UIManager.UIManager.Instance.CloseAllUIForms();
                UIManager.UIManager.Instance.OpenUIForm(EnumUIForm.HUDForm);
                PlayerInputActions.PlayerCamera.Enable();
                PlayerInputActions.Player.Enable();
            }
        }

        /// <summary>
        /// 禁用时取消输入事件注册
        /// </summary>
        private void OnDisable()
        {
            movement.Disable();
            PlayerInputActions.Player.Dodge.Disable();
        }
    }
}
