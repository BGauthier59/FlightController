using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private int index;

    [SerializeField] private PlayerIdentity identity;
    private bool isWaitingToHold, isHolding, isSelectingLevel;
    private float holdTimer;
    [SerializeField] private float holdSpeed;

    private bool inGame;
    private bool inEndGameMenu;

    public Vector2 moveAxis, cameraAxis;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isWaitingToHold) RefreshStartGauge(Time.deltaTime);
        if (inGame) ExecuteState();
        if (inEndGameMenu) RefreshRestartGauge(Time.deltaTime);
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

    public void OnMoveCamera(InputAction.CallbackContext ctx)
    {
        if (!inGame) return;
        if (ctx.canceled)
        {
            cameraAxis = Vector2.zero;
            return;
        }

        cameraAxis = ctx.ReadValue<Vector2>();
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

    public void SetPlayerInMenu()
    {
        inGame = false;
        inEndGameMenu = true;
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
        transform.rotation =
            Quaternion.Euler(0, transform.eulerAngles.y + moveAxis.x * walkRotateSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundOnWalk()
    {
        if (Physics.Raycast(rayOrigin.position, down, out hit, distanceToLand, ground))
        {
            return; // Sol
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
    [SerializeField] private float rotateCorrectionFactor, decelerationFactor;

    private void ToJump()
    {
        // Changer vitesse
        glideSpeed /= decelerationFactor;
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
            currentJumpHeight = math.lerp(jumpHeight, 0, jumpTimer / jumpDuration);
            transform.position += transform.forward * (glideSpeed * Time.deltaTime) +
                                  Vector3.up * (currentJumpHeight * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(initRotation, finalRotation,
                (jumpTimer / jumpDuration) * rotateCorrectionFactor);
            jumpTimer += Time.deltaTime;
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

    private void ToGlide()
    {
        glideSpeed *= decelerationFactor;

        identity.ChangeAnimation(Anim.Glide);
        // Feedbacks
    }

    private void Glide()
    {
        // Execute
        RotateGlide();

        float dot = math.dot(transform.forward, down);

        EvaluateGlideSpeed(dot);

        rb.position += transform.forward * (glideSpeed * Time.deltaTime);

        // Check conditions

        CheckGroundOnGlide();
    }

    private void RotateGlide()
    {
        rb.rotation = Quaternion.Euler(
            transform.eulerAngles.x + moveAxis.y * glideRotateSpeed.x * Time.deltaTime
                                    + attractionOverSpeed.Evaluate(ratioSpeed) * Time.deltaTime * attractionFactor,
            transform.eulerAngles.y + moveAxis.x * glideRotateSpeed.y * Time.deltaTime, 0);
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

        if (!Physics.Raycast(rayOrigin.position, rayDirection, out hit, distanceToLand, ground))
        {
            Debug.DrawRay(rayOrigin.position, rayDirection * distanceToLand, Color.green);
            tutorialText.SetActive(false);

            return; // Pas de sol
        }

        float dot = math.dot(Vector3.up, hit.normal);
        if (dot < .9f) return;

        Debug.DrawRay(rayOrigin.position, rayDirection * distanceToLand, Color.red);

        if (hit.distance > securityDistanceToLand) // Arrêt possible
        {
            EnableLandingFeedback();
            canLand = true;
            tutorialText.SetActive(true);
        }
        else // Arrêt forcé
        {
            tutorialText.SetActive(false);
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

    private void OnCollisionEnter(Collision other)
    {
        glideSpeed = 0;
    }
}