using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SelfBlinkingUI : MonoBehaviour
{
    private CanvasGroup canvasGroup = null;
    public float blinking_speed = 1.0f;

    float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();

        float alpha = 0.0f;
        speed = 1.0f / blinking_speed;
        DOTween.Kill(this.gameObject);
        DOTween.To(() => alpha, x => alpha = x, 1.0f, speed)
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
