using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PostGameSceenManager : MonoBehaviour
{
    [SerializeField] private string winText;
    [SerializeField] private string looseText;
    [SerializeField] private GameObject postGameHandler;
    public PostGamePlayerSection[] playerSections;

    public static PostGameSceenManager instance;

    private void Awake() => instance = this;

    private void Start() => postGameHandler.SetActive(false);
    
    public void DisplayWinner(int index, int p0Score, int p1Score)
    {
        if (index == 0)
        {
            playerSections[0].statusText.text = winText;
            playerSections[1].statusText.text = looseText;
        }
        else
        {
            playerSections[1].statusText.text = winText;
            playerSections[0].statusText.text = looseText;
        }
        
        playerSections[0].scoreText.text = p0Score.ToString();
        playerSections[1].scoreText.text = p1Score.ToString();
        
        playerSections[index].image.enabled = true;
        
        postGameHandler.SetActive(true);
    }
    
    public void RefreshReadyGaugeGUI(int index, float value)
    {
        PostGamePlayerSection data = playerSections[index];
        data.readyGauge.fillAmount = value;
    }
}

[Serializable]
public class PostGamePlayerSection
{
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI scoreText;
    public Image image;
    public Image readyGauge;
}