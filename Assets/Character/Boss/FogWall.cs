using System;
using System.Collections;
using System.Collections.Generic;
using Character.Player;
using UnityEngine;

public class FogWall : MonoBehaviour
{
    public Collider fogWall;
    public BossManager boss;

    private void OnTriggerEnter(Collider other)
    {
        boss?.WakeBoss(other.GetComponentInParent<Player>());
    }
}
