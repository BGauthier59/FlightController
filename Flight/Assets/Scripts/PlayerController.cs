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
    [SerializeField] private float jumpSpeed = 400f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AnimationCurve dragCurve;

    [SerializeField] private float playerSpeed;
    [SerializeField] private float vAxis;
    [SerializeField] private float hAxis;
    [SerializeField] private float baseHSpeed = 10f;
    [SerializeField] private float baseVSpeed = 10f;
    
    private void Update()
    {
        if(isWaitingToHold) RefreshStartGauge(Time.deltaTime);

        if (isWaitingToHold) return;
        
        // Position
        transform.Translate(Vector3.forward * (playerSpeed * Time.deltaTime));
        
        // Rotation 
        var hVelocity = baseHSpeed * vAxis * Time.deltaTime;
        var vVelocity = baseVSpeed * hAxis * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(vVelocity, hVelocity, 0);
        rb.MoveRotation(rb.rotation * rotation);
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
        vAxis = Mathf.Clamp(ctx.ReadValue<Vector2>().x, -1, 1);
        hAxis = Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1);
        Debug.Log($"XAxis : {vAxis} and Y Axis : {hAxis}");
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        Debug.Log("Press Jump");
        rb.AddForce(0, jumpSpeed, 0, ForceMode.Force);
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
