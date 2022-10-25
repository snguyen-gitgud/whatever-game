using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEffects : MonoBehaviour
{
    Animator animator;
    public PlayerAbilityManager playerAbilityManager;

    private void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    public void LightPause()
    {
        StartCoroutine(SlowTime(0.08f));
    }

    public void HeavyPause()
    {
        StartCoroutine(SlowTime(0.125f));
    }

    public void ExtraHeavyPause()
    {
        StartCoroutine(SlowTime(0.2f));
    }

    IEnumerator SlowTime(float dur)
    {
        //Time.timeScale = 0.01f;
        animator.speed = 0.0f;
        yield return new WaitForSecondsRealtime(dur);
        //Time.timeScale = 1.0f;
        animator.speed = 1.0f;
    }

    public void ToggleCanDealDmgOn(int dmg_multiplier = 1)
    {
        playerAbilityManager.m_CanDealDmg = true;
        playerAbilityManager.m_DmgMultiplier = dmg_multiplier;
    }

    public void ToggleCanDealDmgOff()
    {
        playerAbilityManager.m_CanDealDmg = false;
    }
}
