using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostCircle : MonoBehaviour
{
    public float speedBoost;
    public ParticleSystem bubbles;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIdentity player = other.transform.root.GetComponent<PlayerIdentity>();
            player.playerController.glideSpeed += speedBoost;
            bubbles.Play();
        }
    }
}
