using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGamemode : MonoBehaviour
{
    [SerializeField] private GoalCircle[] goalCircleList;
    [SerializeField] private int numberToWin, player1points, player2points;
    public static LevelGamemode instance;
    private void Start()
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
        
        int rand = (int)Random.Range(0, goalCircleList.Length);

        goalCircleList[rand].enabled = true;
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
        }
        else if (player2points >= numberToWin)
        {
            Debug.Log("Player 2 Win");
        }
    }
}

