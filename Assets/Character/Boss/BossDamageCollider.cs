using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

public class BossDamageCollider : MonoBehaviour
{
    public BossManager Boss;
    
    public Player DamagedPlayer;

    private void OnTriggerEnter(Collider other)
    {
        
    }


    private void ClearPlayer()
    {
        DamagedPlayer = null;
    }
}
