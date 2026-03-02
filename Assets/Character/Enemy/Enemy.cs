using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IHitAble
{
    public NavMeshAgent agent;
    public Animator anim;
    
    // 生命值UI显示
    public CharacterHPBar hpBar;
    
    // 生命值相关属性
    public float health = 100;
    public float maxHealth = 100;
    public float damage;           // 敌人攻击伤害
    public float poiseDamage;      // 架势伤害
    public Transform lockOnTransform; // 锁定目标时的参考变换
    public Player currentTarget;   // 当前锁定目标玩家
    public bool isDead = false;    // 死亡状态标识
    public float findDistance = 15; // 发现目标距离

    // 敌人行为树控制器
    public ZombieBehaviourTree BehaviourTree;

    // 伤害碰撞体（用于攻击检测）
    public Collider damage1;
    public Collider damage2;

    // 最近受伤害的玩家
    public Player DamagedPlayer;
    
    [HideInInspector] public Vector3 contactPoint;  // 受击接触点
    [HideInInspector] public float angleHitFrom;    // 受击角度
    
    public EnemyDamageCollider[] damageColliders;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        BehaviourTree = GetComponent<ZombieBehaviourTree>();

        damageColliders = GetComponentsInChildren<EnemyDamageCollider>();
    }

    private void Start()
    {
        BehaviourTree.TreeData.OnStart();
    }

    private void Update()
    {
        anim.SetFloat("Velocity", agent.velocity.magnitude);
    }

    private void OnDisable()
    {
        BehaviourTree.TreeData.OnStop();
    }

    /// <summary>
    /// 承受伤害接口实现
    /// 处理玩家攻击造成的伤害和受击反应
    /// </summary>
    /// <param name="player">攻击来源的玩家</param>
    public void TakeDamage(Player player)
    {
        health -= player.finalDamage;
        
        hpBar.SetStat((int)health);

        if (health <= 0)
        {
            Dead();
        }
        else
        {
            // 计算受击点和受击角度
            contactPoint = GetComponentInChildren<Collider>().ClosestPointOnBounds(player.transform.position);
            angleHitFrom = Vector3.SignedAngle(player.transform.forward, transform.forward, Vector3.up);
                
            // 根据受击角度决定播放前/后受击动画
            if (angleHitFrom >= -45 && angleHitFrom <= 45)
            {
                anim.SetTrigger("Hit_B"); // 背后受击
            }
            else
            {
                anim.SetTrigger("Hit_F"); // 前方受击
            }
        }
    }

    /// <summary>
    /// 死亡处理
    /// 设置死亡状态标识
    /// </summary>
    private void Dead()
    {
        isDead = true;
    }

    private void ApplyRootMotion()
    {
        anim.applyRootMotion = true;
    }
    
    private void DisableRootMotion()
    {
        anim.applyRootMotion = false;
    }

    private void EnableDamageCollider()
    {
        damage1.enabled = true;
        damage2.enabled = true;
    }

    private void DisableDamageCollider()
    {
        damage1.enabled = false;
        damage2.enabled = false;
        DamagedPlayer = null;

        // 清空所有伤害碰撞体记录的玩家
        foreach (var enemyDamageCollider in damageColliders)
        {
            enemyDamageCollider.ClearPlayer();
        }
    }
}
