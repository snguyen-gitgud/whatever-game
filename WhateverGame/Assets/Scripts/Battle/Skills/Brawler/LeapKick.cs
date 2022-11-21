using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LeapKick : BaseSkill
{
    [Header("Specific VFX")]
    public GameObject atkVFX;
    public GameObject hitVFX;
    public GameObject jumpingVFX;
    public GameObject landingVFX;

    public override void CastingSkill(ActorController actor, int overload_level = 1, GridUnit target = null)
    {
        base.CastingSkill(actor, overload_level, target);
        //Debug.Log(actorController.gameObject.name + " casting Monk normal attack.");
        actorAnimationController.PlayCasting();
    }

    public override void ExecuteSkill(bool is_pincer = false)
    {
        Vector3 new_forward = actorController.transform.GetChild(0).forward;
        new_forward = Vector3.ProjectOnPlane(targetGridUnit.cachedWorldPos - actorController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
        actorController.transform.GetChild(0).forward = new_forward;
        //og_model_pos = actorController.transform.GetChild(0).position;
        //actorController.transform.GetChild(0).position += new_forward;

        base.ExecuteSkill(is_pincer);
        //Debug.Log(actorController.gameObject.name + " executing Monk normal attack.");
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();
        GridUnit jump_to_grid_unit = targetGridUnit;
        ActorController landing_actor = jump_to_grid_unit.occupiedActor;

        //Debug.Log(actorController.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayUnarmedAttack_6();
        GameObject atk_vfx = Instantiate(atkVFX, actorController.transform.GetChild(0));
        atk_vfx.transform.position += Vector3.up * vcam_offset_Y;
        
        Instantiate(jumpingVFX, actorController.transform.GetChild(0));

        GridUnit og_grid_unit = actorController.occupied_grid_unit;
        actorController.occupied_grid_unit.occupiedActor = null;
        actorController.transform.DOJump(jump_to_grid_unit.cachedWorldPos, 2f, 1, 0.86667f, false).SetEase(Ease.Linear).OnComplete(() => {
            actorController.occupied_grid_unit = jump_to_grid_unit;
            jump_to_grid_unit.occupiedActor = actorController;
            
            actorController.transform.GetChild(0).localPosition = Vector3.zero;
            Instantiate(landingVFX, actorController.transform.GetChild(0));
        });

        shake.m_FrequencyGain = 5f;
        yield return new WaitForSeconds(.8667f);

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
                Time.timeScale = 0.01f * BattleMaster.GetInstance().baseTimeScale;
                yield return new WaitForSecondsRealtime(.5f);
                Time.timeScale = BattleMaster.GetInstance().baseTimeScale;
                shake.m_AmplitudeGain = 0f;
            }
            else if (data.isMiss)
            {
                // miss
                textManager.Add("Miss", targetController.transform.GetChild(0).position + Vector3.up * vcam_offset_Y, "default");

                targetController.actorAnimationController.PlayDodge();

                Vector3 new_forward = targetController.transform.GetChild(0).forward;
                new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                targetController.transform.GetChild(0).forward = new_forward;
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

        //TODO: find and set secondary target






        yield return new WaitForSeconds(1f);
        if (landing_actor != null /*landing_actor*/)
        {
            //TODO: jump back
            actorAnimationController.PlayJump();
            actorController.occupied_grid_unit.occupiedActor = null;
            actorController.transform.DOJump(og_grid_unit.cachedWorldPos, 2f, 1, 0.75f, false).SetEase(Ease.Linear).OnComplete(() => {
                actorController.occupied_grid_unit = og_grid_unit;
                og_grid_unit.occupiedActor = actorController;

                actorController.transform.GetChild(0).localPosition = Vector3.zero;
                Instantiate(landingVFX, actorController.transform.GetChild(0));
                //actorAnimationController.PlayIdle();
            });
            yield return new WaitForSeconds(1.0f);
        }

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
        ret.value = "-" + sum + " HP";
        return ret;
    }
}
