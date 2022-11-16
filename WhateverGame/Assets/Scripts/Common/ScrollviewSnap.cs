using System.Collections;
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
    int totalIndex = 0;
    float step = 0f;

    private void OnEnable()
    {
        childrenList.Clear();
        childPosList.Clear();

        foreach (Transform child in holder)
        {
            childrenList.Add(child);
        }

        step = 1f / totalIndex;

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
}
