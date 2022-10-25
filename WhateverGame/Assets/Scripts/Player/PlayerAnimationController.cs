using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAbilityManager))]
public class PlayerAnimationController : MonoBehaviour
{
    public GameObject model;
    public PlayerController controller;
    public GameObject dashVFX;
    public Animator animator;
    public PlayerAbilityManager ability_manager;
    public List<GameObject> weapons_list = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        //rotate
        if (controller.m_IsFacingRight)
        {
            model.transform.forward = Vector3.right;
            dashVFX.transform.forward = Vector3.right;
        }
        else
        {
            model.transform.forward = Vector3.left;
            dashVFX.transform.forward = Vector3.left;
        }

        if (controller.m_MovementState == PlayerController.MovementStates.AIR_DASH ||
            controller.m_MovementState == PlayerController.MovementStates.GROUND_DASH)
        {
            dashVFX.SetActive(true);
            animator.SetBool("IsDashing", true);
            animator.ResetTrigger("NormalAttack");
            animator.ResetTrigger("AirAttack");
        }
        else
        {
            dashVFX.SetActive(false);
            animator.SetBool("IsDashing", false);
        }

        //locomotion
        animator.SetBool("IsFacingRight", controller.m_IsFacingRight);
        animator.SetBool("IsGrounded", controller.m_IsGrounded);

        if (controller.m_IsGrounded == true)
        {
            if (InputAdapter.Instance.horizontalInput > 0.1f ||
                InputAdapter.Instance.horizontalInput < -0.1f)
            {
                animator.SetFloat("Move", Mathf.Abs(InputAdapter.Instance.horizontalInput));
            }
            else
            {
                animator.SetFloat("Move", 0.0f);
            }
        }
        else
        {
            if (InputAdapter.Instance.jumpInput && controller.m_AbilityManager.m_CanDoubleJump == true)
            {
                animator.SetTrigger("DoubleJump");
            }
        }

        if (controller.m_MovementState == PlayerController.MovementStates.RIGHT_WALL_HANG ||
            controller.m_MovementState == PlayerController.MovementStates.LEFT_WALL_HANG)
        {
            animator.SetBool("IsWallHanging", true);

            if (InputAdapter.Instance.jumpInput && animator.GetCurrentAnimatorStateInfo(0).IsName("WallJump") == false)
            {
                animator.SetTrigger("WallJump");
            }
        }
        else
        {
            animator.SetBool("IsWallHanging", false);
        }

        //combat
        animator.SetInteger("WeaponIndex", (int)ability_manager.m_EquippedWeapon);
        if (InputAdapter.Instance.normalAttackInput && controller.m_TrueGroundCheck == true)
        {
            animator.SetTrigger("NormalAttack");
        }
        else if (InputAdapter.Instance.normalAttackInput && controller.m_TrueGroundCheck == false)
        {
            animator.SetTrigger("AirAttack");
        }

        //toggle weapons on - off
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("GroundCombat") ||
            animator.GetCurrentAnimatorStateInfo(0).IsTag("AirCombat"))
        {
            weapons_list[(int)ability_manager.m_EquippedWeapon - 1].SetActive(true);
        }
        else
        {
            foreach (GameObject weapon in weapons_list)
            {
                weapon.SetActive(false);
            }
        }
    }

    public void GetKnockBack()
    {
        animator.CrossFadeInFixedTime("KnockBack", 0.1f);
    }
}
