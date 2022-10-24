using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkNormalAttack : BaseSkill
{
    public override void CastingSkill(ActorController actor, int overload_level = 1, GridUnit target = null)
    {
        base.CastingSkill(actor, overload_level, target);
        Debug.Log(this.gameObject.name + " casting Monk normal attack.");
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
        Debug.Log(this.gameObject.name + " executing Monk normal attack.");
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();

        Debug.Log(this.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayNormalAttack_1();
        shake.m_FrequencyGain = 1f;
        yield return new WaitForSeconds(.161f);

        if (targetController != null)
        {
            ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);
            
            if (!data.isMiss && !data.isBlocked)
            {
                targetController.actorAnimationController.PlayGetHit();
                targetController.actorStats.currentStats.GetHit(data.output);
                shake.m_AmplitudeGain = 1f;
                Time.timeScale = 0.1f;
                yield return new WaitForSecondsRealtime(.1f);
                Time.timeScale = 1f;
                shake.m_AmplitudeGain = 0f;
            }
            else if (data.isMiss)
            {
                // miss

            }
            else if (data.isBlocked)
            {
                // blocked 
                Vector3 new_forward = targetController.transform.GetChild(0).forward;
                new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                targetController.transform.GetChild(0).forward = new_forward;
            }
        }

        if (skillOverLoadLevel >= 2)
        {
            actorAnimationController.PlayNormalAttack_2();
            shake.m_FrequencyGain = 3f;
            yield return new WaitForSeconds(.3667f);
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 1);

                if (!data.isMiss && !data.isBlocked)
                {
                    targetController.actorAnimationController.PlayGetHit();
                    targetController.actorStats.currentStats.GetHit(data.output);
                    shake.m_AmplitudeGain = 1f;
                    Time.timeScale = 0.1f;

                    yield return new WaitForSecondsRealtime(.25f);
                    Time.timeScale = 1f;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss

                }
                else if (data.isBlocked)
                {
                    // blocked 
                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }
        
        if (skillOverLoadLevel >= 3)
        {
            actorAnimationController.PlayNormalAttack_3();
            shake.m_FrequencyGain = 5f;
            yield return new WaitForSeconds(.261f);
            if (targetController != null)
            {
                ClashData data = ShieldHelpers.CalculateClashOutput(actorController, targetController, this, 2);

                if (!data.isMiss && !data.isBlocked)
                {
                    targetController.actorAnimationController.PlayGetHit();
                    targetController.actorStats.currentStats.GetHit(data.output);

                    targetController.actorStats.apBar -= 10f;
                    targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar / 100f;

                    shake.m_AmplitudeGain = 1f;
                    Time.timeScale = 0.1f;

                    yield return new WaitForSecondsRealtime(.5f);
                    Time.timeScale = 1f;
                    shake.m_AmplitudeGain = 0f;
                }
                else if (data.isMiss)
                {
                    // miss

                }
                else if (data.isBlocked)
                {
                    // blocked 
                    Vector3 new_forward = targetController.transform.GetChild(0).forward;
                    new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
                    targetController.transform.GetChild(0).forward = new_forward;
                }
            }
        }

        yield return new WaitForSeconds(1f);
        EndSkill();
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }
}
