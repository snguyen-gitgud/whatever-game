using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseReactiveSkill : MonoBehaviour
{
    public BaseSkill react_skill = null;

    public virtual bool ReactiveCheck(ActorController actor, ActorController target)
    {
        BaseSkill skill = actor.actorStats.actorNormalAttack;
        List<GridUnit> aoe = BattleMaster.GetInstance().gridManager.FindArea(actor.occupied_grid_unit, skill.skillRange + 1, actor.actorTeams, true);
        BattleMaster.GetInstance().gridManager.ClearAreaHighlight();
        if (aoe.Contains(target.occupied_grid_unit))
            return true;
        else
            return false;
    }

    public virtual IEnumerator ReactiveSkillSequence(ActorController actor, ActorController target, int overload_level = 1)
    {
        yield return null;
    }
}
