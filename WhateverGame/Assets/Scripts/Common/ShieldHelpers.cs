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

    public static ClashData CalculateClashOutput(ActorController caster, ActorController target, BaseSkill skill, int bonus_multiplier)
    {
        ClashData ret = new ClashData();

        if (skill.damageTypes == DamageTypes.PHYSICAL)
        {
            ret.output = (((skill.baseDamage + caster.actorStats.currentStats.pAtk) * bonus_multiplier)) - target.actorStats.currentStats.pDef;
        }
        else if (skill.damageTypes == DamageTypes.MAGICAL)
        {
            ret.output = (((skill.baseDamage + caster.actorStats.currentStats.mAtk) * bonus_multiplier)) - target.actorStats.currentStats.mDef;
        }
        else if (skill.damageTypes == DamageTypes.MIXED)
        {
            ret.output = (((skill.baseDamage + caster.actorStats.currentStats.pAtk + caster.actorStats.currentStats.mAtk) * bonus_multiplier)) - (target.actorStats.currentStats.pDef + target.actorStats.currentStats.mDef);
        }
        else if (skill.damageTypes == DamageTypes.PURE)
        {
            ret.output = (((skill.baseDamage + caster.actorStats.currentStats.pAtk + caster.actorStats.currentStats.mAtk) * bonus_multiplier));
        }

        if (ShieldHelpers.GetRandomNumber(0, 100) < (caster.actorStats.currentStats.accuracy + skill.skillAccuracyBonus) - target.actorStats.currentStats.dodgeChance)
            ret.isMiss = false;
        else
            ret.isMiss = true;

        if (ShieldHelpers.GetRandomNumber(0, 100) < target.actorStats.currentStats.blockChance)
            ret.isBlocked = true;
        else
            ret.isBlocked = false;

        if (ShieldHelpers.GetRandomNumber(0, 100) < caster.actorStats.currentStats.critChance - target.actorStats.currentStats.critResist)
        {
            ret.output *= 2;
            ret.isCrit = true;
        }    
        else
            ret.isCrit = false;

        return ret;
    }
}

public class ClashData
{
    public bool isMiss = false;
    public bool isBlocked = false;
    public bool isCrit = false;
    public int output = 0;
}
