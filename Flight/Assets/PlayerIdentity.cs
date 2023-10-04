using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    public int index;
    public PlayerController playerController;
    public UIManager uiManager;
    public CameraController cameraController;
    public Transform visuals,headBone;
    public Animator animator;

    public async void ChangeAnimation(Anim anim)
    {
        switch (anim)
        {
            case Anim.Glide:
                animator.SetInteger("Animation",0);
                break;
            case Anim.Flap:
                animator.SetInteger("Animation",1);
                await Task.Delay(200);
                animator.SetInteger("Animation",0);
                break;
            case Anim.Walk:
                animator.SetInteger("Animation",4);
                break;
            case Anim.Idle:
                animator.SetInteger("Animation",3);
                break;
            case Anim.Land:
                animator.SetInteger("Animation",2);
                await Task.Delay(200);
                animator.SetInteger("Animation",3);
                break;
        }
    }



}

public enum Anim
{
    Flap,
    Glide,
    Walk,
    Idle,
    Land
}
