using System;
using BehaviourTree.BehaviourTree;
using BehaviourTree.ExTools;
using Character.Player;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BehaviourTree.Test
{
    /// <summary>
    /// 行为树测试组件
    /// 挂载在游戏对象上，用于在运行时测试行为树功能
    /// 通过Odin Inspector在编辑器中配置行为树数据
    /// </summary>
    public class BehaviourTreeTest : SerializedMonoBehaviour
    {
        /// <summary>
        /// 行为树数据
        /// 通过Odin序列化，支持复杂数据结构的序列化
        /// 通过OpenViewAttribute在Inspector中显示"打开视图"按钮
        /// </summary>
        [OdinSerialize, HideReferenceObjectPicker, OpenView(ButtonName = "打开视图Button")]
        public BehaviourTreeData TreeData;
    }
    
    /// <summary>
    /// 并行执行节点
    /// 同时执行所有子节点，直到所有子节点完成或任一子节点失败
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"同时执行"
    /// </summary>
    [NodeLabel("同时执行")]
    public class Parallel : BtComposite
    {
        /// <summary>
        /// 当前执行的子节点索引
        /// 记录上次执行到的位置，用于恢复执行
        /// </summary>
        [LabelText("当前执行的")]
        private int _playIndex;
        
        /// <summary>
        /// 并行节点执行逻辑
        /// 从上次停止的位置继续执行子节点
        /// 任一子节点执行中时返回执行中，任一子节点失败时返回失败
        /// 所有子节点成功时返回成功
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            for (int i = _playIndex; i < ChildNodes.Count; i++)
            {
                var state = ChildNodes[i].Tick();
                switch (state)
                {
                    case BehaviourState.执行中:
                        return NodeState = state;
                    case BehaviourState.失败:
                        _playIndex = 0;
                        return NodeState = state;
                }
                if (i != ChildNodes.Count - 1) continue;
                _playIndex = 0;
                return NodeState = BehaviourState.成功;
            }
            ChangeFailState();
            return NodeState;
        }
    }
    
    /// <summary>
    /// 延时执行节点
    /// 等待指定时间后执行子节点
    /// 通过NodeLabel特性指定在编辑器中的显示名称和标签
    /// </summary>
    [NodeLabel("延时节点", Label = "延时执行")]
    public class Delay : BtPrecondition
    {
        /// <summary>
        /// 延时时间（秒）
        /// 在Inspector中显示为"延时"
        /// </summary>
        [LabelText("延时"), SerializeField]
        private float timer;
        
        /// <summary>
        /// 当前计时器
        /// 累计经过的时间
        /// </summary>
        private float _currentTimer;
        
        /// <summary>
        /// 延时节点执行逻辑
        /// 每帧累计时间，达到延时时间后执行子节点
        /// 执行中返回执行中状态，执行完成后返回成功状态
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            _currentTimer += Time.deltaTime;
            if (_currentTimer >= timer)
            {
                _currentTimer = 0f;
                ChildNode.Tick();
                return NodeState = BehaviourState.成功;
            }
            return NodeState = BehaviourState.执行中;
        }
    }
    
    /// <summary>
    /// 播放动画触发器节点
    /// 用于触发敌人的动画状态机中的Trigger参数
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"播放动画Trigger"
    /// </summary>
    [NodeLabel("播放动画Trigger")]
    public class AnimatorPlay : BtActionNode
    {
        /// <summary>
        /// 动画参数名称
        /// 在Inspector中显示为"名称"，在折叠组中显示
        /// </summary>
        [LabelText("名称"), SerializeField, FoldoutGroup("@NodeName")] 
        private string animationName;
        
        /// <summary>
        /// 敌人自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private Enemy _self;
        
        /// <summary>
        /// 启用标志
        /// 在Inspector中显示为"启用"，在折叠组中显示
        /// </summary>
        [LabelText("启用"), SerializeField, FoldoutGroup("@NodeName")]
        private bool isTrue;
        
        /// <summary>
        /// 播放动画触发器执行逻辑
        /// 设置敌人的Animator的Trigger参数
        /// 如果敌人Animator为空则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (!_self.anim)
            {
                ChangeFailState();
                return NodeState;
            }
            
            _self.anim.SetTrigger(animationName);
            return NodeState = BehaviourState.成功;
        }
    }
    
    /// <summary>
    /// 播放Boss动画布尔值节点
    /// 用于设置Boss的动画状态机中的Bool参数
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"播放动画Boss"
    /// </summary>
    [NodeLabel("播放动画Boss")]
    public class AnimatorPlayBool : BtActionNode
    {
        /// <summary>
        /// 动画参数名称
        /// 在Inspector中显示为"名称"，在折叠组中显示
        /// </summary>
        [LabelText("名称"), SerializeField, FoldoutGroup("@NodeName")] 
        private string animationName;
        
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// 启用标志
        /// 在Inspector中显示为"启用"，在折叠组中显示
        /// </summary>
        [LabelText("启用"), SerializeField, FoldoutGroup("@NodeName")]
        private bool isTrue;
        
        /// <summary>
        /// 播放Boss动画布尔值执行逻辑
        /// 设置Boss的Animator的Bool参数
        /// 如果Boss Animator为空则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (!_self.anim)
            {
                ChangeFailState();
                return NodeState;
            }
            
            _self.anim.SetBool(animationName, isTrue);
            return NodeState = BehaviourState.成功;
        }
    }
    
    /// <summary>
    /// 看到玩家检测节点
    /// 检测敌人是否能看到玩家
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"看到玩家"
    /// </summary>
    [NodeLabel("看到玩家")]
    public class CanSeePlayer : BtActionNode
    {
        /// <summary>
        /// 敌人自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private Enemy _self;
        
        /// <summary>
        /// 玩家目标引用
        /// 在Inspector中显示为"目标"，在折叠组中显示
        /// </summary>
        [LabelText("目标"), SerializeField, FoldoutGroup("@NodeName")]
        private Player _player;
        
        /// <summary>
        /// 目标偏移量
        /// 在Inspector中显示为"偏移量"，在折叠组中显示
        /// </summary>
        [LabelText("偏移量"), SerializeField, FoldoutGroup("@NodeName")]
        private Vector3 tatgetOffset;
        
        /// <summary>
        /// 发现距离
        /// 在Inspector中显示为"发现距离"，在折叠组中显示
        /// </summary>
        [LabelText("发现距离"), SerializeField, FoldoutGroup("@NodeName")]
        private float _warnRange = 30;
        
        /// <summary>
        /// 发现角度（视野角度）
        /// 在Inspector中显示为"发现角度"，在折叠组中显示
        /// </summary>
        [LabelText("发现角度"), SerializeField, FoldoutGroup("@NodeName")]
        private float fov = 90;
        
        /// <summary>
        /// 看到玩家检测执行逻辑
        /// 检查玩家是否在视野范围内，如果是则设置敌人目标并开始追逐
        /// </summary>
        /// <returns>节点执行状态：成功（看到玩家）或失败（未看到玩家）</returns>
        public override BehaviourState Tick()
        {
            if (WithinSight())
            {
                _self.agent.isStopped = false;
                _self.currentTarget = _player;
                _self.agent.SetDestination(_player.transform.position);
                return BehaviourState.成功;
            }
            return BehaviourState.失败;
        }
        
        /// <summary>
        /// 视野内检测
        /// 检查玩家是否在敌人的视野范围内
        /// 考虑距离、角度和视线遮挡
        /// </summary>
        /// <returns>如果玩家在视野内返回true，否则返回false</returns>
        private bool WithinSight()
        {
            if (_player == null)
            {
                return false;
            }
            
            var targetPosition = _player.transform.TransformPoint(tatgetOffset);
            var direction = targetPosition - _self.transform.position;
            direction.y = 0;
            var angle = Vector3.Angle(direction, _self.transform.forward);
            if (direction.magnitude < _warnRange && angle < fov * 0.5f)
            {
                RaycastHit hit;
                if (Physics.Linecast(_self.transform.position, targetPosition, out hit))
                {
                    if (hit.transform.IsChildOf(_player.transform) || _player.transform.IsChildOf(hit.transform))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// 追逐玩家节点
    /// 敌人追逐当前目标玩家
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"追逐玩家"
    /// </summary>
    [NodeLabel("追逐玩家")]
    public class Seek : BtActionNode
    {
        /// <summary>
        /// 敌人自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private Enemy _self;
        
        /// <summary>
        /// 追逐玩家执行逻辑
        /// 使用NavMeshAgent追逐当前目标，直到到达停止距离
        /// 到达目标时返回成功，否则返回执行中
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (!_self.agent.pathPending && _self.agent.remainingDistance <= _self.agent.stoppingDistance)
            {
                if (_self.agent.isOnNavMesh)
                {
                    _self.agent.isStopped = true;
                }
                return BehaviourState.成功;
            }
            
            if (_self.agent.destination != _self.currentTarget.transform.position)
            {
                _self.agent.SetDestination(_self.currentTarget.transform.position);
            }
            
            return BehaviourState.执行中;
        }
    }
    
    /// <summary>
    /// 追逐玩家Boss节点
    /// Boss追逐玩家
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"追逐玩家Boss"
    /// </summary>
    [NodeLabel("追逐玩家Boss")]
    public class SeekBoss : BtActionNode
    {
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// 追逐玩家Boss执行逻辑
        /// 使用NavMeshAgent追逐玩家，直到到达停止距离
        /// 到达目标时返回成功，否则返回执行中
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (!_self.agent.pathPending && _self.agent.remainingDistance <= _self.agent.stoppingDistance)
            {
                if (_self.agent.isOnNavMesh)
                {
                    //_self.agent.isStopped = true;
                }
                return BehaviourState.成功;
            }
            
            if (_self.agent.destination != _self.player.transform.position)
            {
                _self.agent.SetDestination(_self.player.transform.position);
            }
            
            return BehaviourState.执行中;
        }
    }
    
    /// <summary>
    /// 死亡检测节点
    /// 检测敌人是否死亡，死亡时执行子节点
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"如果死亡"
    /// </summary>
    [NodeLabel("如果死亡")]
    public class IsDead : BtPrecondition
    {
        /// <summary>
        /// 敌人自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private Enemy _self;
        
        /// <summary>
        /// 死亡检测执行逻辑
        /// 如果敌人死亡，执行子节点并返回成功
        /// 否则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (_self.isDead)
            {
                ChildNode.Tick();
                return BehaviourState.成功;
            }
            
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// 玩家入场检测节点
    /// 检测玩家是否进入Boss区域，触发Boss唤醒
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"玩家入场"
    /// </summary>
    [NodeLabel("玩家入场")]
    public class PlayerIn : BtPrecondition
    {
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// 玩家入场检测执行逻辑
        /// 如果玩家存在且Boss尚未唤醒，则唤醒Boss并执行子节点
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (_self.player && !_self.alreadyAwake)
            {
                _self.alreadyAwake = true;
                ChildNode?.Tick();
                
                Debug.Log(_self.agent.isStopped);
                return BehaviourState.成功;
            }
            
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// Boss死亡检测节点
    /// 检测Boss是否死亡，死亡时执行子节点
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"如果Boss死亡"
    /// </summary>
    [NodeLabel("如果Boss死亡")]
    public class IsDeadBoss : BtPrecondition
    {
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// Boss死亡检测执行逻辑
        /// 如果Boss血量小于等于0，执行子节点并返回成功
        /// 否则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (_self.hp <= 0)
            {
                ChildNode.Tick();
                return BehaviourState.成功;
            }
            
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// 玩家已入场检测节点
    /// 检测玩家是否已经入场（Boss已唤醒）
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"玩家是否已经入场"
    /// </summary>
    [NodeLabel("玩家是否已经入场")]
    public class PlayerNotIn : BtPrecondition
    {
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// 玩家已入场检测执行逻辑
        /// 如果Boss已唤醒，执行子节点并返回成功
        /// 否则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (_self.alreadyAwake)
            {
                ChildNode?.Tick();
                return BehaviourState.成功;
            }
            
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// Boss攻击节点
    /// Boss在攻击范围内攻击玩家
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"攻击玩家"
    /// </summary>
    [NodeLabel("攻击玩家")]
    public class BossAttack : BtActionNode
    {
        /// <summary>
        /// Boss自身引用
        /// 在Inspector中显示为"自己"，在折叠组中显示
        /// </summary>
        [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
        private BossManager _self;
        
        /// <summary>
        /// Boss攻击执行逻辑
        /// 如果玩家在攻击范围内，触发攻击动画
        /// 否则返回失败
        /// </summary>
        /// <returns>节点执行状态</returns>
        public override BehaviourState Tick()
        {
            if (_self.atk_range >= Vector3.Distance(_self.transform.position, _self.player.transform.position))
            {
                _self.anim.SetTrigger("Attack");
                return BehaviourState.成功;
            }
            
            return BehaviourState.失败;
        }
    }
    
    /// <summary>
    /// 行为树事件系统节点
    /// 通过委托条件控制子节点执行
    /// 通过NodeLabel特性指定在编辑器中的显示名称为"行为树事件系统"
    /// </summary>
    [NodeLabel("行为树事件系统")]
    public class BtEventSystem : BtPrecondition
    {
        /// <summary>
        /// 条件委托
        /// 用于动态判断是否执行子节点
        /// </summary>
        public Func<bool> a1;
        
        /// <summary>
        /// 事件系统执行逻辑
        /// 直接执行子节点，委托条件a1当前未使用
        /// 可以根据需要扩展为根据a1条件判断是否执行
        /// </summary>
        /// <returns>子节点的执行状态</returns>
        public override BehaviourState Tick()
        {
            return ChildNode.Tick();
        }
    }
}