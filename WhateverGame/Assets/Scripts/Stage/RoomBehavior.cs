using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RoomBehavior : MonoBehaviour
{
    public enum RoomIDs
    {
        NULL = 0,

        //training cave
        TRAINING_CAVE_CAMP_AREA,

        //abbadoned mine
        ABBADONED_MINE_AREA,
    }

    public enum RoomTypes
    {
        NULL = 0,
        PLATFORMER,
        COMBAT,
        PUZZLE,
        BOSS,
        EVENT
    }

    [Header("Identification")]
    public RoomIDs m_ID = RoomIDs.NULL;
    public List<GameObject> m_NeighborRoomsList = new List<GameObject>();

    [Header("Stylize")]
    public RoomTypes m_Type = RoomTypes.NULL;
    public float m_CameraDistanceOverride = 0.0f;
    public Cinemachine.CinemachineVirtualCamera overrideVcam;

    private void Start()
    {
        if (overrideVcam != null)
            overrideVcam.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Player") == true)
        {
            if (overrideVcam != null)
                overrideVcam.gameObject.SetActive(true);

            if (m_CameraDistanceOverride != 0.0f)
            {
                CommonRefManager.Instance.mainVCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = m_CameraDistanceOverride;
            }
            else
            {
                switch (m_Type)
                {
                    case RoomTypes.NULL:
                        CommonRefManager.Instance.mainVCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 12.0f;
                        break;
                    case RoomTypes.PLATFORMER:
                        CommonRefManager.Instance.mainVCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 14.0f;
                        break;
                    case RoomTypes.COMBAT:
                        CommonRefManager.Instance.mainVCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 10.0f;
                        break;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("Player") == true)
        {
            if (overrideVcam != null)
                overrideVcam.gameObject.SetActive(false);

            CommonRefManager.Instance.mainVCam.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 12.0f;
        } 
    }
}
