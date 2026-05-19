using UnityEngine;

public class IsometricHoverTilePreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private Camera mainCamera;

    [Header("Preview")]
    [SerializeField] private GameObject hoverTileObject;
    [SerializeField] private int sortingOrderOffset = 6000;

    private void Awake()
    {
        if (inputProvider == null)
        {
            inputProvider = FindFirstObjectByType<InputProvider>();
        }

        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (hoverTileObject == null || inputProvider == null || gridManager == null || mainCamera == null)
        {
            return;
        }

        Vector2 mouseWorld = GetMouseWorldPosition(inputProvider.MousePosition);
        Vector2Int gridPosition = gridManager.WorldToGrid(mouseWorld);

        hoverTileObject.transform.position = gridManager.GridToWorld(gridPosition);
        int sortingOrder = gridManager.GetSortingOrderForGridPosition(gridPosition, sortingOrderOffset);
        SpriteSortingUtility.ApplySortingOrder(hoverTileObject, sortingOrder);
    }

    private Vector2 GetMouseWorldPosition(Vector2 mousePosition)
    {
        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - gridManager.BaseZ);
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, cameraDepth);
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(worldPoint.x, worldPoint.y);
    }
}
