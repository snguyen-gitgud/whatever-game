using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMaster : SingletonBehavior<BattleMaster>
{
    [Header("Actors settings")]
    public List<ActorController> playerActorsList = new List<ActorController>();
    public List<ActorController> opponentActorsList = new List<ActorController>();

    [Header("Grid settings")]
    public GridManager gridManager;

    [Header("Camera")]
    public Cinemachine.CinemachineVirtualCamera birdEyeVcam;
    public Cinemachine.CinemachineTargetGroup targetGroup;

    //internals
    List<ActorController> allActorsList = new List<ActorController>();

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private void Start()
    {
        allActorsList.Clear();
        allActorsList.AddRange(playerActorsList);
        allActorsList.AddRange(opponentActorsList);

        action_queue.Clear();

        for (int i = 0; i < allActorsList.Count; i++)
        {
            allActorsList[i].transform.position = gridManager.gridUnitsList[i].cachedWorldPos;
            allActorsList[i].occupied_grid_unit = gridManager.gridUnitsList[i];
            allActorsList[i].occupied_grid_unit.occupiedState = allActorsList[i].actorTeams;
        }

        for (int i = 0; i < allActorsList.Count; i++)
        {
            targetGroup.AddMember(allActorsList[i].transform, 1f, 4f);
        }

        CurrentActorTurnEnds();
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
            a.actorControlStates = ActorControlStates.AP_STAG;
        }
        action_queue.Add(actor);
        action_queue[0].StartTurn(action_queue[0]);
        action_queue.RemoveAt(0);
        action_queue.TrimExcess();
    }

    public void CurrentActorTurnEnds()
    {
        foreach (ActorController actor in allActorsList)
        {
            actor.actorControlStates = ActorControlStates.AP_GEN;
        }

        if (action_queue.Count > 0)
        {
            foreach (ActorController a in allActorsList)
            {
                a.actorControlStates = ActorControlStates.AP_STAG;
            }

            action_queue[0].StartTurn(action_queue[0]);
            action_queue.RemoveAt(0);
            action_queue.TrimExcess();
        }
    }
}
