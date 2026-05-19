using UnityEngine;

public class IsometricTileGridSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private Transform tileParent;

    [Header("Tiles")]
    [SerializeField] private GameObject defaultTilePrefab;
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 12;

    [Header("Spawn")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearExistingChildrenBeforeSpawn = true;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnGrid();
        }
    }

    public void SpawnGrid()
    {
        if (gridManager == null || defaultTilePrefab == null)
        {
            Debug.LogWarning("Cannot spawn isometric tile grid. Grid manager or tile prefab is missing.");
            return;
        }

        if (tileParent == null)
        {
            tileParent = transform;
        }

        if (clearExistingChildrenBeforeSpawn)
        {
            ClearChildren();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);
                GameObject tileObject = Instantiate(defaultTilePrefab, gridManager.GridToWorld(gridPosition), Quaternion.identity, tileParent);
                IsometricTile tile = tileObject.GetComponent<IsometricTile>();

                if (tile == null)
                {
                    tile = tileObject.AddComponent<IsometricTile>();
                }

                tile.Initialize(gridPosition, gridManager);
            }
        }
    }

    private void ClearChildren()
    {
        for (int i = tileParent.childCount - 1; i >= 0; i--)
        {
            Transform child = tileParent.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
