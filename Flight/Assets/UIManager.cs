using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Transform arrow;
    public PlayerIdentity player;
    public Transform goal;
    [SerializeField] private TMP_Text meterCount,rankDisplay;
    public int playerIndex;
    private int maxScore;

    // Update is called once per frame
    void Update()
    {
        if(goal == null) return;
        
        arrow.rotation = Quaternion.Lerp(arrow.rotation,Quaternion.LookRotation(Quaternion.Euler(0,-player.transform.eulerAngles.y,0) * (goal.position - player.transform.position).normalized),Time.deltaTime*5);
        meterCount.text = Mathf.Round(Vector3.Distance(goal.position, player.transform.position)) + "m";
        
        // switch (LevelProgressionManager.instance.playersRank[playerIndex])
        // {
        //     case 0:
        //         rankDisplay.text = "1st";
        //         break;
        //     case 1:
        //         rankDisplay.text = "2nd";
        //         break;
        //     case 2:
        //         rankDisplay.text = "3rd";
        //         break;
        //     case 3:
        //         rankDisplay.text = "4th";
        //         break;
        // }
    }

    public void StartGoalSearch(Transform transform, int p1Score, int p2Score, int maxScore)
    {
        this.maxScore = maxScore;
        
        goal = transform;
        if (playerIndex == 0) rankDisplay.text = $"{p1Score} / {this.maxScore}";
        else rankDisplay.text = $"{p2Score} / {this.maxScore}";

    }

    public void AttachToPlayer(PlayerIdentity identity)
    {
        player = identity;
        identity.uiManager = this;
    }
}
