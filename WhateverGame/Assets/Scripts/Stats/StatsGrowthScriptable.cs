using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Class Stats", menuName = "Create Class Stats Obj")]
public class StatsGrowthScriptable : ScriptableObject
{
    [Header("Class parametters")]
    public ActorClass actorClass = ActorClass.MAGITEK;
    public int individualLevel = 0;

    [Header("Stats details")]
    public float healthPoint = 0;
    public float speed = 0;
    public float pAtk = 0;
    public float mAtk = 0;
    public float pDef = 0;
    public float mDef = 0;

    public float dodgeChance = 0;
    public float blockChance = 0;
    public float critChance = 0;
    public float critResist = 0;
    public float accuracy = 0;

    public StatsScriptable GetStatsAtLevel(int level)
    {
        StatsScriptable ret = new StatsScriptable();

        ret.healthPoint = (int)(healthPoint * level);
        ret.speed = (int)(speed * level);
        ret.pAtk = (int)(pAtk * level);
        ret.mAtk = (int)(mAtk * level);
        ret.pDef = (int)(pDef * level);
        ret.mDef = (int)(mDef * level);

        ret.dodgeChance = (int)(dodgeChance * level);
        ret.blockChance = (int)(blockChance * level);
        ret.critChance = (int)(critChance * level);
        ret.critResist = (int)(critResist * level);
        ret.accuracy = (int)(accuracy * level);

        return ret;
    }
}
