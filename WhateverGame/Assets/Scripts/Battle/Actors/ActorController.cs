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

    [Header("UI")]
    public ActorUI actorUI;
    public GameObject moveCostIndicator;
    public TMPro.TextMeshProUGUI moveCostText;
    public RectTransform moveCostRect;
    public BattleActorDetails actorDetails;
    public Color normalAPGenColor;
    public Color exhautedAPGenColor;
    public Color acceleratedAPGenColor;

    [Header("Vcam settings")]
    public Transform vcamTarget;
    public Cinemachine.CinemachineVirtualCamera vcam;
    [Range(0f, 15f)] public float vcamYOffsetMin = 0f;
    [Range(0f, 15f)] public float vcamYOffsetMax = 15f;

    //internals
    [HideInInspector] public Cinemachine.CinemachineOrbitalTransposer vcamTransposer;
    float bonus_stam_regen_rate = 1f;
    bool is_moved = false;
    bool is_acted = false;

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

        if (actorStats == null)
            actorStats = this.GetComponent<ActorInfo>();

        moveCostIndicator.SetActive(false);

        actorAnimationController.PlayIdle();
    }

    // Update is called once per frame
    void Update()
    {
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
            //commands
            if (InputProcessor.GetInstance().buttonSouth)
            {
                ReadyToMoveState();
            }

            if (InputProcessor.GetInstance().buttonShoulderL)
            {
                EndTurn();
            }
        }
        else if (actorControlStates == ActorControlStates.READY_TO_MOVE)
        {
            ProcessReadyToMoveState();
        }
        else if (actorControlStates == ActorControlStates.AP_GEN)
        {
            ProcessAPGenState();
        }
    }

    public void ProcessAPGenState()
    {
        if (vcam_target != null)
            DOTween.Kill(vcam_target);

        vcam_target = vcamTarget.DOMove(BattleMaster.GetInstance().gridManager.gridCur.transform.position, 0.25f);

        actorStats.apBar += (actorStats.baseSpeed * (actorStats.currentStats.speed * 0.01f)) * Time.deltaTime * (1.0f + bonus_stam_regen_rate);
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

        Debug.Log(actor.gameObject.name + " starts turn.");
        vcam.Priority = 11;

        actorStats.staminaPoint = actorStats.maxStaminaPoint;
        actorUI.apPoints.fillAmount = (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);

        actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;

        if (vcam_target != null)
            DOTween.Kill(vcam_target);

        vcam_target = vcamTarget.DOMove(this.transform.position, 1f);

        actorDetails.SetDisplayData(actorStats.actorPortrait, actorStats.actorName, actorStats.currentStats.level, actorStats.currentStats.healthPoint, actorStats.baseStats.healthPoint, actorUI.apBar.fillAmount);
        actorDetails.transform.GetChild(0).DOMoveX(200f, 0.25f);

        moveCostRect.transform.DOMoveY(300f, .25f);

        is_moved = false;
        is_acted = false;
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
            moveCostText.text = 0 + " stamina";
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
            moveCostText.text = (sum - 1) + " stamina";
        }
        else
        {
            moveCostText.text = 0 + " stamina";
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
        moveCostIndicator.SetActive(true);

        move_area = BattleMaster.GetInstance().FindArea(occupied_grid_unit, actorStats.staminaPoint + 1, actorTeams);

        actorDetails.transform.GetChild(0).DOMoveX(-200f, 0.25f);

        moveCostRect.transform.DOMoveY(-600f, .25f);
    }

    public void WaitingForCommandState()
    {
        actorDetails.transform.GetChild(0).DOMoveX(200f, 0.25f);

        moveCostRect.transform.DOMoveY(300f, .25f);

        actorControlStates = ActorControlStates.WAITING_FOR_COMMAND;
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

        moveCostIndicator.SetActive(false);

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

                    CalculateAndReduceAPAfterMove(new_forward);
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

            CalculateAndReduceAPAfterMove(new_forward);
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

    public void CalculateAndReduceAPAfterMove(Vector3 new_forward)
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
    }

    public void EndTurn()
    {
        bonus_stam_regen_rate = 1f + (actorStats.staminaPoint * 1f) / (actorStats.maxStaminaPoint * 1f);
        if (bonus_stam_regen_rate < 1f)
            actorUI.apBar.color = exhautedAPGenColor;
        else if (bonus_stam_regen_rate == 1f)
            actorUI.apBar.color = normalAPGenColor;
        else
            actorUI.apBar.color = acceleratedAPGenColor;

        actorUI.apBar.fillAmount = 0f;
        BattleMaster.GetInstance().birdEyeVcam.GetCinemachineComponent<Cinemachine.CinemachineOrbitalTransposer>().m_XAxis.Value = vcamTransposer.m_XAxis.Value;
        actorDetails.transform.GetChild(0).DOMoveX(-200f, 0.25f);
        moveCostRect.transform.DOMoveY(-600f, .25f);
        //vcam.Priority = 0;
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
    ACTING
}
