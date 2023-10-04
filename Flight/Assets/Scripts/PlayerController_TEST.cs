using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_TEST : MonoBehaviour
{
    public FlyState state;
    public FlyState lastState;
    
    public float speed, maxSpeed, minSpeed, walkSpeed;
    public float ratioSpeed;
    public AnimationCurve downCurve, upCurve;

    private float y;
    private float x;
    public float rotateSpeed;
    public float relativeSpeed;

    [Range(0, 10)] public float jumpHeight = 1f;
    [Range(0.1f, 1f)] public float jumpDuration;

    [SerializeField] private Vector3 nextLandPos;
    [Range(0.1f, 1.5f)] public float landingDuration;

    public LayerMask nonPlayerLayer;
    
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 joystick = ctx.ReadValue<Vector2>();
        y = joystick.y;
        x = joystick.x;
    }
    
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;
        ToFly();
    }

    private void Awake()
    {
        state = FlyState.GLIDE;
        lastState = FlyState.NONE;
    }

    private void Update()
    {
        switch (state)
        {
            case FlyState.GLIDE: Glide(); break;
            case FlyState.FLY:   Fly();   break;
            case FlyState.LAND:  Land();  break;
            case FlyState.WALK:  Walk();  break;
            default: throw new ArgumentOutOfRangeException();
        }
        
        CheckState();
    }

    #region Gliding
    private void ToGlide()
    {
        // Do changes
        
        // Change state 
        Debug.Log("State change to Glide"); //TODO - Se joues plusieurs fois 
        lastState = state;
        state = FlyState.GLIDE;
    }

    private void Glide()
    {
        Rotate();
        
        Vector3 forward = transform.forward;
        Vector3 down = Vector3.down;
        float dot = math.dot(forward, down);
        EvaluateSpeed(dot);
        
        Move();
    }
    

    #endregion
    
    #region Flying
    private void ToFly()
    {
        // Do changements
        speed /= 2.5f;
        if (speed < minSpeed) speed = minSpeed;
        
        // Change state 
        Debug.Log("State change to Fly");
        lastState = state;
        state = FlyState.FLY;
    }
    
    private void Fly()
    {
        bool isPlayerFromWalk = lastState == FlyState.WALK ? true : false;
        OnFly(transform.position, isPlayerFromWalk);
    }
    
    public async void OnFly(Vector3 initPos, bool isBoost = false)
    {
        Vector3 finalPos = initPos;
        
        if (isBoost)
        {
            initPos += Vector3.up * jumpHeight;
            initPos += Vector3.forward * (jumpHeight * 1.5f);
        }
        else
        {
            initPos += Vector3.up * jumpHeight;
        }
       
        
        float timer = 0;

        while (timer < jumpDuration)
        {
            transform.position = Ex.CubicBeziersCurve(initPos, initPos, finalPos, finalPos, timer);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        ToGlide();
        transform.position = finalPos;
    }
    #endregion

    #region Landing
    private void ToLand(Vector3 landPoint)
    {
        // Do changements
        nextLandPos = landPoint;
        
        // Change state 
        Debug.Log("State change to Land");
        lastState = state;
        state = FlyState.LAND;
    }
    
    private void Land()
    {
        OnLand(nextLandPos);
    }

    
    public async void OnLand(Vector3 initPos)
    {
        Vector3 finalPos = initPos;
        initPos = transform.position;
        
        float timer = 0;

        while (timer < landingDuration)
        {
            transform.position = Ex.CubicBeziersCurve(initPos, initPos, finalPos, finalPos, timer);
            await Task.Yield();
            timer += Time.deltaTime;
        }

        transform.position = finalPos;
        ToWalk();
    }
    
    #endregion

    #region Walking
    private void ToWalk()
    {
        // Do changements
        speed = walkSpeed;
        
        // Change state 
        Debug.Log("State change to Walk");
        lastState = state;
        state = FlyState.WALK;
    }
    
    private void Walk()
    {
        Rotate();
        Move();
    }
    #endregion
    
    private void Rotate()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x + y * rotateSpeed * Time.deltaTime,
            transform.eulerAngles.y + x * rotateSpeed * Time.deltaTime, 0);
    }

    private void EvaluateSpeed(float dot)
    {
        ratioSpeed = speed / maxSpeed;
        float factor = 1;

        if (dot > 0)
        {
            factor = downCurve.Evaluate(ratioSpeed);
        }
        else if (dot < 0)
        {
            factor = upCurve.Evaluate(ratioSpeed);
        }

        //Debug.Log($"DOT : {dot} / FACTOR : {factor}");
        speed += (dot * factor * relativeSpeed);
        speed = math.clamp(speed, minSpeed, maxSpeed);
    }

    private void Move()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void CheckState()
    {
        if (state == FlyState.WALK) return;
        
        Vector3 pos = transform.position;
        Ray ray = new Ray(pos, Vector3.down);
        RaycastHit hit;

        Debug.DrawRay(pos, Vector3.down, Color.blue);
        
        if (Physics.Raycast(ray, out hit, 1.75f, nonPlayerLayer))
        {
            ToLand(hit.point);
        }
    }
}

public enum FlyState
{
    GLIDE,
    FLY,
    LAND,
    WALK,
    NONE
}