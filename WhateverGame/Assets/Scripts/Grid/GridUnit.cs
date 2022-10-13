using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class GridUnit : MonoBehaviour
{
    [Range(1, 10)] public int gridUnitCost = 1;
    [Range(1, 10)] public int gridUnitHeight = 1;
    public GridUnitTypes gridUnitType = GridUnitTypes.NEUTRAL;
    public List<GridUnit> neighborsList = new List<GridUnit>();
    public List<GridUnit> neighborsDiagonalExcludedList = new List<GridUnit>();

    public int gridUnitPathScore = -1;
    public int gridUnitAreaScore = -1;

    public Vector3 cachedWorldPos = new Vector3();
    public Vector2Int gridPos = new Vector2Int();
    public bool isTraversible = true;
    public GridUnitOccupationStates occupiedState = GridUnitOccupationStates.FREE;

    public new Renderer renderer = null;
    public GameObject pathIndicator;
    public GameObject aoeIndicator;

    private void Start()
    {
        pathIndicator.SetActive(false);
        aoeIndicator.SetActive(false);
    }

    public void AreaHighlight(int range_score)
    {
        if (renderer == null)
            renderer = this.GetComponent<Renderer>();

        renderer.material.SetFloat("_Transparency", 0.5f);
    }

    public void ClearAreaHighlight()
    {
        if (renderer == null)
            renderer = this.GetComponent<Renderer>();

        renderer.material.SetFloat("_Transparency", 0.0f);
    }

    public void PathHighlight()
    {
        pathIndicator.SetActive(true);
    }

    public void ClearPathHighlight()
    {
        pathIndicator.SetActive(false);
    }
}
