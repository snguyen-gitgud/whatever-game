using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonRefManager : MonoBehaviour
{
    #region singleton
    public static CommonRefManager Instance;

    void Awake()
    {
        if (Instance == null)
        {

            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    #endregion

    [Header("Common In-game Assets")]
    public GameObject commonCanvas;
    public GameObject startMenu;
    public GameObject gameOverMenu;
    public GameObject playerStateInfo;
    public Light commonLight;
    public Cinemachine.CinemachineVirtualCamera mainVCam;

    [Header("Text damage")]
    public Canvas textDamageCanvas;

    [Header("Common Vfx")]
    public GameObject mediumHitVfx;
}
