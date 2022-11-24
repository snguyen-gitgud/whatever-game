using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedStatus : BaseStatus
{
    [Header("Specific status settings")]
    public int spdChangePercentage = 10;

    ActorInfo actorInfo;
    int deltaStats = 0;

    public override void ProcStatus(ActorController actorController, ActorInfo info)
    {
        actorInfo = info;

        if (is_applied == false)
        {
            deltaStats = (int)(info.currentStats.speed * ((spdChangePercentage * 1f) / 100f));
            info.currentStats.speed += deltaStats;
        }

        base.ProcStatus(actorController, info);
    }

    public override void CheckClearStatus()
    {
        if (duration <= 0 && is_applied == true)
        {
            actorInfo.currentStats.speed -= deltaStats;
        }

        base.CheckClearStatus();
    }
}
