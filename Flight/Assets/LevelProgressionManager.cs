using System.Collections.Generic;
using UnityEngine;

public class LevelProgressionManager : MonoBehaviour
{
    // SCRIPT PLUS UTILISé MAIS A GARDER AU CAS OU 
    
    
    public int numberOfPlayers;
    public PlayerIdentity[] players;
    public int[] playersStep;
    public int[] playersRank;
    public float[] playerStepProgression;
    public GoalCircle[] goalCircles;
    public static LevelProgressionManager instance;

    private void Awake()
    {
        instance = this;
    }
    

    private void Update()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if(playersStep[i] == 0 || playersStep[i] == goalCircles.Length) continue; 
            float result = Vector3.Distance(players[i].transform.position, goalCircles[playersStep[i]].transform.position) /
                Vector3.Distance(goalCircles[playersStep[i]-1].transform.position,
                    goalCircles[playersStep[i]].transform.position);
             playerStepProgression[i] = result;
        }

        RankPlayers();
    }

    private void RankPlayers()
    {
        float step;
        float comparison = -1;
        int player = 0;

        List<float> playerRanks = new List<float>(0);
        List<int> playerIndexes = new List<int>(0);

        for (int i = 0; i < numberOfPlayers; i++)
        {
            step = playersStep[i] + (1 - playerStepProgression[i]);
            playerRanks.Add(step);
            playerIndexes.Add(i);
        }

        for (int x = 0; x < numberOfPlayers; x++)
        {
            int index = 0;
            comparison = -1;
            for (int i = 0; i < playerRanks.Count; i++)
            {
                if (playerRanks[i] > comparison)
                {
                    player = playerIndexes[i];
                    comparison = playerRanks[i];
                    index = i;
                }
            }
            playersRank[player] = x;
            playerRanks.RemoveAt(index);   
            playerIndexes.RemoveAt(index);
        }
    }
}
