using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    public void AttachToPlayer(Transform target)
    {
        this.target = target;
    }
    
    private void LateUpdate()
    {
        transform.rotation = target.rotation;
        transform.position = target.position - transform.forward * 10 + Vector3.up * 3;
    }
}