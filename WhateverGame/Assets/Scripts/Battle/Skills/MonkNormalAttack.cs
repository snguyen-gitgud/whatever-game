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
        //Debug.Log(actorController.gameObject.name + " casting Monk normal attack.");
        actorAnimationController.PlayCasting();
    }

    public override void Executekill(bool is_pincer = false)
    {
        Vector3 new_forward = actorController.transform.GetChild(0).forward;
        new_forward = Vector3.ProjectOnPlane(targetGridUnit.cachedWorldPos - actorController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
        actorController.transform.GetChild(0).forward = new_forward;
        //og_model_pos = actorController.transform.GetChild(0).position;
        //actorController.transform.GetChild(0).position += new_forward;

        base.Executekill(is_pincer);
        //Debug.Log(actorController.gameObject.name + " executing Monk normal attack.");
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();

        //Debug.Log(actorController.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayUnarmedAttack_1();
        GameObject atk_vfx = Instantiate(atk1VFX, actorController.transform.GetChild(0));
        atk_vfx.transform.position += Vector3.up * vcam_offset_Y;

        shake.m_FrequencyGain = 1f;
        yield return new WaitForSeconds(.161f);

        if (targetController != null)
        {
            ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);
            if (data.isAmbush)
                textManager.Add("Ambush", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");

            if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
            {
                InputProcessor.GetInstance().VibrateController(.125f, .125f, .25f);
                targetController.actorAnimationController.PlayGetHit();
                GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                targetController.actorStats.currentStats.HealthChange(-data.output);
                if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");
                textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");
                shake.m_AmplitudeGain = 1f;
                Time.timeScale = 0.1f * BattleMaster.GetInstance().baseTimeScale;
                yield return new WaitForSecondsRealtime(.1f);
                Time.timeScale = BattleMaster.GetInstance().baseTimeScale;
                shake.m_AmplitudeGain = 0f;
            }
            else if (data.isMiss)
            {
                // miss
                textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                targetController.actorAnimationController.PlayDodge();
            }
            else if (data.isBlocked)
            {
                // blocked 
                textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                targetController.actorAnimationController.PlayBlock();

                GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                Vector3 new_forward = targetController.transform.GetChild(0).forward;
                new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                targetController.transform.GetChild(0).forward = new_forward;
            }
        }

        if (skillOverLoadLevel >= 2)
        {
            actorAnimationController.PlayUnarmedAttack_2();
            
            atk_vfx = Instantiate(atk2VFX, actorController.transform.GetChild(0));
            atk_vfx.transform.position += Vector3.up * vcam_offset_Y;

            shake.m_FrequencyGain = 3f;
            yield return new WaitForSeconds(.3667f);
            textManager.Add("Combo x2", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);

                if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
                {
                    InputProcessor.GetInstance().VibrateController(.235f, .235f, .5f);
                    targetController.actorAnimationController.PlayGetHit();
                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    targetController.actorStats.currentStats.HealthChange(-data.output);
                    if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");
                    textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");
                    shake.m_AmplitudeGain = 1f;
                    Time.timeScale = 0.1f * BattleMaster.GetInstance().baseTimeScale;
                    yield return new WaitForSecondsRealtime(.25f);
                    Time.timeScale = BattleMaster.GetInstance().baseTimeScale;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss
                    textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                    targetController.actorAnimationController.PlayDodge();
                }
                else if (data.isBlocked)
                {
                    // blocked 
                    textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                    targetController.actorAnimationController.PlayBlock();

                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }
        
        if (skillOverLoadLevel >= 3)
        {
            actorAnimationController.PlayUnarmedAttack_3();
            
            atk_vfx = Instantiate(atk3VFX, actorController.transform.GetChild(0));
            atk_vfx.transform.position += Vector3.up * vcam_offset_Y;

            shake.m_FrequencyGain = 5f;
            yield return new WaitForSeconds(.261f);
            textManager.Add("Combo x3", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 2);

                if (!data.isMiss && !data.isBlocked || data.isAmbush && !data.isBlocked)
                {
                    InputProcessor.GetInstance().VibrateController(.35f, .35f, 1f);
                    targetController.actorAnimationController.PlayGetHit();
                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    targetController.actorStats.currentStats.HealthChange(-data.output);
                    if (data.isCrit) textManager.Add("Critical hit", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "critical");
                    textManager.Add((-data.output).ToString(), targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

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
                    Time.timeScale = 0.1f * BattleMaster.GetInstance().baseTimeScale;
                    yield return new WaitForSecondsRealtime(.5f);
                    Time.timeScale = BattleMaster.GetInstance().baseTimeScale;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss
                    textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                    targetController.actorAnimationController.PlayDodge();
                }
                else if (data.isBlocked)
                {
                    // blocked 
                    textManager.Add("block", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                    targetController.actorAnimationController.PlayBlock();

                    GameObject hit_vfx = Instantiate(hitVFX, targetController.transform.GetChild(0));
                    hit_vfx.transform.position += (Vector3.up * vcam_offset_Y) + new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), Random.Range(-.25f, .25f));

                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }

        yield return new WaitForSeconds(1f);

        if (isReactive == false && isPincer == false)
        {
            StartCoroutine(base.PostAttack());
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    public override SkillPreview GetPreviewValue(ActorController caster, ActorController target, int overload)
    {
        SkillPreview ret = new SkillPreview();

        ClashData data1 = ShieldHelpers.CalculateClashOutput(caster, target, this, 1, true);
        ClashData data2 = ShieldHelpers.CalculateClashOutput(caster, target, this, 1, true);
        ClashData data3 = ShieldHelpers.CalculateClashOutput(caster, target, this, 2, true);

        int sum = 0;
        if (overload == 1)
            sum = data1.output;
        else if (overload == 2)
            sum = data1.output + data2.output;
        else if (overload >= 3)
            sum = data1.output + data2.output + data3.output;

        ret.chance_val = ((caster.actorStats.currentStats.accuracy + this.skillAccuracyBonus) - target.actorStats.currentStats.dodgeChance) / 100f;
        ret.chance_text = (ret.chance_val * 100f) + "%";
        ret.value = "DMG : " + sum + " HP";
        return ret;
    }
}

public class SkillPreview
{
    public float chance_val = 0f; 
    public string chance_text = "";
    public string value = "";
}
