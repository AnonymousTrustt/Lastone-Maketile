using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldGenerationManager : MonoBehaviour
{
    [Serializable]
    private class WorldAssetGroup
    {
        [SerializeField] private TileBase[] tiles;
        [SerializeField] private Sprite[] sprites;

        public bool HasAssets => (tiles != null && tiles.Length > 0) || (sprites != null && sprites.Length > 0);

        public Sprite PickSprite(System.Random random)
        {
            int tileCount = tiles != null ? tiles.Length : 0;
            int spriteCount = sprites != null ? sprites.Length : 0;
            int totalCount = tileCount + spriteCount;

            if (totalCount == 0)
            {
                return null;
            }

            int index = random.Next(0, totalCount);

            if (index < tileCount)
            {
                return GetSpriteFromTile(tiles[index]);
            }

            return sprites[index - tileCount];
        }

        public bool RemoveNulls()
        {
            bool changed = false;

            if (tiles != null)
            {
                int count = 0;

                for (int i = 0; i < tiles.Length; i++)
                {
                    if (tiles[i] != null)
                    {
                        count++;
                    }
                }

                if (count != tiles.Length)
                {
                    TileBase[] compactedTiles = new TileBase[count];
                    int writeIndex = 0;

                    for (int i = 0; i < tiles.Length; i++)
                    {
                        if (tiles[i] != null)
                        {
                            compactedTiles[writeIndex] = tiles[i];
                            writeIndex++;
                        }
                    }

                    tiles = compactedTiles;
                    changed = true;
                }
            }

            if (sprites != null)
            {
                int count = 0;

                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i] != null)
                    {
                        count++;
                    }
                }

                if (count != sprites.Length)
                {
                    Sprite[] compactedSprites = new Sprite[count];
                    int writeIndex = 0;

                    for (int i = 0; i < sprites.Length; i++)
                    {
                        if (sprites[i] != null)
                        {
                            compactedSprites[writeIndex] = sprites[i];
                            writeIndex++;
                        }
                    }

                    sprites = compactedSprites;
                    changed = true;
                }
            }

            return changed;
        }

        private static Sprite GetSpriteFromTile(TileBase tile)
        {
            if (tile is Tile standardTile)
            {
                return standardTile.sprite;
            }

            return null;
        }

#if UNITY_EDITOR
        public void SetTiles(TileBase[] newTiles)
        {
            tiles = newTiles;
        }

        public void SetSprites(Sprite[] newSprites)
        {
            sprites = newSprites;
        }
#endif
    }

    private enum TerrainKind
    {
        Grass,
        Sand,
        Water
    }

    [Header("References")]
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private Transform generatedWorldParent;

    [Header("Click Generation")]
    [SerializeField] private bool generateOnStart;
    [SerializeField] private bool generateOnLeftClick;
    [SerializeField] private bool ignoreClicksOverUI = true;

    [Header("World Size")]
    [SerializeField] private Vector2Int originCell = Vector2Int.zero;
    [SerializeField] private int width = 24;
    [SerializeField] private int height = 24;

    [Header("Terrain Shape")]
    [SerializeField] private bool randomizeSeed = true;
    [SerializeField] private int seed = 12345;
    [SerializeField, Range(0.03f, 0.3f)] private float terrainNoiseScale = 0.09f;
    [SerializeField, Range(0f, 1f)] private float waterLevel = 0.26f;
    [SerializeField, Range(0f, 0.4f)] private float sandBandSize = 0.13f;

    [Header("Terrain Assets")]
    [SerializeField] private WorldAssetGroup grassAssets = new WorldAssetGroup();
    [SerializeField] private WorldAssetGroup sandAssets = new WorldAssetGroup();
    [SerializeField] private WorldAssetGroup waterAssets = new WorldAssetGroup();

    [Header("Decor Assets")]
    [SerializeField] private WorldAssetGroup treeAssets = new WorldAssetGroup();
    [SerializeField] private WorldAssetGroup flowerAssets = new WorldAssetGroup();
    [SerializeField, Range(0f, 1f)] private float treeChance = 0.08f;
    [SerializeField, Range(0f, 1f)] private float flowerChance = 0.12f;
    [SerializeField] private Vector2 decorOffsetRange = new Vector2(0.05f, 0.08f);
    [SerializeField] private Vector2 decorScaleRange = new Vector2(0.9f, 1.1f);

    [Header("Sorting")]
    [SerializeField] private int terrainSortingOrderOffset = -20;
    [SerializeField] private int decorSortingOrderOffset = 20;

    private const string GeneratedRootName = "Generated World";
    private Transform generatedRoot;

    private void Awake()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }

        if (inputProvider == null)
        {
            inputProvider = FindFirstObjectByType<InputProvider>();
        }
    }

    private void OnEnable()
    {
        if (inputProvider != null)
        {
            inputProvider.LeftClicked += HandleLeftClick;
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.LeftClicked -= HandleLeftClick;
        }
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }

    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        ResolveReferences();

        if (gridManager == null)
        {
            Debug.LogWarning("Cannot generate world. IsometricGridManager is missing.");
            return;
        }

        if (!grassAssets.HasAssets || !sandAssets.HasAssets || !waterAssets.HasAssets)
        {
            Debug.LogWarning("World generation needs at least one grass, sand, and water asset assigned.");
            return;
        }

        EnsureGeneratedRoot();
        ClearGeneratedWorld();

        int activeSeed = randomizeSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
        System.Random random = new System.Random(activeSeed);
        Vector2 noiseOffset = new Vector2(
            Mathf.Abs(activeSeed % 10000) * 0.11f,
            Mathf.Abs((activeSeed / 10000) % 10000) * 0.13f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPosition = originCell + new Vector2Int(x, y);
                TerrainKind terrainKind = PickTerrain(x, y, noiseOffset);
                SpawnTerrainTile(gridPosition, terrainKind, random);

                if (terrainKind == TerrainKind.Grass)
                {
                    TrySpawnDecor(gridPosition, random);
                }
            }
        }
    }

    [ContextMenu("Clear Generated World")]
    public void ClearGeneratedWorld()
    {
        EnsureGeneratedRoot();

        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = generatedRoot.GetChild(i);

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

    private void HandleLeftClick(Vector2 mousePosition)
    {
        if (!generateOnLeftClick)
        {
            return;
        }

        if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        GenerateWorld();
    }

    private TerrainKind PickTerrain(int x, int y, Vector2 noiseOffset)
    {
        float noise = Mathf.PerlinNoise(
            noiseOffset.x + x * terrainNoiseScale,
            noiseOffset.y + y * terrainNoiseScale);

        if (noise <= waterLevel)
        {
            return TerrainKind.Water;
        }

        if (noise <= waterLevel + sandBandSize)
        {
            return TerrainKind.Sand;
        }

        return TerrainKind.Grass;
    }

    private void SpawnTerrainTile(Vector2Int gridPosition, TerrainKind terrainKind, System.Random random)
    {
        WorldAssetGroup group;

        switch (terrainKind)
        {
            case TerrainKind.Water:
                group = waterAssets;
                break;
            case TerrainKind.Sand:
                group = sandAssets;
                break;
            default:
                group = grassAssets;
                break;
        }

        Sprite sprite = group.PickSprite(random);
        SpawnSpriteObject($"{terrainKind} {gridPosition.x},{gridPosition.y}", sprite, gridPosition, terrainSortingOrderOffset, Vector2.zero, Vector3.one);
    }

    private void TrySpawnDecor(Vector2Int gridPosition, System.Random random)
    {
        if (treeAssets.HasAssets && random.NextDouble() <= treeChance)
        {
            SpawnDecorObject("Tree", treeAssets, gridPosition, random);
            return;
        }

        if (flowerAssets.HasAssets && random.NextDouble() <= flowerChance)
        {
            SpawnDecorObject("Flowers", flowerAssets, gridPosition, random);
        }
    }

    private void SpawnDecorObject(string objectName, WorldAssetGroup assetGroup, Vector2Int gridPosition, System.Random random)
    {
        Sprite sprite = assetGroup.PickSprite(random);
        Vector2 offset = new Vector2(
            RandomRange(random, -decorOffsetRange.x, decorOffsetRange.x),
            RandomRange(random, -decorOffsetRange.y, decorOffsetRange.y));
        float scale = RandomRange(random, decorScaleRange.x, decorScaleRange.y);
        Vector3 localScale = new Vector3(scale, scale, 1f);

        GameObject decorObject = SpawnSpriteObject(
            $"{objectName} {gridPosition.x},{gridPosition.y}",
            sprite,
            gridPosition,
            decorSortingOrderOffset,
            offset,
            localScale);

        if (decorObject != null && random.NextDouble() > 0.5d)
        {
            Vector3 flippedScale = decorObject.transform.localScale;
            flippedScale.x *= -1f;
            decorObject.transform.localScale = flippedScale;
        }
    }

    private GameObject SpawnSpriteObject(string objectName, Sprite sprite, Vector2Int gridPosition, int sortingOffset, Vector2 offset, Vector3 localScale)
    {
        if (sprite == null)
        {
            return null;
        }

        Vector3 worldPosition = gridManager.GridToWorld(gridPosition);
        worldPosition.x += offset.x;
        worldPosition.y += offset.y;

        GameObject spawnedObject = new GameObject(objectName);
        spawnedObject.transform.SetParent(generatedRoot, false);
        spawnedObject.transform.position = worldPosition;
        spawnedObject.transform.localScale = localScale;

        SpriteRenderer spriteRenderer = spawnedObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = gridManager.GetSortingOrderForGridPosition(gridPosition, sortingOffset);

        return spawnedObject;
    }

    private void EnsureGeneratedRoot()
    {
        if (generatedRoot != null)
        {
            return;
        }

        Transform parent = generatedWorldParent != null ? generatedWorldParent : transform;
        Transform existingRoot = parent.Find(GeneratedRootName);

        if (existingRoot != null)
        {
            generatedRoot = existingRoot;
            return;
        }

        GameObject rootObject = new GameObject(GeneratedRootName);
        rootObject.transform.SetParent(parent, false);
        generatedRoot = rootObject.transform;
    }

    private static float RandomRange(System.Random random, float min, float max)
    {
        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        terrainNoiseScale = Mathf.Max(0.001f, terrainNoiseScale);
        decorScaleRange.x = Mathf.Max(0.01f, decorScaleRange.x);
        decorScaleRange.y = Mathf.Max(decorScaleRange.x, decorScaleRange.y);
        decorOffsetRange.x = Mathf.Max(0f, decorOffsetRange.x);
        decorOffsetRange.y = Mathf.Max(0f, decorOffsetRange.y);

        bool changed = false;
        changed |= grassAssets.RemoveNulls();
        changed |= sandAssets.RemoveNulls();
        changed |= waterAssets.RemoveNulls();
        changed |= treeAssets.RemoveNulls();
        changed |= flowerAssets.RemoveNulls();

#if UNITY_EDITOR
        if (changed)
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Fill Project World Assets")]
    public void AutoFillProjectWorldAssets()
    {
        grassAssets.SetTiles(LoadTiles(
            "Assets/Sprites/Ground Tileset 2_0.asset",
            "Assets/Sprites/Ground Tileset 2_1.asset"));
        sandAssets.SetTiles(LoadTiles(
            "Assets/Sprites/Tiles/Sand Tileset_0.asset"));
        waterAssets.SetTiles(LoadTiles(
            "Assets/Sprites/Tiles/City 17 Water Tileset.asset"));
        treeAssets.SetTiles(LoadTiles(
            "Assets/Sprites/Tiles/Tree_1.asset",
            "Assets/Sprites/Tree 2 from Shivang Pattanayak_0.asset"));
        flowerAssets.SetTiles(LoadTiles(
            "Assets/Sprites/Flowers from Shivang_0.asset",
            "Assets/Sprites/Flowers from Shivang (1)_0.asset",
            "Assets/Sprites/Flowers from Shivang (2)_0.asset"));

        grassAssets.SetSprites(Array.Empty<Sprite>());
        sandAssets.SetSprites(Array.Empty<Sprite>());
        waterAssets.SetSprites(Array.Empty<Sprite>());
        treeAssets.SetSprites(Array.Empty<Sprite>());
        flowerAssets.SetSprites(Array.Empty<Sprite>());

        EditorUtility.SetDirty(this);
    }

    private static TileBase[] LoadTiles(params string[] assetPaths)
    {
        TileBase[] loadedTiles = new TileBase[assetPaths.Length];

        for (int i = 0; i < assetPaths.Length; i++)
        {
            loadedTiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>(assetPaths[i]);
        }

        return loadedTiles;
    }
#endif
}
