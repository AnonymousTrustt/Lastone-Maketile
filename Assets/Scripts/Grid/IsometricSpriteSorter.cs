using UnityEngine;

public class IsometricSpriteSorter : MonoBehaviour
{
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private int sortingOrderOffset;
    [SerializeField] private bool updateEveryFrame;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }
    }

    private void Start()
    {
        UpdateSorting();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
        {
            UpdateSorting();
        }
    }

    public void UpdateSorting()
    {
        if (gridManager == null)
        {
            return;
        }

        Vector2Int gridPosition = gridManager.WorldToGrid(transform.position);
        int sortingOrder = gridManager.GetSortingOrderForGridPosition(gridPosition, sortingOrderOffset);
        SpriteSortingUtility.ApplySortingOrder(gameObject, sortingOrder);
    }
}
