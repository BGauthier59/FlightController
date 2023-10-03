using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlanerController_PROTOTYPE : MonoBehaviour
{
    public float speed, maxSpeed, minSpeed;
    public float ratioSpeed;
    public AnimationCurve downCurve, upCurve;

    private float y;
    private float x;
    public float rotateSpeed;

    public float relativeSpeed;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 joystick = ctx.ReadValue<Vector2>();
        y = joystick.y;
        x = joystick.x;
    }

    private void Update()
    {
        Rotate();

        Vector3 forward = transform.forward;
        Vector3 down = Vector3.down;

        float dot = math.dot(forward, down);

        EvaluateSpeed(dot);

        Move();
    }

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

        Debug.Log($"DOT : {dot} / FACTOR : {factor}");
        speed += (dot * factor * relativeSpeed);
        speed = math.clamp(speed, minSpeed, maxSpeed);
    }

    private void Move()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }
}