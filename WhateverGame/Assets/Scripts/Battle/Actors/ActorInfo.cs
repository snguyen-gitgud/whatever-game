using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ActorInfo : MonoBehaviour
{
    [Header("Core stats")]
    public int maxStaminaPoint = 6;
    public float baseSpeed = 20f;
    public float vcam_offset_Y = 1.5f;

    [Header("Specific stats")]
    public int level = 0;
    public List<ClassLevelData> classLevelDatas = new List<ClassLevelData>();
    public BaseActorStats baseStats = new BaseActorStats();
    public BaseActorStats currentStats;

    [Header("Bio")]
    public string actorName = "";
    public Sprite actorPortrait;

    [Header("Concurrent stats")]
    public float apBar = 0f;
    public int staminaPoint = 6;

    [Header("Abilities")]
    public ActorClass mainActorClass = ActorClass.BRAWLER;
    public Transform normalAttackHolder;
    public Transform mainClassSkillsHolder;
    public Transform subClassSkillsHolder;
    public GameObject skillUIPrefab;
    public GameObject skillUIPaddingPrefab;
    public Transform skillUIsholder;
    public Transform reactiveHolder;
    public BaseSkill actorNormalAttack;
    public List<BaseSkill> actorMainSkillsList = new List<BaseSkill>();
    public List<BaseSkill> actorSubSkillsList = new List<BaseSkill>();
    public BaseReactiveSkill actorReactiveSkill;

    [Header("Equipments")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public Transform bodyHolder;
    public Transform accessory1Holder;
    public Transform accessory2Holder;

    [Header("Statuses")]
    public Transform statusesHolder;

    //internals

    private void Start()
    {
        actorNormalAttack = normalAttackHolder.GetComponentsInChildren<BaseSkill>(true)[0];
        actorMainSkillsList.Clear();
        actorMainSkillsList.AddRange(mainClassSkillsHolder.GetComponentsInChildren<BaseSkill>(true));
        actorSubSkillsList.Clear();
        actorSubSkillsList.AddRange(subClassSkillsHolder.GetComponentsInChildren<BaseSkill>(true));
        actorReactiveSkill = reactiveHolder.GetComponentsInChildren<BaseReactiveSkill>(true)[0];

        Instantiate(skillUIPaddingPrefab, skillUIsholder.GetChild(0).GetChild(0).GetChild(0)); //padding
        Instantiate(skillUIPaddingPrefab, skillUIsholder.GetChild(0).GetChild(0).GetChild(0)); //padding
        foreach (BaseSkill skill in actorMainSkillsList)
        {
            GameObject go = Instantiate(skillUIPrefab, skillUIsholder.GetChild(0).GetChild(0).GetChild(0));
            go.GetComponent<SkillOptionBehavior>().SetData(skill.skillClassIcon, skill.skillName, skill.skillStaminaCost, skill.skillCastingDuration);
        }
        Instantiate(skillUIPaddingPrefab, skillUIsholder.GetChild(0).GetChild(0).GetChild(0)); //padding
        Instantiate(skillUIPaddingPrefab, skillUIsholder.GetChild(0).GetChild(0).GetChild(0)); //padding

        level = 0;
        baseStats = new BaseActorStats(LevelAndStatsManager.GetInstance().baseStats);
        foreach (ClassLevelData level_data in classLevelDatas)
        {
            level += level_data.level;

            foreach (StatsGrowthScriptable classStatsGrowth in LevelAndStatsManager.GetInstance().classesStatsGrowthList)
            {
                if (level_data.classID == classStatsGrowth.actorClass)
                {
                    baseStats.AdjustStatsBasedOnLevel(classStatsGrowth.GetStatsAtLevel(level_data.level));
                    
                    break;
                }
            }
        }

        baseStats.level = level;
        currentStats = new BaseActorStats(baseStats);

        skillUIsholder.gameObject.SetActive(false);
    }
}

[System.Serializable]
public class BaseActorStats
{
    public int level = 1;
    public int healthPoint = 100;
    public int speed = 100;
    public int pAtk = 10;
    public int mAtk = 10;
    public int pDef = 5;
    public int mDef = 5;
    public int jump = 1;
    public int dodgeChance = 5;
    public int blockChance = 10;
    public int critChance = 10;
    public int critResist = 0;
    public int accuracy = 80;

    public BaseActorStats() { }

    public BaseActorStats(BaseActorStats stats)
    {
        level = stats.level;
        healthPoint = stats.healthPoint;
        speed = stats.speed;
        pAtk = stats.pAtk;
        mAtk = stats.mAtk;
        pDef = stats.pDef;
        mDef = stats.mDef;
        jump = stats.jump;
        dodgeChance = stats.dodgeChance;
        blockChance = stats.blockChance;
        critChance = stats.critChance;
        critResist = stats.critResist;
        accuracy = stats.accuracy;
    }

    public BaseActorStats(StatsScriptable stats)
    {
        healthPoint = stats.healthPoint;
        speed = stats.speed;
        pAtk = stats.pAtk;
        mAtk = stats.mAtk;
        pDef = stats.pDef;
        mDef = stats.mDef;
        jump = stats.jump;
        dodgeChance = stats.dodgeChance;
        blockChance = stats.blockChance;
        critChance = stats.critChance;
        critResist = stats.critResist;
        accuracy = stats.accuracy;
    }

    public void AdjustStatsBasedOnLevel(StatsScriptable stats)
    {
        healthPoint += stats.healthPoint;
        speed += stats.speed;
        pAtk += stats.pAtk;
        mAtk += stats.mAtk;
        pDef += stats.pDef;
        mDef += stats.mDef;
        jump += stats.jump;
        dodgeChance += stats.dodgeChance;
        blockChance += stats.blockChance;
        critChance += stats.critChance;
        critResist += stats.critResist;
        accuracy += stats.accuracy;
    }

    public void HealthChange(int damage)
    {
        healthPoint += damage;
    }
}

[System.Serializable]
public enum ElementalProperty
{
    //basic
    PHYSICAL = 0,
    MAGICAL,

    //elementals
    FIRE,
    EARTH,
    WIND, 
    WATER,

    //
    HOLY,
    DARK
}


