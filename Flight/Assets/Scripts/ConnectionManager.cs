using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectionManager : MonoSingleton<ConnectionManager>
{
    [SerializeField] private Transform[] spawnZones;
    [SerializeField] private float startHoldTime;

    [SerializeField] private List<PlayerController> players;
    
    public void OnPlayerJoined(PlayerInput input)
    {
        int number = PlayerInputManager.instance.playerCount - 1;
        PlayerController player = input.GetComponent<PlayerController>();
        player.OnJoined(spawnZones[number].position, number);
        players.Add(player);

        if (number == PlayerInputManager.instance.maxPlayerCount - 1)
        {
            AllPlayerJoined();
        }
    }

    private void AllPlayerJoined()
    {
        Debug.Log("all player joined");
        UIManager.instance.AllPlayerConnect();
        foreach (var player in players)
        {
            player.SetReadyToStart();
        }
    }

    public async void TryStartGame()
    {
        foreach (var pc in players)
        {
            if (!pc.IsHoldingComplete()) return;
        }

        // Plays feedbacks
        foreach (var pc in players)
        {
            pc.ExitLobby();
        }

        UIManager.instance.ExitLobbyGUI();
        await PostProcessManager.instance.SwitchVolume(1, 1);
        await LobbyCameraManager.instance.MoveToBookmark(1, 1);
        
        Debug.Log("cool");
    }
}
