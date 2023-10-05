using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PostGameSceenManager : MonoBehaviour
{
    public static PostGameSceenManager instance;
    
    [SerializeField] private GameObject postGameHandler;
    
    [SerializeField] PostGamePlayerSection[] playerSections;
    
    [SerializeField] private Camera postGameCam;
    
    private Transform postGameCameraFinalTransform;
    private int winnerIndex;

    [SerializeField] private Transform winnerTransform, looserTransform;
    [SerializeField] private GameObject winnerTextCanvas, looserTextCanvas;

    [SerializeField] private Image[] curtains;
    [SerializeField] private float menuTransitionDuration = 1f;
    [SerializeField] private GameObject[] objectToDisable;

    [SerializeField] private Color[] colors;
    

    private void Awake() => instance = this;

    private void Start()
    {
        postGameHandler.SetActive(false);
        postGameCameraFinalTransform = postGameCam.transform;
        postGameCam.enabled = false;
        looserTextCanvas.SetActive(false);
        winnerTextCanvas.SetActive(false);
    }

    public void DisplayWinner(int index)
    {
        winnerIndex = index;
        PrePostGameSequence();
    }
    
    public void RefreshReadyGaugeGUI(int index, float value)
    {
        PostGamePlayerSection data = playerSections[index];
        data.readyGauge.fillAmount = value;
    }

    public float postGameCamSlidingDuration = 5f;
    private async void PrePostGameSequence()
    {
        foreach (var off in objectToDisable)
        {
            off.SetActive(false);   
        }
        
        PlayerIdentity winnerIdentity = LevelGamemode.instance.players[winnerIndex];
        PlayerIdentity looserIdentity = LevelGamemode.instance.players[ReturnTheOtherPlayer(winnerIdentity.index)];
        
        var winnerPos = winnerIdentity.transform.position;
        var winnerRot = winnerIdentity.transform.rotation;

        var initCamPos = winnerIdentity.transform.position + winnerIdentity.transform.forward * 5 + winnerIdentity.transform.up * 3;
        var initCamRot = Quaternion.Euler(-winnerIdentity.transform.eulerAngles.x, -winnerIdentity.transform.eulerAngles.y, 0);

        var finalCamPos = postGameCameraFinalTransform.position;
        var finalCamRot = postGameCameraFinalTransform.rotation;

        var upRatio = (winnerPos.y - initCamPos.y) * 2;


        postGameCam.transform.position = initCamPos;
        postGameCam.transform.LookAt(winnerIdentity.transform);
        postGameCam.enabled = true;

        await Task.Delay(3000);
        
        float timer = 0;
        while (timer < postGameCamSlidingDuration)
        {
            postGameCam.transform.position = Ex.CubicBeziersCurve(initCamPos, initCamPos + Vector3.up * upRatio, finalCamPos + Vector3.up * upRatio, finalCamPos, timer / postGameCamSlidingDuration);
            postGameCam.transform.rotation = Quaternion.Lerp(initCamRot, finalCamRot, timer / postGameCamSlidingDuration);
            await Task.Yield();
            timer += Time.deltaTime;
        }
        
        postGameCam.transform.position = finalCamPos;
        postGameCam.transform.rotation = finalCamRot;

        await Task.Delay(250);
        // Apparition du looser
        looserIdentity.transform.position = looserTransform.position;
        looserIdentity.transform.rotation = looserTransform.rotation;
        looserTextCanvas.SetActive(true);
        
        // VFx
        // Son

        await Task.Delay(1000);
        // Apparition du Winner
        winnerIdentity.transform.position = winnerTransform.position;
        winnerIdentity.transform.rotation = winnerTransform.rotation;
        winnerTextCanvas.SetActive(true);
        
        // Camera Shake
        // Vfx
        // Son

        await Task.Delay(1000);
        postGameHandler.SetActive(true);
        
        winnerIdentity.playerController.SetReadyToHold();
        looserIdentity.playerController.SetReadyToHold();
    }

    public async Task EndLevel()
    {
        float timer = 0;
        
        while (timer <= menuTransitionDuration)
        {
            foreach (var c in curtains)
            {
                c.fillAmount = timer / menuTransitionDuration;
            }
            
            await Task.Yield();
            timer += Time.deltaTime;
        }
    }
    
    private int ReturnTheOtherPlayer(int index)
    {
        return index == 1 ? 0 : 1;
    }
}

[Serializable]
public class PostGamePlayerSection
{
    public Image readyGauge;
}
