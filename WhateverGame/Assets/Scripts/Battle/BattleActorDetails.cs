using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleActorDetails : MonoBehaviour
{
    public Image actorPortrait;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI actorLevel;
    public TextMeshProUGUI actorHP;
    public Image actorHPSlider;
    public TextMeshProUGUI actorAP;
    public Image actorAPSlider;

    public void SetDisplayData(Sprite img, string name, int level, int hp, int hp_max, float ap)
    {
        actorPortrait.sprite = img;
        actorName.text = name;
        actorLevel.text = "Level: " + level.ToString();
        actorHP.text = hp + "/" + hp_max;
        actorHPSlider.fillAmount = (hp * 1f) / (hp_max * 1f);
        actorAP.text = ap * 100f + "%";
        actorAPSlider.fillAmount = ap;
    }
}
