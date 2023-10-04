using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostCircle : MonoBehaviour
{
    public float speedBoost;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIdentity player = other.gameObject.GetComponent<PlayerIdentity>();
            player.planerController.speed += speedBoost;
        }
    }
}
