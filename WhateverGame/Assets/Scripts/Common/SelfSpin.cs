using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSpin : MonoBehaviour {

    [SerializeField]
    public enum SpinDirection
    {
        Y = 0,
        Z,
        X
    }

    public float SpinSpeed = 0.25f;
    public SpinDirection spin_axis = SpinDirection.Y;
    [SerializeField] bool is_reversed = false;
    float reverse = 1.0f;

    // Use this for initialization
    void Start () {
        if (is_reversed == false)
            reverse = 1.0f;
        else
            reverse = -1.0f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (spin_axis == SpinDirection.Y)
            transform.Rotate(Vector3.up, SpinSpeed * reverse);
        else if (spin_axis == SpinDirection.Z)
            transform.Rotate(Vector3.forward, SpinSpeed * reverse);
        else if (spin_axis == SpinDirection.X)
            transform.Rotate(Vector3.right, SpinSpeed * reverse);
    }
}
