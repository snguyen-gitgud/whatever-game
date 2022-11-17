using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[System.Serializable]
public class GridManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Grid settings")]
    public Transform gridDataUnitHolder;
    public Terrain targetTerrain;
    public GameObject gridUnitPref;
    public GameObject gridCur;
    public Color PlayerTeamBGColor;
    public Color OpponentTeamBGColor;
    public Color playerOverloadBGColor;

    [Range(1, 10)] public int gridUnitSize = 2;
    [Range(15, 100)] public int gridSize = 10;
    public List<GridUnit> gridUnitsList = new List<GridUnit>();
    [SerializeField, HideInInspector] public List<GridUnit> _gridUnitsList = new List<GridUnit>();

    [Header("Pointer settings")]
    public Transform cursorRoot;

    [Header("UI")]
    public Transform actorDetails;
    public TextMeshProUGUI reactiveText;

    //internals
    [HideInInspector] public GridCursor gridCursor;
    public GridUnit current_highlighted_grid_unit = null;
    Tween grid_cursor_tween;
    RaycastHit last_hit = new RaycastHit();

    private void OnEnable()
    {
        gridCursor = gridCur.AddComponent<GridCursor>();
    }

    private void Start()
    {
        if (gridUnitsList == null)
            gridUnitsList = new List<GridUnit>();

        targetTerrain.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        DOTween.SetTweensCapacity(1000, 100);
        gridCur.transform.position = gridUnitsList[0].transform.position + Vector3.up * gridUnitSize * 0.5f;

        Ray ray = new Ray(gridUnitsList[0].transform.position + (Vector3.up * 100f), Vector3.down);
        if (Physics.Raycast(ray, out last_hit))
        {
            //intentionally empty
        }
    }

    public void ClearGridData()
    {
        for (int i = gridDataUnitHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(gridDataUnitHolder.GetChild(i).gameObject);
        }

        gridUnitsList.Clear();
    }

    public void GenerateGrid()
    {
        ClearGridData();
        Debug.Log("Start generating grid...");

        if (targetTerrain == null)
            return;

        targetTerrain.tag = "TargetTerrain";
        targetTerrain.gameObject.layer = LayerMask.NameToLayer("Default");

        for (int x = 1; x <= gridSize; x++)
        {
            for (int y = 1; y <= gridSize; y++)
            {
                Vector3 cast_point = new Vector3(x * gridUnitSize * 1.0f, 20.0f, y * gridUnitSize * 1.0f);
                RaycastHit hit;
                if (Physics.Raycast(cast_point, Vector3.down, out hit))
                {
                    if (hit.collider.gameObject.tag.Equals("TargetTerrain"))
                    {
                        GameObject grid_unit = Instantiate(gridUnitPref, gridDataUnitHolder);
                        grid_unit.tag = "GridUnit";
                        grid_unit.name = "grid unit " + x + ", " + y;
                        grid_unit.transform.parent = gridDataUnitHolder;
                        grid_unit.transform.position = hit.point;
                        GridUnit unit = grid_unit.GetComponent<GridUnit>();
                        gridUnitsList.Add(unit);
                        unit.cachedWorldPos = grid_unit.transform.position;
                        unit.gridUnitHeight = (int)unit.cachedWorldPos.y;
                        unit.gridPos = new Vector2Int(x, y);
                        unit.renderer = unit.GetComponent<Renderer>();
                    }
                }
            }
        }

        foreach (GridUnit unit in gridUnitsList)
        {
            unit.neighborsList.Clear();
            unit.neighborsDiagonalExcludedList.Clear();
            foreach (GridUnit possible_neighbor in gridUnitsList)
            {
                if (Math.Abs(unit.gridPos.x - possible_neighbor.gridPos.x) == 1 && Math.Abs(unit.gridPos.y - possible_neighbor.gridPos.y) == 0 ||
                    Math.Abs(unit.gridPos.y - possible_neighbor.gridPos.y) == 1 && Math.Abs(unit.gridPos.x - possible_neighbor.gridPos.x) == 0)
                {
                    unit.neighborsList.Add(possible_neighbor);
                    unit.neighborsDiagonalExcludedList.Add(possible_neighbor);
                }
            }
            foreach (GridUnit possible_neighbor in gridUnitsList)
            {
                if (Math.Abs(unit.gridPos.x - possible_neighbor.gridPos.x) == 1 && Math.Abs(unit.gridPos.y - possible_neighbor.gridPos.y) == 1 ||
                    Math.Abs(unit.gridPos.y - possible_neighbor.gridPos.y) == 1 && Math.Abs(unit.gridPos.x - possible_neighbor.gridPos.x) == 1)
                {
                    unit.neighborsList.Add(possible_neighbor);
                }
            }
        }

        targetTerrain.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public List<GridUnit> FindArea(GridUnit start_point, int stamina_range, int range, GridUnitOccupationStates occupation_team, bool include_occupied = false, bool high_light = true)
    {
        foreach (GridUnit unit in gridUnitsList)
        {
            unit.gridUnitPathScore = -1;
            unit.gridUnitAreaScore = -1;
            if (high_light) unit.ClearAreaHighlight();
        }

        List<GridUnit> ret_list = new List<GridUnit>();
        ret_list.Clear();

        List<GridUnit> processed_grid_unit_list = new List<GridUnit>();
        processed_grid_unit_list.Clear();
        processed_grid_unit_list.Add(start_point);

        start_point.gridUnitPathScore = 0; //root
        int i = 1;
        //Debug.Log("Fanning outward.");
        while (i < range)
        {
            List<GridUnit> temp = new List<GridUnit>();
            temp.Clear();
            foreach (GridUnit unit in processed_grid_unit_list)
            {
                temp.Add(unit);
            }
            processed_grid_unit_list.AddRange(ProcessNeighborsForPathFinding(temp, i, occupation_team, true, include_occupied));
            processed_grid_unit_list = processed_grid_unit_list.Distinct().ToList();
            i++;
        }
        processed_grid_unit_list.Remove(start_point);

        ret_list.AddRange(processed_grid_unit_list);
        if (high_light)
            foreach (GridUnit unit in ret_list)
            {
                if (unit.gridUnitPathScore < stamina_range)
                    unit.AreaHighlight(unit.gridUnitPathScore, occupation_team == GridUnitOccupationStates.PLAYER_TEAM ? PlayerTeamBGColor : OpponentTeamBGColor);
                else
                    unit.AreaHighlight(unit.gridUnitPathScore, occupation_team == GridUnitOccupationStates.PLAYER_TEAM ? playerOverloadBGColor : OpponentTeamBGColor);
            }

        //CustomEvents.GetInstance().ToggleGridUnitRendererLock(false);
        return ret_list;
    }

    public void ClearPathHighlight()
    {
        foreach (GridUnit unit in gridUnitsList)
        {
            unit.gridUnitPathScore = -1;
            unit.ClearPathHighlight();
        }
    }

    public void ClearAreaHighlight()
    {
        foreach (GridUnit unit in gridUnitsList)
        {
            unit.gridUnitPathScore = -1;
            unit.ClearAreaHighlight();
        }
    }

    GridUnit last_end_point;
    List<GridUnit> last_path = null;
    public List<GridUnit> FindPath(GridUnit start_point, GridUnit end_point, GridUnitOccupationStates occupation_team)
    {
        if (last_end_point == end_point && last_path != null)
            return last_path;

        if (end_point == null)
            return null;

        if (start_point == null)
            return null;

        if (start_point == end_point)
            return null;

        last_end_point = end_point;
        foreach (GridUnit unit in gridUnitsList)
        {
            unit.gridUnitPathScore = -1;
            unit.ClearPathHighlight();
        }

        List<GridUnit> ret_list = new List<GridUnit>();
        ret_list.Clear();

        List<GridUnit> processed_grid_unit_list = new List<GridUnit>();
        processed_grid_unit_list.Clear();
        processed_grid_unit_list.Add(start_point);

        start_point.gridUnitPathScore = 0; //root
        int i = 1;
        //Debug.Log("Fanning outward.");
        while (processed_grid_unit_list.Contains(end_point) == false)
        {
            List<GridUnit> temp = new List<GridUnit>();
            temp.Clear();
            foreach (GridUnit unit in processed_grid_unit_list)
            {
                temp.Add(unit);
            }
            processed_grid_unit_list.AddRange(ProcessNeighborsForPathFinding(temp, i, occupation_team));
            processed_grid_unit_list = processed_grid_unit_list.Distinct().ToList();
            i++;

            if (processed_grid_unit_list.Contains(end_point))
                break;
        }
        end_point.gridUnitPathScore = i;

        //Debug.Log("Tracing backward.");
        ret_list.Add(end_point);
        GridUnit current_unit = end_point;
        while (current_unit != start_point)
        {
            if (current_unit.neighborsList.Contains(start_point) == true)
            {
                ret_list.Add(start_point);
                break;
            }

            GridUnit lowest_score = current_unit;
            for (int index = 0; index < current_unit.neighborsList.Count; index++)
            {
                if (lowest_score.gridUnitPathScore > -1 &&
                    current_unit.neighborsList[index].gridUnitPathScore > -1 &&
                    current_unit.neighborsList[index].gridUnitPathScore < lowest_score.gridUnitPathScore)
                {
                    lowest_score = current_unit.neighborsList[index];
                }
            }

            current_unit = lowest_score;

            ret_list.Add(current_unit);
            if (current_unit == start_point)
                break;

            if (current_unit == start_point)
                break;
        }

        ret_list = ret_list.Distinct().ToList();

        foreach (GridUnit unit in ret_list)
        {
            unit.pathIndicator.SetActive(true);
        }

        last_path = ret_list;
        return ret_list;
    }

    public List<GridUnit> ProcessNeighborsForPathFinding(List<GridUnit> processed_grid_unit_list, int iteration, GridUnitOccupationStates team_state = GridUnitOccupationStates.PLAYER_TEAM, bool exclude_diagonal = false, bool include_occupied = false)
    {
        //Debug.Log("Search grid iteration number: " + iterartion);

        List<GridUnit> neighbors_list = new List<GridUnit>();
        neighbors_list.Clear();

        foreach (GridUnit root in processed_grid_unit_list)
        {
            List<GridUnit> processees = root.neighborsDiagonalExcludedList;
            if (exclude_diagonal == false)
                processees = root.neighborsList;

            foreach (GridUnit neighbor in processees)
            {
                if (include_occupied == false)
                {
                    if (neighbor.occupiedState != GridUnitOccupationStates.FREE /*&& root.occupiedState != team_state*/)
                        continue;
                }

                if (neighbor.isTraversible == false)
                    continue;

                if (neighbor.gridUnitPathScore > -1)
                    continue;

                if (processed_grid_unit_list.Contains(neighbor) == false)
                {
                    neighbor.gridUnitPathScore = iteration;
                    neighbors_list.Add(neighbor);
                }
            }
        }

        return neighbors_list;
    }

    [HideInInspector] public bool cursor_lock = false;
    private void Update()
    {
        if (current_highlighted_grid_unit != null && current_highlighted_grid_unit.occupiedActor != null)
        {
            foreach (Transform child in actorDetails.parent.GetComponent<BattleActorDetails>().statusIconsHolder)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in current_highlighted_grid_unit.occupiedActor.actorDetails.statusIconsHolder)
            {
                Instantiate(child.gameObject, actorDetails.parent.GetComponent<BattleActorDetails>().statusIconsHolder);
            }

            actorDetails.DOLocalMoveX(-250f, 0.25f);
            actorDetails.parent.GetComponent<BattleActorDetails>().SetDisplayData(current_highlighted_grid_unit.occupiedActor.actorStats.actorPortrait,
                                                                           current_highlighted_grid_unit.occupiedActor.actorStats.actorName,
                                                                           current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.level,
                                                                           current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.healthPoint,
                                                                           current_highlighted_grid_unit.occupiedActor.actorStats.baseStats.healthPoint,
                                                                           current_highlighted_grid_unit.occupiedActor.actorUI.apBar.fillAmount,
                                                                           current_highlighted_grid_unit.occupiedActor.actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? current_highlighted_grid_unit.occupiedActor.PlayerTeamBGColor : current_highlighted_grid_unit.occupiedActor.OpponentTeamBGColor);
            if (current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill != null)
                reactiveText.text = current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill.reactiveSkillName;
        }

        if (Mathf.Abs(InputProcessor.GetInstance().leftStick.magnitude) > 0f && cursor_lock == false)
        {
            DOTween.Kill(grid_cursor_tween);

            var forward = Camera.main.transform.forward;
            var right = Camera.main.transform.right;
            Vector3 desiredMoveDirection = forward * InputProcessor.GetInstance().leftStick.z + right * InputProcessor.GetInstance().leftStick.x;
            
            desiredMoveDirection = Vector3.ProjectOnPlane(desiredMoveDirection, Vector3.up).normalized;
            gridCur.transform.position += desiredMoveDirection * Time.deltaTime * 8f;

            Ray ray = new Ray(cursorRoot.position + (Vector3.up * 100f), Vector3.down);
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Contains("GridUnit") == true)
                {
                    if (last_hit.transform?.gameObject != null && hit.transform.gameObject == last_hit.transform.gameObject)
                        return;

                    Vector3 desiredUpDirection = (hit.transform.position - new Vector3(hit.transform.position.x, gridCur.transform.position.y, hit.transform.position.z));
                    gridCur.transform.position += desiredUpDirection + Vector3.up * gridUnitSize * 0.5f;

                    last_hit = hit;
                    current_highlighted_grid_unit = hit.transform.GetComponent<GridUnit>();

                    if (current_highlighted_grid_unit.occupiedActor != null)
                    {
                        actorDetails.DOLocalMoveX(-250f, 0.25f);
                        actorDetails.parent.GetComponent<BattleActorDetails>().SetDisplayData(current_highlighted_grid_unit.occupiedActor.actorStats.actorPortrait,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.actorName,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.level,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.healthPoint,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.baseStats.healthPoint,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorUI.apBar.fillAmount,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? current_highlighted_grid_unit.occupiedActor.PlayerTeamBGColor : current_highlighted_grid_unit.occupiedActor.OpponentTeamBGColor);
                        if (current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill != null)
                            reactiveText.text = current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill.reactiveSkillName;
                    }
                    else
                    {
                        if (DOTween.IsTweening(actorDetails) == true)
                            DOTween.Kill(actorDetails);

                        actorDetails.DOLocalMoveX(800f, 0.25f);
                    }
                }
                else
                {
                    if (DOTween.IsTweening(actorDetails) == true)
                        DOTween.Kill(actorDetails);

                    actorDetails.DOLocalMoveX(800f, 0.25f);
                }
            }
        }
        else
        {
            Ray ray = new Ray(cursorRoot.position + (Vector3.up * 100f), Vector3.down);
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Contains("GridUnit") == true)
                {
                    if (last_hit.transform?.gameObject != null && hit.transform.gameObject == last_hit.transform.gameObject && gridCur.transform.position == hit.transform.position + Vector3.up * gridUnitSize * 0.5f)
                        return;

                    last_hit = hit;

                    if (grid_cursor_tween != null)
                        DOTween.Kill(grid_cursor_tween);

                    grid_cursor_tween = gridCur.transform.DOMove(hit.transform.position + Vector3.up * gridUnitSize * 0.5f, 0.1f, false);
                    current_highlighted_grid_unit = hit.transform.GetComponent<GridUnit>();

                    if (current_highlighted_grid_unit.occupiedActor != null)
                    {
                        actorDetails.DOLocalMoveX(-250f, 0.25f);
                        actorDetails.parent.GetComponent<BattleActorDetails>().SetDisplayData(current_highlighted_grid_unit.occupiedActor.actorStats.actorPortrait,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.actorName,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.level,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.currentStats.healthPoint,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorStats.baseStats.healthPoint,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorUI.apBar.fillAmount,
                                                                                       current_highlighted_grid_unit.occupiedActor.actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? current_highlighted_grid_unit.occupiedActor.PlayerTeamBGColor : current_highlighted_grid_unit.occupiedActor.OpponentTeamBGColor);
                        if (current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill != null)
                            reactiveText.text = current_highlighted_grid_unit.occupiedActor.actorStats.actorReactiveSkill.reactiveSkillName;
                    }
                    else
                    {
                        if (DOTween.IsTweening(actorDetails) == true)
                            DOTween.Kill(actorDetails);

                        actorDetails.DOLocalMoveX(800f, 0.25f);
                    }

                    if (current_highlighted_grid_unit.occupiedActor != null)
                    {
                        actorDetails.DOLocalMoveX(-250f, 0.25f);
                    }
                    else
                    {
                        if (DOTween.IsTweening(actorDetails) == true)
                            DOTween.Kill(actorDetails);

                        actorDetails.DOLocalMoveX(800f, 0.25f);
                    }
                }
                else
                {
                    if (grid_cursor_tween != null)
                        DOTween.Kill(grid_cursor_tween);

                    grid_cursor_tween = gridCur.transform.DOMove(last_hit.transform.position + Vector3.up * gridUnitSize * 0.5f, 0.1f, false);
                    current_highlighted_grid_unit = last_hit.transform.GetComponent<GridUnit>();

                    if (DOTween.IsTweening(actorDetails) == true)
                        DOTween.Kill(actorDetails);

                    actorDetails.DOLocalMoveX(800f, 0.25f);
                }
            }
            else
            {
                if (grid_cursor_tween != null)
                    DOTween.Kill(grid_cursor_tween);

                grid_cursor_tween = gridCur.transform.DOMove(last_hit.transform.position + Vector3.up * gridUnitSize * 0.5f, 0.1f, false);
                current_highlighted_grid_unit = last_hit.transform.GetComponent<GridUnit>();
            }
        }
    }

    public GridUnit GetHighLightedGridUnit()
    {
        return current_highlighted_grid_unit;
    }

    public void OnBeforeSerialize()
    {
        _gridUnitsList.Clear();
        _gridUnitsList.AddRange(gridUnitsList);
    }

    public void OnAfterDeserialize()
    {
        gridUnitsList.Clear();
        gridUnitsList.AddRange(_gridUnitsList);
    }
}

public class GridCursor : MonoBehaviour
{

}

public enum GridUnitTypes
{
    NEUTRAL,
    TALL_GRASS,
    WATER,
    ICE
}

public enum GridUnitOccupationStates
{
    FREE,
    PLAYER_TEAM,
    OPPONENT_TEAM,
    GUEST_TEAM
}



