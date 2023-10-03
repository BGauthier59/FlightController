using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private int index;
    private bool isWaitingToHold, isHolding, isSelectingLevel;
    private float holdTimer;
    [SerializeField] private float holdSpeed;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isWaitingToHold) RefreshStartGauge(Time.deltaTime);
    }
    
    #region Input Action Events

    public void OnMove(InputAction.CallbackContext ctx)
    {
    }

    public void OnSwitchLeft(InputAction.CallbackContext ctx)
    {
        if (!isSelectingLevel) return;
        if (ctx.performed) LevelsManager.instance.SwitchLevel(true);
    }

    public void OnSwitchRight(InputAction.CallbackContext ctx)
    {
        if (!isSelectingLevel) return;
        if (ctx.performed) LevelsManager.instance.SwitchLevel(false);
    }

    public void OnStart(InputAction.CallbackContext ctx)
    {
        if (!isWaitingToHold) return;
        if (ctx.canceled) isHolding = false;
        else if (ctx.performed) isHolding = true;
    }

    public void OnSelect(InputAction.CallbackContext ctx)
    {
        if (!isSelectingLevel) return;
        if (ctx.performed) LevelsManager.instance.SelectLevel();
    }

    #endregion

    #region Lobby

    public async void OnJoined(Vector3 initPos, int index)
    {
        this.index = index;

        Vector3 finalPos = initPos;
        initPos += Vector3.up * 10;
        
        transform.eulerAngles = Vector3.up * 180;
        
        transform.position = initPos;
        isWaitingToHold = false;
        isSelectingLevel = false;
        LobbyUIManager.instance.PlayerConnect(index);

        float timer = 0;

        while (timer < 1)
        {
            transform.position = Ex.CubicBeziersCurve(initPos, initPos, finalPos, finalPos, timer);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        transform.position = finalPos;
    }

    private void RefreshStartGauge(float delta)
    {
        if (!isHolding) delta = -delta;
        holdTimer += delta * holdSpeed;
        holdTimer = math.clamp(holdTimer, 0, 1);
        LobbyUIManager.instance.RefreshReadyGaugeGUI(index, holdTimer);

        // We only check on player 1
        if (index == 0)
        {
            ConnectionManager.instance.TryStartGame();
        }
    }

    public bool IsHoldingComplete()
    {
        return holdTimer >= 1;
    }

    public void SetReadyToHold()
    {
        isWaitingToHold = true;
    }

    public void HoldCompleted()
    {
        isWaitingToHold = false;
    }

    public void SetReadyToSelect()
    {
        isSelectingLevel = true;
    }

    public void SelectCompleted()
    {
        isSelectingLevel = false;
    }

    #endregion

    #region Controller In Game

    public void ToDo()
    {
    }

    #endregion
}