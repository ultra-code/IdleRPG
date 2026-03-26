using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public static class SetupTilemap
{
    [MenuItem("Tools/Setup Dungeon Tilemap")]
    public static void Execute()
    {
        // --- 1. Ensure DungeonFloor tile asset exists with correct sprite ---
        const string tilePath = "Assets/Tiles/DungeonFloor.asset";
        const string spritePath = "Assets/Sprites/Backgrounds/tile.png";

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError("SetupTilemap: sprite not found at " + spritePath);
            return;
        }

        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
                AssetDatabase.CreateFolder("Assets", "Tiles");
            AssetDatabase.CreateAsset(tile, tilePath);
        }
        tile.sprite = sprite;
        tile.color = Color.white;
        tile.colliderType = Tile.ColliderType.None;
        EditorUtility.SetDirty(tile);
        AssetDatabase.SaveAssets();
        Debug.Log("SetupTilemap: DungeonFloor tile ready");

        // --- 2. Create tile palette asset (prefab with Grid+Tilemap) ---
        const string palettePath = "Assets/Tiles/DungeonPalette.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(palettePath) == null)
        {
            var paletteGo = new GameObject("DungeonPalette");
            var grid = paletteGo.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            var tmGo = new GameObject("Layer1");
            tmGo.transform.SetParent(paletteGo.transform, false);
            tmGo.AddComponent<Tilemap>();
            tmGo.AddComponent<TilemapRenderer>();

            PrefabUtility.SaveAsPrefabAsset(paletteGo, palettePath);
            Object.DestroyImmediate(paletteGo);
            Debug.Log("SetupTilemap: DungeonPalette palette created");
        }

        // --- 3. Create Grid > Tilemap in scene and paint 30x17 ---
        const string gridName = "DungeonGrid";
        var existing = GameObject.Find(gridName);
        if (existing != null)
        {
            Debug.Log("SetupTilemap: removing existing " + gridName);
            Undo.DestroyObjectImmediate(existing);
        }

        var gridObj = new GameObject(gridName);
        Undo.RegisterCreatedObjectUndo(gridObj, "Create DungeonGrid");
        var sceneGrid = gridObj.AddComponent<Grid>();
        sceneGrid.cellSize = Vector3.one;

        var tilemapObj = new GameObject("Floor");
        tilemapObj.transform.SetParent(gridObj.transform, false);
        var tilemap = tilemapObj.AddComponent<Tilemap>();
        var renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = -10;

        // Paint 30x17 floor (centered: x -15..14, y -8..8)
        int width = 30;
        int height = 17;
        int startX = -width / 2;
        int startY = -height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tilemap.SetTile(new Vector3Int(startX + x, startY + y, 0), tile);
            }
        }

        // Position grid at z=0
        gridObj.transform.position = Vector3.zero;

        EditorUtility.SetDirty(tilemap);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"SetupTilemap: painted {width}x{height} floor ({startX},{startY}) to ({startX + width - 1},{startY + height - 1})");
    }
}
