using System.Collections;
using System.Collections.Generic;
using Save_Load;
using UnityEngine;

namespace Character.Player
{
    /// <summary>
    /// 玩家相机控制器
    /// 处理相机跟随、旋转、碰撞检测和锁定目标系统
    /// 单例模式确保场景中只有一个玩家相机实例
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;

        // 相机组件和参考
        public Camera cameraObject;                     // 相机对象
        [SerializeField] Transform cameraPivotTransform; // 相机旋转支点
        public Player player;                          // 玩家引用

        // 存档相关
        public bool isLoad = false;                     // 是否正在加载存档
        public SaveProfile<PlayerSaveData> playerSaveData; // 玩家存档数据
    
        // 相机设置
        [Header("Camera Settings")]
        private float cameraSmoothSpeed = 1;            // 相机跟随平滑速度
        [SerializeField] private float leftAndRightRotationSpeed = 220; // 左右旋转速度
        [SerializeField] private float upAndDownRotationSpeed = 220;    // 上下旋转速度
        [SerializeField] private float minimumPivot = -30;              // 最小俯仰角
        [SerializeField] private float maximumPivot = 60;               // 最大俯仰角
        [SerializeField] private float cameraCollisionRadius = 0.2f;    // 相机碰撞检测半径
        [SerializeField] private LayerMask collideWithLayers;           // 碰撞检测层级
    
        // 相机数值
        [Header("Camera Values")]
        private Vector3 cameraVelocity;                 // 相机速度（用于平滑跟随）
        private Vector3 cameraObjectPosition;           // 相机本地位置
        [SerializeField] private float leftAndRightLookAngle; // 左右视角角度
        [SerializeField] private float upAndDownLookAngle;    // 上下视角角度
        private float cameraZPosition;                  // 相机Z轴初始位置
        private float targetCameraZPosition;            // 目标Z轴位置（用于碰撞检测）
        
        // 锁定系统
        [Header("Lock On")] 
        [SerializeField] private float lockOnRadius = 20f;              // 锁定半径
        [SerializeField] private float minimumViewableAngle = -50;      // 最小可视角度
        [SerializeField] private float maximumViewableAngle = 50;       // 最大可视角度
        [SerializeField] private float lockOnTargetFollowSpeed = 0.2f;  // 锁定目标跟随速度
        [SerializeField] private float setCameraHeightSpeed = 1;        // 相机高度调整速度
        [SerializeField] private float unlockedCameraHeight = 1.65f;    // 未锁定时的相机高度
        [SerializeField] private float lockedCameraHeight = 2.0f;       // 锁定时的相机高度
        private Coroutine cameraLockOnHeightCoroutine;  // 相机高度调整协程
        private List<Enemy> availableTargets = new List<Enemy>(); // 可锁定目标列表
        public Enemy nearestLockOnTarget;               // 最近的锁定目标
        public Enemy leftLockOnTarget;                  // 左侧锁定目标
        public Enemy rightLockOnTarget;                 // 右侧锁定目标
    
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            cameraZPosition = cameraObject.transform.localPosition.z;
        }

        private void LateUpdate()
        {
            HandleAllCameraActions();
        }

        #region 摄像机移动

        /// <summary>
        /// 处理所有相机动作：跟随、旋转、碰撞检测
        /// </summary>
        public void HandleAllCameraActions()
        {
            if (player != null)
            {
                HandleFollowTarget();
                HandleRotations();
                HandleCollisions();
            }
        }
    
        /// <summary>
        /// 处理相机跟随目标：平滑移动到玩家位置
        /// </summary>
        private void HandleFollowTarget()
        {
            Vector3 targetCameraPosition = Vector3.SmoothDamp(
                transform.position, 
                player.transform.position, 
                ref cameraVelocity, 
                cameraSmoothSpeed * Time.deltaTime);
        
            transform.position = targetCameraPosition;
        }
    
        /// <summary>
        /// 处理相机旋转：根据锁定状态决定旋转方式
        /// </summary>
        private void HandleRotations()
        {
            // 锁定目标时的旋转逻辑
            if (player.isLockOn)
            {
                Vector3 rotationDirection = player.currentTarget.lockOnTransform.position - transform.position;
                rotationDirection.Normalize();
                rotationDirection.y = 0;
            
                // 相机水平旋转（Y轴）面向目标
                Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnTargetFollowSpeed);
            
                // 相机支点旋转，使相机对准目标
                rotationDirection = player.currentTarget.lockOnTransform.position - cameraPivotTransform.position;
                rotationDirection.Normalize();

                targetRotation = Quaternion.LookRotation(rotationDirection);
                cameraPivotTransform.transform.rotation = Quaternion.Slerp(cameraPivotTransform.rotation, targetRotation,
                    lockOnTargetFollowSpeed);
            
                // 更新视角角度
                leftAndRightLookAngle = transform.eulerAngles.y;
                upAndDownLookAngle = transform.eulerAngles.x;
            }
            // 未锁定状态下的自由旋转
            else
            {
                // 根据输入更新视角角度
                leftAndRightLookAngle += (player.inputManager.cameraHorizontalInput * leftAndRightRotationSpeed) * Time.deltaTime;
                upAndDownLookAngle -= (player.inputManager.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
                upAndDownLookAngle  = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

                Vector3 cameraRotation = Vector3.zero;
                Quaternion targetRotation;
    
                // 应用水平旋转
                cameraRotation.y = leftAndRightLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                transform.rotation = targetRotation;
    
                // 应用垂直旋转
                cameraRotation = Vector3.zero;
                cameraRotation.x = upAndDownLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                cameraPivotTransform.localRotation = targetRotation;
            }
        }
    
        /// <summary>
        /// 处理相机碰撞检测：防止相机穿墙
        /// </summary>
        private void HandleCollisions()
        {
            targetCameraZPosition = cameraZPosition;
            RaycastHit hit;
            Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
            direction.Normalize();

            // 球形射线检测相机与墙壁的碰撞
            if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCameraZPosition), collideWithLayers))
            {
                float distanceFromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
                targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);
            }

            // 避免相机过于靠近支点
            if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius)
            {
                targetCameraZPosition = -cameraCollisionRadius;
            }
        
            // 平滑调整相机位置
            cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
            cameraObject.transform.localPosition = cameraObjectPosition;
        }

        #endregion


        #region 锁定

        /// <summary>
        /// 处理锁定目标定位：搜索范围内的敌人并确定最近、左侧和右侧目标
        /// </summary>
        public void HandleLocatingLockOnTargets()
        {
            float shortestDistance = Mathf.Infinity;        // 最近目标距离
            float shortestDistanceOfRightTarget = Mathf.Infinity; // 右侧目标最短距离
            float shortestDistanceOfLeftTarget = -Mathf.Infinity;  // 左侧目标最短距离

            // 在锁定半径内搜索所有角色层级的碰撞体
            Collider[] colliders = Physics.OverlapSphere(player.transform.position, lockOnRadius, LayerMask.GetMask("Character"));

            // 第一遍：收集所有符合条件的敌人
            for (int i = 0; i < colliders.Length; i++)
            {
                Enemy lockOnTarget = colliders[i].gameObject.GetComponent<Enemy>();

                if (lockOnTarget != null) 
                {
                    Vector3 lockOnTargetsDirection = lockOnTarget.transform.position - player.transform.position;
                    float distanceFromTarget = Vector3.Distance(player.transform.position, lockOnTarget.transform.position);
                    float viewableAngle = Vector3.Angle(lockOnTargetsDirection, cameraObject.transform.forward);
                
                    // 排除已死亡的目标
                    if(lockOnTarget.isDead)
                        continue;
                
                    // 排除玩家自身
                    if(lockOnTarget.transform.root == player.transform.root)
                        continue;
                
                    // 检查目标是否在视角范围内
                    if (viewableAngle > minimumViewableAngle && viewableAngle < maximumViewableAngle)
                    {
                        RaycastHit hit;

                        // 检查视线是否被遮挡
                        if (Physics.Linecast(player.lockOnTransform.position,
                                  lockOnTarget.lockOnTransform.position, out hit,
                                LayerMask.GetMask("Default")))
                        {
                            continue;
                        }
                        else
                        {
                            availableTargets.Add(lockOnTarget);
                        }
                    }
                }
            }

            // 第二遍：从可用目标中确定最近、左侧和右侧目标
            for (int k = 0; k < availableTargets.Count; k++)
            {
                if (availableTargets[k] != null)
                {
                    float distanceFromTarget = Vector3.Distance(player.transform.position, availableTargets[k].transform.position);
                
                    // 更新最近目标
                    if (distanceFromTarget < shortestDistance)
                    {
                        shortestDistance = distanceFromTarget;
                        nearestLockOnTarget = availableTargets[k];
                    }

                    // 如果已锁定，寻找左右侧目标
                    if (player.isLockOn)
                    {
                        Vector3 relativeEnemyPosition = player.transform.InverseTransformPoint(availableTargets[k].transform.position);
                        var distanceFromLeftTarget = relativeEnemyPosition.x;
                        var distanceFromRightTarget = relativeEnemyPosition.x;

                        // 跳过当前锁定目标
                        if(availableTargets[k] == player.currentTarget)
                            continue;
                    
                        // 更新左侧目标（相对位置x <= 0）
                        if (relativeEnemyPosition.x <= 0.00 && distanceFromLeftTarget > shortestDistanceOfLeftTarget)
                        {
                            shortestDistanceOfLeftTarget = distanceFromLeftTarget;
                            leftLockOnTarget = availableTargets[k];
                        }
                        // 更新右侧目标（相对位置x >= 0）
                        else if (relativeEnemyPosition.x >= 0.00 && distanceFromRightTarget < shortestDistanceOfRightTarget)
                        {
                            shortestDistanceOfRightTarget = distanceFromRightTarget;
                            rightLockOnTarget = availableTargets[k];
                        }
                    }
                }
                else
                {
                    // 如果目标无效，清除锁定目标
                    ClearLockOnTargets();
                    player.isLockOn = false;
                }
            }
        }

        /// <summary>
        /// 设置锁定相机高度：根据锁定状态调整相机高度
        /// </summary>
        public void SetLockCameraHeight()
        {
            if (cameraLockOnHeightCoroutine != null)
            {
                StopCoroutine(cameraLockOnHeightCoroutine);
            }

            cameraLockOnHeightCoroutine = StartCoroutine(SetCameraHeight());
        }

        /// <summary>
        /// 清除所有锁定目标
        /// </summary>
        public void ClearLockOnTargets()
        {
            nearestLockOnTarget = null;
            leftLockOnTarget = null;
            rightLockOnTarget = null;
            availableTargets.Clear();
        }

        /// <summary>
        /// 等待后寻找新目标协程：在玩家不执行动作时寻找新目标
        /// </summary>
        public IEnumerator WaitThenFindNewTarget()
        {
            while (player.isPerformingAction)
            {
                yield return null;
            }
    
            ClearLockOnTargets();
            HandleLocatingLockOnTargets();

            if (nearestLockOnTarget != null)
            {
                player.currentTarget = nearestLockOnTarget; 
                player.isLockOn = true;
            }
    
            yield return null;
        }

        // 用于相机高度平滑调整的速度向量
        Vector3 velocity = Vector3.zero;

        /// <summary>
        /// 设置相机高度协程：平滑过渡相机高度
        /// </summary>
        private IEnumerator SetCameraHeight()
        {
            float duration = 1;
            float timer = 0;
    
            Vector3 newLockedCameraHeight = new Vector3(cameraPivotTransform.transform.localPosition.x, lockedCameraHeight);
            Vector3 newUnlockedCameraHeight =
                new Vector3(cameraPivotTransform.transform.localPosition.x, unlockedCameraHeight);

            while (timer < duration)
            {
                timer += Time.deltaTime;

                if (player != null)
                {
                    // 锁定状态：相机抬升到锁定高度，并重置旋转
                    if (player.currentTarget != null)
                    {
                        cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(
                        cameraPivotTransform.transform.localPosition, newLockedCameraHeight, ref velocity,
                        lockOnTargetFollowSpeed);
                        cameraPivotTransform.transform.localRotation = Quaternion.Slerp(
                        cameraPivotTransform.transform.localRotation, Quaternion.Euler(0, 0, 0),
                        lockOnTargetFollowSpeed);
                    }
                    // 未锁定状态：相机恢复到未锁定高度
                    else
                    {
                        cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(
                            cameraPivotTransform.transform.localPosition, newUnlockedCameraHeight, ref velocity,
                            setCameraHeightSpeed);
                    }
                }
        
                yield return null;
            }

            // 确保最终位置准确
            if (player != null)
            {
                if (player.currentTarget != null)
                {
                    cameraPivotTransform.transform.localPosition = newLockedCameraHeight;
                    cameraPivotTransform.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    cameraPivotTransform.transform.localPosition = newUnlockedCameraHeight;
                }
            }
    
            yield return null;
        }

    #endregion
    
    }
}
