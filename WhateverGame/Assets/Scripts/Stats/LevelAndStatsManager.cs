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
