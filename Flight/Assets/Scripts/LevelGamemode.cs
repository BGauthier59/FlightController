using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class LevelGamemode : MonoBehaviour
{
    [SerializeField] private List<GoalCircle> goalCircleList;
    [SerializeField] private List<GoalCircle> circleShuffle = new();
    public PlayerIdentity[] players;
    [SerializeField] private int numberToWin, player1points, player2points;
    public static LevelGamemode instance;
    public PostGameSceenManager postGameManager;

    private void Awake()
    {
        instance = this;
    }
    
    public void StartLevel()
    {
        player1points = player2points = 0;
        
        foreach (var goal in goalCircleList)
        {
            goal.gameObject.SetActive(false);
        }

        circleShuffle = Ex.ShuffleList(goalCircleList);
        
        DisplayCircle();
    }

    private void DisplayCircle()
    {
        var rand = circleShuffle[0].transform;
        circleShuffle.RemoveAt(0);
        
        rand.gameObject.SetActive(true);
        
        foreach (var p in players)
        {
            p.uiManager.StartGoalSearch(rand, player1points, player2points, numberToWin);
        }
    }

    public void AddPoint(int i)
    {
        if (i == 0) player1points++;
        else player2points++;

        Debug.Log($"Point for player {i + 1}");
        
        DisplayCircle();
        
        CheckWin();
    }

    private void CheckWin()
    {
        if (player1points >= numberToWin)
        {
            Debug.Log("Player 1 Win");
            postGameManager.DisplayWinner(0);
            StopGameMode();
        }
        else if (player2points >= numberToWin)
        {
            Debug.Log("Player 2 Win");
            postGameManager.DisplayWinner(1);
            StopGameMode();
        }
    }

    private void StopGameMode()
    {
        var players = ConnectionManager.instance.GetPlayers();
        for (int i = 0; i < players.Length; i++)
        {
            players[i].playerController.SetPlayerInMenu();
        }
    }
}

