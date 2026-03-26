using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpriteImportUtility
{
    private const string RootFolder = "Assets/Sprites";
    private const float PixelsPerUnit = 128f;

    [MenuItem("Tools/IdleRPG/Configure Imported Sprite Sheets")]
    public static void ConfigureImportedSpriteSheets()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { RootFolder });
        int configured = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png"))
            {
                continue;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                continue;
            }

            bool changed = ApplyCommonImportSettings(importer);
            changed |= ConfigureSpriteMode(importer, texture, path);

            if (changed)
            {
                importer.SaveAndReimport();
            }

            configured++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SpriteImportUtility] Configured {configured} sprite textures under {RootFolder}.");
    }

    private static bool ApplyCommonImportSettings(TextureImporter importer)
    {
        bool changed = false;

        changed |= SetIfDifferent(() => importer.textureType, value => importer.textureType = value, TextureImporterType.Sprite);
        changed |= SetIfDifferent(() => importer.filterMode, value => importer.filterMode = value, FilterMode.Point);
        changed |= SetIfDifferent(() => importer.textureCompression, value => importer.textureCompression = value, TextureImporterCompression.Uncompressed);
        changed |= SetIfDifferent(() => importer.mipmapEnabled, value => importer.mipmapEnabled = value, false);
        changed |= SetIfDifferent(() => importer.alphaIsTransparency, value => importer.alphaIsTransparency = value, true);
        changed |= SetIfDifferent(() => importer.spritePixelsPerUnit, value => importer.spritePixelsPerUnit = value, PixelsPerUnit);
        changed |= SetIfDifferent(() => importer.wrapMode, value => importer.wrapMode = value, TextureWrapMode.Clamp);

        return changed;
    }

    private static bool ConfigureSpriteMode(TextureImporter importer, Texture2D texture, string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        bool shouldSlice2x2 = fileName.EndsWith("_sheet") && texture.width > 128 && texture.height > 128;

        if (!shouldSlice2x2)
        {
            bool changed = false;
            changed |= SetIfDifferent(() => importer.spriteImportMode, value => importer.spriteImportMode = value, SpriteImportMode.Single);

            if (importer.spritesheet != null && importer.spritesheet.Length > 0)
            {
                importer.spritesheet = new SpriteMetaData[0];
                changed = true;
            }

            return changed;
        }

        int cellWidth = texture.width / 2;
        int cellHeight = texture.height / 2;
        SpriteMetaData[] desiredSheet = Build2x2Sheet(fileName, texture.width, texture.height, cellWidth, cellHeight);

        bool importerChanged = false;
        importerChanged |= SetIfDifferent(() => importer.spriteImportMode, value => importer.spriteImportMode = value, SpriteImportMode.Multiple);

        if (!SpriteSheetMatches(importer.spritesheet, desiredSheet))
        {
            importer.spritesheet = desiredSheet;
            importerChanged = true;
        }

        return importerChanged;
    }

    private static SpriteMetaData[] Build2x2Sheet(string baseName, int textureWidth, int textureHeight, int cellWidth, int cellHeight)
    {
        List<SpriteMetaData> sprites = new List<SpriteMetaData>(4);
        int index = 0;

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                sprites.Add(new SpriteMetaData
                {
                    name = $"{baseName}_{index}",
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    rect = new Rect(col * cellWidth, textureHeight - ((row + 1) * cellHeight), cellWidth, cellHeight)
                });
                index++;
            }
        }

        return sprites.ToArray();
    }

    private static bool SpriteSheetMatches(SpriteMetaData[] current, SpriteMetaData[] desired)
    {
        if (current == null || current.Length != desired.Length)
        {
            return false;
        }

        for (int i = 0; i < desired.Length; i++)
        {
            if (current[i].name != desired[i].name)
            {
                return false;
            }

            if (current[i].rect != desired[i].rect)
            {
                return false;
            }
        }

        return true;
    }

    private static bool SetIfDifferent<T>(System.Func<T> getter, System.Action<T> setter, T desired)
    {
        if (EqualityComparer<T>.Default.Equals(getter(), desired))
        {
            return false;
        }

        setter(desired);
        return true;
    }
}
