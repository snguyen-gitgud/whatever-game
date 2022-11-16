using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInFadeOutCanvasGroup : MonoBehaviour
{
    CanvasGroup canvasGroup;

    private void OnEnable()
    {
        if (canvasGroup == null)
            canvasGroup = this.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        float alpha = 0f;
        DOTween.To(() => alpha, x => alpha = x, 1f, 0.25f)
            .OnUpdate(() => {
                canvasGroup.alpha = alpha;
            })
            .SetUpdate(UpdateType.Normal, true);
    }

    private void OnDisable()
    {
        canvasGroup.alpha = 0f;
    }
}
