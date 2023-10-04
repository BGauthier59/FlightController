using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCircle : MonoBehaviour
{
    public float speedBoost;
    public int goalIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIdentity player = other.transform.parent.GetComponent<PlayerIdentity>();
            player.playerController.glideSpeed += speedBoost;
            LevelGamemode.instance.AddPoint(player.index);
        }
    }
}
