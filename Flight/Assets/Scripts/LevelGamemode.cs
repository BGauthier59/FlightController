using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGamemode : MonoBehaviour
{
    [SerializeField] private GoalCircle[] goalCircleList;
    public PlayerIdentity[] players;
    [SerializeField] private int numberToWin, player1points, player2points;
    public static LevelGamemode instance;
    public PostGameSceenManager postGameManager;

    public void StartLevel()
    {
        player1points = player2points = 0;
        DisplayCircle();
    }

    private void DisplayCircle()
    {
        foreach (var goal in goalCircleList)
        {
            goal.enabled = false;
        }
        
        var rand = (int)Random.Range(0, goalCircleList.Length + 1);

        goalCircleList[rand].enabled = true;

        players[0].uiManager.goal = players[1].uiManager.goal = goalCircleList[rand].transform;
    }

    public void AddPoint(int i)
    {
        if (i == 0) player1points++;
        else player2points++;

        DisplayCircle();
        
        CheckWin();
    }

    private void CheckWin()
    {
        if (player1points >= numberToWin)
        {
            Debug.Log("Player 1 Win");
            postGameManager.DisplayWinner(0,player1points, player2points );
        }
        else if (player2points >= numberToWin)
        {
            Debug.Log("Player 2 Win");
            postGameManager.DisplayWinner(1,player1points, player2points );
        }
    }
}

