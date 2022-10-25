using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RootMotionAdapter : MonoBehaviour
{
    Animator animator;
    public PlayerController controller;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    void OnAnimatorMove()
    {
        if (Physics.CheckSphere(controller.m_RightWallCheckerTransform.position + new Vector3(0.0f, controller.m_CharacterController.height * 0.5f, 0.0f), controller.m_WallCheckDistance, controller.m_GroundMask) == true ||
            Physics.CheckSphere(controller.m_LeftWallCheckerTransform.position + new Vector3(0.0f, controller.m_CharacterController.height * 0.5f, 0.0f), controller.m_WallCheckDistance, controller.m_GroundMask) == true)
        {
            return;
        }

        if (animator != null && !Mathf.Approximately(0f, animator.deltaPosition.x))
        {
            Vector3 newPosition = transform.parent.parent.position;
            newPosition.x += animator.deltaPosition.x;
            transform.parent.parent.position = newPosition;
        }
    }
}
