using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStatus : BaseStatus
{
    [Header("Specific status settings")]
    public int atkChangePercentage = 10;

    public override void ProcStatus(ActorController actorController, ActorInfo info)
    {
        if (is_applied == false)
        {
            info.currentStats.pAtk += (int)(info.currentStats.pAtk * ((atkChangePercentage * 1f) / 100f));
        }

        base.ProcStatus(actorController, info);
    }
}
