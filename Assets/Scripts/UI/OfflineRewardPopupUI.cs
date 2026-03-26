using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineRewardPopupUI : MonoBehaviour
{
    private static OfflineRewardPopupUI instance;

    private OfflineRewardSystem rewardSystem;
    private GameObject panelRoot;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;
    private TextMeshProUGUI goldText;
    private TextMeshProUGUI timeText;
    private TextMeshProUGUI claimButtonText;
    private Button claimButton;

    private float displayedGold;
    private float targetGold;
    private bool animatingOpen;
    private bool closing;
    private float closeTimer;

    public static void EnsureExists(OfflineRewardSystem system)
    {
        if (system == null)
            return;

        if (instance != null)
        {
            instance.Bind(system);
            return;
        }

        Canvas canvas = FindScreenSpaceCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("OfflineRewardPopupUI: screen-space Canvas를 찾지 못했습니다.");
            return;
        }

        GameObject obj = new GameObject("OfflineRewardPopupUI", typeof(RectTransform));
        obj.transform.SetParent(canvas.transform, false);
        instance = obj.AddComponent<OfflineRewardPopupUI>();
        instance.BuildUI();
        instance.Bind(system);
    }

    private void Bind(OfflineRewardSystem system)
    {
        if (rewardSystem == system)
            return;

        if (rewardSystem != null)
            rewardSystem.OnRewardReady -= HandleRewardReady;

        rewardSystem = system;
        rewardSystem.OnRewardReady += HandleRewardReady;

        if (rewardSystem.HasPendingReward)
            Show(rewardSystem.OfflineSeconds, rewardSystem.PendingGold);
    }

    private void BuildUI()
    {
        panelRoot = CreatePanel("OfflineRewardPanel", transform, new Vector2(560f, 360f), new Color(0.08f, 0.06f, 0.14f, 0.94f));
        RectTransform rootRt = panelRoot.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0.5f);
        rootRt.anchorMax = new Vector2(0.5f, 0.5f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = Vector2.zero;

        titleText = CreateTMP("TitleText", panelRoot.transform, new Vector2(0f, 135f), new Vector2(440f, 48f), 34, "Offline Reward");
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.86f, 0.35f, 1f);

        descText = CreateTMP("DescText", panelRoot.transform, new Vector2(0f, 80f), new Vector2(460f, 40f), 22, "Gold accumulated while you were away!");
        descText.alignment = TextAlignmentOptions.Center;

        GameObject amountPanel = CreatePanel("AmountPanel", panelRoot.transform, new Vector2(420f, 110f), new Color(0.14f, 0.1f, 0.2f, 0.95f));
        RectTransform amountRt = amountPanel.GetComponent<RectTransform>();
        amountRt.anchorMin = new Vector2(0.5f, 0.5f);
        amountRt.anchorMax = new Vector2(0.5f, 0.5f);
        amountRt.pivot = new Vector2(0.5f, 0.5f);
        amountRt.anchoredPosition = new Vector2(0f, 10f);

        goldText = CreateTMP("GoldText", amountPanel.transform, Vector2.zero, new Vector2(360f, 72f), 42, "0 G");
        goldText.alignment = TextAlignmentOptions.Center;
        goldText.color = new Color(1f, 0.9f, 0.32f, 1f);

        timeText = CreateTMP("TimeText", panelRoot.transform, new Vector2(0f, -78f), new Vector2(420f, 36f), 22, "Elapsed: 0m");
        timeText.alignment = TextAlignmentOptions.Center;

        GameObject claimObj = CreateButton("ClaimButton", panelRoot.transform, new Vector2(0f, -138f), new Vector2(260f, 54f), new Color(0.45f, 0.22f, 0.72f, 1f));
        claimButton = claimObj.GetComponent<Button>();
        claimButtonText = claimObj.GetComponentInChildren<TextMeshProUGUI>();
        claimButtonText.text = "Claim";
        claimButton.onClick.AddListener(HandleClaim);

        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (animatingOpen)
        {
            displayedGold = Mathf.MoveTowards(displayedGold, targetGold, Mathf.Max(1f, targetGold) * Time.unscaledDeltaTime * 3.5f);
            goldText.text = $"{Mathf.RoundToInt(displayedGold):N0} G";
            if (Mathf.Approximately(displayedGold, targetGold))
                animatingOpen = false;
        }

        if (closing)
        {
            closeTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(closeTimer / 0.28f);
            panelRoot.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.92f, t);
            CanvasGroup cg = GetOrAddCanvasGroup(panelRoot);
            cg.alpha = 1f - t;
            if (t >= 1f)
            {
                closing = false;
                panelRoot.SetActive(false);
                panelRoot.transform.localScale = Vector3.one;
                cg.alpha = 1f;
                claimButtonText.text = "Claim";
                claimButton.interactable = true;
            }
        }
    }

    private void HandleRewardReady(float seconds, float gold)
    {
        Show(seconds, gold);
    }

    private void Show(float seconds, float gold)
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(true);
        panelRoot.transform.localScale = Vector3.one;
        GetOrAddCanvasGroup(panelRoot).alpha = 1f;
        closing = false;
        closeTimer = 0f;

        targetGold = gold;
        displayedGold = 0f;
        animatingOpen = true;

        goldText.text = "0 G";
        timeText.text = $"Elapsed: {FormatTime(seconds)}";
        claimButtonText.text = "Claim";
        claimButton.interactable = true;
    }

    private void HandleClaim()
    {
        if (rewardSystem == null || !rewardSystem.HasPendingReward || closing)
            return;

        claimButton.interactable = false;
        claimButtonText.text = "Claiming...";
        rewardSystem.ClaimReward();
        animatingOpen = false;
        closing = true;
        closeTimer = 0f;
    }

    private static Canvas FindScreenSpaceCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null) continue;
            if (canvas.name == "Canvas" && canvas.renderMode != RenderMode.WorldSpace)
                return canvas;
        }

        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                return canvas;
        }

        return null;
    }

    private static string FormatTime(float seconds)
    {
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        if (h > 0)
            return $"{h}h {m}m";
        return $"{m}m";
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        obj.GetComponent<Image>().color = color;
        return obj;
    }

    private static TextMeshProUGUI CreateTMP(string name, Transform parent, Vector2 pos, Vector2 size, int fontSize, string text)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.text = text;
        tmp.color = Color.white;
        return tmp;
    }

    private static GameObject CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image image = obj.GetComponent<Image>();
        image.color = color;

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Claim";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return obj;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        if (group == null)
            group = obj.AddComponent<CanvasGroup>();
        return group;
    }
}
