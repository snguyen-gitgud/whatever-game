using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

[System.Serializable]
public class BaseSkill : MonoBehaviour
{
    [Header("Skill id")]
    public string skillName = "";
    [TextArea(15, 20)] public string skillDescription = "";

    [Header("Skill settings")]
    public int skillStaminaCost = 2;
    public int skillOverLoadLevel = 1;
    public int skillMaxOverLoadLevel = 3;
    public int skillAccuracyBonus = 0;
    public DamageTypes damageTypes = DamageTypes.PHYSICAL;
    public int baseDamage = 5;

    [Header("Vcam settings")]
    public Cinemachine.CinemachineVirtualCamera castingVCam;
    public Cinemachine.CinemachineVirtualCamera executingVCam;

    //internal
    [Header("Skill internals")]
    public ActorController actorController;
    public ActorController targetController;
    public GridUnit targetGridUnit;
    public ActorAnimationController actorAnimationController;
    public CinemachineBasicMultiChannelPerlin shake;

    [Header("Common VFX")]
    [Range(1, 10)] public int skillRange = 1;
    public float skillCastingDuration = 0.5f;
    public GameObject skillCastingVfx;

    [HideInInspector] public Vector3 og_model_pos = new Vector3();

    [HideInInspector] public Guirao.UltimateTextDamage.UltimateTextDamageManager textManager;

    [HideInInspector] public bool isReactive = false;

    [HideInInspector] public GameObject skillCastingVfxObj;

    public virtual void CastingSkill(ActorController actor, int overload_level = 1, GridUnit target_grid_tile = null)
    {
        GameObject go = GameObject.FindGameObjectWithTag("TextDamage");
        textManager = go.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();

        castingVCam = actor.castingVCamsList[Random.Range(0, actor.castingVCamsList.Count)];
        executingVCam = actor.executingVCamsList[Random.Range(0, actor.executingVCamsList.Count)];
        shake = executingVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        actorController = actor;
        actorAnimationController = actor.actorAnimationController;
        skillOverLoadLevel = overload_level;
        targetGridUnit = target_grid_tile;
        targetController = target_grid_tile.occupiedActor;

        actorController.is_acted = true;
        actorController.actorStats.staminaPoint -= skillStaminaCost * skillOverLoadLevel;
        actorController.actorUI.apPoints.fillAmount = (actorController.actorStats.staminaPoint * 1f) / (actorController.actorStats.maxStaminaPoint * 1f);
        actorController.actorDetails.actorStaminaSlider.fillAmount = actorController.actorUI.apPoints.fillAmount * 0.5f;
        actorController.actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        actorController.actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;

        if (skillCastingVfx != null)
            skillCastingVfxObj = Instantiate(skillCastingVfx, actorController.transform.GetChild(0));
    }

    public virtual void Executekill()
    {
        actorController.actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        actorController.actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;

        if (isReactive == false) 
            castingVCam.Priority = 99;
        targetController = targetGridUnit.occupiedActor;
        StartCoroutine(ExecuteSkillSequence());
    }

    public virtual IEnumerator ExecuteSkillSequence()
    {
        BattleMaster.GetInstance().gridManager.cursor_lock = true;
        DOTween.Kill(BattleMaster.GetInstance().gridManager.gridCursor.transform);
        BattleMaster.GetInstance().gridManager.gridCursor.transform.position = targetGridUnit.cachedWorldPos + Vector3.up * BattleMaster.GetInstance().gridManager.gridUnitSize * 0.5f;
        if (isReactive == false) 
            yield return new WaitForSeconds(2f);

        Destroy(skillCastingVfxObj);
        executingVCam.Priority = 99;
        castingVCam.Priority = 0;
        yield return new WaitForSeconds(1f);
    }

    public virtual IEnumerator TriggerReactive()
    {
        if (targetController.actorStats.actorReactiveSkill.ReactiveCheck(targetController, actorController) == true)
        {
            Vector3 new_forward = targetController.transform.GetChild(0).forward;
            new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
            targetController.transform.GetChild(0).forward = new_forward;

            yield return StartCoroutine(targetController.actorStats.actorReactiveSkill.ReactiveSkillSequence(targetController, actorController, skillOverLoadLevel));
            yield return new WaitForSeconds(1f);
            targetController.actorStats.actorReactiveSkill.react_skill.castingVCam.Priority = 0;
            targetController.actorStats.actorReactiveSkill.react_skill.executingVCam.Priority = 0;
            targetController.actorAnimationController.PlayIdle();
        }
        
        EndSkill();
    }

    public virtual void EndSkill()
    {
        castingVCam.Priority = 0;
        executingVCam.Priority = 0;
        actorController.transform.GetChild(0).position = og_model_pos;
        BattleMaster.GetInstance().gridManager.cursor_lock = false;
        actorAnimationController.PlayIdle();
        actorController.WaitingForCommandState();
    }
}

public enum DamageTypes
{
    PHYSICAL,
    MAGICAL,
    MIXED,
    PURE
}
