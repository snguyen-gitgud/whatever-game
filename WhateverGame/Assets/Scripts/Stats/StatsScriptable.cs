using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Actor Stats", menuName = "Create Base Stats Obj")]
public class StatsScriptable : ScriptableObject
{
    public int healthPoint = 100;
    public int speed = 100;
    public int pAtk = 10;
    public int mAtk = 10;
    public int pDef = 5;
    public int mDef = 5;
    public int jump = 1;
    public int dodgeChance = 5;
    public int blockChance = 5;
    public int critChance = 10;
    public int critResist = 0;
    public int accuracy = 90;

    public List<int> expRequirementsList = new List<int>()
    {
        0,
        100,
        200,
        300,
        400,
        500,
        600,
        700,
        800,
        900,
        2000,
        2200,
        2400,
        2600,
        2800,
        3000,
        3200,
        3400,
        3600,
        3800,
        8000,
        8400,
        8800,
        9200,
        9600,
        10000,
        10400,
        10800,
        11200,
        11600,
        24000,
        24800,
        25600,
        26400,
        27200,
        28000,
        28800,
        29600,
        30400,
        31200,
        64000,
        65600,
        67200,
        68800,
        70400,
        72000,
        73600,
        75200,
        76800,
        78400,
        160000,
        163200,
        166400,
        169600,
        172800,
        176000,
        179200,
        182400,
        185600,
        188800,
        384000,
        390400,
        396800,
        403200,
        409600,
        416000,
        422400,
        428800,
        435200,
        441600,
        896000,
        908800,
        921600,
        934400,
        947200,
        960000,
        972800,
        985600,
        998400,
        1011200,
        2048000,
        2073600,
        2099200,
        2124800,
        2150400,
        2176000,
        2201600,
        2227200,
        2252800,
        2278400,
        4608000,
        4659200,
        4710400,
        4761600,
        4812800,
        4864000,
        4915200,
        4966400,
        5017600,
        5068800,
    };

    public int GetLevelFromExp(int exp)
    {
        int level = 0;
        foreach (int value in expRequirementsList)
        {
            if (exp < value)
            {
                level = expRequirementsList.IndexOf(value) - 1;
                break;
            }
        }

        if (level < 0)
            level = 0;

        return level;
    }
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
