using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetupManager : MonoBehaviour
{
    [SerializeField] private Transform[] initPos;
    [SerializeField] private CameraController[] cameras;
    [SerializeField] private UIManager[] uiManagers;
    private void Start()
    {
        var players = ConnectionManager.instance.GetPlayers();
        for (int i = 0; i < initPos.Length; i++)
        {
            players[i].playerController.SetPlayerInGame(initPos[i].position);
            cameras[i].AttachToPlayer(players[i]);
            uiManagers[i].AttachToPlayer(players[i]);
            players[i].index = i;
        }

        LevelProgressionManager.instance.players = players;
        LevelGamemode.instance.StartLevel();
    }
}
