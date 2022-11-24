using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ActorAnimationController))]
[RequireComponent(typeof(ActorInfo))]
[RequireComponent(typeof(ActorUI))]
public class ActorController : MonoBehaviour
{
    public GridUnitOccupationStates actorTeams = GridUnitOccupationStates.PLAYER_TEAM;
    public ActorControlStates actorControlStates = ActorControlStates.AP_STAG;
    public GridUnit occupied_grid_unit = null;

    [Header("Animations and effects")]
    public ActorAnimationController actorAnimationController;

    [Header("Stats")]
    public ActorInfo actorStats;
    public float burn_out_dur = 5f;

    [Header("UI")]
    public ActorUI actorUI;
    public BattleActorDetails actorDetails;
    public TextMeshProUGUI apText;

    public Color normalAPGenColor;
    public Color exhautedAPGenColor;
    public Color acceleratedAPGenColor;
    public Color castingGenColor;

    public Color PlayerTeamBGColor;
    public Color OpponentTeamBGColor;

    public GameObject commandControlUI;

    [Header("Action preview")]
    public ScrollviewSnap scrollviewSnap;
    public GameObject actionPreviewUI;
    public Image actionPreviewBG;
    public Image casterPortrait;
    public Image targetPortrait;
    public Image probabilityImg;
    public List<GameObject> previewOverloadLevelImgList = new List<GameObject>(); 
    public TextMeshProUGUI actionNameText;
    public TextMeshProUGUI actionChanceText;
    public TextMeshProUGUI outputText;
    public GameObject unloadObj;
    public GameObject backObj;

    public GameObject line;

    [Header("Vcam settings")]
    public Transform vcamTarget;
    public Cinemachine.CinemachineVirtualCamera vcam;
    [Range(0f, 15f)] public float vcamYOffsetMin = 0f;
    [Range(0f, 15f)] public float vcamYOffsetMax = 15f;
    public Transform castingVCamsHolder;
    public Transform executingVCamsHolder;
    public List<Cinemachine.CinemachineVirtualCamera> castingVCamsList = new List<Cinemachine.CinemachineVirtualCamera>();
    public List<Cinemachine.CinemachineVirtualCamera> executingVCamsList = new List<Cinemachine.CinemachineVirtualCamera>();

    //internals
    [HideInInspector] public Cinemachine.CinemachineOrbitalTransposer vcamTransposer;
    //float stam_regen_rate = 1f;
    bool is_moved = false;
    [HideInInspector] public bool is_acted = false;
    float current_burn_out = 0.0f;
    public BaseSkill currentChosenSkill = null;
    [HideInInspector] public float current_casting_value = 0f;

    private void OnEnable()
    {

    }

    private void OnDestroy()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        vcamTransposer = vcam.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>();

        castingVCamsList.Clear();
        castingVCamsList.AddRange(castingVCamsHolder.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>());

        executingVCamsList.Clear();
        executingVCamsList.AddRange(executingVCamsHolder.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>());

        if (actorStats == null)
            actorStats = this.GetComponent<ActorInfo>();

        line.GetComponent<ArcTarget_C>().StartColor = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
        line.GetComponent<ArcTarget_C>().EndColor = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
        line.GetComponent<ArcTarget_C>().EndPoint = this.line.GetComponent<ArcTarget_C>().StartPoint;
        line.SetActive(true);

        DOTween.Kill(commandControlUI.transform);
        commandControlUI.transform.localScale = Vector3.zero;

        DOTween.Kill(actionPreviewUI.transform);
        actionPreviewUI.transform.localScale = Vector3.zero;

        actorAnimationController.PlayIdle();
    }

    // Update is called once per frame
    void Update()
    {
        //sync UI display
        actorDetails.SetDisplayData(this, actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);

        //vcam control
        if (InputProcessor.GetInstance().rightStick.y > 0.05f && vcamTransposer.m_FollowOffset.y <= vcamYOffsetMax)
        {
            vcamTransposer.m_FollowOffset += new Vector3(0f, InputProcessor.GetInstance().rightStick.y * Time.deltaTime * 8f, 0f);
        }

        if (InputProcessor.GetInstance().rightStick.y < -0.05f && vcamTransposer.m_FollowOffset.y >= vcamYOffsetMin)
        {
            vcamTransposer.m_FollowOffset += new Vector3(0f, InputProcessor.GetInstance().rightStick.y * Time.deltaTime * 8f, 0f);
        }

        if (Mathf.Abs(InputProcessor.GetInstance().rightStick.x) >= 0.05f)
        {
            vcamTransposer.m_XAxis.Value -= InputProcessor.GetInstance().rightStick.x;
        }

        //process states
        if (actorControlStates == ActorControlStates.WAITING_FOR_COMMAND)
        {
            ProcessWaitingForCommandState();
        }
        else if (actorControlStates == ActorControlStates.READY_TO_MOVE)
        {
            ProcessReadyToMoveState();
        }
        else if (actorControlStates == ActorControlStates.AP_GEN)
        {
            ProcessAPGenState();
        }
        else if (actorControlStates == ActorControlStates.BURNED_OUT_GEN)
        {
            current_burn_out += Time.deltaTime;
            actorUI.apBar.color = exhautedAPGenColor;
            actorStats.apBar = current_burn_out / burn_out_dur;
            actorUI.apBar.fillAmount = actorStats.apBar;

            if (current_burn_out >= burn_out_dur)
            {
                current_burn_out = 0f;
                actorStats.apBar = 0f;
                ap_bank = 0;
                actorUI.apBar.fillAmount = actorStats.apBar;
                //stam_regen_rate = 1f;
                actorUI.apBar.color = normalAPGenColor;
                actorControlStates = ActorControlStates.AP_GEN;
                actorAnimationController.PlayIdle();
                if (actorUI.apBar.gameObject.GetComponent<SelfBlinkingUI>() != null)
                    Destroy(actorUI.apBar.gameObject.GetComponent<SelfBlinkingUI>());
            }
        }
        else if (actorControlStates == ActorControlStates.WAITING_FOR_TARGET)
        {
            ProcessWaitingForTarget();
        }
        else if (actorControlStates == ActorControlStates.CASTING_GEN)
        {
            current_casting_value += Time.deltaTime;
            actorUI.apBar.color = castingGenColor;
            actorStats.apBar = current_casting_value / currentChosenSkill.skillCastingDuration;
            actorUI.apBar.fillAmount = actorStats.apBar;

            if (current_casting_value >= currentChosenSkill.skillCastingDuration)
            {
                actorControlStates = ActorControlStates.CASTING;
                actorUI.apBar.color = normalAPGenColor;
                actorStats.apBar = 0f;
                actorUI.apBar.fillAmount = actorStats.apBar;
                BattleMaster.GetInstance().StartNewActorTurn(this);
            }
        }
    }

    public void ProcessWaitingForCommandState()
    {
        vcam_target = vcamTarget.DOMove(BattleMaster.GetInstance().gridManager.gridCur.transform.position, 0.25f);

        if (actorStats.skillUIsholder.gameObject.activeSelf == true)
        {
            if (InputProcessor.GetInstance().buttonSouth ||
                InputProcessor.GetInstance().buttonNorth)
            {
                actorControlStates = ActorControlStates.WAITING_FOR_TARGET;
                current_skill_overload_level = 1;

                //TODO: check and swap main skills list / sub skills list
                currentChosenSkill = actorStats.actorMainSkillsList[scrollviewSnap.highlightIndex - 2];

                skill_range_area.Clear();
                skill_range_area = BattleMaster.GetInstance().gridManager.FindArea(occupied_grid_unit, currentChosenSkill.skillRange + 1, currentChosenSkill.skillRange + 1, actorTeams, true);
                List<GridUnit> exclude_list = new List<GridUnit>(skill_range_area);
                if (currentChosenSkill.excludeDiagonal)
                {
                    foreach (GridUnit grid_unit in exclude_list)
                    {
                        if (occupied_grid_unit.cachedWorldPos.x != grid_unit.cachedWorldPos.x && occupied_grid_unit.cachedWorldPos.z != grid_unit.cachedWorldPos.z)
                        {
                            grid_unit.ClearAreaHighlight();
                            grid_unit.ClearPathHighlight();
                            skill_range_area.Remove(grid_unit);
                        }
                    }
                }
                if (currentChosenSkill.mustTargetEmptyGrid)
                {
                    foreach (GridUnit grid_unit in exclude_list)
                    {
                        if (grid_unit.occupiedActor != null)
                        {
                            grid_unit.ClearAreaHighlight();
                            grid_unit.ClearPathHighlight();
                            skill_range_area.Remove(grid_unit);
                        }
                    }
                }
                skill_range_area.TrimExcess();
                if (currentChosenSkill.includeSelfCast == true)
                {
                    skill_range_area.Add(occupied_grid_unit);
                    occupied_grid_unit.AreaHighlight(occupied_grid_unit.gridUnitPathScore, actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);
                }

                actionPreviewUI.transform.DOScale(Vector3.one, 0.25f);
                actionPreviewBG.color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
                casterPortrait.sprite = actorStats.actorPortrait;
                actionNameText.text = currentChosenSkill.skillName;
                targetPortrait.gameObject.SetActive(false);
                probabilityImg.gameObject.SetActive(false);
                actionChanceText.gameObject.SetActive(false);
                outputText.gameObject.SetActive(false);

                BattleMaster.GetInstance().gridManager.cursor_lock = false;
                //actorStats.skillUIsholder.gameObject.SetActive(false);
                actorStats.skillUIsholder.GetComponent<FadeInFadeOutCanvasGroup>().FadeOut();
            }

            if (InputProcessor.GetInstance().buttonShoulderL)
            {
                BattleMaster.GetInstance().gridManager.cursor_lock = false;
                actorStats.skillUIsholder.GetComponent<FadeInFadeOutCanvasGroup>().FadeOut();
                WaitingForCommandState();
            }
        }
        else
        {
            //commands
            if (InputProcessor.GetInstance().buttonSouth)
            {
                if (is_moved == true)
                    return;

                DOTween.Kill(commandControlUI.transform);
                commandControlUI.transform.DOScale(Vector3.zero, .25f);

                ReadyToMoveState();
            }

            if (InputProcessor.GetInstance().buttonShoulderL)
            {
                DOTween.Kill(commandControlUI.transform);
                commandControlUI.transform.DOScale(Vector3.zero, .25f);
                EndTurn();
            }

            if (InputProcessor.GetInstance().buttonWest)
            {
                if (is_acted == true)
                    return;

                DOTween.Kill(commandControlUI.transform);
                commandControlUI.transform.DOScale(Vector3.zero, .25f);
                NormalAttackCommandSelected();
            }

            if (InputProcessor.GetInstance().buttonNorth)
            {
                if (is_acted == true)
                    return;

                if (actorStats.skillUIsholder.gameObject.activeSelf == false)
                {
                    DOTween.Kill(commandControlUI.transform);
                    commandControlUI.transform.DOScale(Vector3.zero, .25f);
                    SkillsCommandSelected();
                }
            }
        }
    }

    public List<GridUnit> skill_range_area = new List<GridUnit>();
    public void NormalAttackCommandSelected()
    {
        actorControlStates = ActorControlStates.WAITING_FOR_TARGET;
        current_skill_overload_level = 1;
        currentChosenSkill = actorStats.actorNormalAttack;
        skill_range_area.Clear();
        skill_range_area = BattleMaster.GetInstance().gridManager.FindArea(occupied_grid_unit, currentChosenSkill.skillRange + 1, currentChosenSkill.skillRange + 1, actorTeams, true);
        if (currentChosenSkill.includeSelfCast == true)
            skill_range_area.Add(occupied_grid_unit);

        actionPreviewUI.transform.DOScale(Vector3.one, 0.25f);
        actionPreviewBG.color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM? PlayerTeamBGColor:OpponentTeamBGColor;
        casterPortrait.sprite = actorStats.actorPortrait;
        actionNameText.text = currentChosenSkill.skillName;
        targetPortrait.gameObject.SetActive(false);
        probabilityImg.gameObject.SetActive(false);
        actionChanceText.gameObject.SetActive(false);
        outputText.gameObject.SetActive(false);

        BattleMaster.GetInstance().gridManager.gridCur.transform.position = this.occupied_grid_unit.transform.position + Vector3.up * BattleMaster.GetInstance().gridManager.gridUnitSize * 0.5f;
    }

    public void SkillsCommandSelected()
    {
        BattleMaster.GetInstance().gridManager.cursor_lock = true;
        actorStats.skillUIsholder.gameObject.SetActive(true);
        foreach (Transform child in actorStats.skillUIsholder)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void SkillCommandWaitingForTarget(BaseSkill skill)
    {
        actorControlStates = ActorControlStates.WAITING_FOR_TARGET;
        current_skill_overload_level = 1;
        currentChosenSkill = skill;
        skill_range_area.Clear();
        skill_range_area = BattleMaster.GetInstance().gridManager.FindArea(occupied_grid_unit, currentChosenSkill.skillRange + 1, currentChosenSkill.skillRange + 1, actorTeams, true);
        if (currentChosenSkill.includeSelfCast == true)
            skill_range_area.Add(occupied_grid_unit);

        if (currentChosenSkill.excludeDiagonal)
        {
            List<GridUnit> exclude_list = new List<GridUnit>(skill_range_area);
            foreach (GridUnit grid_unit in exclude_list)
            {
                if (occupied_grid_unit.cachedWorldPos.x != grid_unit.cachedWorldPos.x && occupied_grid_unit.cachedWorldPos.z != grid_unit.cachedWorldPos.z)
                {
                    grid_unit.ClearAreaHighlight();
                    grid_unit.ClearPathHighlight();
                    skill_range_area.Remove(grid_unit);
                }
            }
        }
        skill_range_area.TrimExcess();

        actionPreviewUI.transform.DOScale(Vector3.one, 0.25f);
        actionPreviewBG.color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
        casterPortrait.sprite = actorStats.actorPortrait;
        actionNameText.text = currentChosenSkill.skillName;
        targetPortrait.gameObject.SetActive(false);
        probabilityImg.gameObject.SetActive(false);
        actionChanceText.gameObject.SetActive(false);
        outputText.gameObject.SetActive(false);

        BattleMaster.GetInstance().gridManager.gridCur.transform.position = this.occupied_grid_unit.transform.position + Vector3.up * BattleMaster.GetInstance().gridManager.gridUnitSize * 0.5f;
    }

    int current_skill_overload_level = 1;
    List<ActorController> last_pincer_actor_list = new List<ActorController>();
    public List<GridUnit> preview_aoe = null;
    public void ProcessWaitingForTarget()
    {
        if (currentChosenSkill == null)
        {
            actorControlStates = ActorControlStates.AP_GEN;
            return;
        }

        if (skill_range_area != null)
        {
            //Debug.Log("Pong!!!!");

            preview_aoe = currentChosenSkill.GetPreviewAoE(this.occupied_grid_unit, BattleMaster.GetInstance().gridManager.current_highlighted_grid_unit);

            if (skill_range_area.Contains(BattleMaster.GetInstance().gridManager.current_highlighted_grid_unit) &&
                preview_aoe != null)
            {
                foreach (GridUnit grid_unit in preview_aoe)
                {
                    grid_unit?.AoEHighlight();
                }
            }
        }

        int ap_cost = current_skill_overload_level * currentChosenSkill.skillStaminaCost;
        actorDetails.actorStaminaPreviewSlider.fillAmount = (ap_cost) / 16f;
        if (actorDetails.actorStaminaPreviewSlider.fillAmount > actorDetails.actorStaminaSlider.fillAmount)
        {
            actorDetails.actorStaminaPreviewSlider.fillAmount = actorDetails.actorStaminaSlider.fillAmount;
            actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = (current_skill_overload_level * currentChosenSkill.skillStaminaCost) / 16f;
        }
        else
        {
            actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
        }

        apText.text = actorStats.staminaPoint.ToString() + (ap_cost >= 0 ? "<color=#D41F21>-" + ap_cost + "</color>": "<color=#D41F21>+" + ap_cost + "</color>");

        List<ActorController> pincer_actor_list = new List<ActorController>();
        pincer_actor_list.Clear();

        //target line
        if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit() != null)
        {
            if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor == null)
                line.GetComponent<ArcTarget_C>().EndPoint = BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().transform;
            else
                line.GetComponent<ArcTarget_C>().EndPoint = BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor.line.GetComponent<ArcTarget_C>().StartPoint;

            ActorController targetController = BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor;
            if (targetController != null /*&& last_pincer_actor_list.Contains(targetController)*/ && targetController != this)
            {
                List<GridUnit> pincer_range = BattleMaster.GetInstance().gridManager.FindArea(targetController.occupied_grid_unit, 2, 2, targetController.actorTeams, true, false);
                foreach (GridUnit tile in pincer_range)
                {
                    if (tile.occupiedActor != null)
                    {
                        //if (Vector3.Dot(Vector3.ProjectOnPlane(tile.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up),
                        //                Vector3.ProjectOnPlane(this.occupied_grid_unit.cachedWorldPos - targetController.occupied_grid_unit.cachedWorldPos, Vector3.up))
                        //    <= -0.9f)
                        //{
                        //    if (tile.occupiedActor.actorTeams == this.actorTeams)
                        //        pincer_actor_list.Add(tile.occupiedActor);
                        //}

                        if (tile.occupiedActor.actorTeams == this.actorTeams && tile.occupiedActor != this)
                            pincer_actor_list.Add(tile.occupiedActor);
                    }
                }

                foreach (ActorController pincer_actor in pincer_actor_list)
                {
                    if (pincer_actor != null)
                    {
                        pincer_actor.line.GetComponent<ArcTarget_C>().EndPoint = targetController.line.GetComponent<ArcTarget_C>().StartPoint;
                    }
                }

                last_pincer_actor_list.Clear();
                last_pincer_actor_list = new List<ActorController>(pincer_actor_list);
            }
            else
            {
                foreach (ActorController last_pincer_actor in last_pincer_actor_list)
                {
                    if (last_pincer_actor != null)
                        last_pincer_actor.line.GetComponent<ArcTarget_C>().EndPoint = last_pincer_actor.line.GetComponent<ArcTarget_C>().StartPoint;
                }
            }
        }

        //preview arrow
        if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit() != null)
        {
            ActorController targetActor = BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor;
            if (targetActor != null &&
                skill_range_area.Contains(targetActor.occupied_grid_unit) == true)
            {
                targetPortrait.gameObject.SetActive(true);
                probabilityImg.gameObject.SetActive(true);
                actionChanceText.gameObject.SetActive(true);
                outputText.gameObject.SetActive(true);

                SkillPreview preview = currentChosenSkill.GetPreviewValue(this, targetActor, current_skill_overload_level);
                targetPortrait.sprite = targetActor.actorStats.actorPortrait;
                probabilityImg.fillAmount = preview.chance_val;
                probabilityImg.color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
                foreach (GameObject img in previewOverloadLevelImgList)
                {
                    img.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
                    img.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = probabilityImg.fillAmount;
                }
                actionChanceText.text = preview.chance_text;
                outputText.text = preview.value;
            }
            else
            {
                targetPortrait.gameObject.SetActive(false);
                probabilityImg.gameObject.SetActive(false);
                actionChanceText.gameObject.SetActive(false);
                outputText.gameObject.SetActive(false);
            }
        }     
        else
        {
            ActorController targetActor = this;
            if (targetActor != null &&
                skill_range_area.Contains(targetActor.occupied_grid_unit) == true)
            {
                targetPortrait.gameObject.SetActive(true);
                probabilityImg.gameObject.SetActive(true);
                actionChanceText.gameObject.SetActive(true);
                outputText.gameObject.SetActive(true);

                SkillPreview preview = currentChosenSkill.GetPreviewValue(this, targetActor, current_skill_overload_level);
                targetPortrait.sprite = targetActor.actorStats.actorPortrait;
                probabilityImg.fillAmount = preview.chance_val;
                probabilityImg.color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
                foreach (GameObject img in previewOverloadLevelImgList)
                {
                    img.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor;
                    img.transform.GetChild(1).GetChild(0).GetComponent<Image>().fillAmount = probabilityImg.fillAmount;
                }
                actionChanceText.text = preview.chance_text;
                outputText.text = preview.value;
            }
            else
            {
                targetPortrait.gameObject.SetActive(false);
                probabilityImg.gameObject.SetActive(false);
                actionChanceText.gameObject.SetActive(false);
                outputText.gameObject.SetActive(false);
            }
        }

        //choose target
        if (InputProcessor.GetInstance().buttonNorth ||
            InputProcessor.GetInstance().buttonSouth ||
            InputProcessor.GetInstance().buttonEast ||
            InputProcessor.GetInstance().buttonWest)
        {
            if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit() == null && currentChosenSkill.includeSelfCast == false)
            {
                return;
            }
            else if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit() == null && currentChosenSkill.includeSelfCast == true)
            {
                BattleMaster.GetInstance().gridManager.current_highlighted_grid_unit = this.occupied_grid_unit;
            }

                //BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor == null ||
            if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor == this && currentChosenSkill.includeSelfCast == false)
                return;

            if (skill_range_area.Contains(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit()) == false)
                return;

            if (currentChosenSkill.mustTargetEmptyGrid == true && BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor != null)
                return;

            apText.text = (actorStats.staminaPoint - current_skill_overload_level * currentChosenSkill.skillStaminaCost).ToString();

            actionPreviewUI.transform.DOScale(Vector3.zero, 0.25f);

            actorUI.apBar.fillAmount = actorStats.apBar / 100f;

            //line.SetActive(false);
            line.GetComponent<ArcTarget_C>().EndPoint = this.line.GetComponent<ArcTarget_C>().StartPoint;
            //if (pincer_actor)
            //    pincer_actor.line.SetActive(false);

            DOTween.Kill(commandControlUI.transform);
            commandControlUI.transform.DOScale(Vector3.zero, .25f);

            actorDetails.transform.GetChild(0).DOLocalMoveX(-800f, 0.25f).SetDelay(.25f);
            currentChosenSkill.CastingSkill(this, current_skill_overload_level, BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit());
            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
            actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
            BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
            actorControlStates = ActorControlStates.CASTING_STAG;
            BattleMaster.GetInstance().CurrentActorTurnEnds(vcamTransposer.m_FollowOffset, vcamTransposer.m_XAxis.Value);

            if (preview_aoe != null)
            {
                foreach (GridUnit grid_unit in preview_aoe)
                {
                    grid_unit?.ClearAoEHighlight();
                }
            }
        }

        if (InputProcessor.GetInstance().buttonShoulderR && current_skill_overload_level < currentChosenSkill.skillMaxOverLoadLevel /*&& (ap_bank >= 0 || ap_bank < 0 && actorStats.staminaPoint >= currentChosenSkill.skillStaminaCost)*/)
        {
            current_skill_overload_level++;
        }

        if (InputProcessor.GetInstance().buttonShoulderL && current_skill_overload_level >= 1)
        {
            current_skill_overload_level--;
        }

        if (current_skill_overload_level == 1)
        {
            foreach (GameObject img in previewOverloadLevelImgList)
            {
                img.gameObject.SetActive(false);
            }

            for (int i = 0; i < current_skill_overload_level - 1; i++)
            {
                previewOverloadLevelImgList[i].gameObject.SetActive(true);
            }

            backObj.SetActive(true);
            unloadObj.SetActive(false);
        }
        else
        {
            foreach (GameObject img in previewOverloadLevelImgList)
            {
                img.gameObject.SetActive(false);
            }

            for (int i = 0; i < current_skill_overload_level - 1; i++)
            {
                previewOverloadLevelImgList[i].gameObject.SetActive(true);
            }

            backObj.SetActive(false);
            unloadObj.SetActive(true);
        }

        if (InputProcessor.GetInstance().buttonShoulderL && current_skill_overload_level <= 0)
        {
            if (preview_aoe != null)
            {
                foreach (GridUnit grid_unit in preview_aoe)
                {
                    grid_unit?.ClearAoEHighlight();
                }
            }

            //line.SetActive(false);
            line.GetComponent<ArcTarget_C>().EndPoint = this.line.GetComponent<ArcTarget_C>().StartPoint;
            //if (pincer_actor)
            //    pincer_actor.line.SetActive(false);

            actionPreviewUI.transform.DOScale(Vector3.zero, 0.25f);

            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
            actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
            currentChosenSkill = null;
            WaitingForCommandState();
        }
    }

    public void ProcessAPGenState()
    {
        if (vcam_target != null)
            DOTween.Kill(vcam_target);

        vcam_target = vcamTarget.DOMove(BattleMaster.GetInstance().gridManager.gridCur.transform.position, 0.25f);

        actorStats.apBar += (actorStats.baseSpeed * ((actorStats.currentStats.speed + ap_bank) * 0.01f)) * Time.deltaTime /** Mathf.Abs(stam_regen_rate + 1f)*/;
        actorUI.apBar.fillAmount = actorStats.apBar / 100f;

        if (actorStats.apBar >= 100f)
        {
            actorUI.apBar.color = normalAPGenColor;
            actorStats.apBar = 0f;
            BattleMaster.GetInstance().StartNewActorTurn(this);
        }
    }

    /*[HideInInspector]*/ public int ap_bank = 0;
    public void StartTurn(ActorController actor)
    {
        if (this != actor)
            return;

        if (currentChosenSkill == null) //new turn
        {
            StartCoroutine(StartTurnSequence(actor));
        }
        else //continue casting
        {
            actorDetails.transform.GetChild(0).DOLocalMoveX(250f, 0.25f);
            vcam.Priority = 11;

            if (preview_aoe != null)
            {
                foreach (GridUnit grid_unit in preview_aoe)
                {
                    grid_unit?.ClearAoEHighlight();
                }
            }
            preview_aoe = null;
            //actorUI.headerHolder.gameObject.SetActive(false);
            currentChosenSkill.ExecuteSkill();
            currentChosenSkill = null;
        }
    }

    IEnumerator StartTurnSequence(ActorController actor)
    {
        foreach (Transform status_obj in actorStats.statusesHolder)
        {
            BaseStatus status = status_obj.GetComponent<BaseStatus>();
            if (status != null)
            {
                BattleMaster.GetInstance().OnShowAnnounce(status.name, actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor, status.sprite);
                status.ProcStatus(this, actorStats);
                yield return new WaitForSeconds(2f);
            }
        }
        yield return null;
        Debug.Log(actor.gameObject.name + " starts turn.");
        vcam.Priority = 11;

        if (ap_bank < 0)
            BattleMaster.GetInstance().OnShowAnnounce("Stamina reduced", actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor, StatusManager.GetInstance().GetStatusIconSprite(StatusManager.StatusIcons.AP_DECREASE));
        else if (ap_bank > 0)
            BattleMaster.GetInstance().OnShowAnnounce("Stamina increased", actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor, StatusManager.GetInstance().GetStatusIconSprite(StatusManager.StatusIcons.AP_INCREASE));

        actorStats.staminaPoint = actorStats.maxStaminaPoint + ap_bank;
        actorUI.apPoints.fillAmount = (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);

        actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;
        foreach (Transform child in commandControlUI.transform.GetChild(1))
        {
            if (child.GetComponent<Image>() != null)
                child.GetComponent<Image>().color = Color.white;
            foreach (Transform grandchild in child)
                grandchild.GetComponent<Image>().color = Color.white;
        }
        DOTween.Kill(commandControlUI.transform);
        commandControlUI.transform.DOScale(Vector3.one, .25f);

        if (vcam_target != null)
            DOTween.Kill(vcam_target);

        vcam_target = vcamTarget.DOMove(this.transform.position, 1f);

        actorDetails.SetDisplayData(this, actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);
        actorDetails.transform.GetChild(0).DOLocalMoveX(250f, 0.25f);

        actorDetails.actorStaminaSlider.fillAmount = ((actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f)) * 0.5f;
        int ap_cost = actorStats.staminaPoint;
        apText.text = ap_cost.ToString();

        actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;

        is_moved = false;
        is_acted = false;
    }

    new Camera camera;
    Tween vcam_target;
    public void ProcessReadyToMoveState()
    {
        if (is_moved == true)
            return;

        if (InputProcessor.GetInstance().buttonShoulderL)
        {
            BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
            WaitingForCommandState();
            return;
        }

        vcam_target = vcamTarget.DOMove(BattleMaster.GetInstance().gridManager.gridCur.transform.position, 0.25f);

        if (move_area.Contains(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit()) == false)
        {
            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
            return;
        }

        List<GridUnit> path = BattleMaster.GetInstance().gridManager.FindPath(occupied_grid_unit, BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit(), actorTeams);

        if (vcam_target != null)
            DOTween.Kill(vcam_target);

        if (path != null)
        {
            int sum = 0;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 new_forward = new Vector3();
                if (path.Count > 1 && i >= 1)
                    new_forward = path[i].cachedWorldPos - path[i - 1].cachedWorldPos;
                else if (path.Count == 1)
                    new_forward = path[0].cachedWorldPos - this.transform.position;

                new_forward = Vector3.ProjectOnPlane(new_forward, Vector3.up).normalized;

                float dot = Vector3.Dot(new_forward, Vector3.right);
                if (Mathf.Abs(dot) <= 0.1f) //90
                {
                    sum += 1;
                }
                else if (Mathf.Abs(dot) <= 1.1f &&
                         Mathf.Abs(dot) >= 0.9f) //0 and 180
                {
                    sum += 1;
                }
                else //diagonals
                {
                    sum += 2;
                }
            }
            actorDetails.actorStaminaPreviewSlider.fillAmount = ((sum - 1) * 1f) / 16f;
            if (actorDetails.actorStaminaPreviewSlider.fillAmount > actorDetails.actorStaminaSlider.fillAmount)
            {
                actorDetails.actorStaminaPreviewSlider.fillAmount = actorDetails.actorStaminaSlider.fillAmount;
                actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = ((sum - 1) * 1f) / 16f;
            }
            else
            {
                actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
            }

            int ap_cost = (sum - 1);
            apText.text = actorStats.staminaPoint.ToString() + (ap_cost >= 0 ? "<color=#D41F21>-" + ap_cost + "</color>": "<color=#D41F21>+" + ap_cost + "</color>");
        }
        else
        {
            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        }

        if (camera == null)
            camera = Camera.main;

        if (InputProcessor.GetInstance().buttonSouth)
        {
            BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
            ReceiveDestinationGridUnit(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit());
        }
    }

    List<GridUnit> move_area = new List<GridUnit>();
    public void ReadyToMoveState()
    {
        if (is_moved == true)
            return;

        actorControlStates = ActorControlStates.READY_TO_MOVE;
        BattleMaster.GetInstance().gridManager.gridCur.transform.position = this.occupied_grid_unit.transform.position + Vector3.up * BattleMaster.GetInstance().gridManager.gridUnitSize * 0.5f;

        //int range = actorStats.staminaPoint;
        //if (range > actorStats.maxStaminaPoint)
        //    range = actorStats.maxStaminaPoint;

        move_area = BattleMaster.GetInstance().FindArea(occupied_grid_unit, actorStats.staminaPoint + 1, actorStats.maxStaminaPoint + 1, actorTeams);
    }

    public void WaitingForCommandState()
    {
        BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
        BattleMaster.GetInstance().gridManager.ClearPathHighlight();
        //actorUI.headerHolder.gameObject.SetActive(true);

        actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
        actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;


        actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;
        foreach (Transform child in commandControlUI.transform.GetChild(1))
        {
            if (is_acted == false && (child.GetSiblingIndex() == 1 ||
                                      child.GetSiblingIndex() == 2 ||
                                      child.GetSiblingIndex() == 3))
            {
                if (child.GetComponent<Image>() != null)
                    child.GetComponent<Image>().color = Color.white;
                foreach (Transform grandchild in child)
                    grandchild.GetComponent<Image>().color = Color.white;
            }    
            else if (is_acted && (child.GetSiblingIndex() == 1 ||
                                  child.GetSiblingIndex() == 2 ||
                                  child.GetSiblingIndex() == 3))
            {
                if (child.GetComponent<Image>() != null)
                    child.GetComponent<Image>().color = Color.gray * .7f;
                foreach (Transform grandchild in child)
                    grandchild.GetComponent<Image>().color = Color.gray * .7f; 
            }

            if (is_moved == false && child.GetSiblingIndex() == 0)
            {
                if (child.GetComponent<Image>() != null)
                    child.GetComponent<Image>().color = Color.white;
                foreach (Transform grandchild in child)
                    grandchild.GetComponent<Image>().color = Color.white;
            }
            else if (is_moved == true && child.GetSiblingIndex() == 0)
            {
                if (child.GetComponent<Image>() != null)
                    child.GetComponent<Image>().color = Color.gray * .7f;
                foreach (Transform grandchild in child)
                    grandchild.GetComponent<Image>().color = Color.gray * .7f;
            }
        }
        DOTween.Kill(commandControlUI.transform);
        commandControlUI.transform.DOScale(Vector3.one, .25f);
        DOTween.Kill(actorDetails.transform.GetChild(0));
        actorDetails.transform.GetChild(0).DOLocalMoveX(250f, 0.25f);

        apText.text = actorStats.staminaPoint.ToString();

        BattleMaster.GetInstance().gridManager.gridCur.transform.position = this.occupied_grid_unit.transform.position + Vector3.up * BattleMaster.GetInstance().gridManager.gridUnitSize * 0.5f;
    }

    public void ReceiveDestinationGridUnit(GridUnit destination)
    {
        //Debug.Log("Clicked.");
        if (actorControlStates != ActorControlStates.READY_TO_MOVE)
        {
            return;
        }

        if (move_area.Contains(destination) == false)
            return;

        actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
        List<GridUnit> move_path = BattleMaster.GetInstance().FindPath(occupied_grid_unit, destination, actorTeams);
        StartCoroutine(MoveSequence(move_path));
    }

    IEnumerator MoveSequence(List<GridUnit> move_path)
    {
        commandControlUI.transform.DOScale(Vector3.zero, .25f);

        actorControlStates = ActorControlStates.MOVING;

        vcamTarget.DOMove(this.transform.position, 1f);
        yield return new WaitForSeconds(1.0f);

        occupied_grid_unit.occupiedState = GridUnitOccupationStates.FREE;
        occupied_grid_unit.occupiedActor = null;
        actorAnimationController.PlayMove();
        move_path.RemoveAt(move_path.Count - 1);
        Vector3[] path = new Vector3[move_path.Count];
        for (int i = move_path.Count - 1; i >= 0; i--)
        {
            path[move_path.Count - 1 - i] = move_path[i].cachedWorldPos;
        }

        if (path.Length > 1)
        {
            this.transform.DOPath(path, (move_path.Count - 1) * 0.5f, PathType.Linear, PathMode.Full3D).SetEase(Ease.Linear).OnWaypointChange((way_point_index) =>
            {
                if (path.Length > 0 && way_point_index < path.Length)
                {
                    Vector3 new_forward = Vector3.ProjectOnPlane((path[way_point_index] - this.transform.GetChild(0).position).normalized, Vector3.up);
                    if (new_forward.magnitude > 0.1f)
                        this.transform.GetChild(0).forward = new_forward;

                    CalculateAndReduceStaminaAfterMove(new_forward);
                }
            });
            yield return new WaitForSeconds(.5f * (move_path.Count - 1));
        }
        else if (path.Length > 0)
        {
            Vector3 new_forward = Vector3.ProjectOnPlane((path[0] - this.transform.GetChild(0).position).normalized, Vector3.up);
            if (new_forward.magnitude > 0.1f)
                this.transform.GetChild(0).forward = new_forward;
            this.transform.DOMove(path[0], .5f).SetEase(Ease.Linear);

            CalculateAndReduceStaminaAfterMove(new_forward);
            yield return new WaitForSeconds(.5f);
        }

        actorAnimationController.PlayIdle();
        occupied_grid_unit = move_path[0];
        move_path[0].occupiedActor = this;
        BattleMaster.GetInstance().ClearPathHighlight();

        yield return new WaitForSeconds(1.0f);
        occupied_grid_unit.occupiedState = actorTeams;

        is_moved = true;
        WaitingForCommandState();
    }

    public void CalculateAndReduceStaminaAfterMove(Vector3 new_forward)
    {
        float dot = Vector3.Dot(new_forward, Vector3.right);
        if (Mathf.Abs(dot) <= 0.1f) //90
        {
            actorStats.staminaPoint -= 1;
        }
        else if (Mathf.Abs(dot) <= 1.1f &&
                 Mathf.Abs(dot) >= 0.9f) //0 and 180
        {
            actorStats.staminaPoint -= 1;
        }
        else //diagonals
        {
            actorStats.staminaPoint -= 2;
        }

        actorUI.apPoints.fillAmount = (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);
        actorDetails.actorStaminaSlider.fillAmount = actorUI.apPoints.fillAmount * 0.5f;

        apText.text = actorStats.staminaPoint.ToString();
    }

    public void EndTurn()
    {
        ap_bank = actorStats.staminaPoint;
        if (ap_bank > 8)
            ap_bank = 8;
        else if (ap_bank <= -8)
        {
            current_burn_out = 0f;
            burn_out_dur = 3f;
            actorControlStates = ActorControlStates.BURNED_OUT_GEN;
            actorAnimationController.PlayBurnOut();
        }

        foreach (Transform status_obj in actorStats.statusesHolder)
        {
            BaseStatus status = status_obj.GetComponent<BaseStatus>();
            if (status != null)
            {
                status.CheckClearStatus();
            }
        }

        DOTween.Kill(commandControlUI.transform);
        commandControlUI.transform.DOScale(Vector3.zero, .25f);

        actorUI.apBar.fillAmount = 0f;
        BattleMaster.GetInstance().birdEyeVcam.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>().m_XAxis.Value = vcamTransposer.m_XAxis.Value;
        actorDetails.transform.GetChild(0).DOLocalMoveX(-800f, 0.25f).SetDelay(.25f);
        currentChosenSkill = null;
        BattleMaster.GetInstance().CurrentActorTurnEnds(vcamTransposer.m_FollowOffset, vcamTransposer.m_XAxis.Value);
    }
}

public enum ActorControlStates
{
    AP_GEN = 0,
    AP_STAG,
    WAITING_FOR_COMMAND,
    READY_TO_MOVE,
    MOVING,
    WAITING_FOR_TARGET,
    CASTING_GEN,
    CASTING_STAG,
    CASTING,
    BURNED_OUT_GEN,
    BURN_OUT_STAG,
    ALL_STAG
}
