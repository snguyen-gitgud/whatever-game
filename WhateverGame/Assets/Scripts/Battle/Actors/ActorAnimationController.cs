using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimationController : MonoBehaviour
{
    public Animator animator;

    public void PlayIdle()
    {
        animator.CrossFadeInFixedTime("Locomotion", .25f);
        animator.SetFloat("Move", 0f);
    }

    public void PlayMove()
    {
        animator.CrossFadeInFixedTime("Locomotion", .25f);
        animator.SetFloat("Move", 1f);
    }

    public void PlayBurnOut()
    {
        animator.CrossFadeInFixedTime("BurnOut", .25f);
    }

    public void PlayGetHit()
    {
        animator.CrossFadeInFixedTime("GetHit" + Random.Range(1, 4), .1f);
    }

    public void PlayBlock()
    {
        animator.CrossFadeInFixedTime("Block", .1f);
    }

    public void PlayDodge()
    {
        animator.CrossFadeInFixedTime("Dodge", .1f);
    }

    public void PlayCasting()
    {
        animator.CrossFadeInFixedTime("Casting", .25f);
    }

    public void PlayNormalAttack_1()
    {
        animator.CrossFadeInFixedTime("NormalAttack_1", .01f);
    }

    public void PlayNormalAttack_2(GameObject vfx = null)
    {
        animator.CrossFadeInFixedTime("NormalAttack_2", .01f);
    }

    public void PlayNormalAttack_3(GameObject vfx = null)
    {
        animator.CrossFadeInFixedTime("NormalAttack_3", .01f);
    }
}
