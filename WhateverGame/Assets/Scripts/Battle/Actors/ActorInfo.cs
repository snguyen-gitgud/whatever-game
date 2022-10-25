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

    [Header("Specific stats")]
    public BaseActorStats baseStats = new BaseActorStats();
    public BaseActorStats currentStats;
    public ActorClass actorClass = ActorClass.MONK;

    [Header("Bio")]
    public string actorName = "";
    public Sprite actorPortrait;

    [Header("Concurrent stats")]
    public float apBar = 0f;
    public int staminaPoint = 6;

    [Header("Abilities")]
    public Transform normalAttackHolder;
    public Transform skillsHolder;
    public Transform reactiveHolder;
    public BaseSkill actorNormalAttack;
    public List<BaseSkill> actorSkillsList = new List<BaseSkill>();
    public BaseReactiveSkill actorReactiveSkill;

    //internals

    private void Start()
    {
        actorNormalAttack = normalAttackHolder.GetComponentsInChildren<BaseSkill>(true)[0];
        actorSkillsList.Clear();
        actorSkillsList.AddRange(skillsHolder.GetComponentsInChildren<BaseSkill>(true));
        actorReactiveSkill = reactiveHolder.GetComponentsInChildren<BaseReactiveSkill>(true)[0];

        currentStats = new BaseActorStats(baseStats);
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

    public void HealthChange(int damage)
    {
        healthPoint += damage;
    }
}

[System.Serializable]
public enum ActorClass
{
    KNIGHT = 0,
    MONK,
    SCHOLAR,
    BERSERKER,
    NATURE_WITCH
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


