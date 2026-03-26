using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupBloom
{
    private const string ProfilePath = "Assets/Settings/BloomProfile.asset";

    [MenuItem("Tools/Setup Bloom")]
    public static void Execute()
    {
        int steps = 0;

        // ---- Step 1: URP Pipeline Asset — enable post-processing ----
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null)
        {
            Debug.LogError("[Bloom] No URP pipeline asset found in Graphics Settings.");
            return;
        }
        Debug.Log("[Bloom] URP Pipeline Asset: " + pipeline.name);

        // URP post-processing is on by default at the pipeline level,
        // but we ensure the renderer data has it enabled via the camera (step 2).
        steps++;

        // ---- Step 2: Main Camera — renderPostProcessing = true ----
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[Bloom] No Main Camera found in scene.");
            return;
        }

        var camData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null)
            camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

        if (camData.renderPostProcessing)
        {
            Debug.Log("[Bloom] Camera post-processing already enabled.");
        }
        else
        {
            camData.renderPostProcessing = true;
            EditorUtility.SetDirty(camData);
            Debug.Log("[Bloom] Camera post-processing ENABLED.");
        }
        steps++;

        // ---- Step 3: Create Volume Profile asset ----
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            AssetDatabase.CreateFolder("Assets", "Settings");

        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            Debug.Log("[Bloom] Created VolumeProfile at " + ProfilePath);
        }
        else
        {
            Debug.Log("[Bloom] VolumeProfile already exists at " + ProfilePath);
        }

        // ---- Add or update Bloom override ----
        Bloom bloom;
        if (profile.TryGet<Bloom>(out bloom))
        {
            Debug.Log("[Bloom] Bloom override already exists, updating values.");
        }
        else
        {
            bloom = profile.Add<Bloom>(true);
            Debug.Log("[Bloom] Added Bloom override to profile.");
        }

        bloom.active = true;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.8f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 1.5f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.6f;
        bloom.tint.overrideState = true;
        bloom.tint.value = new Color(255f / 255f, 240f / 255f, 230f / 255f, 1f);
        bloom.highQualityFiltering.overrideState = true;
        bloom.highQualityFiltering.value = true;

        EditorUtility.SetDirty(profile);
        steps++;

        // ---- Step 4: Create Global Volume in scene ----
        var existingVol = GameObject.Find("PostProcessVolume");
        Volume volume;
        if (existingVol != null)
        {
            volume = existingVol.GetComponent<Volume>();
            if (volume == null)
                volume = existingVol.AddComponent<Volume>();
            Debug.Log("[Bloom] PostProcessVolume already exists in scene, reusing.");
        }
        else
        {
            var volGo = new GameObject("PostProcessVolume");
            volume = volGo.AddComponent<Volume>();
            Debug.Log("[Bloom] Created PostProcessVolume GameObject.");
        }

        volume.isGlobal = true;
        volume.profile = profile;
        EditorUtility.SetDirty(volume);
        EditorUtility.SetDirty(volume.gameObject);
        steps++;

        // ---- Step 5: Mark scene dirty ----
        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        AssetDatabase.SaveAssets();
        Debug.Log("[Bloom] Setup complete (" + steps + " steps). All saved.");
    }
}
