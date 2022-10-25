using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Color normalAPGenColor;
    public Color exhautedAPGenColor;
    public Color acceleratedAPGenColor;
    public Color castingGenColor;

    public Color PlayerTeamBGColor;
    public Color OpponentTeamBGColor;
    
    public GameObject commandControlUI;

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
    float stam_regen_rate = 1f;
    bool is_moved = false;
    [HideInInspector] public bool is_acted = false;
    float current_burn_out = 0.0f;
    [HideInInspector] public BaseSkill currentChosenSkill = null;
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

        actorAnimationController.PlayIdle();
    }

    // Update is called once per frame
    void Update()
    {
        //sync UI display
        actorDetails.SetDisplayData(actorStats.actorPortrait,
                                    actorStats.actorName,
                                    actorStats.currentStats.level,
                                    actorStats.currentStats.healthPoint,
                                    actorStats.baseStats.healthPoint,
                                    actorUI.apBar.fillAmount,
                                    actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);

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
                actorUI.apBar.fillAmount = actorStats.apBar;
                stam_regen_rate = 1f;
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
    }

    List<GridUnit> skill_range_area = new List<GridUnit>();
    public void NormalAttackCommandSelected()
    {
        actorControlStates = ActorControlStates.WAITING_FOR_TARGET;
        current_skill_overload_level = 1;
        currentChosenSkill = actorStats.actorNormalAttack;
        skill_range_area.Clear();
        skill_range_area = BattleMaster.GetInstance().gridManager.FindArea(occupied_grid_unit, currentChosenSkill.skillRange + 1, actorTeams, true);
    }

    int current_skill_overload_level = 1;
    public void ProcessWaitingForTarget()
    {
        if (currentChosenSkill == null)
        {
            actorControlStates = ActorControlStates.AP_GEN;
            return;
        }

        actorDetails.actorStaminaPreviewSlider.fillAmount = (current_skill_overload_level * currentChosenSkill.skillStaminaCost) / 16f;
        if (actorDetails.actorStaminaPreviewSlider.fillAmount > actorDetails.actorStaminaSlider.fillAmount)
            actorDetails.actorStaminaPreviewSlider.fillAmount = actorDetails.actorStaminaSlider.fillAmount;
        actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = (current_skill_overload_level * currentChosenSkill.skillStaminaCost) / 16f;

        if (InputProcessor.GetInstance().buttonSouth)
        {
            if (BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().occupiedActor == this)
                return;

            if (skill_range_area.Contains(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit()) == false)
                return;

            currentChosenSkill.CastingSkill(this, current_skill_overload_level, BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit());
            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;
            actorDetails.actorStaminaInDebtPreviewSlider.fillAmount = 0f;
            BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
            actorControlStates = ActorControlStates.CASTING_STAG;
            BattleMaster.GetInstance().CurrentActorTurnEnds(vcamTransposer.m_FollowOffset, vcamTransposer.m_XAxis.Value);
        }

        if (InputProcessor.GetInstance().buttonShoulderR && current_skill_overload_level < currentChosenSkill.skillMaxOverLoadLevel)
        {
            current_skill_overload_level++;
        }

        if (InputProcessor.GetInstance().buttonShoulderL && current_skill_overload_level >= 1)
        {
            current_skill_overload_level--;
        }

        if (InputProcessor.GetInstance().buttonShoulderL && current_skill_overload_level <= 0)
        {
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

        actorStats.apBar += (actorStats.baseSpeed * (actorStats.currentStats.speed * 0.01f)) * Time.deltaTime * Mathf.Abs(stam_regen_rate);
        actorUI.apBar.fillAmount = actorStats.apBar / 100f;

        if (actorStats.apBar >= 100f)
        {
            actorUI.apBar.color = normalAPGenColor;
            actorStats.apBar = 0f;
            BattleMaster.GetInstance().StartNewActorTurn(this);
        }
    }

    public void StartTurn(ActorController actor)
    {
        if (this != actor)
            return;
        
        if (currentChosenSkill == null) //new turn
        {
            Debug.Log(actor.gameObject.name + " starts turn.");
            vcam.Priority = 11;

            actorStats.staminaPoint = actorStats.maxStaminaPoint;
            actorUI.apPoints.fillAmount = (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);

            actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;
            DOTween.Kill(commandControlUI.transform);
            commandControlUI.transform.DOScale(Vector3.one, .25f).SetDelay(.25f);

            if (vcam_target != null)
                DOTween.Kill(vcam_target);

            vcam_target = vcamTarget.DOMove(this.transform.position, 1f);

            actorDetails.SetDisplayData(actorStats.actorPortrait, actorStats.actorName, actorStats.currentStats.level, actorStats.currentStats.healthPoint, actorStats.baseStats.healthPoint, actorUI.apBar.fillAmount, actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);
            actorDetails.transform.GetChild(0).DOLocalMoveX(250f, 0.25f);

            actorDetails.actorStaminaSlider.fillAmount = (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f) * 0.5f;
            actorDetails.actorStaminaPreviewSlider.fillAmount = 0f;

            is_moved = false;
            is_acted = false;
        }
        else //continue casting
        {
            vcam.Priority = 11;
            actorUI.headerHolder.gameObject.SetActive(false);
            currentChosenSkill.Executekill();
        }
    }

    new Camera camera;
    Tween vcam_target;
    public void ProcessReadyToMoveState()
    {
        if (is_moved == true)
            return;

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

        move_area = BattleMaster.GetInstance().FindArea(occupied_grid_unit, actorStats.staminaPoint + 1, actorTeams);
    }

    public void WaitingForCommandState()
    {
        actorUI.headerHolder.gameObject.SetActive(true);
        actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;
        DOTween.Kill(commandControlUI.transform);
        commandControlUI.transform.DOScale(Vector3.one, .25f);
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
            this.transform.DOPath(path, move_path.Count - 1, PathType.Linear, PathMode.Full3D).OnWaypointChange((way_point_index) =>
            {
                if (path.Length > 0 && way_point_index < path.Length)
                {
                    Vector3 new_forward = Vector3.ProjectOnPlane((path[way_point_index] - this.transform.GetChild(0).position).normalized, Vector3.up);
                    if (new_forward.magnitude > 0.1f)
                        this.transform.GetChild(0).forward = new_forward;

                    CalculateAndReduceStaminaAfterMove(new_forward);
                }
            });
            yield return new WaitForSeconds(1.0f * (move_path.Count - 1));
        }
        else if (path.Length > 0)
        {
            Vector3 new_forward = Vector3.ProjectOnPlane((path[0] - this.transform.GetChild(0).position).normalized, Vector3.up);
            if (new_forward.magnitude > 0.1f)
                this.transform.GetChild(0).forward = new_forward;
            this.transform.DOMove(path[0], 1.0f);

            CalculateAndReduceStaminaAfterMove(new_forward);
            yield return new WaitForSeconds(1.0f);
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
    }

    public void EndTurn()
    {
        stam_regen_rate = 1f + (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);
        if (stam_regen_rate < 1f)
        {
            //stam_regen_rate *= .5f;
            actorUI.apBar.color = exhautedAPGenColor;
        }
        else if (stam_regen_rate == 1f)
            actorUI.apBar.color = normalAPGenColor;
        else if (stam_regen_rate > 1f)
        {
            //stam_regen_rate *= 2f;
            actorUI.apBar.color = acceleratedAPGenColor;
        }
        else if (stam_regen_rate <= 0.0f)
        {
            stam_regen_rate = 0.0f;
            actorUI.apBar.color = exhautedAPGenColor;
            current_burn_out = 0f;
            SelfBlinkingUI selfBlinking = actorUI.apBar.gameObject.AddComponent<SelfBlinkingUI>();
            selfBlinking.blinking_speed = 4f;
            actorControlStates = ActorControlStates.BURNED_OUT_GEN;
            actorAnimationController.PlayBurnOut();
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
