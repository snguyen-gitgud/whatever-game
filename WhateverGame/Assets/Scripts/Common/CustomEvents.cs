using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : PersistantAndSingletonBehavior<CustomEvents>
{
    public System.Action<bool> onToggleGridUnitRendererLock;
    public void ToggleGridUnitRendererLock(bool render_lock)
    {
        if (onToggleGridUnitRendererLock != null)
            onToggleGridUnitRendererLock(render_lock);
    }

    public System.Action<GridUnit> onGridUnitClicked;
    public void GridUnitClicked(GridUnit unit)
    {
        if (onGridUnitClicked != null)
        {
            onGridUnitClicked(unit);
        }
    }

    public System.Action<ActorController> onActorStartTurn;
    public void ActorStartTurn(ActorController actor)
    {
        if (onActorStartTurn != null)
        {
            onActorStartTurn(actor);
        }
    }

    public System.Action onActorEndTurn;
    public void ActorEndTurn()
    {
        if (onActorEndTurn != null)
            onActorEndTurn();
    }
}
