using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGamemode : MonoBehaviour
{
    [SerializeField] private GoalCircle[] goalCircleList;
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
        
        DisplayCircle();
    }

    private void DisplayCircle()
    {
        var rand = (int)Random.Range(0, goalCircleList.Length);

        goalCircleList[rand].gameObject.SetActive(true);
        
        foreach (var p in players)
        {
            p.uiManager.StartGoalSearch(goalCircleList[rand].transform, player1points, player2points, numberToWin);
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

