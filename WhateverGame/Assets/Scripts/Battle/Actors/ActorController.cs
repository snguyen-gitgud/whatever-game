using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ActorAnimationController))]
[RequireComponent(typeof(ActorStats))]
[RequireComponent(typeof(ActorUI))]
public class ActorController : MonoBehaviour
{
    public GridUnitOccupationStates actorTeams = GridUnitOccupationStates.PLAYER_TEAM;
    public ActorControlStates actorControlStates = ActorControlStates.AP_STAG;
    public GridUnit occupied_grid_unit = null;

    [Header("Animations and effects")]
    public ActorAnimationController actorAnimationController;

    [Header("Stats")]
    public ActorStats actorStats;

    [Header("UI")]
    public ActorUI actorUI;
    public Sprite actorPortrait;
    public GameObject moveCostIndicator;
    public TMPro.TextMeshProUGUI moveCostText;
    public RectTransform moveCostRect;

    private void OnEnable()
    {
        
    }

    private void OnDestroy()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        if (actorStats == null)
            actorStats = this.GetComponent<ActorStats>();

        moveCostIndicator.SetActive(false);

        actorAnimationController.PlayIdle();
    }

    // Update is called once per frame
    void Update()
    {
        if (actorControlStates == ActorControlStates.READY_TO_MOVE)
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
        actorStats.apBar += (actorStats.baseSpeed * (actorStats.speed * 0.01f)) * Time.deltaTime * 3.0f;
        actorUI.apBar.fillAmount = actorStats.apBar / 100f;

        if (actorStats.apBar >= 100f)
        {
            actorStats.apBar = 0f;
            BattleMaster.GetInstance().StartNewActorTurn(this);
        }
    }

    public void StartTurn(ActorController actor)
    {
        if (this != actor)
            return;

        Debug.Log(actor.gameObject.name + " starts turn.");

        actorStats.actionPoint = actorStats.maxActionPoint;
        actorUI.apPoints.fillAmount = (actorStats.actionPoint * 1f) / (actorStats.maxActionPoint * 1f);

        actorControlStates = ActorControlStates.START_TURN;

        

        ReadyToMoveState();
    }

    new Camera camera;
    public void ProcessReadyToMoveState()
    {
        if (move_area.Contains(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit()) == false)
            return;

        List<GridUnit> path = BattleMaster.GetInstance().gridManager.FindPath(occupied_grid_unit, BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit(), actorTeams);
        
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
            moveCostText.text = (sum - 1) + " AP";
        }

        if (camera == null)
            camera = Camera.main;

        moveCostRect.anchoredPosition = camera.WorldToScreenPoint(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit().cachedWorldPos - (Vector3.up * 1f));

        if (InputProcessor.GetInstance().buttonSouth)
        {
            BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
            ReceiveDestinationGridUnit(BattleMaster.GetInstance().gridManager.GetHighLightedGridUnit());
        }
    }

    List<GridUnit> move_area = new List<GridUnit>();
    public void ReadyToMoveState()
    {
        actorControlStates = ActorControlStates.READY_TO_MOVE;
        moveCostIndicator.SetActive(true);
        move_area = BattleMaster.GetInstance().FindArea(occupied_grid_unit, actorStats.actionPoint + 1, actorTeams);
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
            this.transform.DOPath(path, move_path.Count - 1, PathType.Linear, PathMode.Full3D).OnWaypointChange((way_point_index) => {
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

        BattleMaster.GetInstance().CurrentActorTurnEnds();
    }

    public void CalculateAndReduceAPAfterMove(Vector3 new_forward)
    {
        float dot = Vector3.Dot(new_forward, Vector3.right);
        if (Mathf.Abs(dot) <= 0.1f) //90
        {
            actorStats.actionPoint -= 1;
        }
        else if (Mathf.Abs(dot) <= 1.1f &&
                 Mathf.Abs(dot) >= 0.9f) //0 and 180
        {
            actorStats.actionPoint -= 1;
        }
        else //diagonals
        {
            actorStats.actionPoint -= 2;
        }

        actorUI.apPoints.fillAmount = (actorStats.actionPoint * 1f) / (actorStats.maxActionPoint * 1f);
    }

    public void EndTurn()
    {
        actorUI.apBar.fillAmount = 0f;
        CustomEvents.GetInstance().ActorEndTurn();
    }
}

public enum ActorControlStates
{
    AP_GEN = 0,
    AP_STAG,
    START_TURN,
    READY_TO_MOVE,
    MOVING,
    ACTING
}
