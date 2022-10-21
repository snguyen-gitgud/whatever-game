using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSpinning : MonoBehaviour
{
    [SerializeField] public bool allowToSpin = true;

    [SerializeField]
    public enum SpinDirection
    {
        Y = 0,
        Z,
        X
    }

    private Vector2 previous;

    public float SpinSpeed = 0.25f;
    public SpinDirection spin_axis = SpinDirection.Y;
    public Cinemachine.CinemachineFreeLook vcam;

    [SerializeField] bool is_reversed = false;
    float reverse = 1.0f;

    bool spinning;

    Quaternion ogRotation;

    private void OnDisable()
    {
        if (vcam != null)
            vcam.Priority = 0;

        this.transform.rotation = ogRotation;
    }

    private void OnEnable()
    {
        if (vcam != null)
            vcam.Priority = 15;

        this.transform.rotation = ogRotation;
    }

    // Use this for initialization
    void Start()
    {
        ogRotation = this.transform.rotation;
        if (vcam != null)
            vcam.Priority = 15;

        if (is_reversed == false)
            reverse = 1.0f;
        else
            reverse = -1.0f;
    }

    // Update is called once per frame
    /*void FixedUpdate()
    {
        if (spin_axis == SpinDirection.Y)
            transform.Rotate(Vector3.up, SpinSpeed * reverse);
        else if (spin_axis == SpinDirection.Z)
            transform.Rotate(Vector3.forward, SpinSpeed * reverse);
        else if (spin_axis == SpinDirection.X)
            transform.Rotate(Vector3.right, SpinSpeed * reverse);
    }*/

    private void Update()
    {
        if (allowToSpin)
        {
            if (Input.GetMouseButtonDown(0))
            {
                previous = Input.mousePosition;
                spinning = true;
            }

            if (Input.GetMouseButton(0) && spinning)
            {
                var cur = Input.mousePosition;
                var diff = cur.x - previous.x;
                transform.Rotate(Vector3.up, -diff);
                previous = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                spinning = false;
                previous = Vector2.zero;
            }
        }
    }
}
