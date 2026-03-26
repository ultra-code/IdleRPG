using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public static class ManualAttackButtonSetup
{
    [MenuItem("Tools/Setup Manual Attack Button")]
    public static void Setup()
    {
        // Find screen-space Canvas
        Canvas canvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c != null && c.renderMode != RenderMode.WorldSpace)
            { canvas = c; break; }
        }
        if (canvas == null)
        {
            Debug.LogError("ManualAttackButtonSetup: No screen-space Canvas found");
            return;
        }

        // Remove existing if any
        var existing = canvas.transform.Find("ManualAttackButton");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // --- Button Root ---
        GameObject btnObj = new GameObject("ManualAttackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(btnObj, "Create ManualAttackButton");
        btnObj.transform.SetParent(canvas.transform, false);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0f, 0f); // bottom-left
        btnRt.anchorMax = new Vector2(0f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(80f, 80f);
        btnRt.sizeDelta = new Vector2(120f, 120f);

        Image bgImage = btnObj.GetComponent<Image>();
        bgImage.color = new Color(0.24f, 0.13f, 0.32f, 1f); // #3D2252

        // --- Border (Outline via slightly larger background) ---
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(btnObj.transform, false);
        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.offsetMin = new Vector2(-3f, -3f);
        borderRt.offsetMax = new Vector2(3f, 3f);
        borderObj.GetComponent<Image>().color = new Color(0.91f, 0.27f, 0.48f, 1f); // #E8457B
        borderObj.GetComponent<Image>().raycastTarget = false;
        borderObj.transform.SetAsFirstSibling(); // behind button content

        // Move bg in front of border by re-parenting trick: just set sibling order
        // Actually, border behind bg won't work. Use Outline component instead.
        Object.DestroyImmediate(borderObj);

        // Use Unity Outline component on the button image
        var outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.91f, 0.27f, 0.48f, 1f); // #E8457B
        outline.effectDistance = new Vector2(3f, -3f);

        // --- Main Text "ATTACK" ---
        GameObject textObj = new GameObject("AttackText", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 0.3f);
        textRt.anchorMax = new Vector2(1f, 1f);
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI mainText = textObj.AddComponent<TextMeshProUGUI>();
        mainText.text = "ATTACK";
        mainText.fontSize = 22;
        mainText.fontStyle = FontStyles.Bold;
        mainText.color = Color.white;
        mainText.alignment = TextAlignmentOptions.Center;
        mainText.raycastTarget = false;

        // --- Sub Text "×10 PWR" ---
        GameObject subObj = new GameObject("SubText", typeof(RectTransform));
        subObj.transform.SetParent(btnObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0f, 0f);
        subRt.anchorMax = new Vector2(1f, 0.35f);
        subRt.offsetMin = Vector2.zero;
        subRt.offsetMax = Vector2.zero;

        TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
        subText.text = "x10 PWR";
        subText.fontSize = 14;
        subText.color = new Color(0.83f, 0.64f, 0.30f, 1f); // #D4A44C
        subText.alignment = TextAlignmentOptions.Center;
        subText.raycastTarget = false;

        // --- Attach ManualAttackButton component ---
        var manualAttack = btnObj.AddComponent<ManualAttackButton>();

        // Wire up serialized fields via SerializedObject
        var so = new SerializedObject(manualAttack);
        so.FindProperty("attackButton").objectReferenceValue = btnObj.GetComponent<Button>();
        so.FindProperty("buttonBackground").objectReferenceValue = bgImage;

        // Find player animator
        var player = Object.FindAnyObjectByType<CharacterStats>();
        if (player != null)
        {
            var animator = player.GetComponent<Animator>();
            if (animator != null)
                so.FindProperty("heroAnimator").objectReferenceValue = animator;
        }

        so.ApplyModifiedProperties();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("ManualAttackButtonSetup: Button created at bottom-left (80, 80), 120x120");
    }
}
