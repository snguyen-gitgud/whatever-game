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

    public void FadeOut()
    {
        if (canvasGroup == null)
            canvasGroup = this.GetComponent<CanvasGroup>();

        float alpha = 1f;
        DOTween.To(() => alpha, x => alpha = x, 0f, 0.25f)
            .OnUpdate(() => {
                canvasGroup.alpha = alpha;
            })
            .SetUpdate(UpdateType.Normal, true)
            .OnComplete(() => {
                this.gameObject.SetActive(false);
            });
    }

    private void OnDisable()
    {
        canvasGroup.alpha = 0f;
    }
}
