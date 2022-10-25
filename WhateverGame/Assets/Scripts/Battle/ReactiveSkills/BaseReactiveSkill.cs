using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseReactiveSkill : MonoBehaviour
{
    public virtual IEnumerator ReactiveSkillSequence(ActorController actor, ActorController target, int overload_level = 1)
    {
        yield return null;
    }
}
