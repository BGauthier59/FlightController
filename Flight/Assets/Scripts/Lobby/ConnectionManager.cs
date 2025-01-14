using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoSingleton<ConnectionManager>
{
    [SerializeField] private Transform[] spawnZones;

    [SerializeField] private List<PlayerIdentity> players;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        int number = PlayerInputManager.instance.playerCount - 1;
        PlayerIdentity player = input.GetComponent<PlayerIdentity>();
        player.playerController.OnJoined(spawnZones[number].position, number);
        players.Add(player);
        player.ChangeBody(players.Count - 1);

        if (number == PlayerInputManager.instance.maxPlayerCount - 1)
        {
            AllPlayerJoined();
        }
    }

    private void AllPlayerJoined()
    {
        LobbyUIManager.instance.AllPlayerConnect();
        foreach (var player in players)
        {
            player.playerController.SetReadyToHold();
        }
    }

    public async void TryStartGame()
    {
        foreach (var pc in players)
        {
            if (!pc.playerController.IsHoldingComplete()) return;
        }

        // Plays feedbacks
        foreach (var pc in players)
        {
            pc.playerController.HoldCompleted();
        }

        LobbyUIManager.instance.ExitLobbyGUI();
        await PostProcessManager.instance.SwitchVolume(.5f, 1);
        await LobbyCameraManager.instance.MoveToBookmark(1, 1);
        await PostProcessManager.instance.SwitchVolume(.5f, 0);

        // Enable selection
        foreach (var pc in players)
        {
            pc.playerController.SetReadyToSelect();
        }
    }

    public async void TryToGoMenu()
    {
        foreach (var pc in players)
        {
            if (!pc.playerController.IsHoldingComplete()) return;
        }

        foreach (var pc in players)
        {
            pc.playerController.HoldCompleted();
        }

        await PostGameSceenManager.instance.EndLevel();
        
        for (int i = players.Count - 1; i >= 0; i--)
        {
            Destroy(players[i].gameObject);
        }
        players.Clear();
        
        SceneManager.LoadScene(0);
    }
    
    public PlayerIdentity[] GetPlayers()
    {
        return players.ToArray();
    }
}