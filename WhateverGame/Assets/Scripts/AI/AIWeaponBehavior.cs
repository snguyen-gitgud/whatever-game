using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWeaponBehavior : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Player") == true)
        {
            other.GetComponent<PlayerStateModifier>().GetHit(damage, other.ClosestPointOnBounds(this.transform.position));
            this.gameObject.SetActive(false);
        }
    }
}
