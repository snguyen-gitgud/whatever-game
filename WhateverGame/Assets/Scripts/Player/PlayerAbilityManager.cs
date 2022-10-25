using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    [Header("Abilities Controller")]
    public bool m_CanDoubleJump = false;
    public bool m_CanDash = false;
    public bool m_CanWallHang = false;

    [Header("Combat")]
    [HideInInspector] public bool m_CanDealDmg = false;
    [HideInInspector] public int m_DmgMultiplier = 1;
    [HideInInspector] public int m_DmgBonus = 1;

    public enum WeaponTypes
    {
        NULL = 0,
        SMALL_HAND_AXE,
        SWORD_AND_SHIELD,
        DUAL_DAGGER,
        SPEAR,
        BOW,
        FIRE_BOMB
    }
    [Header("Weapon Manager")]
    public WeaponTypes m_EquippedWeapon = WeaponTypes.NULL;
    public List<bool> m_IsWeaponUnlockedList = new List<bool>();

    private void Start()
    {
        //lock all weapons except small axe by default
        for (int i = 0; i < 7; i++)
        {
            m_IsWeaponUnlockedList.Add(false);
        }
        m_IsWeaponUnlockedList[0] = true;
        m_IsWeaponUnlockedList[1] = true;
    }
}
