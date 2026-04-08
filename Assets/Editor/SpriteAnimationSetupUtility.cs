using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SpriteAnimationSetupUtility
{
    private const string AnimationRoot = "Assets/Animations";
    private const string ControllerRoot = "Assets/Animations/Controllers";
    private const string HeroSpriteRoot = "Assets/Sprites/Characters";
    private const string MonsterSpriteRoot = "Assets/Sprites/Monsters";

    [MenuItem("Tools/IdleRPG/Build Sprite Animation Assets")]
    public static void BuildSpriteAnimationAssets()
    {
        EnsureFolders();

        AnimatorController heroController = BuildHeroAnimations();
        Dictionary<string, AnimatorController> enemyControllers = BuildEnemyAnimations();

        AssignHeroController(heroController);
        AssignEnemyControllers(enemyControllers);

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SpriteAnimationSetupUtility] Sprite animation assets built and assigned.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Animations");
        EnsureFolder(AnimationRoot, "Hero");
        EnsureFolder(AnimationRoot, "Monsters");
        EnsureFolder("Assets/Animations", "Controllers");
    }

    private static AnimatorController BuildHeroAnimations()
    {
        var clipMap = new Dictionary<string, AnimationClip>
        {
            ["idle"] = CreateClipFromPath($"{HeroSpriteRoot}/hero_idle_sheet.png", "Assets/Animations/Hero/hero_idle.anim", true),
            ["walk"] = CreateClipFromPath($"{HeroSpriteRoot}/hero_walk_sheet.png", "Assets/Animations/Hero/hero_walk.anim", true),
            ["attack"] = CreateClipFromPath($"{HeroSpriteRoot}/hero_attack_sheet.png", "Assets/Animations/Hero/hero_attack.anim", false),
            ["hit"] = CreateClipFromPath($"{HeroSpriteRoot}/hero_hit_sheet.png", "Assets/Animations/Hero/hero_hit.anim", false),
            ["skill"] = CreateClipFromPath($"{HeroSpriteRoot}/hero_skill_sheet.png", "Assets/Animations/Hero/hero_skill.anim", false)
        };

        string controllerPath = $"{ControllerRoot}/Hero.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        ConfigureController(controller, clipMap, "idle");
        return controller;
    }

    private static Dictionary<string, AnimatorController> BuildEnemyAnimations()
    {
        var controllers = new Dictionary<string, AnimatorController>();
        string[] ids = { "flesh_slime", "eye_stalker", "skeleton_demon", "blood_cells", "twin_head" };

        foreach (string id in ids)
        {
            var clipMap = new Dictionary<string, AnimationClip>
            {
                ["idle"] = CreateClipFromPath($"{MonsterSpriteRoot}/{id}_idle_sheet.png", $"Assets/Animations/Monsters/{id}_idle.anim", true),
                ["walk"] = CreateClipFromPath($"{MonsterSpriteRoot}/{id}_walk_sheet.png", $"Assets/Animations/Monsters/{id}_walk.anim", true),
                ["attack"] = CreateClipFromPath($"{MonsterSpriteRoot}/{id}_attack_sheet.png", $"Assets/Animations/Monsters/{id}_attack.anim", false),
                ["hit"] = CreateClipFromPath($"{MonsterSpriteRoot}/{id}_hit_sheet.png", $"Assets/Animations/Monsters/{id}_hit.anim", false),
                ["die"] = CreateClipFromPath($"{MonsterSpriteRoot}/{id}_die_sheet.png", $"Assets/Animations/Monsters/{id}_die.anim", false)
            };

            string controllerPath = $"{ControllerRoot}/{id}.controller";
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            ConfigureController(controller, clipMap, clipMap["walk"] != null ? "walk" : "idle");
            controllers[id] = controller;
        }

        return controllers;
    }

    private static void ConfigureController(AnimatorController controller, Dictionary<string, AnimationClip> clips, string defaultState)
    {
        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorStateMachine stateMachine = layer.stateMachine;

        controller.parameters = new AnimatorControllerParameter[0];
        foreach (ChildAnimatorState state in stateMachine.states.ToArray())
        {
            stateMachine.RemoveState(state.state);
        }

        AddTrigger(controller, "Attack");
        AddTrigger(controller, "Hit");
        AddTrigger(controller, "Skill");
        AddTrigger(controller, "Die");

        var stateMap = new Dictionary<string, AnimatorState>();
        foreach (KeyValuePair<string, AnimationClip> pair in clips)
        {
            if (pair.Value == null)
                continue;

            AnimatorState state = stateMachine.AddState(pair.Key);
            state.motion = pair.Value;
            stateMap[pair.Key] = state;

            if (pair.Key == defaultState)
                stateMachine.defaultState = state;
        }

        if (stateMachine.defaultState == null && stateMap.Count > 0)
            stateMachine.defaultState = stateMap.Values.First();

        AnimatorState defaultAnimatorState = stateMachine.defaultState;
        if (defaultAnimatorState == null)
            return;

        AddOneShotTransition(stateMachine, defaultAnimatorState, stateMap, "attack", "Attack", 0.9f);
        AddOneShotTransition(stateMachine, defaultAnimatorState, stateMap, "hit", "Hit", 0.9f);
        AddOneShotTransition(stateMachine, defaultAnimatorState, stateMap, "skill", "Skill", 0.95f);
        AddDieTransition(stateMachine, stateMap);
    }

    private static AnimationClip CreateClipFromPath(string spritePath, string clipPath, bool loop)
    {
        List<Sprite> sprites = LoadSprites(spritePath);
        if (sprites.Count == 0)
            return null;

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = GetClipFrameRate(clipPath);

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / clip.frameRate,
                value = sprites[i]
            };
        }

        if (sprites.Count == 1)
        {
            keyframes[0].time = 0f;
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
        if (loopProperty != null)
        {
            loopProperty.boolValue = loop;
            serializedClip.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static List<Sprite> LoadSprites(string assetPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToList();
    }

    private static void AssignHeroController(AnimatorController controller)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
        GameObject player = GameObject.Find("Player");
        if (player == null)
            return;

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
            animator = player.AddComponent<Animator>();

        ActorAnimationEvents animationEvents = player.GetComponent<ActorAnimationEvents>();
        if (animationEvents == null)
            player.AddComponent<ActorAnimationEvents>();

        animator.runtimeAnimatorController = controller;
        EditorUtility.SetDirty(player);
    }

    private static void AssignEnemyControllers(Dictionary<string, AnimatorController> controllers)
    {
        string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
        if (prefab != null)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null)
                animator = prefabRoot.AddComponent<Animator>();

            if (prefabRoot.GetComponent<ActorAnimationEvents>() == null)
                prefabRoot.AddComponent<ActorAnimationEvents>();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, enemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssignEnemyController("Assets/Data/Enemies/EnemyData_Slime.asset", controllers, "flesh_slime");
        AssignEnemyController("Assets/Data/Enemies/EnemyData_EyeStalker.asset", controllers, "eye_stalker");
        AssignEnemyController("Assets/Data/Enemies/EnemyData_SkeletonDemon.asset", controllers, "skeleton_demon");
        AssignEnemyController("Assets/Data/Enemies/EnemyData_BloodCells.asset", controllers, "blood_cells");
        AssignEnemyController("Assets/Data/Enemies/EnemyData_TwinHeadMutant.asset", controllers, "twin_head");
    }

    private static void AssignEnemyController(string assetPath, Dictionary<string, AnimatorController> controllers, string key)
    {
        EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath);
        if (data == null || !controllers.TryGetValue(key, out AnimatorController controller))
            return;

        data.AnimatorController = controller;
        EditorUtility.SetDirty(data);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string fullPath = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(fullPath))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static float GetClipFrameRate(string clipPath)
    {
        string path = clipPath.ToLowerInvariant();
        if (path.Contains("_attack"))
            return 12f;
        if (path.Contains("_hit"))
            return 14f;
        if (path.Contains("_skill"))
            return 10f;
        if (path.Contains("_die"))
            return 8f;
        if (path.Contains("_walk"))
            return 8f;

        return 6f;
    }

    private static void AddTrigger(AnimatorController controller, string name)
    {
        if (!controller.parameters.Any(parameter => parameter.name == name))
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
    }

    private static void AddOneShotTransition(AnimatorStateMachine stateMachine, AnimatorState defaultState, Dictionary<string, AnimatorState> stateMap, string stateKey, string triggerName, float exitTime)
    {
        if (!stateMap.TryGetValue(stateKey, out AnimatorState targetState))
            return;

        AnimatorStateTransition anyToTarget = stateMachine.AddAnyStateTransition(targetState);
        anyToTarget.hasExitTime = false;
        anyToTarget.duration = 0.05f;
        anyToTarget.AddCondition(AnimatorConditionMode.If, 0f, triggerName);

        AnimatorStateTransition targetToDefault = targetState.AddTransition(defaultState);
        targetToDefault.hasExitTime = true;
        targetToDefault.exitTime = exitTime;
        targetToDefault.duration = 0.05f;
    }

    private static void AddDieTransition(AnimatorStateMachine stateMachine, Dictionary<string, AnimatorState> stateMap)
    {
        if (!stateMap.TryGetValue("die", out AnimatorState dieState))
            return;

        AnimatorStateTransition anyToDie = stateMachine.AddAnyStateTransition(dieState);
        anyToDie.hasExitTime = false;
        anyToDie.duration = 0.02f;
        anyToDie.AddCondition(AnimatorConditionMode.If, 0f, "Die");
    }
}
