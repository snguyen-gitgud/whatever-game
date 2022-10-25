using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputAdapter : MonoBehaviour
{
    #region singleton
    public static InputAdapter Instance;

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

    public float horizontalInput = 0.0f;
    public float verticalInput = 0.0f;
    public bool jumpInput = false;
    public bool dashInput = false;
    public bool normalAttackInput = false;

    // Update is called once per frame
    void Update()
    {
        //axises
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //buttons
        jumpInput = Input.GetButtonDown("Jump");
        dashInput = Input.GetButtonDown("Dash");

        //attack
        normalAttackInput = Input.GetButtonDown("NormalAttack");
    }
}
