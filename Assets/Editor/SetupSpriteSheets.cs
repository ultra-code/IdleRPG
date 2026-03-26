using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupSpriteSheets
{
    [MenuItem("Tools/Setup Sprite Sheets")]
    public static void Execute()
    {
        // 1. Slice both sprite sheets
        SliceSheet("Assets/Sprites/Effects/Staff_Rush.png", "Staff_Rush");
        SliceSheet("Assets/Sprites/Effects/Meteor_Rain.png", "Meteor_Rain");
        AssetDatabase.Refresh();

        // 2. Create additive material
        CreateAdditiveMaterial();
        AssetDatabase.Refresh();

        // 3. Link to skill components
        LinkAll();

        // 4. Save
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupSheets] All done.");
    }

    // ── Slice ────────────────────────────────────────────

    static void SliceSheet(string path, string baseName)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogError("[SetupSheets] Importer missing: " + path); return; }

        // Need readable texture to get dimensions
        imp.isReadable = true;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.spritePixelsPerUnit = 128;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.maxTextureSize = 512;

        // First pass: import to get real dimensions
        imp.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogError("[SetupSheets] Texture load failed: " + path); return; }

        int texW = tex.width;
        int texH = tex.height;
        int fW = texW / 2;
        int fH = texH / 2;

        Debug.Log($"[SetupSheets] {baseName}: {texW}x{texH} -> frames {fW}x{fH}");

        // Build 2x2 grid SpriteMetaData (reading order: top-left, top-right, bottom-left, bottom-right)
        var metas = new SpriteMetaData[4];
        for (int i = 0; i < 4; i++)
        {
            int col = i % 2;
            int row = 1 - (i / 2); // i=0,1 → top row (y=fH), i=2,3 → bottom row (y=0)
            metas[i] = new SpriteMetaData
            {
                name = baseName + "_" + i,
                rect = new Rect(col * fW, row * fH, fW, fH),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
        }

#pragma warning disable 0618
        imp.spritesheet = metas;
#pragma warning restore 0618

        imp.SaveAndReimport();
        Debug.Log($"[SetupSheets] Sliced {baseName} into 4 frames.");
    }

    // ── Material ─────────────────────────────────────────

    static void CreateAdditiveMaterial()
    {
        string matPath = "Assets/Materials/Effects/SkillVFX_Additive.mat";

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Materials/Effects"))
            AssetDatabase.CreateFolder("Assets/Materials", "Effects");

        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null)
        {
            Debug.Log("[SetupSheets] Material already exists: " + matPath);
            return;
        }

        var shader = Shader.Find("Sprites/Default");
        if (shader == null) { Debug.LogError("[SetupSheets] Sprites/Default shader not found"); return; }

        var mat = new Material(shader);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.One);

        AssetDatabase.CreateAsset(mat, matPath);
        Debug.Log("[SetupSheets] Created additive material: " + matPath);
    }

    // ── Link ─────────────────────────────────────────────

    static void LinkAll()
    {
        var staffSprites = LoadSubSprites("Assets/Sprites/Effects/Staff_Rush.png", "Staff_Rush");
        var meteorSprites = LoadSubSprites("Assets/Sprites/Effects/Meteor_Rain.png", "Meteor_Rain");
        var additiveMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Materials/Effects/SkillVFX_Additive.mat");

        int ok = 0, fail = 0;

        // FanSkill
        var fan = Object.FindAnyObjectByType<FanSkill>();
        if (fan != null)
        {
            var so = new SerializedObject(fan);
            ok += SetSpriteArray(so, "staffSpriteFrames", staffSprites, "FanSkill");
            ok += SetRef(so, "additiveMaterial", additiveMat, "FanSkill");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(fan);
        }
        else { Debug.LogWarning("[SetupSheets] FanSkill not found in scene"); fail += 2; }

        // MeteorSkill
        var meteor = Object.FindAnyObjectByType<MeteorSkill>();
        if (meteor != null)
        {
            var so = new SerializedObject(meteor);
            ok += SetSpriteArray(so, "meteorSpriteFrames", meteorSprites, "MeteorSkill");
            ok += SetRef(so, "additiveMaterial", additiveMat, "MeteorSkill");
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(meteor);
        }
        else { Debug.LogWarning("[SetupSheets] MeteorSkill not found in scene"); fail += 2; }

        Debug.Log($"[SetupSheets] Link results — OK: {ok}, Fail: {fail}");
    }

    static Sprite[] LoadSubSprites(string path, string baseName)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        var sprites = new List<Sprite>();
        foreach (var obj in all)
        {
            if (obj is Sprite s && s.name.StartsWith(baseName))
                sprites.Add(s);
        }
        sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        Debug.Log($"[SetupSheets] Loaded {sprites.Count} sub-sprites from {baseName}");
        return sprites.ToArray();
    }

    static int SetSpriteArray(SerializedObject so, string propName, Sprite[] sprites, string owner)
    {
        var prop = so.FindProperty(propName);
        if (prop == null)
        {
            Debug.LogError($"[SetupSheets] Property '{propName}' not found on {owner}");
            return 0;
        }
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError($"[SetupSheets] No sprites to assign to {owner}.{propName}");
            return 0;
        }

        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

        Debug.Log($"[SetupSheets] {owner}.{propName} <- {sprites.Length} frames");
        return 1;
    }

    static int SetRef(SerializedObject so, string propName, Object asset, string owner)
    {
        var prop = so.FindProperty(propName);
        if (prop == null)
        {
            Debug.LogError($"[SetupSheets] Property '{propName}' not found on {owner}");
            return 0;
        }
        if (asset == null)
        {
            Debug.LogError($"[SetupSheets] Asset null for {owner}.{propName}");
            return 0;
        }

        prop.objectReferenceValue = asset;
        Debug.Log($"[SetupSheets] {owner}.{propName} <- {asset.name}");
        return 1;
    }
}
