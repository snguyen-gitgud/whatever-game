using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActorStats : MonoBehaviour
{
    [Header("Core stats")]
    public int maxActionPoint = 6;
    public float baseSpeed = 20f;

    [Header("Specific stats")]
    public int speed = 100;

    [Header("Concurrent stats")]
    public float apBar = 0f;
    public int actionPoint = 6;
}
