using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    public Vector2 RightDragDelta { get; private set; }
    public float ScrollValue { get; private set; }
    public bool IsRightClickHeld { get; private set; }

    public event Action<Vector2> LeftClicked;
    public event Action<Vector2> RightClicked;
    public event Action<Vector2> RightClickStarted;
    public event Action<Vector2> RightClickEnded;
    public event Action<Vector2> RightDragged;
    public event Action<float> Scrolled;

    private Vector2 previousMousePosition;
    private bool hasPreviousMousePosition;

    private void OnEnable()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            previousMousePosition = mouse.position.ReadValue();
            hasPreviousMousePosition = true;
        }
    }

    private void Update()
    {
        ReadKeyboard();
        ReadMouse();
    }

    private void ReadKeyboard()
    {
        Movement = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            Movement += Vector2.up;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            Movement += Vector2.down;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            Movement += Vector2.left;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            Movement += Vector2.right;
        }

        Movement = Vector2.ClampMagnitude(Movement, 1f);
    }

    private void ReadMouse()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            MousePosition = Vector2.zero;
            MouseDelta = Vector2.zero;
            RightDragDelta = Vector2.zero;
            ScrollValue = 0f;
            IsRightClickHeld = false;
            hasPreviousMousePosition = false;
            return;
        }

        MousePosition = mouse.position.ReadValue();

        if (!hasPreviousMousePosition)
        {
            previousMousePosition = MousePosition;
            hasPreviousMousePosition = true;
        }

        MouseDelta = MousePosition - previousMousePosition;
        RightDragDelta = Vector2.zero;
        ScrollValue = mouse.scroll.ReadValue().y;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            LeftClicked?.Invoke(MousePosition);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            IsRightClickHeld = true;
            RightClicked?.Invoke(MousePosition);
            RightClickStarted?.Invoke(MousePosition);
        }

        if (mouse.rightButton.isPressed)
        {
            IsRightClickHeld = true;
            RightDragDelta = MouseDelta;

            if (RightDragDelta.sqrMagnitude > 0.01f)
            {
                RightDragged?.Invoke(RightDragDelta);
            }
        }

        if (mouse.rightButton.wasReleasedThisFrame)
        {
            IsRightClickHeld = false;
            RightClickEnded?.Invoke(MousePosition);
        }

        if (Mathf.Abs(ScrollValue) > 0.01f)
        {
            Scrolled?.Invoke(ScrollValue);
        }

        previousMousePosition = MousePosition;
    }
}
