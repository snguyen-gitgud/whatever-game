using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

[System.Serializable]
public class BaseSkill : MonoBehaviour
{
    [Header("Skill id")]
    public Sprite skillClassIcon;
    public string skillName = "";
    [TextArea(15, 20)] public string skillDescription = "";

    [Header("Skill settings")]
    public SkillSlot skillSlot = SkillSlot.NORMAL;
    public SkillTypes skillTypes = SkillTypes.OFFENSIVE;
    public ActorClass requiredClass = ActorClass.BRAWLER;
    public int skillStaminaCost = 2;
    public int skillOverLoadLevel = 1;
    public int skillMaxOverLoadLevel = 3;
    public int skillAccuracyBonus = 0;
    public DamageTypes damageTypes = DamageTypes.PHYSICAL;
    public float baseDamageMultiplier = 1f;
    public float heightLimit = 1f;
    public bool canTriggerReactiveAndPincer = true;

    [Header("Vcam settings")]
    public float vcam_offset_Y = 1.5f;
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
    [Range(0, 10)] public int skillRange = 1;
    public bool includeSelfCast = false;
    public bool mustTargetEmptyGrid = false;
    public bool excludeDiagonal = false;
    public float skillCastingDuration = 0.5f;
    public GameObject skillCastingVfx;

    [HideInInspector] public Vector3 og_model_pos = new Vector3();

    [HideInInspector] public Guirao.UltimateTextDamage.UltimateTextDamageManager textManager;

    [HideInInspector] public bool isReactive = false;
    [HideInInspector] public bool isPincer = false;

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
        og_model_pos = actorController.transform.GetChild(0).localPosition;

        vcam_offset_Y = actor.actorStats.vcam_offset_Y;

        actorController.is_acted = true;
        actorController.actorStats.staminaPoint -= skillStaminaCost * skillOverLoadLevel;
        actorController.actorUI.apPoints.fillAmount = (actorController.actorStats.staminaPoint * 1f) / (actorController.actorStats.maxStaminaPoint * 1f);
        actorController.actorDetails.actorStaminaSlider.fillAmount = actorController.actorUI.apPoints.fillAmount * 0.5f;
        actorController.actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        actorController.actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;

        if (skillCastingVfxObj != null)
            Destroy(skillCastingVfxObj);

        if (skillCastingVfx != null)
            skillCastingVfxObj = Instantiate(skillCastingVfx, actorController.transform.GetChild(0));

        //if (skillCastingDuration == 0f)
        //    ExecuteSkill();
    }

    public virtual void ExecuteSkill(bool is_pincer = false)
    {
        actorController.actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        actorController.actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;

        isPincer = is_pincer;

        if (is_pincer == false)
            BattleMaster.GetInstance().OnShowAnnounce(this.skillName + " <color=#FFD700>x" + skillOverLoadLevel + "</color>", actorController.actorTeams == GridUnitOccupationStates.PLAYER_TEAM? actorController.PlayerTeamBGColor:actorController.OpponentTeamBGColor);
        else
            BattleMaster.GetInstance().OnShowAnnounce("Pincer: " + this.skillName, actorController.actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? actorController.PlayerTeamBGColor : actorController.OpponentTeamBGColor);

        if (isReactive == false) 
            castingVCam.Priority = 99;
        targetController = targetGridUnit.occupiedActor;
        if (is_pincer == false) 
            StartCoroutine(ExecuteSkillSequence());
    }

    public virtual IEnumerator ExecuteSkillSequence()
    {
        //TODO: preemptive skill goes here:
        if (canTriggerReactiveAndPincer == true)
        {

        }
        //---------------------------

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

    public virtual IEnumerator PostAttack()
    {
        if (canTriggerReactiveAndPincer == true) //AoE does not trigger pincer or reactive
        {
            //reactive
            yield return StartCoroutine(ReactiveSkill());

            //pincer
            yield return StartCoroutine(PincerSkill());
        }

        yield return new WaitForSeconds(2f);
        EndSkill();
    }

    public virtual IEnumerator ReactiveSkill()
    {
        if (targetController != null && targetController.actorStats.actorReactiveSkill.ReactiveCheck(targetController, actorController) == true)
        {
            Vector3 new_forward = targetController.transform.GetChild(0).forward;
            new_forward = Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up).normalized;
            targetController.transform.GetChild(0).forward = new_forward;

            BattleMaster.GetInstance().OnShowAnnounce(targetController.actorStats.actorReactiveSkill.reactiveSkillName, targetController.actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? actorController.PlayerTeamBGColor : actorController.OpponentTeamBGColor);

            yield return StartCoroutine(targetController.actorStats.actorReactiveSkill.ReactiveSkillSequence(targetController, actorController, skillOverLoadLevel));
            yield return new WaitForSeconds(1f);
            targetController.actorStats.actorReactiveSkill.react_skill.castingVCam.Priority = 0;
            targetController.actorStats.actorReactiveSkill.react_skill.executingVCam.Priority = 0;
            targetController.actorAnimationController.PlayIdle();
        }
    }

    public virtual IEnumerator PincerSkill()
    {
        if (targetController != null)
        {
            List<ActorController> pincer_actor_list = new List<ActorController>();
            pincer_actor_list.Clear();
            List<GridUnit> pincer_range = BattleMaster.GetInstance().gridManager.FindArea(targetController.occupied_grid_unit, 2, 2, targetController.actorTeams, true, false);
            foreach (GridUnit tile in pincer_range)
            {
                if (tile.occupiedActor != null)
                {
                    //if (Vector3.Dot(Vector3.ProjectOnPlane(tile.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up),
                    //                Vector3.ProjectOnPlane(actorController.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up))
                    //    <= -0.9f)
                    //{
                    //    if (tile.occupiedActor.actorTeams == actorController.actorTeams)
                    //        pincer_actor_list.Add(tile.occupiedActor);
                    //}
                    if (tile.occupiedActor.actorTeams == actorController.actorTeams && tile.occupiedActor != actorController)
                        pincer_actor_list.Add(tile.occupiedActor);
                }
            }

            foreach (ActorController pincer_actor in pincer_actor_list)
            {
                if (pincer_actor != null)
                {
                    BaseSkill pincer_skill = pincer_actor.actorStats.actorNormalAttack;
                    pincer_skill.isReactive = true;
                    pincer_skill.CastingSkill(pincer_actor, skillOverLoadLevel, targetController.occupied_grid_unit);
                    pincer_skill.ExecuteSkill(true);
                    yield return StartCoroutine(pincer_skill.ExecuteSkillSequence());

                    pincer_skill.isReactive = false;
                    yield return new WaitForSeconds(1f);
                    pincer_actor.actorStats.actorNormalAttack.castingVCam.Priority = 0;
                    pincer_actor.actorStats.actorNormalAttack.executingVCam.Priority = 0;
                    pincer_actor.actorAnimationController.PlayIdle();
                    //yield return new WaitForSeconds(1f);
                }
            }
            
        }
    }

    public virtual void EndSkill()
    {
        castingVCam.Priority = 0;
        executingVCam.Priority = 0;
        actorController.transform.GetChild(0).localPosition = og_model_pos;
        BattleMaster.GetInstance().gridManager.cursor_lock = false;
        actorAnimationController.PlayIdle();
        actorController.WaitingForCommandState();
    }

    public virtual SkillPreview GetPreviewValue(ActorController caster, ActorController target, int overload)
    {
        return null;
    }

    List<GridUnit> aoePreview;
    public virtual List<GridUnit> GetPreviewAoE(GridUnit root, GridUnit highlighted_grid_unit)
    {
        if (aoePreview != null)
        {
            foreach (GridUnit grid_unit in aoePreview)
                grid_unit?.ClearAoEHighlight();
        }

        aoePreview = new List<GridUnit>();
        aoePreview.Clear();

        return aoePreview;
    }
}

public enum DamageTypes
{
    PHYSICAL,
    MAGICAL,
    MIXED,
    PURE,

    BUFF
}

public enum SkillTypes
{
    OFFENSIVE,
    SUPPORTIVE
}

public enum SkillSlot
{
    NORMAL,
    MAIN_SUB,
    PREEMPTIVE,
    REACTIVE,
    PASSIVE,
    ULTIMATE
}
