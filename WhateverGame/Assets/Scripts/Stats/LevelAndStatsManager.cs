using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAndStatsManager : PersistantAndSingletonBehavior<LevelAndStatsManager>
{
    [Header("Base stats")]
    public StatsScriptable baseStats;

    [Header("Class stats")]
    public List<StatsGrowthScriptable> classesStatsGrowthList = new List<StatsGrowthScriptable>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class ClassLevelData
{
    public int level = 0;
    public ActorClass classID = ActorClass.MAGITEK;
}


[System.Serializable]
public enum ActorClass
{
    //tier 1 classes
    MAGITEK = 0,
    BRAWLER,
    ARCHER,
    PRIEST,
    SCHOLAR,
    WITCH,
    THIEF,

    //tier 2 classes

    //tier 3 classes

    //tier 4 classes

    //tier 5 classes

}
