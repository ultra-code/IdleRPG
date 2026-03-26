#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class ShopSetupGenerator
{
    private static Canvas FindScreenSpaceCanvas()
    {
        // Find the main Screen Space Overlay canvas by name first
        var canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            var c = canvasObj.GetComponent<Canvas>();
            if (c != null && c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        }

        // Fallback: search all canvases
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        }

        return null;
    }

    [MenuItem("IdleRPG/Setup/Rebuild Shop UI (Delete + Create)")]
    public static void RebuildShopUI()
    {
        var canvas = FindScreenSpaceCanvas();

        // Delete ShopUI from all canvases (cleanup)
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            var shop = c.GetComponent<ShopUI>();
            if (shop != null)
            {
                Object.DestroyImmediate(shop);
                Debug.Log($"Removed ShopUI from {c.name}");
            }
        }

        // Delete existing objects
        DestroyByName("ShopButton");
        DestroyByName("ShopPanel");

        CreateShopUI();
        Debug.Log("Shop UI rebuilt!");
    }

    private static void DestroyByName(string name)
    {
        // Find all, not just first (in case of duplicates)
        while (true)
        {
            var obj = GameObject.Find(name);
            if (obj == null) break;
            Object.DestroyImmediate(obj);
            Debug.Log($"Deleted {name}");
        }
    }

    [MenuItem("IdleRPG/Setup/Create Shop UI")]
    public static void CreateShopUI()
    {
        var canvas = FindScreenSpaceCanvas();
        if (canvas == null)
        {
            Debug.LogError("Screen Space Overlay Canvas not found. Run 'Create Canvas + PlayerHUD' first.");
            return;
        }
        var root = canvas.transform;

        // Check for duplicates
        if (root.Find("ShopButton") != null)
        {
            Debug.LogWarning("ShopButton already exists. Use 'Rebuild Shop UI' instead.");
            return;
        }

        // === Shop Toggle Button (bottom-right) ===
        var shopBtnObj = CreateButtonObj(root, "ShopButton",
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-20, 20), new Vector2(120, 50),
            "Shop", 22, new Color(0.15f, 0.4f, 0.7f, 1f));

        // === Shop Panel (bottom-center, starts hidden) ===
        var panelObj = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
        panelObj.transform.SetParent(root, false);
        var panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0);
        panelRt.anchorMax = new Vector2(0.5f, 0);
        panelRt.pivot = new Vector2(0.5f, 0);
        panelRt.anchoredPosition = new Vector2(0, 20);
        panelRt.sizeDelta = new Vector2(420, 280);
        panelObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        var goldText = CreateTMP(panelObj.transform, "GoldText",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -15), new Vector2(380, 40),
            "Gold: 0", 24, TextAlignmentOptions.Center, new Color(1f, 0.85f, 0.2f));

        var hpBtn = CreateButtonObj(panelObj.transform, "BtnUpgradeHP",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -65), new Vector2(360, 45),
            "HP Up Lv.0 | 50G", 20, new Color(0.2f, 0.6f, 0.2f, 1f));

        var atkBtn = CreateButtonObj(panelObj.transform, "BtnUpgradeATK",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -120), new Vector2(360, 45),
            "ATK Up Lv.0 | 50G", 20, new Color(0.6f, 0.2f, 0.2f, 1f));

        var defBtn = CreateButtonObj(panelObj.transform, "BtnUpgradeDEF",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -175), new Vector2(360, 45),
            "DEF Up Lv.0 | 50G", 20, new Color(0.2f, 0.3f, 0.6f, 1f));

        var closeBtn = CreateButtonObj(panelObj.transform, "BtnClose",
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-10, -10), new Vector2(40, 40),
            "X", 20, new Color(0.5f, 0.1f, 0.1f, 1f));

        // Attach ShopUI to the Canvas GameObject (always active)
        var shopUI = canvas.gameObject.AddComponent<ShopUI>();
        var so = new SerializedObject(shopUI);
        so.FindProperty("shopPanel").objectReferenceValue = panelObj;
        so.FindProperty("shopToggleButton").objectReferenceValue = shopBtnObj.GetComponent<Button>();
        so.FindProperty("closeButton").objectReferenceValue = closeBtn.GetComponent<Button>();
        so.FindProperty("goldText").objectReferenceValue = goldText;
        so.FindProperty("hpButton").objectReferenceValue = hpBtn.GetComponent<Button>();
        so.FindProperty("hpButtonText").objectReferenceValue = hpBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("atkButton").objectReferenceValue = atkBtn.GetComponent<Button>();
        so.FindProperty("atkButtonText").objectReferenceValue = atkBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.FindProperty("defButton").objectReferenceValue = defBtn.GetComponent<Button>();
        so.FindProperty("defButtonText").objectReferenceValue = defBtn.GetComponentInChildren<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(canvas.gameObject);

        // Panel starts hidden (ShopUI.Start will also call SetShopVisible(false))
        panelObj.SetActive(false);

        Debug.Log($"Shop UI created on '{canvas.name}'! ShopUI component on Canvas, ShopPanel toggles.");
        Selection.activeGameObject = canvas.gameObject;
    }

    private static GameObject CreateButtonObj(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size,
        string label, int fontSize, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        obj.GetComponent<Image>().color = bgColor;

        var btn = obj.GetComponent<Button>();
        btn.targetGraphic = obj.GetComponent<Image>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = colors;

        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        var textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(5, 0);
        textRt.offsetMax = new Vector2(-5, 0);

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return obj;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size,
        string text, int fontSize, TextAlignmentOptions align, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;

        return tmp;
    }
}
#endif
