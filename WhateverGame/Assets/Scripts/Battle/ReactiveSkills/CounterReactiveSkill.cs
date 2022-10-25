using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterReactiveSkill : BaseReactiveSkill
{
    public override IEnumerator ReactiveSkillSequence(ActorController actor, ActorController target, int overload_level = 1)
    {
        //TODO: validate reactive skill




        yield return base.ReactiveSkillSequence(actor, target, overload_level);
        
        BaseSkill react_skill = actor.actorStats.actorNormalAttack;
        react_skill.isReactive = true;
        react_skill.CastingSkill(actor, overload_level, target.occupied_grid_unit);

        yield return StartCoroutine(react_skill.ExecuteSkillSequence());
        react_skill.isReactive = false;
        actor.actorAnimationController.PlayIdle();
    }
}
