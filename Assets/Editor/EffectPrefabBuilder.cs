using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public static class EffectPrefabBuilder
{
    private const string PrefabDir = "Assets/Prefabs/Effects";

    [MenuItem("Tools/Build Effect Prefabs")]
    public static void BuildAll()
    {
        if (!AssetDatabase.IsValidFolder(PrefabDir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "Effects");
        }

        BuildMeteorExplosionFX();
        BuildChainSparkFX();
        BuildWarningCircleFX();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("EffectPrefabBuilder: All 3 prefabs created in " + PrefabDir);
    }

    // ========================================================
    // 1. MeteorExplosionFX
    // ========================================================
    private static void BuildMeteorExplosionFX()
    {
        GameObject root = new GameObject("MeteorExplosionFX");

        // --- Main particles (fire fragments) ---
        var mainPS = root.AddComponent<ParticleSystem>();
        var mainModule = mainPS.main;
        mainModule.duration = 0.5f;
        mainModule.loop = false;
        mainModule.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        mainModule.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.38f, 0.19f, 1f),   // #FF6030
            new Color(1f, 0.27f, 0f, 1f));       // #FF4500
        mainModule.gravityModifier = 1.5f;
        mainModule.maxParticles = 30;
        mainModule.stopAction = ParticleSystemStopAction.Destroy;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = mainPS.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        var shape = mainPS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        renderer.sortingOrder = 26;

        // --- Sub particles (shockwave ring) ---
        GameObject subObj = new GameObject("ShockwaveRing");
        subObj.transform.SetParent(root.transform, false);
        var subPS = subObj.AddComponent<ParticleSystem>();

        var subMain = subPS.main;
        subMain.duration = 0.3f;
        subMain.loop = false;
        subMain.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        subMain.startSpeed = new ParticleSystem.MinMaxCurve(4f, 7f);
        subMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
        subMain.startColor = new Color(1f, 0.88f, 0.5f, 1f); // #FFE080
        subMain.maxParticles = 20;
        subMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var subEmission = subPS.emission;
        subEmission.rateOverTime = 0f;
        subEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var subShape = subPS.shape;
        subShape.shapeType = ParticleSystemShapeType.Circle;
        subShape.radius = 0f;

        var subRenderer = subObj.GetComponent<ParticleSystemRenderer>();
        subRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        subRenderer.sortingOrder = 27;

        // --- Light2D ---
        var light = root.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = new Color(1f, 0.38f, 0.19f, 1f);
        light.intensity = 3.0f;
        light.pointLightOuterRadius = 4.0f;
        light.pointLightInnerRadius = 0.5f;

        var fade = root.AddComponent<FXLightFade>();
        fade.duration = 0.3f;

        // Save prefab
        string path = PrefabDir + "/MeteorExplosionFX.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log("Created: " + path);
    }

    // ========================================================
    // 2. ChainSparkFX
    // ========================================================
    private static void BuildChainSparkFX()
    {
        GameObject root = new GameObject("ChainSparkFX");

        // --- Main particles (electric fragments) ---
        var mainPS = root.AddComponent<ParticleSystem>();
        var mainModule = mainPS.main;
        mainModule.duration = 0.3f;
        mainModule.loop = false;
        mainModule.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        mainModule.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.19f, 0.63f, 1f, 1f),    // #30A0FF
            new Color(0.5f, 0.88f, 1f, 1f));     // #80E0FF
        mainModule.maxParticles = 25;
        mainModule.stopAction = ParticleSystemStopAction.Destroy;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = mainPS.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

        var shape = mainPS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        renderer.sortingOrder = 32;

        // --- Center flash ---
        GameObject flashObj = new GameObject("CenterFlash");
        flashObj.transform.SetParent(root.transform, false);
        var flashPS = flashObj.AddComponent<ParticleSystem>();

        var flashMain = flashPS.main;
        flashMain.duration = 0.1f;
        flashMain.loop = false;
        flashMain.startLifetime = 0.1f;
        flashMain.startSpeed = 0f;
        flashMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        flashMain.startColor = Color.white;
        flashMain.maxParticles = 1;
        flashMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var flashEmission = flashPS.emission;
        flashEmission.rateOverTime = 0f;
        flashEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var flashShape = flashPS.shape;
        flashShape.enabled = false;

        var flashRenderer = flashObj.GetComponent<ParticleSystemRenderer>();
        flashRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        flashRenderer.sortingOrder = 33;

        // --- Light2D ---
        var light = root.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = new Color(0.19f, 0.63f, 1f, 1f);
        light.intensity = 4.0f;
        light.pointLightOuterRadius = 3.0f;
        light.pointLightInnerRadius = 0.3f;

        var fade = root.AddComponent<FXLightFade>();
        fade.duration = 0.15f;

        // Save prefab
        string path = PrefabDir + "/ChainSparkFX.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log("Created: " + path);
    }

    // ========================================================
    // 3. WarningCircleFX
    // ========================================================
    private static void BuildWarningCircleFX()
    {
        GameObject root = new GameObject("WarningCircleFX");

        // SpriteRenderer with warning circle image
        var sr = root.AddComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Effects/낙하_경고원_이미지.png");
        if (sprite != null)
            sr.sprite = sprite;
        else
            Debug.LogWarning("WarningCircleFX: warning circle sprite not found, using fallback");

        sr.color = new Color(1f, 0.15f, 0.1f, 0.3f);
        sr.sortingOrder = 1;

        // Try Additive material
        var additiveMat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        if (additiveMat != null)
            sr.material = additiveMat;

        // Blink script
        root.AddComponent<FXWarningCircle>();

        // Save prefab
        string path = PrefabDir + "/WarningCircleFX.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log("Created: " + path);
    }
}
