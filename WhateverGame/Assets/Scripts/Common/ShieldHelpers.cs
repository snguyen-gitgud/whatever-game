using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShieldHelpers 
{
    public static int GetRandomNumber(int min, int max)
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        Random.InitState(cur_time);

        return Random.Range(min, max);
    }
}
