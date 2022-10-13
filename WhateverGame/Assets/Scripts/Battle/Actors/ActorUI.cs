using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorUI : MonoBehaviour
{
    public GameObject headerHolder;

    [Header("Bars")]
    public Image apBar;
    public Image apPoints;

    [Header("Camera")]
    public Vector2 offset;

    //internal
    new private Camera camera;
    RectTransform rectTransform;
    GameObject target;

    void Start()
    {
        camera = Camera.main;
        rectTransform = headerHolder.GetComponent<RectTransform>();
        target = this.gameObject;
    }

    void Update()
    {
        rectTransform.anchoredPosition = camera.WorldToScreenPoint(target.transform.position) + (Vector3)offset;
    }
}
