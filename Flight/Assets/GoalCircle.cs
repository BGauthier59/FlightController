using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCircle : MonoBehaviour
{
    [SerializeField] private float speedBoost;
    [SerializeField] private GameObject bubbles;
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
