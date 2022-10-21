using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimationController : MonoBehaviour
{
    public Animator animator;

    public void PlayIdle()
    {
        animator.SetTrigger("Idle");
        animator.SetFloat("Move", 0f);
    }

    public void PlayMove()
    {
        animator.SetFloat("Move", 1f);
    }

    public void PlayBurnOut()
    {
        animator.SetTrigger("BurnOut");
    }
}
