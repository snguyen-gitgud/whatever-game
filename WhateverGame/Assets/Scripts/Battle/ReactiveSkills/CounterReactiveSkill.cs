using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterReactiveSkill : BaseReactiveSkill
{
    public override bool ReactiveCheck(ActorController actor, ActorController target)
    {
        return base.ReactiveCheck(actor, target);
    }

    public override IEnumerator ReactiveSkillSequence(ActorController actor, ActorController target, int overload_level = 1)
    {
        yield return base.ReactiveSkillSequence(actor, target, overload_level);

        react_skill = actor.actorStats.actorNormalAttack;
        react_skill.isReactive = true;
        react_skill.CastingSkill(actor, overload_level, target.occupied_grid_unit);

        yield return StartCoroutine(react_skill.ExecuteSkillSequence());
        react_skill.isReactive = false;
    }
}
