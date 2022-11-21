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
    public Image actorAPSlider;
    public Image actorStaminaSlider;
    public Image actorStaminaPreviewSlider;
    public Image actorStaminaInDebtPreviewSlider;
    public Image teamBG;
    public Transform statusIconsHolder;
    public GameObject statusIconPref;

    public ActorController actorController = null;

    public void SetDisplayData(ActorController actor, Color team_color)
    {
        actorController = actor;
        actorPortrait.sprite = actor.actorStats.actorPortrait;
        actorName.text = actor.actorStats.actorName;
        actorLevel.text = "Level: " + actor.actorStats.level.ToString();
        actorHP.text = "HP: " + actor.actorStats.currentStats.healthPoint + "/" + actor.actorStats.baseStats.healthPoint;
        actorHPSlider.fillAmount = (actor.actorStats.currentStats.healthPoint * 1f) / (actor.actorStats.baseStats.healthPoint * 1f);
        if (actorAPSlider != null) actorAPSlider.fillAmount = actor.actorStats.apBar / 100f;
        teamBG.color = team_color;
    }
}
