using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private int index;
    private bool isWaitingToHold, isHolding, isSelectingLevel;
    private float holdTimer;
    [SerializeField] private float holdSpeed;

    private bool inGame;

    private Vector2 moveAxis;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isWaitingToHold) RefreshStartGauge(Time.deltaTime);
        if (inGame) ExecuteState();
    }

    #region Input Action Events

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!inGame) return;
        if (ctx.canceled) return;
        moveAxis = ctx.ReadValue<Vector2>();
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

    public void OnLand(InputAction.CallbackContext ctx)
    {
        if (!inGame) return;
        
        // DEBUG
        if (currentState == State.WALK && ctx.started)
        {
            transform.position+=Vector3.up * 50;
            SwitchState(State.GLIDE);
            return;
        }
        
        if (canLand && ctx.started) SwitchState(State.LAND);
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

    private enum State
    {
        WALK,
        JUMP,
        GLIDE,
        LAND
    }

    [SerializeField] private State currentState = State.WALK;

    #region Controller In Game

    public void SetPlayerInGame(Vector3 position)
    {
        transform.position = position;

        SwitchState(State.GLIDE);

        inGame = true;
    }

    private void ExecuteState()
    {
        if (!inGame) return;

        switch (currentState)
        {
            case State.WALK:
                Walk();
                break;
            case State.JUMP:
                Jump();
                break;
            case State.GLIDE:
                Glide();
                break;
            case State.LAND:
                Land();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SwitchState(State current)
    {
        canLand = false;

        switch (current)
        {
            case State.WALK:
                ToWalk();
                break;
            case State.JUMP:
                ToJump();
                break;
            case State.GLIDE:
                ToGlide();
                break;
            case State.LAND:
                ToLand();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(current), current, null);
        }

        currentState = current;
    }

    #region Walk

    private void ToWalk()
    {
        // Changer variables pour le joystick
    }

    private void Walk()
    {
        // Marcher et rotate

        // Chgmt de state -> check raycast vers le sol => Glide
    }

    #endregion

    #region Jump

    private void ToJump()
    {
        // Changer vitesse
    }

    private void Jump()
    {
        // Check timer jump => Glide / Land (si trop proche)
    }

    #endregion

    #region Glide

    public float glideSpeed, glideMaxSpeed;
    private float ratioSpeed;
    public AnimationCurve accelerationCurve, decelerationCurve;

    public float glideRotateSpeed, speedFactor;
    private Vector3 down = Vector3.down;

    private void ToGlide()
    {
        // Feedbacks
    }

    private void Glide()
    {
        // Execute
        RotateGlide();

        float dot = math.dot(transform.forward, down);

        EvaluateGlideSpeed(dot);

        transform.position += transform.forward * (glideSpeed * Time.deltaTime);

        // Check conditions

        CheckGround();
    }

    private void RotateGlide()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + moveAxis.y * glideRotateSpeed * Time.deltaTime,
            transform.eulerAngles.y + moveAxis.x * glideRotateSpeed * Time.deltaTime, 0);
    }

    private void EvaluateGlideSpeed(float dot)
    {
        ratioSpeed = glideSpeed / glideMaxSpeed;
        float factor = 1;

        if (dot > 0) factor = accelerationCurve.Evaluate(ratioSpeed);
        else if (dot < 0) factor = decelerationCurve.Evaluate(ratioSpeed);

        glideSpeed += (dot * factor * speedFactor);
        glideSpeed = math.clamp(glideSpeed, 0, glideMaxSpeed);
    }

    #endregion

    #region Land

    [SerializeField] private float distanceToLand, securityDistanceToLand;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private LayerMask ground;
    private RaycastHit hit;

    private Vector3 rayDirection;
    private bool canLand;
    Vector3 p1, p2, p3, p4;

    private Quaternion initRotation;
    private Quaternion finaleRotation = Quaternion.Euler(Vector3.zero);

    [SerializeField] private AnimationCurve landingSpeedCurve;
    private float landingSpeed;
    public float baseLandingSpeed;

    public float baseLandingDuration;
    private float baseLandingTimer;

    private void ToLand()
    {
        p1 = transform.position;
        p2 = transform.position + Vector3.up;
        p3 = hit.point + Vector3.up;
        p4 = hit.point;

        initRotation = transform.rotation;
        finaleRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        landingSpeed = baseLandingSpeed * landingSpeedCurve.Evaluate(ratioSpeed);
    }

    private void Land()
    {
        // Detect hit point

        if (baseLandingTimer > baseLandingDuration)
        {
            baseLandingTimer = 0;
            transform.rotation = finaleRotation;
            transform.position = p4;
            SwitchState(State.WALK);
        }
        else
        {
            transform.position = Ex.CubicBeziersCurve(p1, p2, p3, p4, baseLandingTimer / baseLandingDuration);
            transform.rotation = Quaternion.Lerp(initRotation, finaleRotation, baseLandingTimer / baseLandingDuration);
            //baseLandingTimer += Time.deltaTime * landingSpeed;
            baseLandingTimer += Time.deltaTime;
        }
    }

    private void CheckGround()
    {
        rayDirection = (transform.forward + down);

        if (!Physics.Raycast(rayOrigin.position, rayDirection, out hit, distanceToLand, ground))
        {
            Debug.DrawRay(rayOrigin.position, rayDirection * distanceToLand, Color.green);

            return; // Pas de sol
        }

        float dot = math.dot(Vector3.up, hit.normal);
        if (dot < .9f) return;
        
        Debug.DrawRay(rayOrigin.position, rayDirection * distanceToLand, Color.red);

        if (hit.distance > securityDistanceToLand) // Arrêt possible
        {
            EnableLandingFeedback();
            canLand = true;
        }
        else // Arrêt forcé
        {
            SwitchState(State.LAND);
        }
    }

    private void EnableLandingFeedback()
    {
        // Feedback
        Debug.Log("tu peux aterrir");
    }

    #endregion

    #endregion
}