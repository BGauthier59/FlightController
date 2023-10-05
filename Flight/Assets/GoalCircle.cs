using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCircle : MonoBehaviour
{
    public float speedBoost;
    public int goalIndex;
    public GameObject bubbles;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerIdentity player = other.transform.root.GetComponent<PlayerIdentity>();
            player.playerController.glideSpeed += speedBoost;
            LevelGamemode.instance.AddPoint(player.index);
            gameObject.SetActive(false);
            GameObject effect = Instantiate(bubbles, transform.position, transform.rotation);
        }
    }
}
