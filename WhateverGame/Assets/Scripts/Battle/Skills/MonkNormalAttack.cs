using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MonkNormalAttack : BaseSkill
{
    [Header("Specific VFX")]
    public GameObject atk1VFX;
    public GameObject atk2VFX;
    public GameObject atk3VFX;
    public GameObject hitVFX;

    public override void CastingSkill(ActorController actor, int overload_level = 1, GridUnit target = null)
    {
        base.CastingSkill(actor, overload_level, target);
        Debug.Log(actorController.gameObject.name + " casting Monk normal attack.");
        actorAnimationController.PlayCasting();
    }

    public override void Executekill()
    {
        Vector3 new_forward = actorController.transform.GetChild(0).forward;
        new_forward = Vector3.ProjectOnPlane(targetGridUnit.cachedWorldPos - actorController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
        actorController.transform.GetChild(0).forward = new_forward;
        og_model_pos = actorController.transform.GetChild(0).position;
        actorController.transform.GetChild(0).position += new_forward;

        base.Executekill();
        Debug.Log(actorController.gameObject.name + " executing Monk normal attack.");
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();

        Debug.Log(actorController.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayNormalAttack_1();
        GameObject atk_vfx = Instantiate(atk1VFX, actorController.transform.GetChild(0));
        atk_vfx.transform.position += Vector3.up * 1.5f;

        shake.m_FrequencyGain = 1f;
        yield return new WaitForSeconds(.161f);

        if (targetController != null)
        {
            ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);
            if (data.isAmbush)
                textManager.Add("Ambush", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "critical");

            if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
            {
                InputProcessor.GetInstance().VibrateController(.25f, .25f, .1f);
                targetController.actorAnimationController.PlayGetHit();
                GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                hit_vfx.transform.position += (Vector3.up * 1.5f) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                targetController.actorStats.currentStats.HealthChange(-data.output);
                if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "critical");
                textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");
                shake.m_AmplitudeGain = 1f;
                Time.timeScale = 0.1f;
                yield return new WaitForSecondsRealtime(.1f);
                Time.timeScale = 1f;
                shake.m_AmplitudeGain = 0f;
            }
            else if (data.isMiss)
            {
                // miss
                textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");
            }
            else if (data.isBlocked)
            {
                // blocked 
                textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");

                Vector3 new_forward = targetController.transform.GetChild(0).forward;
                new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                targetController.transform.GetChild(0).forward = new_forward;
            }
        }

        if (skillOverLoadLevel >= 2)
        {
            actorAnimationController.PlayNormalAttack_2();
            atk_vfx = Instantiate(atk2VFX, actorController.transform.GetChild(0));
            atk_vfx.transform.position += Vector3.up * 1.5f;

            shake.m_FrequencyGain = 3f;
            yield return new WaitForSeconds(.3667f);
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);

                if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
                {
                    InputProcessor.GetInstance().VibrateController(.35f, .35f, .25f);
                    targetController.actorAnimationController.PlayGetHit();
                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * 1.5f) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    targetController.actorStats.currentStats.HealthChange(-data.output);
                    if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "critical");
                    textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");
                    shake.m_AmplitudeGain = 1f;
                    Time.timeScale = 0.1f;

                    yield return new WaitForSecondsRealtime(.25f);
                    Time.timeScale = 1f;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss
                    textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");
                }
                else if (data.isBlocked)
                {
                    // blocked 
                    textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");

                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }
        
        if (skillOverLoadLevel >= 3)
        {
            actorAnimationController.PlayNormalAttack_3();
            atk_vfx = Instantiate(atk3VFX, actorController.transform.GetChild(0));
            atk_vfx.transform.position += Vector3.up * 1.5f;

            shake.m_FrequencyGain = 5f;
            yield return new WaitForSeconds(.261f);
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 2);

                if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
                {
                    InputProcessor.GetInstance().VibrateController(.5f, .5f, .5f);
                    targetController.actorAnimationController.PlayGetHit();
                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * 1.5f) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    targetController.actorStats.currentStats.HealthChange(-data.output);
                    if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "critical");
                    textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");

                    if (targetController.actorControlStates == ActorControlStates.AP_STAG)
                    {
                        targetController.actorStats.apBar -= 10f;
                        targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar / 100f;
                    }
                    else if (targetController.actorControlStates == ActorControlStates.CASTING_STAG)
                    {
                        targetController.current_casting_value -= (targetController.current_casting_value * 0.9f);
                        targetController.actorStats.apBar = targetController.current_casting_value / targetController.currentChosenSkill.skillCastingDuration;
                        targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar;
                    }

                    shake.m_AmplitudeGain = 1f;
                    Time.timeScale = 0.1f;

                    yield return new WaitForSecondsRealtime(.5f);
                    Time.timeScale = 1f;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss
                    textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");
                }
                else if (data.isBlocked)
                {
                    // blocked 
                    textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * 1.5f, "default");

                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }

        yield return new WaitForSeconds(1f);

        if (isReactive == false)
        {
            StartCoroutine(base.PostAttack());
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }
}
