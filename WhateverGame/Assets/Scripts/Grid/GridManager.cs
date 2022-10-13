using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GridManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Grid settings")]
    public Transform gridDataUnitHolder;
    public Terrain targetTerrain;
    public GameObject gridUnitPref;
    public GameObject gridCur;

    [Range(1, 10)] public int gridUnitSize = 2;
    [Range(15, 100)] public int gridSize = 10;
    public List<GridUnit> gridUnitsList = new List<GridUnit>();
    [SerializeField, HideInInspector] public List<GridUnit> _gridUnitsList = new List<GridUnit>();

    //internals
    [HideInInspector] public GridCursor gridCursor;
    GridUnit current_highlighted_grid_unit = null;
    Tween grid_cursor_tween;
    RaycastHit last_hit;

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

    public List<GridUnit> FindArea(GridUnit start_point, int range, GridUnitOccupationStates occupation_team)
    {
        foreach (GridUnit unit in gridUnitsList)
        {
            unit.gridUnitPathScore = -1;
            unit.ClearAreaHighlight();
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
            processed_grid_unit_list.AddRange(ProcessNeighborsForPathFinding(temp, i, occupation_team, true));
            processed_grid_unit_list = processed_grid_unit_list.Distinct().ToList();
            i++;
        }
        processed_grid_unit_list.Remove(start_point);

        ret_list.AddRange(processed_grid_unit_list);
        foreach (GridUnit unit in ret_list)
        {
            unit.AreaHighlight(unit.gridUnitPathScore);
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

    public List<GridUnit> ProcessNeighborsForPathFinding(List<GridUnit> processed_grid_unit_list, int iteration, GridUnitOccupationStates team_state = GridUnitOccupationStates.PLAYER_TEAM, bool exclude_diagonal = false)
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
                if (neighbor.occupiedState != GridUnitOccupationStates.FREE /*&& root.occupiedState != team_state*/)
                    continue;

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

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag.Contains("GridUnit") == true)
            {
                if (last_hit.transform?.gameObject != null && hit.transform.gameObject == last_hit.transform.gameObject)
                    return;

                last_hit = hit;

                if (grid_cursor_tween != null)
                    DOTween.Kill(grid_cursor_tween);

                grid_cursor_tween = gridCur.transform.DOMove(hit.transform.position + Vector3.up * gridUnitSize * 0.5f, 0.1f, false);

                //FindPath(gridUnitsList[0], hit.transform.GetComponent<GridUnit>());
                //FindArea(hit.transform.GetComponent<GridUnit>(), 4);

                current_highlighted_grid_unit = hit.transform.GetComponent<GridUnit>();
            }
            else
            {
                gridCur.transform.position = Vector3.one * 9999.9f;
            }
        }
        else
        {
            gridCur.transform.position = Vector3.one * 9999.9f;
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



