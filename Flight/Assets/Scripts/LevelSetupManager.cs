using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetupManager : MonoBehaviour
{
    [SerializeField] private Transform[] initPos;
    [SerializeField] private CameraController[] cameras;
    private void Start()
    {
        var players = ConnectionManager.instance.GetPlayers();
        for (int i = 0; i < initPos.Length; i++)
        {
            players[i].SetPlayerInGame(initPos[i].position);
            cameras[i].AttachToPlayer(players[i].transform);
        }
    }
}
