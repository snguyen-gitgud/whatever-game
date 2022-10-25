using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerAbilityManager))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum MovementStates
    {
        NORMAL = 0,
        GROUND_DASH,
        AIR_DASH,
        RIGHT_WALL_HANG,
        LEFT_WALL_HANG,
        RECOVER,
    }

    [HideInInspector] public CharacterController m_CharacterController;

    [Header("Movement Parametters")]
    public float m_Speed = 6.0f;
    public float m_Gravity = -9.81f;
    public Transform m_GroundCheckerTransform;
    public float m_GroundDistance = 0.25f;
    public LayerMask m_GroundMask;
    public float m_CoyoteTime = 0.1f;
    public float m_JumpHeight = 1.5f;
    public float m_DashTime = 1.0f;
    public float m_DashSpeedMultiplier = 5.0f;
    public Transform m_RightWallCheckerTransform;
    public Transform m_LeftWallCheckerTransform;
    public float m_WallCheckDistance = 0.1f;
    public float m_WallGlideSpeed = -0.5f;
    public Animator animator;

    //internal
    [HideInInspector] public Vector3 m_Velocity = new Vector3();
    [HideInInspector] public bool m_IsGrounded = false;
    [HideInInspector] public bool m_IsDoubleJump = false;
    private float m_CurrentCoyoteTime = 0.0f;
    [HideInInspector] public MovementStates m_MovementState = MovementStates.NORMAL;
    private float m_CurrentDashTime = 0.0f;
    [HideInInspector] public bool m_IsFacingRight = true;
    private bool m_IsDashAvailable = true;
    private Vector3 m_HangingVelocity = new Vector3();
    [HideInInspector] public PlayerAbilityManager m_AbilityManager;
    [HideInInspector] public bool m_TrueGroundCheck = false;
    private bool m_IsBouncedOffEnemy = false;

    //on-off switch
    [HideInInspector] public bool m_IsActive = true;

    // Start is called before the first frame update
    void Start()
    {
        m_CharacterController = this.GetComponent<CharacterController>();
        m_AbilityManager = this.GetComponent<PlayerAbilityManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsActive == false)
            return;

        //lock Z axis
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.0f);

        //ground check
        m_IsGrounded = GroundCheck();
        if (m_TrueGroundCheck == true)
        {
            m_CharacterController.stepOffset = 0.3f; //default
        }
        else
        {
            m_CharacterController.stepOffset = 0.0001f; //fix jittering on edge
        }

        //movement state switching
        if (m_MovementState == MovementStates.NORMAL)
        {
            NormalStateMovement();
        }
        else if (m_MovementState == MovementStates.GROUND_DASH)
        {
            GroundDashMovement();
        }
        else if(m_MovementState == MovementStates.AIR_DASH)
        {
            AirDashMovement();
        }
        else if(m_MovementState == MovementStates.RIGHT_WALL_HANG)
        {
            RightWallHangMovement();
        }
        else if (m_MovementState == MovementStates.LEFT_WALL_HANG)
        {
            LeftWallHangMovement();
        }
    }

    private bool GroundCheck()
    {
        bool ret = true;

        bool ret_temp = Physics.CheckSphere(m_GroundCheckerTransform.position, m_GroundDistance, m_GroundMask);
        
        if (ret_temp == false && m_CurrentCoyoteTime < m_CoyoteTime) //in air, on Coyote time
        {
            ret = true;
            m_CurrentCoyoteTime += Time.deltaTime;
            m_TrueGroundCheck = false;
        }
        else if (ret_temp == false && m_CurrentCoyoteTime >= m_CoyoteTime) //actually in air
        {
            ret = false;
            m_TrueGroundCheck = false;
        }
        else if (ret_temp == true) //actually on ground
        {
            ret = true;
            m_CurrentCoyoteTime = 0.0f;
            m_IsDoubleJump = false;
            m_IsDashAvailable = true;
            m_TrueGroundCheck = true;

            //enemies bounce
            //TODO: add enemies bouncing here
            RaycastHit hit;
            bool check_step = Physics.Raycast(m_GroundCheckerTransform.position, Vector3.down, out hit, m_GroundDistance, m_GroundMask);
            //TODO: check hit info to get what is stepped on?
            if (check_step == true && hit.transform.tag.Contains("Enemy") == true && m_IsBouncedOffEnemy == false)
            {
                m_Velocity.y = Mathf.Sqrt(m_JumpHeight * -2.0f * m_Gravity);

                AIStatBehavior hitTarget = hit.transform.GetComponent<AIStatBehavior>();
                if (hitTarget != null)
                {
                    hitTarget.GetHit(1, hit.point);
                }

                m_IsBouncedOffEnemy = true;
                StartCoroutine(DelayedResetBouncing());
            }
        }

        return ret;
    }

    IEnumerator DelayedResetBouncing()
    {
        yield return new WaitForSeconds(0.125f);
        m_IsBouncedOffEnemy = false;
    }

    private void NormalStateMovement()
    {
        //dash
        if (InputAdapter.Instance.dashInput && m_IsDashAvailable == true && m_AbilityManager.m_CanDash == true)
        {
            m_IsDashAvailable = false;

            if (m_IsGrounded == false)
            {
                m_Velocity.y = 0.0f;
                m_MovementState = MovementStates.AIR_DASH;
            }
            else
            {
                m_Velocity.y = 0.0f;
                m_MovementState = MovementStates.GROUND_DASH;
            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Locomotion") == false)
            return;

        //get inputs
        float x = InputAdapter.Instance.horizontalInput;
        if (x > 0.1f)
        {
            m_IsFacingRight = true;
        }
        else if (x < -0.1f)
        {
            m_IsFacingRight = false;
        }

        //jump
        if (InputAdapter.Instance.jumpInput && m_IsGrounded == true)
        {
            m_Velocity.y = Mathf.Sqrt(m_JumpHeight * -2.0f * m_Gravity);
        }
        //double jump
        else if (InputAdapter.Instance.jumpInput && m_IsGrounded == false && m_IsDoubleJump == false && m_AbilityManager.m_CanDoubleJump == true)
        {
            m_Velocity.y = Mathf.Sqrt(m_JumpHeight * -1.5f * m_Gravity);
            m_IsDoubleJump = true;
        }

        //check for wall hanging
        if (m_IsGrounded == false && Physics.CheckSphere(m_RightWallCheckerTransform.position, m_WallCheckDistance, m_GroundMask) == true && x > 0.1f && m_AbilityManager.m_CanWallHang) //hang right
        {
            m_Velocity.y = 0.0f;
            m_IsDoubleJump = false;
            m_IsDashAvailable = true;
            m_MovementState = MovementStates.RIGHT_WALL_HANG;
            return;
        }
        if (m_IsGrounded == false && Physics.CheckSphere(m_LeftWallCheckerTransform.position, m_WallCheckDistance, m_GroundMask) == true && x < -0.1f && m_AbilityManager.m_CanWallHang) //hang left
        {
            m_Velocity.y = 0.0f;
            m_IsDoubleJump = false;
            m_IsDashAvailable = true;
            m_MovementState = MovementStates.LEFT_WALL_HANG;
            return;
        }

        //combine all movements 
        Vector3 move_dir = new Vector3(x, 0.0f, 0.0f).normalized;

        //move 
        if (move_dir.magnitude >= 0.1f)
        {
            m_CharacterController.Move(move_dir * m_Speed * Time.deltaTime);
        }

        //gravity
        m_Velocity.y += m_Gravity * Time.deltaTime;

        if (m_IsGrounded == true && m_Velocity.y < 0)
        {
            m_Velocity.y = -2.0f;
        }

        m_CharacterController.Move(m_Velocity * Time.deltaTime);
    }

    private void GroundDashMovement()
    {
        m_CurrentDashTime += Time.deltaTime;
        if (m_CurrentDashTime >= m_DashTime)
        {
            m_CurrentDashTime = 0.0f;
            m_MovementState = MovementStates.NORMAL;
            return;
        }

        ////dashing on ground with gravity on
        ////gravity
        //m_Velocity.y += m_Gravity * Time.deltaTime;

        //if (m_IsGrounded == true && m_Velocity.y < 0)
        //{
        //    m_Velocity.y = -2.0f;
        //}

        //m_CharacterController.Move(m_Velocity * Time.deltaTime);

        //dash
        if (m_IsFacingRight == true)
        {
            m_CharacterController.Move(new Vector3(1.0f, 0.0f, 0.0f) * m_Speed * m_DashSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            m_CharacterController.Move(new Vector3(-1.0f, 0.0f, 0.0f) * m_Speed * m_DashSpeedMultiplier * Time.deltaTime);
        }
    }

    private void AirDashMovement()
    {
        m_CurrentDashTime += Time.deltaTime;
        if (m_CurrentDashTime >= m_DashTime)
        {
            m_CurrentDashTime = 0.0f;
            m_MovementState = MovementStates.NORMAL;
            return;
        }

        //dashing in air with gravity off
        //dash
        if (m_IsFacingRight == true)
        {
            m_CharacterController.Move(new Vector3(1.0f, 0.0f, 0.0f) * m_Speed * m_DashSpeedMultiplier * Time.deltaTime);
        }
        else
        {
            m_CharacterController.Move(new Vector3(-1.0f, 0.0f, 0.0f) * m_Speed * m_DashSpeedMultiplier * Time.deltaTime);
        }
    }

    private void RightWallHangMovement()
    {
        float x = InputAdapter.Instance.horizontalInput;

        if (Physics.CheckSphere(m_RightWallCheckerTransform.position, m_WallCheckDistance, m_GroundMask) == false)
        {
            m_MovementState = MovementStates.NORMAL;
            m_Velocity = m_HangingVelocity;
            m_HangingVelocity = Vector3.zero;
            return;
        }

        if (x > 0.1f)
        {
            if (InputAdapter.Instance.jumpInput && m_HangingVelocity.y <= 0.0f && animator.GetCurrentAnimatorStateInfo(0).IsName("WallJump") == false)
            {
                m_HangingVelocity.y = Mathf.Sqrt(m_JumpHeight * -1.5f * m_Gravity);
            }

            if (m_HangingVelocity.y > 0.0f)
                m_HangingVelocity.y += m_Gravity * Time.deltaTime;
            else
                m_HangingVelocity.y += m_WallGlideSpeed * Time.deltaTime;

            m_CharacterController.Move(m_HangingVelocity * Time.deltaTime);
        }
        else
        {
            m_MovementState = MovementStates.NORMAL;
            m_HangingVelocity = Vector3.zero;
            return;
        }
    }

    private void LeftWallHangMovement()
    {
        float x = InputAdapter.Instance.horizontalInput;

        if (Physics.CheckSphere(m_LeftWallCheckerTransform.position, m_WallCheckDistance, m_GroundMask) == false)
        {
            m_MovementState = MovementStates.NORMAL;
            m_Velocity = m_HangingVelocity;
            m_HangingVelocity = Vector3.zero;
            return;
        }

        if (x < -0.1f)
        {
            if (InputAdapter.Instance.jumpInput && m_HangingVelocity.y <= 0.0f && animator.GetCurrentAnimatorStateInfo(0).IsName("WallJump") == false)
            {
                m_HangingVelocity.y = Mathf.Sqrt(m_JumpHeight * -1.5f * m_Gravity);
            }

            if (m_HangingVelocity.y > 0.0f)
                m_HangingVelocity.y += m_Gravity * Time.deltaTime;
            else
                m_HangingVelocity.y += m_WallGlideSpeed * Time.deltaTime;

            m_CharacterController.Move(m_HangingVelocity * Time.deltaTime);
        }
        else
        {
            m_MovementState = MovementStates.NORMAL;
            m_HangingVelocity = Vector3.zero;
            return;
        }
    }
}
