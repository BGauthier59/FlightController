using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectionManager : MonoSingleton<ConnectionManager>
{
    [SerializeField] private Transform[] spawnZones;

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
        UIManager.instance.AllPlayerConnect();
        foreach (var player in players)
        {
            player.SetReadyToHold();
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
            pc.HoldCompleted();
        }

        UIManager.instance.ExitLobbyGUI();
        await PostProcessManager.instance.SwitchVolume(.5f, 1);
        await LobbyCameraManager.instance.MoveToBookmark(1, 1);
        PostProcessManager.instance.SwitchVolume(1, 0);
        foreach (var pc in players)
        {
            pc.SetReadyToSelect();
        }
    }

    public PlayerController[] GetPlayers()
    {
        return players.ToArray();
    }
}
