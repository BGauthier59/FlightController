using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerIdentity identity;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Camera cam;
    [SerializeField] private Vector2 fieldOfViewBounds;
    public float axisRot;

    public void AttachToPlayer(PlayerIdentity identity)
    {
        this.identity = identity;
        identity.cameraController = this;
    }
    
    private void LateUpdate()
    {
        axisRot = Mathf.Lerp(axisRot, -identity.playerController.moveAxis.x, Time.deltaTime * 2);
        
        identity.visuals.localRotation = Quaternion.Euler(0,0,axisRot * 60);
        identity.headBone.localRotation = Quaternion.Euler(4.41f,axisRot * -80,0);
        Quaternion rot = Quaternion.Euler(identity.transform.eulerAngles.x, identity.transform.eulerAngles.y +axisRot * 5,
            identity.transform.eulerAngles.z);
        
        transform.rotation = identity.transform.rotation;
        transform.position = identity.transform.position + (rot * offset);

        if (identity.playerController.GetState() == PlayerController.State.JUMP) return;
        
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,
            Mathf.Lerp(fieldOfViewBounds.x, fieldOfViewBounds.y,
                Mathf.InverseLerp(0, identity.playerController.glideMaxSpeed, identity.playerController.glideSpeed)),
            Time.deltaTime * 5);
        
    }
}