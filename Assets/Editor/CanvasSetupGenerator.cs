#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class CanvasSetupGenerator
{
    [MenuItem("IdleRPG/Setup/Create Canvas + PlayerHUD")]
    public static void CreateCanvasAndHUD()
    {
        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // PlayerHUD 패널 (좌상단)
        var hudObj = CreatePanel(canvasObj.transform, "PlayerHUD",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(400, 260));

        var hudImage = hudObj.GetComponent<Image>();
        hudImage.color = new Color(0, 0, 0, 0.5f);

        // Stage/Wave (우상단)
        var stagePanel = CreatePanel(canvasObj.transform, "StagePanel",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(360, 50));
        stagePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        // HUD 내부 요소
        var levelText = CreateTMP(hudObj.transform, "LevelText",
            new Vector2(20, -10), new Vector2(360, 40), 28, "Lv.1", TextAlignmentOptions.Left);

        var hpBarBg = CreateBarBackground(hudObj.transform, "HPBar",
            new Vector2(20, -55), new Vector2(360, 24), new Color(0.3f, 0, 0, 1));
        var hpFill = CreateBarFill(hpBarBg.transform, "HPFill", new Color(0.2f, 0.8f, 0.2f, 1));
        var hpText = CreateTMP(hpBarBg.transform, "HPText",
            Vector2.zero, new Vector2(360, 24), 16, "100 / 100", TextAlignmentOptions.Center);

        var expBarBg = CreateBarBackground(hudObj.transform, "EXPBar",
            new Vector2(20, -85), new Vector2(360, 16), new Color(0.15f, 0.15f, 0.3f, 1));
        var expFill = CreateBarFill(expBarBg.transform, "EXPFill", new Color(0.3f, 0.5f, 1f, 1));

        var atkText = CreateTMP(hudObj.transform, "ATKText",
            new Vector2(20, -115), new Vector2(170, 30), 20, "ATK 10", TextAlignmentOptions.Left);

        var goldText = CreateTMP(hudObj.transform, "GoldText",
            new Vector2(200, -115), new Vector2(170, 30), 20, "0 G", TextAlignmentOptions.Left);

        // Stage/Wave 텍스트
        var stageWaveText = CreateTMP(stagePanel.transform, "StageWaveText",
            Vector2.zero, new Vector2(340, 40), 22, "Stage 1 - Wave 0/3", TextAlignmentOptions.Center);

        // PlayerHUD 컴포넌트 연결
        var hud = hudObj.AddComponent<PlayerHUD>();
        var so = new SerializedObject(hud);
        so.FindProperty("levelText").objectReferenceValue = levelText;
        so.FindProperty("hpText").objectReferenceValue = hpText;
        so.FindProperty("atkText").objectReferenceValue = atkText;
        so.FindProperty("goldText").objectReferenceValue = goldText;
        so.FindProperty("stageWaveText").objectReferenceValue = stageWaveText;
        so.FindProperty("hpFill").objectReferenceValue = hpFill.GetComponent<Image>();
        so.FindProperty("expFill").objectReferenceValue = expFill.GetComponent<Image>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // EventSystem 확인
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("Canvas + PlayerHUD 생성 완료! Inspector에서 Player, WaveManager 연결 필요");
        Selection.activeGameObject = canvasObj;
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        return obj;
    }

    private static GameObject CreateBarBackground(Transform parent, string name,
        Vector2 localPos, Vector2 size, Color bgColor)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = localPos;
        rt.sizeDelta = size;

        obj.GetComponent<Image>().color = bgColor;
        return obj;
    }

    private static GameObject CreateBarFill(Transform parent, string name, Color fillColor)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = obj.GetComponent<Image>();
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillAmount = 1f;
        img.color = fillColor;

        return obj;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name,
        Vector2 localPos, Vector2 size, int fontSize, string defaultText, TextAlignmentOptions align)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = localPos;
        rt.sizeDelta = size;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = Color.white;

        return tmp;
    }
}
#endif
