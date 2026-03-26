using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AutoLinkEffectPrefabs
{
    private struct LinkInfo
    {
        public string prefabPath;
        public string propertyName;
        public string componentName;
    }

    [MenuItem("Tools/Link Effect Prefabs")]
    public static void LinkAll()
    {
        int success = 0;
        int alreadyLinked = 0;
        int failed = 0;

        // --- MeteorSkill ---
        var meteor = Object.FindAnyObjectByType<MeteorSkill>();
        if (meteor != null)
        {
            success += LinkPrefab(meteor, "meteorExplosionFXPrefab",
                "Assets/Prefabs/Effects/MeteorExplosionFX.prefab",
                ref alreadyLinked, ref failed);
            success += LinkPrefab(meteor, "warningCircleFXPrefab",
                "Assets/Prefabs/Effects/WarningCircleFX.prefab",
                ref alreadyLinked, ref failed);
        }
        else
        {
            Debug.LogWarning("[AutoLink] MeteorSkill not found in scene!");
            failed += 2;
        }

        // --- ChainSkill ---
        var chain = Object.FindAnyObjectByType<ChainSkill>();
        if (chain != null)
        {
            success += LinkPrefab(chain, "chainSparkFXPrefab",
                "Assets/Prefabs/Effects/ChainSparkFX.prefab",
                ref alreadyLinked, ref failed);
        }
        else
        {
            Debug.LogWarning("[AutoLink] ChainSkill not found in scene!");
            failed += 1;
        }

        // Mark scene dirty
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log($"[AutoLink] Complete — Success: {success}, Already linked: {alreadyLinked}, Failed: {failed}");
    }

    private static int LinkPrefab(Component target, string propertyName, string prefabPath,
        ref int alreadyLinked, ref int failed)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(propertyName);

        if (prop == null)
        {
            Debug.LogError($"[AutoLink] Property '{propertyName}' not found on {target.GetType().Name}");
            failed++;
            return 0;
        }

        // Already linked check
        if (prop.objectReferenceValue != null)
        {
            Debug.Log($"[AutoLink] {target.GetType().Name}.{propertyName} — already linked to '{prop.objectReferenceValue.name}'");
            alreadyLinked++;
            return 0;
        }

        // Load prefab
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[AutoLink] Prefab not found at '{prefabPath}'");
            failed++;
            return 0;
        }

        prop.objectReferenceValue = prefab;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);

        Debug.Log($"[AutoLink] OK — {target.GetType().Name}.{propertyName} ← {prefab.name}");
        return 1;
    }
}
