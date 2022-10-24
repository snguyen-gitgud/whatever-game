using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputProcessor : PersistantAndSingletonBehavior<InputProcessor>
{
    public ControllerTypes controllerTypes = ControllerTypes.PC_XBOX;

    [Header("Main buttons")]
    public bool buttonSouth = false;
    public bool buttonNorth = false;
    public bool buttonEast = false;
    public bool buttonWest = false;

    [Header("Shoulders")]
    public bool buttonShoulderR = false;
    public bool buttonShoulderL = false;

    [Header("Left stick")]
    public Vector3 leftStick = new Vector3();
    
    [Header("Right stick")]
    public Vector2 rightStick = new Vector2();
    public bool invertRightStick = false;

    private void Start()
    {
        
    }

    private void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            controllerTypes = ControllerTypes.MOUSE_KEYBOARD;
        }

        if (controllerTypes == ControllerTypes.PC_XBOX)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                buttonSouth = true;
            }
            else
            {
                buttonSouth = false;
            }

            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                buttonNorth = true;
            }
            else
            {
                buttonNorth = false;
            }

            if (gamepad.buttonEast.wasPressedThisFrame)
            {
                buttonEast = true;
            }
            else
            {
                buttonEast = false;
            }

            if (gamepad.buttonWest.wasPressedThisFrame)
            {
                buttonWest = true;
            }
            else
            {
                buttonWest = false;
            }

            if (gamepad.rightShoulder.wasPressedThisFrame)
            {
                buttonShoulderR = true;
            }
            else
            {
                buttonShoulderR = false;
            }

            if (gamepad.leftShoulder.wasPressedThisFrame)
            {
                buttonShoulderL = true;
            }
            else
            {
                buttonShoulderL = false;
            }

            Vector2 move = gamepad.leftStick.ReadValue();
            if (move.magnitude > 0.05f)
                leftStick = new Vector3(move.x, 0f, move.y).normalized;
            else
                leftStick = Vector3.zero;

            Vector2 cam = gamepad.rightStick.ReadValue();
            if (cam.magnitude > 0.05f)
                rightStick = new Vector2(cam.x, cam.y).normalized;
            else
                rightStick = Vector2.zero;

            if (invertRightStick)
                rightStick *= -1f;
        }

        if (controllerTypes == ControllerTypes.MOUSE_KEYBOARD)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                buttonSouth = true;
            }
            else
            {
                buttonSouth = false;
            }

            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                buttonNorth = true;
            }
            else
            {
                buttonNorth = false;
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                buttonEast = true;
            }
            else
            {
                buttonEast = false;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                buttonWest = true;
            }
            else
            {
                buttonWest = false;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                buttonShoulderR = true;
            }
            else
            {
                buttonShoulderR = false;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                buttonShoulderL = true;
            }
            else
            {
                buttonShoulderL = false;
            }

            Vector2 move = new Vector2();
            if (keyboard.wKey.isPressed)
                move = Vector2.up;
            else
                move = Vector2.zero;

            if (keyboard.sKey.isPressed)
                move = Vector2.down;
            else
                move = Vector2.zero;

            if (keyboard.aKey.isPressed)
                move = Vector2.left;
            else
                move = Vector2.zero;

            if (keyboard.dKey.isPressed)
                move = Vector2.right;
            else
                move = Vector2.zero;

            leftStick = new Vector3(move.x, 0f, move.y);

            //Vector2 cam = gamepad.rightStick.ReadValue();
            //if (cam.magnitude > 0.05f)
            //    rightStick = new Vector2(cam.x, cam.y).normalized;
            //else
            //    rightStick = Vector2.zero;

            //if (invertRightStick)
            //    rightStick *= -1f;
        }
    }
}

public enum ControllerTypes
{
    PC_XBOX,
    PS,
    SWITCH,
    MOUSE_KEYBOARD
}
