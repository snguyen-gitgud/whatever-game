using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleMaster : SingletonBehavior<BattleMaster>
{
    [Header("Actors settings")]
    public Transform playerActorHolder;
    public Transform opponentActorHolder;

    [Header("Grid settings")]
    public GridManager gridManager;

    [Header("Camera")]
    public Cinemachine.CinemachineVirtualCamera birdEyeVcam;
    public Cinemachine.CinemachineTargetGroup targetGroup;
    public Vector3 vcamFollowOffset = new Vector3();
    public float vcamXAxisValue = 0f;

    [Header("UI")]
    public GameObject actorAPProgressionPref;
    public Transform actorAPProgressionHolder;
    public Transform actorCastingProgressionHolder;
    public GameObject announceObj;
    public TextMeshProUGUI announceText;

    //internals
    List<GameObject> actorAPProgressionsList = new List<GameObject>();
    List<GameObject> actorCastingProgressionsList = new List<GameObject>();
    List<ActorController> playerActorsList = new List<ActorController>();
    List<ActorController> opponentActorsList = new List<ActorController>();
    List<ActorController> allActorsList = new List<ActorController>();
    ActorController last_actor;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Start()
    {
        announceObj.transform.localScale = new Vector3(1f, 0f, 1f);

        playerActorsList.Clear();
        opponentActorsList.Clear();
        playerActorsList.AddRange(playerActorHolder.GetComponentsInChildren<ActorController>(true));
        opponentActorsList.AddRange(opponentActorHolder.GetComponentsInChildren<ActorController>(true));

        allActorsList.Clear();
        allActorsList.AddRange(playerActorsList);
        allActorsList.AddRange(opponentActorsList);

        action_queue.Clear();

        for (int i = 0; i < allActorsList.Count; i++)
        {
            allActorsList[i].transform.position = gridManager.gridUnitsList[i].cachedWorldPos;
            allActorsList[i].occupied_grid_unit = gridManager.gridUnitsList[i];
            allActorsList[i].occupied_grid_unit.occupiedState = allActorsList[i].actorTeams;
            allActorsList[i].occupied_grid_unit.occupiedActor = allActorsList[i];
        }

        actorAPProgressionsList.Clear();
        actorCastingProgressionsList.Clear();
        for (int i = 0; i < allActorsList.Count; i++)
        {
            targetGroup.AddMember(allActorsList[i].transform, 1f, 4f);

            GameObject go = Instantiate(actorAPProgressionPref, actorAPProgressionHolder);
            go.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = allActorsList[i].actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? allActorsList[i].PlayerTeamBGColor : allActorsList[i].OpponentTeamBGColor;
            go.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = allActorsList[i].actorStats.actorPortrait;
            actorAPProgressionsList.Add(go);

            go = Instantiate(actorAPProgressionPref, actorCastingProgressionHolder);
            go.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = allActorsList[i].actorTeams == GridUnitOccupationStates.PLAYER_TEAM ? allActorsList[i].PlayerTeamBGColor : allActorsList[i].OpponentTeamBGColor;
            go.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = allActorsList[i].actorStats.actorPortrait;
            actorCastingProgressionsList.Add(go);
        }

        last_actor = allActorsList[0];

        CurrentActorTurnEnds(new Vector3(0f, 6f, -15f), 0);
    }

    public List<GridUnit> FindArea(GridUnit start_point, int range, GridUnitOccupationStates occupation_team)
    {
        return gridManager.FindArea(start_point, range, occupation_team);
    }

    public List<GridUnit> FindPath(GridUnit start_point, GridUnit end_point, GridUnitOccupationStates occupation_team)
    {
        return gridManager.FindPath(start_point, end_point, occupation_team);
    }

    public void ClearPathHighlight()
    {
        gridManager.ClearPathHighlight();
    }

    List<ActorController> action_queue = new List<ActorController>();
    public void StartNewActorTurn(ActorController actor)
    {
        foreach (ActorController a in allActorsList)
        {
            if (a.actorControlStates == ActorControlStates.AP_GEN)
                a.actorControlStates = ActorControlStates.AP_STAG;
            else if (a.actorControlStates == ActorControlStates.CASTING_GEN)
                a.actorControlStates = ActorControlStates.CASTING_STAG;
            else if (a.actorControlStates == ActorControlStates.BURNED_OUT_GEN)
                a.actorControlStates = ActorControlStates.BURN_OUT_STAG;
        }
        action_queue.Add(actor);
        ProcessNextTurn(last_actor.vcamTransposer.m_FollowOffset, last_actor.vcamTransposer.m_XAxis.Value);
    }

    public void CurrentActorTurnEnds(Vector3 follow_offset, float x_value)
    {
        foreach (ActorController actor in allActorsList)
        {
            if (actor.actorControlStates == ActorControlStates.CASTING_STAG)
                actor.actorControlStates = ActorControlStates.CASTING_GEN;
            else if (actor.actorControlStates == ActorControlStates.AP_STAG)
                actor.actorControlStates = ActorControlStates.AP_GEN;
            else if (actor.actorControlStates == ActorControlStates.BURN_OUT_STAG)
                actor.actorControlStates = ActorControlStates.BURNED_OUT_GEN;
            else
                actor.actorControlStates = ActorControlStates.AP_GEN;
        }

        if (action_queue.Count > 0)
        {
            foreach (ActorController a in allActorsList)
            {
                if (a.actorControlStates == ActorControlStates.AP_GEN)
                    a.actorControlStates = ActorControlStates.AP_STAG;
                else if (a.actorControlStates == ActorControlStates.CASTING_GEN)
                    a.actorControlStates = ActorControlStates.CASTING_STAG;
                else if (a.actorControlStates == ActorControlStates.BURNED_OUT_GEN)
                    a.actorControlStates = ActorControlStates.BURN_OUT_STAG;
            }

            ProcessNextTurn(follow_offset, x_value);
        }
    }

    public void ProcessNextTurn(Vector3 follow_offset, float x_value)
    {
        vcamFollowOffset = follow_offset;
        vcamXAxisValue = x_value;
        foreach (ActorController actor in allActorsList)
        {
            actor.vcam.Priority = 0;
        }

        gridManager.gridCur.transform.position = action_queue[0].occupied_grid_unit.cachedWorldPos + Vector3.up * gridManager.gridUnitSize * 0.5f;
        action_queue[0].vcamTransposer.m_FollowOffset = vcamFollowOffset;
        action_queue[0].vcamTransposer.m_XAxis.Value = vcamXAxisValue;

        action_queue[0].StartTurn(action_queue[0]);
        last_actor = action_queue[0];
        action_queue.RemoveAt(0);
        action_queue.TrimExcess();
    }

    private void Update()
    {
        foreach (GameObject go in actorAPProgressionsList)
        {
            int index = actorAPProgressionsList.IndexOf(go);
            if (allActorsList[index].actorControlStates == ActorControlStates.AP_GEN ||
                allActorsList[index].actorControlStates == ActorControlStates.AP_STAG ||
                allActorsList[index].actorControlStates == ActorControlStates.ALL_STAG)
            {
                go.SetActive(true);
                go.GetComponent<Slider>().value = allActorsList[index].actorUI.apBar.fillAmount;
            }
            else if (allActorsList[index].actorControlStates != ActorControlStates.CASTING_GEN ||  
                     allActorsList[index].actorControlStates != ActorControlStates.CASTING_STAG)
            {
                go.SetActive(true);
                go.GetComponent<Slider>().value = 1f;
            }
            else
                go.SetActive(false);
        }

        foreach (GameObject go in actorCastingProgressionsList)
        {
            int index = actorCastingProgressionsList.IndexOf(go);
            if (allActorsList[index].actorControlStates == ActorControlStates.CASTING_GEN ||
                allActorsList[index].actorControlStates == ActorControlStates.CASTING_STAG)
            {
                go.SetActive(true);
                go.GetComponent<Slider>().value = allActorsList[index].actorUI.apBar.fillAmount;
            }
            else if (allActorsList[index].actorControlStates == ActorControlStates.CASTING)
            {
                go.SetActive(true);
                go.GetComponent<Slider>().value = 1f;
            }
            else
                go.SetActive(false);
        }
    }

    public void OnShowAnnounce(string txt, Color color)
    {
        DOTween.Kill(announceObj);
        announceText.text = txt;
        announceObj.transform.GetChild(0).GetComponent<Image>().color = color;
        announceObj.transform.DOScaleY(1f, 0.25f).OnComplete(() => {
            announceObj.transform.DOScaleY(0f, 0.25f).SetDelay(2f);
        });
    }
}
