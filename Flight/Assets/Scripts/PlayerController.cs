using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private int index;
    
    public Vector2 moveAxis;

    [SerializeField] private PlayerIdentity identity;
    [SerializeField] private float holdSpeed;
    
    private bool isWaitingToHold, isHolding, isSelectingLevel;
    private float holdTimer;
    private bool inGame;
    private bool inEndGameMenu;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isWaitingToHold && !inEndGameMenu) RefreshStartGauge(Time.deltaTime);
        if (inGame) ExecuteState();
        if (isWaitingToHold && inEndGameMenu) RefreshRestartGauge(Time.deltaTime);
    }

    #region Input Action Events

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!inGame) return;
        if (ctx.canceled)
        {
            moveAxis = Vector2.zero;
            return;
        }

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

        if (canLand && ctx.started) SwitchState(State.LAND);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (!inGame) return;
        if (isJumping) return;
        SwitchState(State.JUMP);
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
        isHolding = false;
        holdTimer = 0;
        isWaitingToHold = false;
        inEndGameMenu = false;
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

    public enum State
    {
        WALK,
        JUMP,
        GLIDE,
        LAND
    }

    [SerializeField] private State currentState = State.WALK;

    #region Controller In Game

    public State GetState()
    {
        return currentState;
    }

    public void SetPlayerInGame(Vector3 position)
    {
        transform.position = position;
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
        SetCanLand(false);

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

    [SerializeField] private float walkRotateSpeed;
    [SerializeField] private float walkSpeed;

    private void ToWalk()
    {
        glideSpeed = 1;
    }

    private void Walk()
    {
        if (moveAxis.magnitude > .2f)
        {
            RotateWalk();

            rb.position += transform.forward * (moveAxis.y * walkSpeed * Time.deltaTime);
            identity.ChangeAnimation(Anim.Walk);
        }
        else
        {
            identity.ChangeAnimation(Anim.Idle);
        }

        CheckGroundOnWalk();
    }

    private void RotateWalk()
    {
        transform.rotation = Quaternion.Euler(0,
            transform.eulerAngles.y + moveAxis.x * walkRotateSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundOnWalk()
    {
        // Is there a ground?
        if (Physics.Raycast(rayOrigin.position, down, out hit, .55f, ground))
        {
            return;
        }

        SwitchState(State.GLIDE);
    }

    #endregion

    #region Jump

    private bool isJumping;
    [SerializeField] private float jumpDuration;
    private float jumpTimer;
    [SerializeField] private float jumpHeight;
    private float currentJumpHeight;
    [SerializeField] private AnimationCurve jumpBoost;

    private void ToJump()
    {
        glideSpeed += jumpBoost.Evaluate(ratioSpeed);
        isJumping = true;

        currentJumpHeight = jumpHeight;
        initRotation = transform.rotation;
        finalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        identity.ChangeAnimation(Anim.Flap);
    }

    private void Jump()
    {
        if (jumpTimer > jumpDuration)
        {
            jumpTimer = 0;
            isJumping = false;
            SwitchState(State.GLIDE);
        }
        else
        {
            float factor = Mathf.Clamp01(1 - (transform.position.y - 2) / 3);
            currentJumpHeight = math.lerp(jumpHeight, 0, jumpTimer / jumpDuration);
            transform.position += transform.forward * (glideSpeed * Time.deltaTime) +
                                  Vector3.up * (currentJumpHeight * Time.deltaTime) +
                                  transform.forward * (currentJumpHeight * Time.deltaTime * 0.3f) +
                                  Vector3.up * factor * 20 * Time.deltaTime;
            jumpTimer += Time.deltaTime;
            RotateGlide();
        }
    }

    #endregion

    #region Glide

    public float glideSpeed, glideMaxSpeed;
    private float ratioSpeed;
    public AnimationCurve accelerationCurve, decelerationCurve, attractionOverSpeed;

    public float2 glideRotateSpeed;
    public float speedFactor, attractionFactor;
    private Vector3 down = Vector3.down;

    public Rigidbody rb;
    [SerializeField] private float dotToleranceAngle;
    [SerializeField] private Transform rotationLooker;

    private void ToGlide()
    {
        identity.ChangeAnimation(Anim.Glide);
    }

    private void Glide()
    {
        RotateGlide();

        float dot = math.dot(transform.forward, down);
        EvaluateGlideSpeed(dot);

        float factor = Mathf.Clamp01(1 - (transform.position.y - 2) / 3);

        rb.position += transform.forward * (glideSpeed * Time.deltaTime) + Vector3.up * (factor * 20 * Time.deltaTime);

        // Check conditions
        CheckGroundOnGlide();
    }

    private void RotateGlide()
    {
        // Calculates cloud force factor
        float factor = Mathf.Clamp01(1 - (transform.position.y - 2) / 3);

        // Calculate rotation
        Quaternion next = Quaternion.Euler(
            transform.eulerAngles.x + moveAxis.y * glideRotateSpeed.x * Time.deltaTime +
            attractionOverSpeed.Evaluate(ratioSpeed) * Time.deltaTime * attractionFactor -
            factor * Time.deltaTime * attractionFactor * 2,
            transform.eulerAngles.y + moveAxis.x * glideRotateSpeed.y * Time.deltaTime, 0);

        // Evaluate rotation and prevent pigeon from rotating more than possible
        rotationLooker.rotation = next;
        float dot = math.dot(rotationLooker.forward, Vector3.up);
        if (math.abs(dot) < dotToleranceAngle) rb.rotation = next;
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
    private Quaternion finalRotation;

    public float baseLandingDuration;
    private float baseLandingTimer;

    [SerializeField] private GameObject tutorialText;

    private void ToLand()
    {
        p1 = transform.position;
        p2 = transform.position + Vector3.up;
        p3 = hit.point + Vector3.up;
        p4 = hit.point;

        initRotation = transform.rotation;
        finalRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        identity.ChangeAnimation(Anim.Land);
    }

    private void Land()
    {
        // Detect hit point

        if (baseLandingTimer > baseLandingDuration)
        {
            baseLandingTimer = 0;
            rb.rotation = finalRotation;
            rb.position = p4;
            SwitchState(State.WALK);
        }
        else
        {
            rb.position = Ex.CubicBeziersCurve(p1, p2, p3, p4, baseLandingTimer / baseLandingDuration);
            rb.rotation = Quaternion.Lerp(initRotation, finalRotation, baseLandingTimer / baseLandingDuration);
            baseLandingTimer += Time.deltaTime;
        }
    }

    private void CheckGroundOnGlide()
    {
        rayDirection = (transform.forward + down);

        // Is there a ground?
        if (!Physics.Raycast(rayOrigin.position, rayDirection, out hit, distanceToLand, ground))
        {
            SetCanLand(false);
            return;
        }

        float dot = math.dot(Vector3.up, hit.normal);
        if (dot < .9f) return;

        // You can land
        if (hit.distance > securityDistanceToLand)
        {
            SetCanLand(true);
        }
        // You must land
        else
        {
            SwitchState(State.LAND);
        }
    }

    private void SetCanLand(bool can)
    {
        canLand = can;
        tutorialText.SetActive(can);
    }

    #endregion

    #endregion

    #region Post Game

    public void SetPlayerInMenu()
    {
        SetCanLand(false);
        inGame = false;
        inEndGameMenu = true;
    }
    
    private void RefreshRestartGauge(float delta)
    {
        if (!isHolding) delta = -delta;
        holdTimer += delta * holdSpeed;
        holdTimer = math.clamp(holdTimer, 0, 1);

        PostGameSceenManager.instance.RefreshReadyGaugeGUI(index, holdTimer);

        // We only check on player 1
        if (index == 0)
        {
            ConnectionManager.instance.TryToGoMenu();
        }
    }

    #endregion

    private void OnCollisionEnter(Collision other)
    {
        glideSpeed = 0;
    }
}