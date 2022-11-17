using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollviewSnap : MonoBehaviour
{
    public Transform holder;
    public ScrollRect scrollRect;

    List<Transform> childrenList = new List<Transform>();
    List<float> childPosList = new List<float>();
    int highlightIndex = 0;
    int totalIndex = 1;
    float step = 0f;

    private void OnEnable()
    {
        childrenList.Clear();
        childPosList.Clear();

        foreach (Transform child in holder)
        {
            childrenList.Add(child);
        }
        totalIndex = childrenList.Count;
        step = 1f / (totalIndex - 4);

        for (int i = 0; i < totalIndex; i++)
        {
            childPosList.Add(step * i);
        }
    }

    public void OnValueChanged(Vector2 pos)
    {
        float closest_pos = 0f;


        //scrollRect.verticalNormalizedPosition = 1f - closest_pos;
    }

    Tween scroller = null;
    private void Update()
    {
        if (Mathf.Abs(InputProcessor.GetInstance().leftStick.z) >= 0.1f)
        {
            scrollRect.verticalNormalizedPosition += InputProcessor.GetInstance().leftStick.z * Time.deltaTime * 1f;
        }
        else
        {
            //TODO: snap
            Debug.Log(scrollRect.verticalNormalizedPosition);
        }
    }
}
