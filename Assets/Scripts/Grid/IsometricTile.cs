using UnityEngine;

public class IsometricTile : MonoBehaviour
{
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private int sortingOrderOffset = -5;

    public Vector2Int GridPosition => gridPosition;

    public void Initialize(Vector2Int position, IsometricGridManager gridManager)
    {
        gridPosition = position;
        Apply2DPositionAndSorting(gridManager);
    }

    public void Apply2DPositionAndSorting(IsometricGridManager gridManager)
    {
        if (gridManager == null)
        {
            return;
        }

        transform.position = gridManager.GridToWorld(gridPosition);
        int sortingOrder = gridManager.GetSortingOrderForGridPosition(gridPosition, sortingOrderOffset);
        SpriteSortingUtility.ApplySortingOrder(gameObject, sortingOrder);
    }
}
