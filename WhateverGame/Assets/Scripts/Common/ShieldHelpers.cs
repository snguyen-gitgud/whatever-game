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

    public static ClashData CalculateClashOutput(ActorController caster, ActorController target, BaseSkill skill, int bonus_multiplier, bool bypass_crit = false)
    {
        ClashData ret = new ClashData();

        float dot_prod = Vector3.Dot(caster.transform.GetChild(0).forward.normalized, target.transform.GetChild(0).forward.normalized);
        float dist = Vector3.Distance(caster.transform.position, target.transform.position);
        if (dot_prod > 0.9f)
        {
            ret.isAmbush = true;
        }
        else
        {
            ret.isAmbush = false;

            if (dot_prod < -0.9f)
                ret.isDirect = true;
            else
                ret.isDirect = false;
        }

        if (skill.damageTypes == DamageTypes.PHYSICAL)
        {
            ret.output = (int)((skill.baseDamageMultiplier * caster.actorStats.currentStats.pAtk) - target.actorStats.currentStats.pDef) * bonus_multiplier;
        }
        else if (skill.damageTypes == DamageTypes.MAGICAL)
        {
            ret.output = (int)((skill.baseDamageMultiplier * caster.actorStats.currentStats.mAtk) - target.actorStats.currentStats.mDef) * bonus_multiplier;
        }
        else if (skill.damageTypes == DamageTypes.MIXED)
        {
            ret.output = (int)((skill.baseDamageMultiplier * (caster.actorStats.currentStats.pAtk + caster.actorStats.currentStats.mAtk)) - (target.actorStats.currentStats.pDef + target.actorStats.currentStats.mDef)) * bonus_multiplier;
        }
        else if (skill.damageTypes == DamageTypes.PURE)
        {
            ret.output = (int)(((skill.baseDamageMultiplier * (caster.actorStats.currentStats.pAtk + caster.actorStats.currentStats.mAtk)) * bonus_multiplier));
        }

        if (ret.output < 0)
            ret.output = 1;

        if (ShieldHelpers.GetRandomNumber(0, 100) < (caster.actorStats.currentStats.accuracy + skill.skillAccuracyBonus) - target.actorStats.currentStats.dodgeChance)
            ret.isMiss = false;
        else
            ret.isMiss = true;

        if (ShieldHelpers.GetRandomNumber(0, 100) < target.actorStats.currentStats.blockChance)
            ret.isBlocked = true;
        else
            ret.isBlocked = false;

        if (ShieldHelpers.GetRandomNumber(0, 100) < (caster.actorStats.currentStats.critChance + (ret.isAmbush == true? 10 : 0)) - target.actorStats.currentStats.critResist && bypass_crit == false)
        {
            ret.output *= 2;
            ret.isCrit = true;
        }    
        else
            ret.isCrit = false;

        if (ret.isAmbush)
        {
            ret.output *= 3;
            ret.output /= 2;
            ret.isBlocked = false;
        }
        
        if (ret.isDirect == false && ret.isAmbush == false)
        {
            ret.output *= 5;
            ret.output /= 4;
        }

        if (caster.actorTeams == target.actorTeams)
            ret.output /= 2;

        return ret;
    }

    public static void ChangeAP(ActorController targetController, int amount)
    {
        if (targetController.actorControlStates == ActorControlStates.AP_STAG)
        {
            targetController.actorStats.apBar += amount * 1f;
            targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar / 100f;
        }
        else if (targetController.actorControlStates == ActorControlStates.CASTING_STAG)
        {
            targetController.current_casting_value += (targetController.current_casting_value * (1f - (amount / 100f)));
            targetController.actorStats.apBar = targetController.current_casting_value / targetController.currentChosenSkill.skillCastingDuration;
            targetController.actorUI.apBar.fillAmount = targetController.actorStats.apBar;
        }
    }

    public static void ChangeStamina(ActorController targetController, int amount)
    {
        targetController.ap_bank += amount;
    }
}

public class ClashData
{
    public bool isMiss = false;
    public bool isBlocked = false;
    public bool isCrit = false;
    public bool isAmbush = false;
    public bool isDirect = false;
    public int output = 0;
}
