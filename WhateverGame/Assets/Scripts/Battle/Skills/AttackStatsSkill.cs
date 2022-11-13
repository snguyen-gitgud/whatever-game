using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStatsSkill : BaseSkill
{
    [Header("Specific VFX")]
    public GameObject statusPref;
    public string animParam = "";

    public override void CastingSkill(ActorController actor, int overload_level = 1, GridUnit target_grid_tile = null)
    {
        base.CastingSkill(actor, overload_level, target_grid_tile);

        actorAnimationController.PlayCasting();
    }

    public override void ExecuteSkill(bool is_pincer = false)
    {
        Vector3 new_forward = actorController.transform.GetChild(0).forward;
        new_forward = Vector3.ProjectOnPlane(targetGridUnit.cachedWorldPos - actorController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
        actorController.transform.GetChild(0).forward = new_forward;

        base.ExecuteSkill(is_pincer);
    }

    public override IEnumerator ExecuteSkillSequence()
    {
        yield return base.ExecuteSkillSequence();
        //Debug.Log(actorController.gameObject.name + " executing Monk normal attack sequence.");
        actorAnimationController.PlayUnarmedBuff();
        GameObject atk_vfx = Instantiate(statusPref, actorController.actorStats.statusesHolder);
        AttackStatus status = atk_vfx.GetComponent<AttackStatus>();
        status.atkChangePercentage = (int)(baseDamageMultiplier * 100f);
        status.ProcStatus(actorController, actorController.actorStats);

        yield return new WaitForSeconds(1f);
        actorAnimationController.PlayIdle();

        yield return new WaitForSeconds(1f);
        StartCoroutine(base.PostAttack());
    }

    public override void EndSkill()
    {
        base.EndSkill();
    }

    public override SkillPreview GetPreviewValue(ActorController caster, ActorController target, int overload)
    {
        SkillPreview ret = new SkillPreview();

        ret.chance_text = "100%";
        ret.value = "+" + (baseDamageMultiplier * caster.actorStats.currentStats.pAtk) + " P.ATK";
        return ret;
    }
}
