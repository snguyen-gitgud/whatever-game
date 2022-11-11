using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTStatus : BaseStatus
{
    [Header("Specific status settings")]
    public int hpChangePercentage = 3;

    public override void ProcStatus(ActorController actorController, ActorInfo info)
    {
        if (is_applied == false)
        {
            info.currentStats.healthPoint += (int)(info.currentStats.healthPoint * ((hpChangePercentage * 1f) / 100f));
        }

        base.ProcStatus(actorController, info);
    }
}
