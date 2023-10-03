using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private int index;
    private bool isWaitingToHold, isHolding;
    private float holdTimer;
    [SerializeField] private float holdSpeed;

    private void Update()
    {
        if(isWaitingToHold) RefreshStartGauge(Time.deltaTime);
    }

    public void OnJoined(Vector3 initPos, int index)
    {
        this.index = index;
        transform.position = initPos;
        isWaitingToHold = false;
        UIManager.instance.PlayerConnect(index);
    }

    public void SetReadyToStart()
    {
        isWaitingToHold = true;
    }

    public void ExitLobby()
    {
        isWaitingToHold = false;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        
    }

    public void OnStart(InputAction.CallbackContext ctx)
    {
        if (!isWaitingToHold) return;
        if (ctx.canceled) isHolding = false;
        else if (ctx.performed) isHolding = true;
    }

    private void RefreshStartGauge(float delta)
    {
        if (!isHolding) delta = -delta;
        holdTimer += delta * holdSpeed;
        holdTimer = math.clamp(holdTimer, 0, 1);
        UIManager.instance.RefreshReadyGaugeGUI(index, holdTimer);

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
}
