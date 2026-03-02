using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using Save_Load;
using UnityEngine;
using UnityEngine.AI;

public class BossManager : MonoBehaviour, IHitAble
{
    public int bossID;
    [SerializeField] private bool hasBeenDefeated = false;
    [SerializeField] private bool hasBeenAwakened = false;
    
    public Player player;

    public Animator anim;
    public NavMeshAgent  agent;

    public bool isSecondState = false;

    public float atk_cd;
    public float atk_range;
    public float hp;
    public float maxHp;

    public float poise;
    public bool isHurt;
    
    public BossBehaviourTree bossBehaviourTree;
    public bool alreadyAwake;

    public void Awake()
    {
        if (!PlayerCamera.instance.playerSaveData.saveData.bossesAwakened.ContainsKey(bossID))
        {
            PlayerCamera.instance.playerSaveData.saveData.bossesAwakened.Add(bossID, false);
            PlayerCamera.instance.playerSaveData.saveData.bossesDefeated.Add(bossID, false);
        }
        else
        {
            hasBeenDefeated = PlayerCamera.instance.playerSaveData.saveData.bossesDefeated[bossID];

            if (hasBeenDefeated)
            {
                gameObject.SetActive(false);
            }
        }
        
        
    }

    private void OnEnable()
    {
        
    }

    private void Start()
    {
        bossBehaviourTree.TreeData.OnStart();
    }

    private void OnDisable()
    {
        bossBehaviourTree.TreeData.OnStop();
    }

    private void Update()
    {
        anim.SetFloat("Velocity", agent.velocity.magnitude);
    }

    public void WakeBoss(Player player)
    {
        hasBeenAwakened = true;
        this.player = player;
        
        PlayerSaveData playerSaveData = PlayerCamera.instance.playerSaveData.saveData;

        if (!playerSaveData.bossesAwakened.ContainsKey(bossID))
        {
            playerSaveData.bossesAwakened.Add(bossID, true);
        }
        else
        {
            playerSaveData.bossesAwakened.Remove(bossID);
            playerSaveData.bossesAwakened.Add(bossID, true);
        }
    }

    public void Stop()
    {
        agent.isStopped = true;
    }

    public void ReadyDie()
    {
        Invoke("Die", 5);
    }

    void Die()
    {
        gameObject.SetActive(false);
    }

    public void WalkTo()
    {
        agent.isStopped = false;
        agent.SetDestination(player.transform.position);
    }

    public void TakeDamage(Player player)
    {
        hp -= player.finalDamage;

        if (hp <= maxHp / 2)
        {
            anim.SetBool("IsSecondState", true);
        }
        
        isHurt = true;
    }

    private void SecondState()
    {
        anim.SetBool("IsSecondState", true);
    }

    private void EnableRootMotion()
    {
        anim.applyRootMotion = true;
    }

    private void DisableRootMotion()
    {
        anim.applyRootMotion = false;
    }

    private void Shout()
    {
        anim.SetBool("AlreadyShout", true);
    }
}
