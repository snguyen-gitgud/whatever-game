using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponBehavior : MonoBehaviour
{
    public PlayerAbilityManager abilityManager;
    public int baseDamage = 1;

    //internal

    private void OnTriggerEnter(Collider other)
    {
        if (abilityManager.m_CanDealDmg == false)
            return;

        AIStatBehavior hitTarget = other.gameObject.GetComponent<AIStatBehavior>();
        if (hitTarget != null)
        {
            Vector3 closestPoint = other.ClosestPointOnBounds(this.transform.position);

            hitTarget.GetHit(baseDamage * abilityManager.m_DmgMultiplier * abilityManager.m_DmgBonus, closestPoint);
        }
    }
}
