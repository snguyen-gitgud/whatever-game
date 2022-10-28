using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : PersistantAndSingletonBehavior<StatusManager>
{
    public enum StatusIcons
    {
        NULL = 0,
        AP_INCREASE,
        AP_DECREASE,
    }

    public List<Sprite> statusIconsList = new List<Sprite>();

    public Sprite GetStatusIconSprite(StatusIcons icon)
    {
        return statusIconsList[(int)icon];
    }
}
