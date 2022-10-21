using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SelfBlinkingUI : MonoBehaviour
{
    private CanvasGroup canvasGroup = null;
    public float blinking_speed = 1.0f; 

    // Start is called before the first frame update
    void OnEnable()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();

        float alpha = 0.0f;
        DOTween.To(() => alpha, x => alpha = x, 1.0f, 1.0f / blinking_speed)
            .SetLoops(-1, LoopType.Yoyo)
            .OnUpdate(() => {
                canvasGroup.alpha = alpha;
            });
    }

    private void OnDisable()
    {
        DOTween.Kill(this.gameObject);
    }
}
