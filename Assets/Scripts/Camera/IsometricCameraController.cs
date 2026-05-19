using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private Camera controlledCamera;

    [Header("2D Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Right Click Drag")]
    [SerializeField] private bool enableRightClickDrag = true;
    [SerializeField] private float dragSensitivity = 1f;

    [Header("Zoom")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSpeed = 0.08f;
    [SerializeField] private float minOrthographicSize = 2f;
    [SerializeField] private float maxOrthographicSize = 14f;

    [Header("Optional Boundaries")]
    [SerializeField] private bool useBoundaries;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    private void Awake()
    {
        if (inputProvider == null)
        {
            inputProvider = FindFirstObjectByType<InputProvider>();
        }

        if (controlledCamera == null)
        {
            controlledCamera = GetComponentInChildren<Camera>();
        }

        if (controlledCamera == null)
        {
            controlledCamera = Camera.main;
        }

        if (controlledCamera != null)
        {
            controlledCamera.orthographic = true;
        }
    }

    private void OnEnable()
    {
        if (inputProvider != null)
        {
            inputProvider.RightDragged += HandleRightDrag;
            inputProvider.Scrolled += HandleScroll;
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.RightDragged -= HandleRightDrag;
            inputProvider.Scrolled -= HandleScroll;
        }
    }

    private void Update()
    {
        HandleKeyboardMovement();
    }

    private void HandleKeyboardMovement()
    {
        if (inputProvider == null)
        {
            return;
        }

        Vector2 movementInput = inputProvider.Movement;
        if (movementInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 movement = new Vector3(movementInput.x, movementInput.y, 0f);
        transform.position += movement * moveSpeed * Time.deltaTime;
        ClampToBounds();
    }

    private void HandleRightDrag(Vector2 screenDelta)
    {
        if (!enableRightClickDrag || controlledCamera == null)
        {
            return;
        }

        float screenHeight = Mathf.Max(1f, Screen.height);
        float worldUnitsPerPixel = (controlledCamera.orthographicSize * 2f) / screenHeight;
        Vector2 worldDelta = screenDelta * worldUnitsPerPixel * dragSensitivity;

        transform.position -= new Vector3(worldDelta.x, worldDelta.y, 0f);
        ClampToBounds();
    }

    private void HandleScroll(float scrollValue)
    {
        if (!enableZoom || controlledCamera == null || !controlledCamera.orthographic)
        {
            return;
        }

        controlledCamera.orthographicSize -= scrollValue * zoomSpeed;
        controlledCamera.orthographicSize = Mathf.Clamp(controlledCamera.orthographicSize, minOrthographicSize, maxOrthographicSize);
    }

    private void ClampToBounds()
    {
        if (!useBoundaries)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        transform.position = position;
    }
}
