using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 角色生命条UI组件
/// 用于显示敌人的生命值和受到的伤害
/// 当敌人受到伤害时显示血条，一段时间后自动隐藏
/// 支持显示累计伤害和始终面向相机
/// </summary>
public class CharacterHPBar : MonoBehaviour
{
    /// <summary>
    /// 关联的敌人组件
    /// 用于获取敌人的生命值信息
    /// </summary>
    private Enemy enemy;

    /// <summary>
    /// 生命条滑块
    /// 用于显示敌人的当前生命值百分比
    /// </summary>
    [SerializeField] private Slider healthBar;

    /// <summary>
    /// 是否在受到伤害时显示角色名称
    /// 扩展功能标志，当前未使用
    /// </summary>
    [SerializeField] private bool displayCharacterNameOnDamage;
    
    /// <summary>
    /// 血条默认隐藏前的时间
    /// 血条显示的时间长度（秒）
    /// </summary>
    [SerializeField] private float defaultTimeBeforeBarHides = 3;
    
    /// <summary>
    /// 血条隐藏计时器
    /// 记录血条还需要显示多长时间
    /// </summary>
    [SerializeField] private float hideTimer = 0;
    
    /// <summary>
    /// 当前受到的伤害
    /// 累计显示敌人从满血状态开始受到的伤害
    /// </summary>
    [SerializeField] private int currentDamageTaken = 0;
    
    /// <summary>
    /// 伤害文本组件
    /// 用于显示累计受到的伤害数值
    /// </summary>
    [SerializeField] private TMP_Text characterDamage;
    
    /// <summary>
    /// 旧的健康值
    /// 用于比较生命值变化，但当前实现有逻辑问题
    /// 应存储上一次的生命值而非伤害值
    /// </summary>
    public int oldHealthValue = 0;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置生命条状态
    /// 更新生命条显示，计算累计伤害，控制血条显示/隐藏
    /// </summary>
    public void SetStat(int newValue)
    {
        // 设置生命条最大值
        healthBar.maxValue = enemy.maxHealth;
        
        // 计算本次受到的伤害
        int damageThisTime = 0;
        if (oldHealthValue > newValue)
        {
            damageThisTime = oldHealthValue - newValue;
        }
        
        // 累加伤害
        currentDamageTaken += damageThisTime;
    
        // 更新旧的生命值
        oldHealthValue = newValue;
    
        // 显示累计伤害
        characterDamage.text = currentDamageTaken.ToString();
    
        // 更新生命条当前值
        healthBar.value = newValue;
        
        // 如果敌人不是满血状态，显示血条并重置隐藏计时器
        if (enemy.health != enemy.maxHealth)
        {
            hideTimer = defaultTimeBeforeBarHides;
            gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        // 让血条始终面向相机
        transform.LookAt(transform.position + Camera.main.transform.forward);

        // 更新隐藏计时器
        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
        }
        else
        {
            // 计时器到期，隐藏血条
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 当血条被禁用时
    /// 重置累计伤害值
    /// 确保下次显示时从0开始累计
    /// </summary>
    private void OnDisable()
    {
        currentDamageTaken = 0;
    }
}
