using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoSingleton<LevelsManager>
{
    [Serializable]
    public struct Level
    {
        public int sceneIndex;
        public Sprite sprite;
        public string name;
    }

    [SerializeField] private Level[] levels;
    private Level current;
    private int currentIndex;

    public SpriteRenderer levelDisplay;
    public TMP_Text levelName;

    private void Start()
    {
        SetCurrent();
    }

    public void SwitchLevel(bool left)
    {
        if (left)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = levels.Length - 1;
        }
        else
        {
            currentIndex++;
            if (currentIndex == levels.Length) currentIndex = 0;
        }

        SetCurrent();
    }

    private void SetCurrent()
    {
        current = levels[currentIndex];
        levelDisplay.sprite = current.sprite;
        levelName.text = current.name;
    }

    public async void SelectLevel()
    {
        var players = ConnectionManager.instance.GetPlayers();
        foreach (var pc in players)
        {
            pc.playerController.SelectCompleted();
        }

        PostProcessManager.instance.SwitchVolume(5, 2);
        await LobbyCameraManager.instance.MoveToBookmark(2, 5);

        SceneManager.LoadScene(current.sceneIndex);
    }
}