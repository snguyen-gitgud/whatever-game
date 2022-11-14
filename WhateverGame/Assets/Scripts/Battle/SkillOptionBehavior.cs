using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillOptionBehavior : MonoBehaviour
{
    public Image skillIcon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillCost;
    public TextMeshProUGUI skillCastTime;

    public void SetData(Sprite icon, string name, int cost, float cast_time)
    {
        skillIcon.sprite = icon;
        skillName.text = name;
        skillCost.text = cost + " St.";
        skillCastTime.text = cast_time + "s";
    }    
}
