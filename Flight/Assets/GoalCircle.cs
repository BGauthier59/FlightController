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
            PlayerIdentity player = other.gameObject.GetComponent<PlayerIdentity>();
            player.playerController.glideSpeed += speedBoost;
            LevelProgressionManager.instance.PlayerGetGoal(player.index,goalIndex);
        }
    }
}
