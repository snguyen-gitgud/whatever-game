using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseStatus : MonoBehaviour
{
    public Statuses id = Statuses.NULL;
    public new string name = "";
    public int duration = 3;
    public Sprite sprite;
    public GameObject vfx;

    public bool is_applied = false;

    GameObject ui_icon;

    public virtual void ProcStatus(ActorController actorController, ActorInfo info)
    {
        if (is_applied == false)
        {
            duration += 1;
            ui_icon = Instantiate(actorController.actorDetails.statusIconPref, actorController.actorDetails.statusIconsHolder);
            ui_icon.transform.GetChild(0).GetComponent<Image>().sprite = sprite;

            GameObject go = GameObject.FindGameObjectWithTag("TextDamage");
            Guirao.UltimateTextDamage.UltimateTextDamageManager textManager = go.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();
            textManager.Add(name, actorController.transform.GetChild(0).position + Vector3.up * actorController.actorStats.vcam_offset_Y, "status");

            is_applied = true;
        }

        if (vfx != null)
            Instantiate(vfx, this.transform.position + Vector3.up * 1.5f, new Quaternion(), this.transform);
        
        duration--;
    }

    public virtual void CheckClearStatus()
    {
        if (duration <= 0)
        {
            Destroy(ui_icon);
            Destroy(this.gameObject);
        }
    }
}

[System.Serializable]
public enum Statuses
{
    NULL = 0,

    REGEN, //heal HP
    POISON, //finite DOT % max HP
    BURN, //finite DOT % matk
    PARALIZE, //50% stun
    DAMP, //remove buffs + increase lightning dmg + reduce fire dmg
    ROOTED, //prevent move
    STUN, //skip turn, cancel casting
    FOCUS, //+50% ACC
    BLIND, //-50% ACC
    DOOM, //death on timer
    BLESSED, //auto revive
    BLEED, //infite DOT % atk
    MAIM, //reduce move range to 4

    HASTE, //increase spd
    SLOW, //reduce spd

    BRAVE, //increase atk
    FEAR, //reduce atk

    BRILIANT, //increase matk
    DUMB, //reduce matk

    FORTIFIED, //increase pdef
    BREAK, //decrease pdef

    HOPEFUL, //increase mdef
    DEPRESSED, //decrease mdef

    BERSERKER, //player lose control and actor only use normal attack automatically at full overload
    SILENCED, //player cannot use skill

    SLEEP, //stop AP gen
}
