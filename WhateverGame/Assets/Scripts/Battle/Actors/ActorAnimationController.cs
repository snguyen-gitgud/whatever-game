using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimationController : MonoBehaviour
{
    public Animator animator;

    public void PlayIdle()
    {
        animator.SetFloat("Move", 0f);
    }

    public void PlayMove()
    {
        animator.SetFloat("Move", 1f);
    }
}
