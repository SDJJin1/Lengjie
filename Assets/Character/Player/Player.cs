using System;
using System.Collections.Generic;
using Character.Player.States;
using Character.Player.States.ActionStates;
using Character.Player.States.ActionStates.BlockState;
using Character.Player.States.ActionStates.HeavyAttack;
using Character.Player.States.ActionStates.Jump;
using Character.Player.States.UpperBodyStates;
using EventCenter;
using EventCenter.Events;
using Item;
using Save_Load;
using UnityEngine;

namespace Character.Player
{
    /// <summary>
    /// 玩家主控制器
    /// 管理玩家的所有核心功能：状态机、移动、战斗、装备、交互等
    /// </summary>
    public class Player : MonoBehaviour
    {
        [HideInInspector] public PlayerInputManager inputManager;
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Animator anim;
    
        public PlayerFSM PlayerFsmBase;
        public PlayerFSM PlayerUpperBody;
        public PlayerFSM PlayerActions;
        
        public bool isPerformingAction = false;
        public bool isLockOn = false;
        
        // 玩家基础属性
        [Header("PLAYER STATS")]
        public int endurance = 10;              // 耐力
        public float currentStamina;            // 当前耐力值
        public float maxStamina;                // 最大耐力值
        public int vitality = 10;               // 生命值
        public float currentHealth;             // 当前生命值
        public float maxHealth;                 // 最大生命值
        public float physicalDamageAbsorption;  // 物理伤害吸收
        
        // 耐力恢复系统
        [Header("Stamina Regeneration")]
        private float staminaTickTimer = 0;
        [HideInInspector] public float staminaRegenerationTimer = 0;
        [SerializeField] private float staminaRegenerationDelay = 2;    // 耐力恢复延迟
        [SerializeField] private float staminaRegenerationAmount = 2;   // 每次恢复量
    
        // 移动设置
        [Header("Movement Settings")]
        public float walkingSpeed = 2;          // 行走速度
        public float runningSpeed = 5;          // 奔跑速度
        public float sprintingSpeed = 7;        // 冲刺速度
        public float rotationSpeed = 15;        // 旋转速度
        public int sprintingStaminaCost = 2;    // 冲刺耐力消耗
        
        public Vector3 moveDirection;           // 当前移动方向
        [HideInInspector] public Vector3 targetRotationDirection; // 目标旋转方向
        
        // 闪避/翻滚设置
        [Header("Dodge")] 
        public float dodgeStaminaCost = 25;     // 闪避耐力消耗
        public Vector3 rollDirection;           // 翻滚方向

        // 跳跃设置
        [Header("Jump")] 
        public float jumpHeight = 4;            // 跳跃高度
        public float jumpStaminaCost = 25;      // 跳跃耐力消耗
        public float jumpForwardSpeed = 5;      // 跳跃前进速度
        public float freeFallSpeed = 2;         // 空中移动速度
        [HideInInspector] public Vector3 jumpDirection; // 跳跃方向
        
        // 地面检测
        [Header("Ground Check")]
        public bool isGrounded;                 // 是否在地面
        public LayerMask groundLayer;           // 地面层级
        public float groundCheckSphereRadius = 0.3f; // 地面检测球半径
        [HideInInspector] public Vector3 yVelocity;   // Y轴速度
        public float groundedYVelocity = -20;   // 地面Y轴速度
        public float fallStartYVelocity = -5;   // 下落开始Y轴速度
        public bool fallingVelocityHasBeenSet = false; // 下落速度是否已设置
        public float inAirTimer = 0;            // 空中计时器
        public float gravityForce = -5.55f;     // 重力
        
        [HideInInspector] public Vector3 contactPoint; // 受击接触点
        [HideInInspector] public float angleHitFrom;   // 受击角度
        
        // 装备槽位
        [Header("Equipments")] 
        public WeaponModelInstantiationSlot leftHandWeaponSlot;   // 左手武器槽位
        public WeaponModelInstantiationSlot leftHandShieldSlot;   // 左手盾牌槽位
        public WeaponModelInstantiationSlot rightHandSlot;        // 右手武器槽位
        public GameObject leftHandWeaponModel;                    // 左手武器模型
        public GameObject rightHandWeaponModel;                   // 右手武器模型
        
        // 护甲装备
        [Header("Armor")]
        public HeadArmorItem headArmorItem;     // 头部护甲
        public BodyArmorItem bodyArmorItem;     // 身体护甲
        public LegArmorItem legArmorItem;       // 腿部护甲
        
        // 背包系统
        [Header("Inventory")]
        public WeaponItem leftHandWeaponItem;   // 左手武器数据
        public WeaponItem rightHandWeaponItem;  // 右手武器数据
        public List<Item.Item> itemsInInventory;// 背包物品列表


        // 快捷槽系统
        [Header("Quick Slots", order = 1)] 
        public WeaponItem[] weaponsInRightHandSlots = new WeaponItem[3]; // 右手武器槽位
        public int rightHandWeaponIndex = 0;                            // 右手当前武器索引
        public WeaponItem[] weaponsInLeftHandSlots = new WeaponItem[3]; // 左手武器槽位
        public int leftHandWeaponIndex = 0;                             // 左手当前武器索引
        
        // 战斗状态
        public int currentLightAttackCombo = 0; // 当前轻攻击连击数
        public int currentHeavyAttackCombo = 0; // 当前重攻击连击数
        public float finalDamage = 0;           // 最终伤害值
        
        // 锁定系统
        [Header("Lock On")]
        public Enemy currentTarget;             // 当前锁定目标
        public Transform lockOnTransform;       // 锁定目标变换
        private Coroutine lockOnCoroutine;      // 锁定协程

        // 格挡系统
        [Header("Block")] 
        public Vector3 directionFromAttackToDamageTarget; // 攻击到受击目标方向
        private float dotValueFromAttackToDamageTarget;   // 方向点乘值
        public float blockingPhysicalAbsorption;          // 格挡物理伤害吸收

        // 药水系统
        [Header("HealthFlask")] 
        public int remainingFocusPointsFlasks = 5; // 剩余药水数量
        public int flaskRestoration = 50;          // 药水恢复量
        public GameObject flaskModel;              // 药水模型

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputManager = GetComponent<PlayerInputManager>();
            anim = GetComponent<Animator>();
            
            flaskModel.SetActive(false); // 初始隐藏药水模型
            
            InitializeWeaponSlots(); // 初始化武器槽位
        }

        private void OnEnable()
        {
            #region 状态机
            // 基础状态机：控制移动和基本状态
            PlayerFsmBase = new PlayerFSM();
            PlayerFsmBase.AddState(new PlayerIdleState(this, "Idle_1h", false));
            PlayerFsmBase.AddState(new PlayerGroundMoveState(this, "Move_1h", false));
            PlayerFsmBase.AddState(new BlockingIdleState(this, "BlockingIdle_1h", false));
            PlayerFsmBase.AddState(new BlockingMoveState(this, "BlockingMove_1h", false));
            
            // 上半身状态机：控制武器切换和物品使用
            PlayerUpperBody  = new PlayerFSM();
            PlayerUpperBody.AddState(new UpperBodyEmptyState(this, "UpperBodyEmpty", false));
            PlayerUpperBody.AddState(new SwitchLeftHandWeaponState(this, "Swap_weapon_L", false));
            PlayerUpperBody.AddState(new SwitchRightHandWeaponState(this, "Swap_weapon_R", false));
            PlayerUpperBody.AddState(new DrinkUpState(this, "Drink_Up", false));
            PlayerUpperBody.AddState(new DrinkDownState(this, "Drink_Down", false));
            PlayerUpperBody.AddState(new Drink01State(this, "Drink_01", false));
            PlayerUpperBody.AddState(new Drink02State(this, "Drink_02", false));
            PlayerUpperBody.AddState(new DrinkEmptyState(this, "Drink_Empty", false));
            
            // 动作状态机：控制攻击、闪避、跳跃等动作
            PlayerActions = new PlayerFSM();
            PlayerActions.AddState(new ActionEmptyState(this, "ActionEmpty", false));
            PlayerActions.AddState(new BackStepState(this, "Back_Step", true));
            PlayerActions.AddState(new RollState(this, "Roll", true));
            PlayerActions.AddState(new JumpStartState(this, "Jump_Start", true));
            PlayerActions.AddState(new JumpLiftState(this, "Jump_Lift", false));
            PlayerActions.AddState(new JumpIdleState(this, "Jump_Idle", false));
            PlayerActions.AddState(new JumpEndState(this, "Jump_End", true));
            PlayerActions.AddState(new PlayerDeadState(this, "Dead", true));
            PlayerActions.AddState(new LightAttackState(this, "Light_Attack", true));
            PlayerActions.AddState(new HitBackwardState(this, "Hit_Backward", true));
            PlayerActions.AddState(new HitForwardState(this, "Hit_Forward", true));
            PlayerActions.AddState(new HitRightState(this, "Hit_Right", true));
            PlayerActions.AddState(new HitLeftState(this, "Hit_Left", true));
            PlayerActions.AddState(new HeavyAttackChargeState(this, "Heavy_Attack_Charge", true));
            PlayerActions.AddState(new HeavyAttackHoldState(this, "Heavy_Attack_Hold", true));
            PlayerActions.AddState(new HeavyAttackReleaseState(this, "Heavy_Attack_Release", true));
            PlayerActions.AddState(new HeavyAttackReleaseFullState(this, "Heavy_Attack_Release_Full", true));
            PlayerActions.AddState(new RunAttackState(this, "Run_Attack", true));
            PlayerActions.AddState(new RollAttackState(this, "Roll_Attack", true));
            PlayerActions.AddState(new BackStepAttackState(this, "BackStep_Attack", true));
            PlayerActions.AddState(new BlockLightState(this, "Block_Light", true));
            PlayerActions.AddState(new BlockMediumState(this, "Block_Medium", true));
            PlayerActions.AddState(new BlockPingState(this, "Block_Ping", true));
            PlayerActions.AddState(new BlockColossal(this, "Block_Colossal", true));
            PlayerActions.AddState(new BlockHeavyState(this, "Block_Heavy", true));
            
            #endregion
        }

        private void Start()
        {
            // 忽略玩家自身碰撞体
            IgnoreMyOwnColliders();
            
            // 设置相机跟随玩家
            PlayerCamera.instance.player = this;
            
            // 加载初始武器
            LoadRightHandWeapon();
            LoadLeftHandWeapon();
            
            // 启动状态机
            PlayerFsmBase.SwitchOn(typeof(PlayerIdleState));
            PlayerUpperBody.SwitchOn(typeof(UpperBodyEmptyState));
            PlayerActions.SwitchOn(typeof(ActionEmptyState));

            // 加载存档数据
            if (PlayerCamera.instance.isLoad)
            {
                PlayerCamera.instance.isLoad = false;
                
                PlayerSaveData playerSaveData = PlayerCamera.instance.playerSaveData.saveData;
                transform.position = playerSaveData.position;
                transform.rotation = playerSaveData.rotation;
                vitality = playerSaveData.vitality;
                endurance = playerSaveData.endurance;
            }
            
            // 计算初始属性
            maxStamina = 10 * endurance;
            currentStamina = maxStamina;
            EventCenter.EventCenter.Fire(this, UpdateStaminaBarMaxValueEventArgs.Create(this, maxStamina));
            
            maxHealth = 15 * vitality;
            currentHealth = maxHealth;
            EventCenter.EventCenter.Fire(this, UpdateHealthBarMaxValueEventArgs.Create(this, maxHealth));
            
            // 更新UI武器图标
            EventCenter.EventCenter.Fire(this, UpdateLeftHandWeaponSlotEventArgs.Create(leftHandWeaponItem.itemIcon));
            EventCenter.EventCenter.Fire(this, UpdateRightHandWeaponSlotEventArgs.Create(rightHandWeaponItem.itemIcon));
        }

        private void FixedUpdate()
        {
            if (inputManager.interactInput)
            {
                inputManager.interactInput = false;
                Interact();
            }
        }

        private void Update()
        {
            // 更新所有状态机
            PlayerFsmBase.OnUpdate();
            PlayerUpperBody.OnUpdate();
            PlayerActions.OnUpdate();
            
            // 处理锁定目标输入
            HandleLockOnInput();
            HandleLockOnSwitchTargetInput();

            // 耐力恢复
            RegenerationStamina();

            // 地面检测
            HandleGroundCheck();
        }

        #region Input

        private void HandleLockOnInput()
        {
            if (isLockOn)
            {
                if (currentTarget == null)
                    return;

                // 如果目标死亡，取消锁定并寻找新目标
                if (currentTarget.isDead)
                {
                    isLockOn = false;
                    
                    if(lockOnCoroutine != null)
                        StopCoroutine(lockOnCoroutine);
                    
                    lockOnCoroutine = StartCoroutine(PlayerCamera.instance.WaitThenFindNewTarget());
                }
            }
        
            // 如果已锁定且按下锁定键，取消锁定
            if (inputManager.lockOnInput && isLockOn)
            {
                inputManager.lockOnInput = false;
                PlayerCamera.instance.ClearLockOnTargets();
                isLockOn = false;
            
                return;
            }
        
            // 如果未锁定且按下锁定键，尝试锁定目标
            if (inputManager.lockOnInput && !isLockOn)
            {
                inputManager.lockOnInput = false;
            
                PlayerCamera.instance.HandleLocatingLockOnTargets();

                if (PlayerCamera.instance.nearestLockOnTarget != null)
                {
                    currentTarget = PlayerCamera.instance.nearestLockOnTarget;
                    isLockOn = true;
                }
            }
        }
        
        
        private void HandleLockOnSwitchTargetInput()
        {
            // 切换到左侧目标
            if (inputManager.lockOnLeftInput)
            {
                inputManager.lockOnLeftInput = false;

                if (isLockOn)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.leftLockOnTarget != null)
                    {
                        currentTarget = PlayerCamera.instance.leftLockOnTarget;
                    }
                }
            }

            // 切换到右侧目标
            if (inputManager.lockOnRightInput)
            {
                inputManager.lockOnRightInput = false;

                if (isLockOn)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.rightLockOnTarget != null)
                    {
                        currentTarget = PlayerCamera.instance.rightLockOnTarget;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 地面检测：检查玩家是否站立在地面上
        /// </summary>
        private void HandleGroundCheck()
        {
            isGrounded = Physics.CheckSphere(transform.position, groundCheckSphereRadius, groundLayer);
        }

        /// <summary>
        /// 耐力恢复逻辑
        /// 当玩家不冲刺且处于空闲动作状态时，开始恢复耐力
        /// </summary>
        private void RegenerationStamina()
        {
            // 冲刺时不恢复耐力
            if(inputManager.sprintInput) return;
            // 非空闲动作状态时不恢复耐力
            if(PlayerActions.CurrentState.GetType() != typeof(ActionEmptyState)) return;
            
            staminaRegenerationTimer += Time.deltaTime;
            // 达到恢复延迟后开始恢复
            if (staminaRegenerationTimer >= staminaRegenerationDelay)
            {
                if (currentStamina < maxStamina)
                {
                    staminaTickTimer += Time.deltaTime;

                    // 每0.1秒恢复一次耐力
                    if (staminaTickTimer >= 0.1f)
                    {
                        staminaTickTimer = 0;
                        currentStamina += staminaRegenerationAmount;
                        EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, currentStamina / maxStamina));
                    }
                }
            }
        }

        #region Attack && Defense

         /// <summary>
        /// 承受伤害
        /// 处理玩家受到敌人攻击的逻辑，包括格挡判断、受击动画和伤害计算
        /// </summary>
        /// <param name="enemy">攻击来源的敌人</param>
        public void TakeDamage(Enemy enemy)
        {
            // 格挡状态下的受击处理
            if (PlayerFsmBase.CurrentState.GetType() == typeof(BlockingMoveState) || 
                PlayerFsmBase.CurrentState.GetType() == typeof(BlockingIdleState))
            {
                directionFromAttackToDamageTarget = transform.position - enemy.transform.position;
                dotValueFromAttackToDamageTarget =
                    Vector3.Dot(directionFromAttackToDamageTarget, enemy.transform.forward);
                // 如果攻击来自前方（点乘值大于0.3），则触发格挡
                if (dotValueFromAttackToDamageTarget > 0.3f)
                {
                    // 根据敌人架势伤害类型播放不同格挡动画
                    if (enemy.poiseDamage >= 120)
                    {
                        PlayerActions.ChangeState(typeof(BlockColossal));
                    }
                    else if (enemy.poiseDamage >= 70)
                    {
                        PlayerActions.ChangeState(typeof(BlockHeavyState));
                    }
                    else if (enemy.poiseDamage >= 30)
                    {
                        PlayerActions.ChangeState(typeof(BlockMediumState));
                    }
                    else if(enemy.poiseDamage >= 10)
                    {
                        PlayerActions.ChangeState(typeof(BlockLightState));
                    }

                    // 格挡状态下减免伤害
                    currentHealth -= Mathf.Clamp(enemy.damage, 0, enemy.damage - blockingPhysicalAbsorption);
                }
                else // 来自后方的攻击无法格挡
                {
                    currentHealth -= Mathf.Clamp(enemy.damage, 0, enemy.damage - physicalDamageAbsorption);
                }
            }
            else // 非格挡状态下的受击处理
            {
                // 计算受击点和受击角度
                contactPoint = GetComponentInChildren<Collider>().ClosestPointOnBounds(enemy.transform.position);
                angleHitFrom = Vector3.SignedAngle(enemy.transform.forward, transform.forward, Vector3.up);
                
                // 根据受击角度播放不同方向的受击动画
                if (angleHitFrom >= 145 && angleHitFrom <= 180)
                {
                    PlayerActions.ChangeState(typeof(HitForwardState));
                }
                else if (angleHitFrom <= -145 && angleHitFrom >= -180)
                {
                    PlayerActions.ChangeState(typeof(HitForwardState));
                }
                else if (angleHitFrom >= -45 && angleHitFrom <= 45)
                {
                    PlayerActions.ChangeState(typeof(HitBackwardState));
                }
                else if (angleHitFrom >= -144 && angleHitFrom <= -45)
                {
                    PlayerActions.ChangeState(typeof(HitLeftState));
                }
                else if (angleHitFrom >= 45 && angleHitFrom <= 144)
                {
                    PlayerActions.ChangeState(typeof(HitRightState));
                }
            
                // 非格挡状态下仅减免基础物理伤害
                currentHealth -= Mathf.Clamp(enemy.damage, 0, enemy.damage - physicalDamageAbsorption);
            }
            
            // 触发事件更新生命值UI
            EventCenter.EventCenter.Fire(this, UpdateHealthBarValueEventArgs.Create(this, currentHealth / maxHealth));
        }

        /// <summary>
        /// 初始化武器槽位引用
        /// </summary>
        private void InitializeWeaponSlots()
        {
            WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

            foreach (var weaponSlot in weaponSlots)
            {
                if (weaponSlot.weaponSlot == WeaponModelSlot.LeftHandWeapon) leftHandWeaponSlot = weaponSlot;
                else if (weaponSlot.weaponSlot == WeaponModelSlot.RightHand) rightHandSlot = weaponSlot;
                else if (weaponSlot.weaponSlot == WeaponModelSlot.LeftHandShield) leftHandShieldSlot = weaponSlot;
            }
        }
        
        /// <summary>
        /// 加载左手武器模型
        /// </summary>
        public void LoadLeftHandWeapon()
        {
            if (leftHandWeaponItem)
            {
                // 卸载当前武器模型
                if(leftHandShieldSlot.currentWeaponModel != null)
                    leftHandShieldSlot.UnLoadWeapon();
                if(leftHandWeaponSlot.currentWeaponModel != null)
                    leftHandWeaponSlot.UnLoadWeapon();

                // 根据武器类型加载到对应槽位
                if (leftHandWeaponItem.itemName == "Shield")
                {
                    leftHandWeaponModel = Instantiate(leftHandWeaponItem.weaponModel);
                    leftHandShieldSlot.LoadWeapon(leftHandWeaponModel);
                }
                else
                {
                    leftHandWeaponModel = Instantiate(leftHandWeaponItem.weaponModel);
                    leftHandWeaponSlot.LoadWeapon(leftHandWeaponModel);
                }
            }
        }

        /// <summary>
        /// 加载右手武器模型
        /// </summary>
        public void LoadRightHandWeapon()
        {
            if (rightHandWeaponItem)
            {
                rightHandSlot.UnLoadWeapon();
                rightHandWeaponModel = Instantiate(rightHandWeaponItem.weaponModel);
                rightHandSlot.LoadWeapon(rightHandWeaponModel);
            }
        }

        /// <summary>
        /// 忽略玩家自身碰撞体之间的碰撞
        /// 防止玩家武器伤害到自己或装备之间相互碰撞
        /// </summary>
        private void IgnoreMyOwnColliders()
        {
            Collider characterControllerCollider = GetComponent<Collider>();
            Collider[] damageableCharacterColliders = GetComponentsInChildren<Collider>();
            List<Collider> ignoreColliders = new List<Collider>();

            foreach (var collider in damageableCharacterColliders)
            {
                ignoreColliders.Add(collider);
            }
            
            ignoreColliders.Add(characterControllerCollider);

            foreach (var collider in ignoreColliders)
            {
                foreach (var otherCollider in ignoreColliders)
                {
                    Physics.IgnoreCollision(collider, otherCollider, true);
                }
            }
        }
        
        public void OpenDamageCollider()
        {
            rightHandSlot.damageCollider.enabled = true;
        }

        public void CloseDamageCollider()
        {
            rightHandSlot.damageCollider.enabled = false;
        }

        #endregion

        #region Equipment

        /// <summary>
        /// 加载头部护甲并重新计算伤害吸收
        /// </summary>
        /// <param name="headArmorItem">头部护甲数据</param>
        public void LoadHeadEquipment(HeadArmorItem headArmorItem)
        {
            this.headArmorItem = headArmorItem;
            CalculateArmorAbsorption();
        }

        /// <summary>
        /// 加载身体护甲并重新计算伤害吸收
        /// </summary>
        /// <param name="bodyArmorItem">身体护甲数据</param>
        public void LoadBodyEquipment(BodyArmorItem bodyArmorItem)
        {
            this.bodyArmorItem = bodyArmorItem;
            CalculateArmorAbsorption();
        }

        /// <summary>
        /// 加载腿部护甲并重新计算伤害吸收
        /// </summary>
        /// <param name="legArmorItem">腿部护甲数据</param>
        public void LoadLegEquipment(LegArmorItem legArmorItem)
        {
            this.legArmorItem = legArmorItem;
            CalculateArmorAbsorption();
        }

        /// <summary>
        /// 计算总护甲伤害吸收值
        /// </summary>
        public void CalculateArmorAbsorption()
        {
            physicalDamageAbsorption = 0;

            if (headArmorItem)
            {
                physicalDamageAbsorption += headArmorItem.physicalDamageAbsorption;
            }

            if (legArmorItem)
            {
                physicalDamageAbsorption += legArmorItem.physicalDamageAbsorption;
            }

            if (bodyArmorItem)
            {
                physicalDamageAbsorption += bodyArmorItem.physicalDamageAbsorption;
            }
        }

        #endregion

        /// <summary>
        /// 恢复生命值
        /// </summary>
        /// <param name="healthRestoration">恢复量</param>
        public void RestoreHealth(int healthRestoration)
        {
            currentHealth = Mathf.Clamp(currentHealth, currentHealth + healthRestoration, maxHealth);
            EventCenter.EventCenter.Fire(this, UpdateHealthBarValueEventArgs.Create(this, currentHealth));
        }
        
        /// <summary>
        /// 动画根运动处理
        /// 当动画应用根运动时，将动画的位移和旋转应用到角色控制器
        /// </summary>
        private void OnAnimatorMove()
        {
            if (anim.applyRootMotion)
            {
                Vector3 velocity = anim.deltaPosition;
                characterController.Move(velocity);
                transform.rotation *= anim.deltaRotation;
            }
        }

        /// <summary>
        /// 与周围可交互对象进行交互
        /// </summary>
        private void Interact()
        {
            // 检测玩家周围1米内的可交互对象
            Collider[] result = Physics.OverlapSphere(this.transform.position, 1f, LayerMask.GetMask("Interactable"));
            if (result.Length > 0)
            {
                foreach (Collider collider in result)
                {
                    Interactable[] interactables = collider.GetComponents<Interactable>();
                    foreach (Interactable interactable in interactables)
                    {
                        interactable.Interact(this);
                    }
                }
            }
        }

        public void CostHealth(int value)
        {
            currentHealth = Mathf.Clamp(currentHealth - value, 0, maxHealth);
            EventCenter.EventCenter.Fire(this, UpdateHealthBarValueEventArgs.Create(this, currentHealth / maxHealth));
        }
        
        public void CostStamina(int value)
        {
            currentStamina = Mathf.Clamp(currentStamina - value, 0, maxStamina);
            EventCenter.EventCenter.Fire(this, UpdateStaminaBarValueEventArgs.Create(this, currentStamina / maxStamina));
        }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        /// <param name="item">要添加的物品</param>
        public void AddItemToInventory(Item.Item item)
        {
            itemsInInventory.Add(item);
        }

        /// <summary>
        /// 从背包中移除物品
        /// </summary>
        /// <param name="item">要移除的物品</param>
        public void RemoveItemFromInventory(Item.Item item)
        {
            itemsInInventory.Remove(item);

            // 清理背包中的空引用
            for (int i = itemsInInventory.Count - 1; i >= 0; i--)
            {
                if (itemsInInventory[i] == null)
                {
                    itemsInInventory.RemoveAt(i);
                }
            }
        }
    }
}
