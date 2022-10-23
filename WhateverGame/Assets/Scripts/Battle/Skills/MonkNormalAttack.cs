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
        base.Executekill();
        Debug.Log(this.gameObject.name + " executing Monk normal attack.");
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();

        Debug.Log(this.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayNormalAttack_1();
        shake.m_FrequencyGain = 3f;
        yield return new WaitForSeconds(.1f);
        targetController.actorAnimationController.PlayGetHit();
        shake.m_AmplitudeGain = 1f;
        yield return new WaitForSeconds(.15f);
        shake.m_AmplitudeGain = 0f;

        if (skillOverLoadLevel >= 2)
        {
            actorAnimationController.PlayNormalAttack_2();
            shake.m_FrequencyGain = 5f;
            yield return new WaitForSeconds(.3667f);
            targetController.actorAnimationController.PlayGetHit();
            shake.m_AmplitudeGain = 1f;
            yield return new WaitForSeconds(.5f);
            shake.m_AmplitudeGain = 0f;
        }
        
        if (skillOverLoadLevel >= 3)
        {
            actorAnimationController.PlayNormalAttack_3();
            shake.m_FrequencyGain = 7f;
            yield return new WaitForSeconds(.125f);
            targetController.actorAnimationController.PlayGetHit();
            if (targetController.actorControlStates == ActorControlStates.AP_STAG)
            {
                targetController.actorStats.apBar -= 10f;
                targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar / 100f;
            }
            else if (targetController.actorControlStates == ActorControlStates.CASTING_STAG)
            {
                targetController.current_casting_value -= targetController.currentChosenSkill.skillCastingDuration * .1f;
                if (targetController.current_casting_value <= 0f)
                    targetController.current_casting_value = 0f;

                targetController.actorStats.apBar = targetController.current_casting_value / targetController.currentChosenSkill.skillCastingDuration;
                targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar;
            }
            shake.m_AmplitudeGain = 1f;
            yield return new WaitForSeconds(.875f);
            shake.m_AmplitudeGain = 0f;
        }

        yield return new WaitForSeconds(1f);
        EndSkill();
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }
}
