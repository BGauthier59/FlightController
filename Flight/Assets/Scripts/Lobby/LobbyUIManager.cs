using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public class LobbyUIManager : MonoSingleton<LobbyUIManager>
{
    [Serializable]
    public struct PlayerDataGUI
    {
        public TextMeshProUGUI state;
        public Transform icon;
        public Image readyGauge;
    }

    [SerializeField] private string waitingText, connectedText;

    [SerializeField] private PlayerDataGUI[] playerData;
    [SerializeField] private GameObject readyZone;

    [SerializeField] private Animation anim;
    [SerializeField] private AnimationClip exitLobbyClip;
    
    private void Start()
    {
        readyZone.SetActive(false);
        foreach (var data in playerData)
        {
            data.state.text = waitingText;
            data.icon.localScale = Vector3.zero;
        }
    }

    public async void PlayerConnect(int index)
    {
        PlayerDataGUI data = playerData[index];
        float timer = 0;
        Vector3 zero = Vector3.zero;
        Vector3 one = Vector3.one;
                
        data.state.text = connectedText;

        while (timer < .5f)
        {
            data.icon.localScale = Vector3.Lerp(zero, one, timer / .5f);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        data.icon.localScale = one;
    }

    public void AllPlayerConnect()
    {
        readyZone.SetActive(true);
    }

    public void RefreshReadyGaugeGUI(int index, float value)
    {
        PlayerDataGUI data = playerData[index];
        data.readyGauge.fillAmount = value;
    }

    public void ExitLobbyGUI()
    {
        anim.clip = exitLobbyClip;
        anim.Play();
    }
}
