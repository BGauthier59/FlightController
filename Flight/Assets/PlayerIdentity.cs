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
    public Transform visuals, headBone;
    public Animator animator;
    public GameObject[] player1Body;
    public GameObject[] player2Body;

    public async void ChangeAnimation(Anim anim)
    {
        // Pour changer le state de l'animator 
        switch (anim)
        {
            case Anim.Glide:
                animator.SetInteger("Animation", 0);
                break;
            case Anim.Flap:
                animator.SetInteger("Animation", 1);
                await Task.Delay(200);
                animator.SetInteger("Animation", 0);
                break;
            case Anim.Walk:
                animator.SetInteger("Animation", 4);
                break;
            case Anim.Idle:
                animator.SetInteger("Animation", 3);
                break;
            case Anim.Land:
                animator.SetInteger("Animation", 2);
                await Task.Delay(200);
                if (playerController.GetState() == PlayerController.State.GLIDE) animator.SetInteger("Animation", 0);
                else animator.SetInteger("Animation", 3);
                break;
        }
    }

    public void ChangeBody(int i)
    {
        // On change l'apparence du joueur en fonction de si c'est le J1 ou J2
        if (i == 0)
        {
            for (int j = 0; j < player1Body.Length; j++)
            {
                player1Body[j].SetActive(true);
                player2Body[j].SetActive(false);
            }
        }
        else
        {
            for (int j = 0; j < player1Body.Length; j++)
            {
                player1Body[j].SetActive(false);
                player2Body[j].SetActive(true);
            }
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