using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorBehavior : MonoBehaviour
{
    public bool is_activated = true;
    public Transform top_pivot;
    public Transform bot_pivot;
    public GameObject moving_platform;

    bool is_moving_up = true;
    float moving_dist = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        moving_platform.transform.position = bot_pivot.position;
        moving_dist = Vector3.Distance(top_pivot.position, bot_pivot.position);

        MovePlatform();
    }

    public void MovePlatform()
    {
        if (is_moving_up == true)
        {
            LeanTween.move(moving_platform, top_pivot.position, moving_dist).setOnComplete(() => {
                is_moving_up = !is_moving_up;
                MovePlatform();
            });
        }
        else
        {
            LeanTween.move(moving_platform, bot_pivot.position, moving_dist).setOnComplete(() => {
                is_moving_up = !is_moving_up;
                MovePlatform();
            });
        }
    }
}
