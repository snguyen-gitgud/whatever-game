using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class StageManagerBehavior : MonoBehaviour
{
    #region singleton
    public static StageManagerBehavior Instance;

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

    [Header("Player")]
    public GameObject m_Player;
    public Transform m_PlayerTransformHolder;
    public Cinemachine.CinemachineVirtualCamera m_PlayerCam;

    [Header("Rooms Manager")]
    public Vector3 m_RoomOffset = new Vector3(20.0f, 10.0f, 0.0f);
    public List<GameObject> m_RoomsList = new List<GameObject>();

    //internal 
    private GameObject player = null;

    public void StartGame()
    {
        player = Instantiate(m_Player, m_PlayerTransformHolder);
        m_PlayerCam.m_Follow = player.transform;
    }
}
