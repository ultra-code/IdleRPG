using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUIController : MonoBehaviour
{
    private static BossUIController instance;

    private WaveSpawnManager waveManager;
    private Enemy currentBoss;
    private GameObject panelRoot;
    private Image hpFill;
    private TextMeshProUGUI bossNameText;
    private TextMeshProUGUI hpText;
    private TextMeshProUGUI alertText;
    private float alertTimer;

    public static void EnsureExists(WaveSpawnManager manager)
    {
        if (manager == null) return;
        if (instance != null)
        {
            instance.Bind(manager);
            return;
        }

        Canvas canvas = FindScreenSpaceCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("BossUIController: no screen-space Canvas found, cannot create boss UI.");
            return;
        }

        GameObject obj = new GameObject("BossUIController", typeof(RectTransform));
        obj.transform.SetParent(canvas.transform, false);
        instance = obj.AddComponent<BossUIController>();
        instance.BuildUI();
        instance.Bind(manager);
    }

    private void Bind(WaveSpawnManager manager)
    {
        if (waveManager == manager) return;

        if (waveManager != null)
        {
            waveManager.OnBossSpawned -= HandleBossSpawned;
            waveManager.OnBossDefeated -= HandleBossDefeated;
        }

        waveManager = manager;
        waveManager.OnBossSpawned += HandleBossSpawned;
        waveManager.OnBossDefeated += HandleBossDefeated;
    }

    private void BuildUI()
    {
        panelRoot = CreatePanel("BossPanel", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(720f, 88f));
        panelRoot.SetActive(false);

        Image bg = panelRoot.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.03f, 0.03f, 0.9f);

        bossNameText = CreateTMP(panelRoot.transform, "BossNameText", new Vector2(0f, -10f), new Vector2(680f, 28f), 24, "BOSS");
        bossNameText.alignment = TextAlignmentOptions.Center;
        bossNameText.color = new Color(1f, 0.82f, 0.3f, 1f);

        GameObject hpBg = CreatePanel("BossHPBackground", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(660f, 22f), panelRoot.transform);
        hpBg.GetComponent<Image>().color = new Color(0.18f, 0.08f, 0.08f, 1f);

        GameObject fill = new GameObject("BossHPFill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(hpBg.transform, false);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        hpFill = fill.GetComponent<Image>();
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillAmount = 1f;
        hpFill.color = new Color(0.88f, 0.15f, 0.15f, 1f);

        hpText = CreateTMP(panelRoot.transform, "BossHPText", new Vector2(0f, -70f), new Vector2(680f, 18f), 16, "0 / 0");
        hpText.alignment = TextAlignmentOptions.Center;

        alertText = CreateTMP(transform, "BossAlertText", new Vector2(0f, -170f), new Vector2(640f, 80f), 54, "BOSS!");
        alertText.alignment = TextAlignmentOptions.Center;
        alertText.color = new Color(1f, 0.2f, 0.15f, 0f);
    }

    private void Update()
    {
        if (currentBoss != null && !currentBoss.IsDead)
        {
            panelRoot.SetActive(true);
            bossNameText.text = currentBoss.EnemyName;
            hpFill.fillAmount = currentBoss.MaxHP > 0f ? currentBoss.CurrentHP / currentBoss.MaxHP : 0f;
            hpText.text = $"{Mathf.CeilToInt(currentBoss.CurrentHP)} / {Mathf.CeilToInt(currentBoss.MaxHP)}";
        }
        else if (panelRoot != null && panelRoot.activeSelf)
        {
            panelRoot.SetActive(false);
        }

        if (alertText != null && alertTimer > 0f)
        {
            alertTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(alertTimer / 1.2f);
            alertText.color = new Color(1f, 0.2f, 0.15f, alpha);
            alertText.rectTransform.localScale = Vector3.one * (1f + (1f - alpha) * 0.15f);
        }
        else if (alertText != null)
        {
            alertText.color = new Color(1f, 0.2f, 0.15f, 0f);
        }
    }

    private void HandleBossSpawned(Enemy boss)
    {
        currentBoss = boss;
        alertTimer = 1.2f;
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    private void HandleBossDefeated(Enemy boss)
    {
        if (currentBoss == boss)
            currentBoss = null;
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

    private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Transform parentOverride = null)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parentOverride != null ? parentOverride : transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return obj;
    }

    private TextMeshProUGUI CreateTMP(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, string text)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.text = text;
        tmp.color = Color.white;
        return tmp;
    }
}
