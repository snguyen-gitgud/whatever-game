using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimationController : MonoBehaviour
{
    public Animator animator;
    public EquippedWeaponTypes weaponTypes = EquippedWeaponTypes.UNARMED;
    public BodyTypes bodyTypes = BodyTypes.FEMALE;
    public ActorInfo actorInfo;

    private void Start()
    {
        actorInfo = this.GetComponent<ActorInfo>();
    }

    public void PlayIdle()
    {
        if (actorInfo == null)
            actorInfo = this.GetComponent<ActorInfo>();

        animator.SetFloat("Speed", (actorInfo.currentStats.speed * 1f) / 100f);
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("Locomotion", .25f);
        animator.SetFloat("Move", 0f);
        animator.SetBool("IsCasting", false);
    }

    public void PlayMove()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("Locomotion", .25f);
        animator.SetFloat("Move", 1f);
    }

    public void PlayBurnOut()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("BurnOut", .25f);
    }

    public void PlayGetHit()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("GetHit" + Random.Range(1, 4), .1f);
    }

    public void PlayBlock()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("Block", .1f);
    }

    public void PlayDodge()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("Dodge", .1f);
    }

    public void PlayCasting()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("Casting", .25f);
        animator.SetBool("IsCasting", true);
    }

    public void PlayUnarmedAttack_1()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("UnarmedAttack_1", .01f);
    }

    public void PlayUnarmedAttack_2(GameObject vfx = null)
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("UnarmedAttack_2", .01f);
    }

    public void PlayUnarmedAttack_3(GameObject vfx = null)
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);

        animator.CrossFadeInFixedTime("UnarmedAttack_3", .01f);
    }

    public void PlayUnarmedBuff()
    {
        animator.SetFloat("EquippedWeaponType", (int)weaponTypes * 1f);
        animator.SetFloat("Gender", (int)bodyTypes * 1f);
        animator.CrossFadeInFixedTime("UnarmedBuff", .01f);
    }    
}

[System.Serializable]
public enum EquippedWeaponTypes
{
    UNARMED = 0,
}

[System.Serializable]
public enum BodyTypes
{
    FEMALE,
    MALE
}
