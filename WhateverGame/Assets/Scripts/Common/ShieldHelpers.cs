using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShieldHelpers 
{
    public static int GetRandomNumber(int min, int max)
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        return Random.Range(min, max);
    }

    public static ClashData CalculateClashOutput(ActorController caster, ActorController target, BaseSkill skill, int bonus_multiplier)
    {
        ClashData ret = new ClashData();

        float dot_prod = Vector3.Dot(caster.transform.GetChild(0).forward.normalized, target.transform.GetChild(0).forward.normalized);
        float dist = Vector3.Distance(caster.transform.position, target.transform.position);
        if (dot_prod > 0.9f)
        {
            ret.isAmbush = true;
        }
        else
            ret.isAmbush = false;

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

        if (ret.isAmbush)
        {
            ret.output *= 2;
        }

        return ret;
    }
}

public class ClashData
{
    public bool isMiss = false;
    public bool isBlocked = false;
    public bool isCrit = false;
    public bool isAmbush = false;
    public int output = 0;
}
